using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Common.Collections;
using Common.Helpers;
using Common.Properties;
using ICSharpCode.SharpZipLib.Zip;

namespace Common.Storage
{
    /// <summary>
    /// Provides a virtual file system for combining data from multiple directories and archives (includes modding support)
    /// </summary>
    public static class ContentManager
    {
        #region Constants
        /// <summary>
        /// The file extensions of content archives
        /// </summary>
        public const string FileExt = ".pk5";
        #endregion

        #region Variables
        private static DirectoryInfo _baseDir, _modDir;
        private static List<ZipFile> _baseArchives, _modArchives;
        private static readonly Dictionary<string, ContentArchiveEntry>
            BaseArchiveData = new Dictionary<string, ContentArchiveEntry>(StringComparer.OrdinalIgnoreCase ),
            ModArchiveData = new Dictionary<string, ContentArchiveEntry>(StringComparer.OrdinalIgnoreCase );
        #endregion

        #region Properties
        /// <summary>
        /// The base directory where all the content files are stored
        /// </summary>
        /// <exception cref="DirectoryNotFoundException">Is thrown when the specified directory could not be found</exception>
        public static DirectoryInfo BaseDir
        {
            get { return _baseDir; }
            set
            {
                if (value != null && !value.Exists)
                    throw new DirectoryNotFoundException(Resources.NotFoundGameDataDir + "\n" + value.FullName);
                _baseDir = value;
            }
        }

        /// <summary>
        /// A directory overriding the base directory for creating mods; may be <see langword="null"/>
        /// </summary>
        /// <exception cref="DirectoryNotFoundException">Is thrown when the specified directory could not be found</exception>
        public static DirectoryInfo ModDir
        {
            get { return _modDir; }
            set
            {
                if (value != null && !value.Exists)
                    throw new DirectoryNotFoundException(Resources.NotFoundModDataDir + "\n" + value.FullName);
                _modDir = value;
            }
        }
        #endregion

        //--------------------//

        #region Load archives
        /// <summary>
        /// Loads content archives from the base directory into the <see cref="ContentManager"/>.
        /// </summary>
        public static void LoadArchives()
        {
            if (_baseArchives != null)
                throw new InvalidOperationException(Resources.ContentArchivesAlreadyLoaded);

            #region Base files
            FileInfo[] baseFiles = BaseDir.GetFiles("*" + FileExt);
            _baseArchives = new List<ZipFile>(baseFiles.Length); // Use exact size for list capacity
            foreach (FileInfo file in baseFiles)
            {
                Log.Write("Load base data archive: " + file.Name);
                var zipFile = new ZipFile(file.FullName);
                foreach (ZipEntry zipEntry in zipFile)
                {
                    if (zipEntry.IsFile)
                    {
                        // Unify directory directory separator character
                        string filename = StringHelper.UnifySlashes(zipEntry.Name);

                        // Overwrite existing entries
                        if (BaseArchiveData.ContainsKey(filename)) BaseArchiveData.Remove(filename);
                        BaseArchiveData.Add(filename, new ContentArchiveEntry(zipFile, zipEntry));
                    }
                }
                _baseArchives.Add(zipFile);
            }
            #endregion

            #region Mod files
            if (ModDir != null)
            {
                FileInfo[] modFiles = ModDir.GetFiles("*" + FileExt);
                _modArchives = new List<ZipFile>(modFiles.Length); // Use exact size for list capacity
                foreach (FileInfo file in modFiles)
                {
                    Log.Write("Load mod data archive: " + file.Name);
                    var zipFile = new ZipFile(file.FullName);
                    foreach (ZipEntry zipEntry in zipFile)
                    {
                        if (zipEntry.IsFile)
                        {
                            // Unify directory directory separator character
                            string filename = StringHelper.UnifySlashes(zipEntry.Name);

                            // Overwrite existing entries
                            if (ModArchiveData.ContainsKey(filename)) ModArchiveData.Remove(filename);
                            ModArchiveData.Add(filename, new ContentArchiveEntry(zipFile, zipEntry));
                        }
                    }
                    _modArchives.Add(zipFile);
                }
            }
            #endregion
        }
        #endregion

        #region Close archives
        /// <summary>
        /// Closes the content archives loaded into the <see cref="ContentManager"/>.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Errors on shutdown because of an inconsistent state are useless and annoying")]
        public static void CloseArchives()
        {
            #region Base files
            if (_baseArchives != null)
            {
                foreach (ZipFile zipFile in _baseArchives)
                {
                    Log.Write("Close base data archive: " + zipFile.Name);
                    try { zipFile.Close(); }
// ReSharper disable EmptyGeneralCatchClause
                    catch {}
// ReSharper restore EmptyGeneralCatchClause
                }
                _baseArchives = null;
                BaseArchiveData.Clear();
            }
            #endregion

            #region Mod files
            if (_modArchives != null)
            {
                foreach (ZipFile zipFile in _modArchives)
                {
                    Log.Write("Close mod data archive: " + zipFile.Name);
                    try { zipFile.Close(); }
// ReSharper disable EmptyGeneralCatchClause
                    catch {}
// ReSharper restore EmptyGeneralCatchClause
                }
                _modArchives = null;
                ModArchiveData.Clear();
            }
            #endregion
        }
        #endregion

        //--------------------//

        #region Create directory path
        /// <summary>
        /// Creates a path for a content directory (using the current mod directory if available)
        /// </summary>
        /// <param name="type">The type of file you want (e.g. Textures, Sounds, ...)</param>
        /// <returns>The absolute path to the requested directory</returns>
        /// <exception cref="DirectoryNotFoundException">Is thrown when the specified directory could not be found</exception>
        public static string CreateDirPath(string type)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(type)) throw new ArgumentNullException("type");
            #endregion

            // Unify directory directory separator character
            type = StringHelper.UnifySlashes(type);

            // Use mod directory if available
            string pathBase;
            if (ModDir != null) pathBase = ModDir.FullName;
            else if (BaseDir != null) pathBase = _baseDir.FullName;
            else throw new DirectoryNotFoundException(Resources.NotFoundGameDataDir + "\n-");

            // Check the path before returning it
            var directory = new DirectoryInfo(Path.Combine(pathBase, type));
            if (!directory.Exists) directory.Create();
            return directory.FullName;
        }
        #endregion

        #region Create file path
        /// <summary>
        /// Creates a path for a content file (using the current mod directory if available)
        /// </summary>
        /// <param name="type">The type of file you want (e.g. Textures, Sounds, ...)</param>
        /// <param name="id">The file name of the content</param>
        /// <returns>The absolute path to the requested content file</returns>
        public static string CreateFilePath(string type, string id)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(type)) throw new ArgumentNullException("type");
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException("id");
            #endregion

            // Unify directory directory separator character
            type = StringHelper.UnifySlashes(type);
            id = StringHelper.UnifySlashes(id);

            return Path.Combine(CreateDirPath(type), id);
        }
        #endregion

        #region File exists
        /// <summary>
        /// Checks whether a certain content file exists
        /// </summary>
        /// <param name="type">The type of file you want (e.g. Textures, Sounds, ...)</param>
        /// <param name="id">The file name of the content</param>
        /// <param name="searchArchives">Whether to search for the file in archives as well</param>
        /// <returns><see langword="true"/> if the requested content file exists</returns>
        public static bool FileExists(string type, string id, bool searchArchives)
        {
            // Unify directory directory separator character
            type = StringHelper.UnifySlashes(type);
            id = StringHelper.UnifySlashes(id);

            string fullID = Path.Combine(type, id);

            if (ModDir != null && File.Exists(Path.Combine(ModDir.FullName, fullID)))
                return true;
            if (BaseDir != null && File.Exists(Path.Combine(BaseDir.FullName, fullID)))
                return true;
            return searchArchives && BaseArchiveData.ContainsKey(fullID);
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Recursively finds all files in <paramref name="directory"/> ending with <paramref name="extension"/> and adds them to the <paramref name="files"/> collection
        /// </summary>
        /// <param name="files">The collection to add the found files to</param>
        /// <param name="extension">The file extension to look for</param>
        /// <param name="directory">The directory to look in</param>
        /// <param name="prefix">A prefix to add before the file name in the list (used to indicate current sub-directory)</param>
        private static void RecursiveDirHelper(ICollection<string> files, string extension, DirectoryInfo directory, string prefix)
        {
            // Add the files in this directory to the list
            foreach (FileInfo file in directory.GetFiles("*" + extension))
                files.Add(prefix + file.Name);

            // Recursively call this method for all sub-directories
            foreach (DirectoryInfo subDir in directory.GetDirectories())
                RecursiveDirHelper(files, extension, subDir, prefix + subDir.Name + Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Finds all files in <paramref name="archive"/> ending with <paramref name="extension"/> and adds them to the <paramref name="files"/> collection
        /// </summary>
        /// <param name="files">The collection to add the found files to</param>
        /// <param name="extension">The file extension to look for</param>
        /// <param name="type">The type-subdirectory to look in</param>
        /// <param name="archive">The archive to look in</param>
        private static void ArchiveHelper(ICollection<string> files, string extension, string type, IEnumerable<KeyValuePair<string, ContentArchiveEntry>> archive)
        {
            foreach (KeyValuePair<string, ContentArchiveEntry> pair in archive)
            {
                if (pair.Key.StartsWith(type, StringComparison.OrdinalIgnoreCase) && pair.Key.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                    files.Add(pair.Key.Substring(type.Length + 1)); // Cut away the type part of the path
            }
        }
        #endregion

        #region Get file list
        /// <summary>
        /// Gets a list of all files of a certain type
        /// </summary>
        /// <param name="type">The type of files you want (e.g. Textures, Sounds, ...)</param>
        /// <param name="extension">The file extension to so search for</param>
        /// <param name="markModFiles">Mark files overwritten by a mod with a * and files added by a mod with a +</param>
        /// <param name="searchArchives">Whether to search for the file in archives as well</param>
        /// <returns>An collection of strings with file IDs</returns>
        public static IEnumerable<string> GetFileList(string type, string extension, bool markModFiles, bool searchArchives)
        {
            // Unify directory directory separator character
            type = StringHelper.UnifySlashes(type);

            var files = new OrderedSet<string>();

            #region Base files
            if (Directory.Exists(Path.Combine(BaseDir.FullName, type)))
            {
                RecursiveDirHelper(files, extension,
                    new DirectoryInfo(Path.Combine(BaseDir.FullName, type)), "");
            }

            if (searchArchives)
                ArchiveHelper(files, extension, type, BaseArchiveData);
            #endregion

            if (ModDir != null)
            {
                #region Mod files
                var modFiles = new OrderedSet<string>();

                if (Directory.Exists(Path.Combine(ModDir.FullName, type)))
                {
                    RecursiveDirHelper(modFiles, extension,
                        new DirectoryInfo(Path.Combine(ModDir.FullName, type)), "");
                }

                if (searchArchives)
                    ArchiveHelper(modFiles, extension, type, ModArchiveData);
                #endregion

                #region Merge
                foreach (string file in modFiles)
                {
                    if (markModFiles)
                    {
                        // Replace existing entries
                        if (files.Remove(file)) files.Add(file + " *"); // File changed by mod
                        else files.Add(file + " +"); // File added by mod
                    }
                    else if (!file.Contains(file)) files.Add(file);
                }
                #endregion
            }

            return files;
        }
        #endregion

        #region Get file path
        /// <summary>
        /// Gets the file path for a content file (does not search in archives)
        /// </summary>
        /// <param name="type">The type of file you want (e.g. Textures, Sounds, ...)</param>
        /// <param name="id">The file name of the content</param>
        /// <returns>The absolute path to the requested content file</returns>
        /// <exception cref="FileNotFoundException">Is thrown when the specified file could not be found</exception>
        public static string GetFilePath(string type, string id)
        {
            // Unify directory directory separator character
            type = StringHelper.UnifySlashes(type);
            id = StringHelper.UnifySlashes(id);

            string path;

            if (ModDir != null)
            {
                path = Path.Combine(ModDir.FullName, Path.Combine(type, id));
                if (File.Exists(path)) return path;
            }

            if (BaseDir != null)
            {
                path = Path.Combine(BaseDir.FullName, Path.Combine(type, id));
                if (File.Exists(path)) return path;
            }

            throw new FileNotFoundException(Resources.NotFoundGameContentFile + "\n" + Path.Combine(type, id), Path.Combine(type, id));
        }
        #endregion

        #region Get file stream
        /// <summary>
        /// Gets a reading stream for a content file (searches in archives)
        /// </summary>
        /// <param name="type">The type of file you want (e.g. Textures, Sounds, ...)</param>
        /// <param name="id">The file name of the content</param>
        /// <returns>The absolute path to the requested content file</returns>
        /// <exception cref="FileNotFoundException">Is thrown when the specified file could not be found</exception>
        public static Stream GetFileStream(string type, string id)
        {
            // Unify directory directory separator character
            type = StringHelper.UnifySlashes(type);
            id = StringHelper.UnifySlashes(id);

            // First try to load a real file
            if (FileExists(type, id, false))
                return File.OpenRead(GetFilePath(type, id));

            // Then look in the archives
            string fullID = Path.Combine(type, id);

            #region Mod
            if (ModDir != null)
            {
                // Real file
                string path =  Path.Combine(ModDir.FullName, fullID);
                if (File.Exists(path)) return File.OpenRead(path);

                // Archive entry
                if (ModArchiveData.ContainsKey(fullID))
                {
                    // Copy from ZIP file to MemoryStream to provide seeking capability
                    Stream memoryStream = new MemoryStream();
                    using (var inputStream = ModArchiveData[fullID].ZipFile.GetInputStream(ModArchiveData[fullID].ZipEntry))
                        StreamHelper.Copy(inputStream, memoryStream);
                    return memoryStream;
                }
            }
            #endregion

            #region Base
            if (BaseDir != null)
            {
                // Real file
                string path =  Path.Combine(BaseDir.FullName, fullID);
                if (File.Exists(path)) return File.OpenRead(path);

                // Archive entry
                if (BaseArchiveData.ContainsKey(fullID))
                {
                    // Copy from ZIP file to MemoryStream to provide seeking capability
                    Stream memoryStream = new MemoryStream();
                    using (var inputStream = BaseArchiveData[fullID].ZipFile.GetInputStream(BaseArchiveData[fullID].ZipEntry))
                        StreamHelper.Copy(inputStream, memoryStream);
                    return memoryStream;
                }
            }
            #endregion

            throw new FileNotFoundException(Resources.NotFoundGameContentFile + "\n" + Path.Combine(type, id), Path.Combine(type, id));
        }
        #endregion
    }
}