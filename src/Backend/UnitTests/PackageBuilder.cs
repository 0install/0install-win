/*
 * Copyright 2010 Roland Leopold Walkling
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;

namespace ZeroInstall
{
    public abstract class HierarchyEntry
    {
        private readonly EntryContainer _parent;

        public string Name { get; private set; }
        public DateTime LastWriteTime { get; private set; }

        public string RelativePath
        {
            get
            {
                if (IsRoot()) return Name;
                else return Path.Combine(_parent.RelativePath, Name);
            }
        }

        protected HierarchyEntry(string name, EntryContainer parent, DateTime lastWriteTime)
        {
            #region Preconditions
            Debug.Assert(name != null);
            #endregion

            if (name != null && name.IndexOfAny(Path.GetInvalidFileNameChars()) != -1) throw new ArgumentException("Invalid file name.");

            _parent = parent;
            Name = name;
            LastWriteTime = lastWriteTime;
        }

        public bool IsRoot()
        {
            return _parent == null;
        }

        public abstract void AcceptVisitor(HierarchyVisitor visitor);
    }

    public class FileEntry : HierarchyEntry
    {
        private readonly MemoryStream _content;

        public byte[] Content { get { return _content.ToArray(); } }

        internal FileEntry(string name, byte[] content, EntryContainer parent, DateTime lastWrite)
            : base(name, parent, lastWrite)
        {
            #region Preconditions
            Debug.Assert(!string.IsNullOrEmpty(name));
            Debug.Assert(content != null);
            Debug.Assert(parent != null);
            #endregion

            _content = new MemoryStream(content.Length);
            _content.Write(content, 0, content.Length);
            _content.Seek(0, SeekOrigin.Begin);
        }

        public override void AcceptVisitor(HierarchyVisitor visitor)
        {
            visitor.VisitFile(this);
        }
    }

    public abstract class EntryContainer : HierarchyEntry
    {
        protected readonly List<HierarchyEntry> Entries = new List<HierarchyEntry>();

        protected EntryContainer(string name, EntryContainer parent, DateTime lastWrite)
            : base(name, parent, lastWrite)
        {}

        public void Add(HierarchyEntry newEntry)
        {
            Entries.Add(newEntry);
            Entries.Sort((left, right) => StringComparer.InvariantCulture.Compare(left.Name, right.Name));
        }

        public IEnumerable<HierarchyEntry> Children { get { return Entries; } }
    }

    public class FolderEntry : EntryContainer
    {
        internal FolderEntry(string name, EntryContainer parent, DateTime lastWrite)
            : base(name, parent, lastWrite)
        {
            #region Preconditions
            Debug.Assert(!string.IsNullOrEmpty(name));
            Debug.Assert(parent != null);
            #endregion
        }

        public override void AcceptVisitor(HierarchyVisitor visitor)
        {
            visitor.VisitFolder(this);
        }
    }

    public class RootEntry : EntryContainer
    {
        internal RootEntry(DateTime lastWrite)
            : base("", null, lastWrite)
        {}

        public override void AcceptVisitor(HierarchyVisitor visitor)
        {
            visitor.VisitRoot(this);
        }
    }

    public abstract class HierarchyVisitor
    {
        public virtual void VisitFile(FileEntry entry)
        {}

        public virtual void VisitFolder(FolderEntry entry)
        {
            VisitChildren(entry);
        }

        public virtual void VisitRoot(RootEntry entry)
        {
            VisitChildren(entry);
        }

        protected void VisitChildren(EntryContainer entry)
        {
            foreach (var child in entry.Children)
                child.AcceptVisitor(this);
        }
    }

    internal class HierarchyToZip : HierarchyVisitor
    {
        private readonly ZipOutputStream _zip;

        internal HierarchyToZip(ZipOutputStream zipOut)
        {
            Debug.Assert(zipOut != null);
            _zip = zipOut;
        }

        public override void VisitFile(FileEntry entry)
        {
            WriteFileEntryToZip(entry);
        }

        private void WriteFileEntryToZip(FileEntry entry)
        {
            _zip.PutNextEntry(CreateZipEntry(entry));
            var writer = new BinaryWriter(_zip);
            writer.Write(entry.Content);
            writer.Flush();
        }

        private static ZipEntry CreateZipEntry(HierarchyEntry entry)
        {
            var zipEntry = new ZipEntry(entry.RelativePath) {DateTime = entry.LastWriteTime};
            return zipEntry;
        }

        public override void VisitFolder(FolderEntry entry)
        {
            WriteFolderEntryToZip(_zip, entry);
            VisitChildren(entry);
        }

        private static void WriteFolderEntryToZip(ZipOutputStream zip, FolderEntry entry)
        {
            var zipEntry = new ZipEntry(entry.RelativePath + "/") {DateTime = entry.LastWriteTime};
            zip.PutNextEntry(zipEntry);
        }

        public override void VisitRoot(RootEntry entry)
        {
            VisitChildren(entry);
        }
    }

    internal class HierarchyToFolder : HierarchyVisitor
    {
        private readonly string _folder;

        public HierarchyToFolder(string targetFolder)
        {
            if (targetFolder.IndexOfAny(Path.GetInvalidPathChars()) != -1) throw new ArgumentException("Invalid path supplied.");
            _folder = targetFolder;
        }

        public override void VisitFile(FileEntry entry)
        {
            string combinedPath = Path.Combine(_folder, entry.RelativePath);
            CheckWritePath(combinedPath);
            WriteFileEntryTo(entry, combinedPath);
        }

        private static void CheckWritePath(string combinedPath)
        {
            if (File.Exists(combinedPath)) throw new InvalidOperationException("Can't overwrite existing file.");
            if (Directory.Exists(combinedPath)) throw new InvalidOperationException("Can't overwrite existing _folder.");
        }

        private static void WriteFileEntryTo(FileEntry entry, string combinedPath)
        {
            File.WriteAllBytes(combinedPath, entry.Content);
            SetDestinationDate(combinedPath, entry);
        }

        protected static void SetDestinationDate(string combinedPath, HierarchyEntry entry)
        {
            Directory.SetLastWriteTimeUtc(combinedPath, entry.LastWriteTime);
        }

        public override void VisitFolder(FolderEntry entry)
        {
            string combinedPath = Path.Combine(_folder, entry.RelativePath);
            CheckAndPrepareWritePathForFolder(combinedPath);
            SetDestinationDate(combinedPath, entry);
            VisitChildren(entry);
        }

        protected static void CheckAndPrepareWritePathForFolder(string thisFoldersPath)
        {
            if (Directory.Exists(thisFoldersPath)) throw new InvalidOperationException("Can't overwrite existing _folder.");
            if (File.Exists(thisFoldersPath)) throw new InvalidOperationException("Can't overwrite existing file.");
            Directory.CreateDirectory(thisFoldersPath);
        }

        public override void VisitRoot(RootEntry entry)
        {
            if (!Directory.Exists(_folder))
            {
                Directory.CreateDirectory(_folder);
                SetDestinationDate(_folder, entry);
            }

            VisitChildren(entry);
        }
    }

    public class PackageBuilder
    {
        public static readonly DateTime DefaultDate = new DateTime(2000, 1, 1);

        private readonly EntryContainer _currentSubhierarchy;
        private readonly RootEntry _packageRoot;

        public EntryContainer Hierarchy { get { return _packageRoot; } }

        public PackageBuilder()
        {
            _currentSubhierarchy = _packageRoot = new RootEntry(DefaultDate);
        }

        private PackageBuilder(EntryContainer folder, RootEntry root)
        {
            _currentSubhierarchy = folder;
            _packageRoot = root;
        }

        public PackageBuilder AddFolder(string name)
        {
            return AddFolder(name, DefaultDate);
        }

        public PackageBuilder AddFolder(string name, DateTime lastWrite)
        {
            var item = new FolderEntry(name, _currentSubhierarchy, lastWrite);
            _currentSubhierarchy.Add(item);
            return new PackageBuilder(item, _packageRoot);
        }

        public PackageBuilder AddFile(string name, byte[] content)
        {
            return AddFile(name, content, DefaultDate);
        }

        public PackageBuilder AddFile(string name, string content)
        {
            byte[] contentData = Encoding.UTF8.GetBytes(content);
            return AddFile(name, contentData);
        }

        public PackageBuilder AddFile(string name, byte[] content, DateTime lastWrite)
        {
            _currentSubhierarchy.Add(new FileEntry(name, content, _currentSubhierarchy, lastWrite));
            return this;
        }

        public PackageBuilder AddFile(string name, string content, DateTime lastWrite)
        {
            byte[] contentData = Encoding.UTF8.GetBytes(content);
            return AddFile(name, contentData, lastWrite);
        }

        public void WritePackageInto(string packageDirectory)
        {
            var hierarchyExpander = new HierarchyToFolder(packageDirectory);
            Hierarchy.AcceptVisitor(hierarchyExpander);
        }

        public void GeneratePackageArchive(string destination)
        {
            using (var fileStream = File.Create(destination))
                GeneratePackageArchive(fileStream);
        }

        public void GeneratePackageArchive(Stream output)
        {
            using (var zip = new ZipOutputStream(output) {IsStreamOwner = false})
            {
                zip.SetLevel(9);
                var hierarchyToZip = new HierarchyToZip(zip);
                Hierarchy.AcceptVisitor(hierarchyToZip);
            }
        }
    }
}
