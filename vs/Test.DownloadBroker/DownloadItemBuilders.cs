using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Zip;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;
using Common.Helpers;

namespace ZeroInstall.DownloadBroker
{
    public abstract class HierarchyEntry
    {
        public delegate void EntryHandler(HierarchyEntry entry);

        private HierarchyEntry _parent;

        public string Name { get; private set; }
        public DateTime LastWriteTime { get; private set; }
        public string RelativePath
        {
            get
            {
                if (_parent == null) return Name;
                if (_parent.RelativePath == "") return Name;
                else return _parent.RelativePath + "/" + Name;
            }
        }

        protected HierarchyEntry(string name, HierarchyEntry parent)
        {
            _parent = parent;
            Name = name;
            LastWriteTime = DateTime.Today.ToUniversalTime();
        }

        public abstract void WriteIntoFolder(string folderPath);
        public virtual void RecurseInto(EntryHandler action)
        {
            action.Invoke(this);
        }
    }

    public class FileEntry : HierarchyEntry
    {
        private MemoryStream _content;

        public byte[] Content
        {
            get { return _content.ToArray(); }
        }

        public FileEntry(string name, byte[] content) : this(name, content, null)
        { }

        public FileEntry(string name, byte[] content, FolderEntry parent) : base(name, parent)
        {
            using (_content = new MemoryStream(content.Length))
            {
                _content.Write(content, 0, content.Length);
            }
        }

        public override void WriteIntoFolder(string folderPath)
        {
            if (ReferenceEquals(null, folderPath)) throw new ArgumentNullException("folderPath");
            string combinedPath = Path.Combine(folderPath, Name);
            if (File.Exists(combinedPath)) throw new InvalidOperationException("Can't overwrite existing file or folder!");
            File.WriteAllBytes(combinedPath, _content.ToArray());
            File.SetLastWriteTimeUtc(combinedPath, LastWriteTime);
        }
    }

    public class FolderEntry : HierarchyEntry
    {
        private List<HierarchyEntry> entries = new List<HierarchyEntry>();

        public FolderEntry(string name) : this(name, null)
        { }

        public FolderEntry(string name, FolderEntry parent) : base(name, parent)
        { }

        public void Add(HierarchyEntry newEntry)
        {
            entries.Add(newEntry);
            entries.Sort((HierarchyEntry left, HierarchyEntry right) => StringComparer.InvariantCulture.Compare(left.Name, right.Name));
        }

        public override void WriteIntoFolder(string folderPath)
        {
            if (ReferenceEquals(null, folderPath)) throw new ArgumentNullException("folderPath");
            string combinedPath;
            if (Name != "")
            {
                combinedPath = Path.Combine(folderPath, Name);
                if (Directory.Exists(combinedPath)) throw new InvalidOperationException("Can't overwrite existing folder with hierarchy!");
                Directory.CreateDirectory(combinedPath);
            }
            else
            {
                combinedPath = folderPath;
                if (!Directory.Exists(combinedPath)) Directory.CreateDirectory(combinedPath);
            }
            foreach (var currentEntry in entries)
            {
                currentEntry.WriteIntoFolder(combinedPath);
            }
        }

        public override void RecurseInto(EntryHandler action)
        {
            base.RecurseInto(action);
            foreach (var entry in entries)
            {
                entry.RecurseInto(action);
            }
        }
    }

    public class PackageBuilder
    {
        FolderEntry packageHierarchy;

        public PackageBuilder() : this(new FolderEntry("")) { }

        public PackageBuilder(FolderEntry folder)
        {
            packageHierarchy = folder;
        }

        public PackageBuilder AddFolder(string name)
        {
            var item = new FolderEntry(name, packageHierarchy);
            packageHierarchy.Add(item);
            return new PackageBuilder(item);
        }

        public void AddFile(string name, byte[] content)
        {
            packageHierarchy.Add(new FileEntry(name, content, packageHierarchy));
        }

        public void WritePackageInto(string packageDirectory)
        {
            packageHierarchy.WriteIntoFolder(packageDirectory);
        }

        public void GeneratePackageArchive(Stream output)
        {
            using (var zip = new ZipOutputStream(output))
            {
                zip.SetLevel(9);

                HierarchyEntry.EntryHandler entryToZip = delegate(HierarchyEntry entry)
                {
                    if (entry.GetType() == typeof (FolderEntry))
                        return;
                    var zipEntry = new ZipEntry(entry.RelativePath);
                    zipEntry.DateTime = entry.LastWriteTime;
                    zip.PutNextEntry(zipEntry);
                    if (entry.GetType() == typeof (FileEntry))
                    {
                        var fileEntry = (FileEntry)entry;
                        var writer = new BinaryWriter(zip);
                        writer.Write(fileEntry.Content);
                    }
                };
                packageHierarchy.RecurseInto(entryToZip);
            }
        }

        public ManifestDigest ComputePackageDigest()
        {
            throw new Exception();
        }
    }
}
