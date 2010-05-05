using System;
using System.IO;
using Common.Helpers;

namespace ZeroInstall.Store.Utilities
{
    /// <summary>
    /// Helper class to move an existing directory to a temporary directory only within a
    /// using statement block.
    /// </summary>
    public class TemporaryMove : IDisposable
    {
        private readonly string _originalPath, _movedPath;

        public string OriginalPath
        {
            get { return _originalPath; }
        }

        public string BackupPath
        {
            get { return _movedPath; }
        }

        /// <summary>
        /// Renames an existing directory by moving it to a path found by
        /// <see cref="FileHelper.GetUniqueFileName"/>
        /// If the path doesn't point to anything, it does nothing.
        /// </summary>
        /// <param name="path">file system path to move</param>
        public TemporaryMove(string path)
        {
            if (Directory.Exists(path))
            {
                string inexistantPath = FileHelper.GetUniqueFileName(Path.Combine(path, ".."));
                Directory.Move(path, inexistantPath);
                _originalPath = path;
                _movedPath = inexistantPath;
            }
        }

        /// <summary>
        /// Deletes the directory currently existing at the original path and
        /// moves the previously renamed directory to it's original path.
        /// </summary>
        public void Dispose()
        {
            if (!String.IsNullOrEmpty(_originalPath))
            {
                Directory.Delete(_originalPath, true);
                Directory.Move(_movedPath, _originalPath);
            }
        }
    }

    /// <summary>
    /// Disposable class to create a temporary folder and delete it again when disposed.
    /// </summary>
    public class TemporaryDirectory : IDisposable
    {
        private readonly string _path;

        public string Path
        {
            get { return _path; }
        }

        /// <summary>
        /// Creates a temporary directory in the system's temp directory.
        /// </summary>
        public TemporaryDirectory()
        {
            _path = FileHelper.GetTempDirectory();
        }

        /// <summary>
        /// Creates a temporary directory at a given path.
        /// </summary>
        /// <param name="path">file system path where the new folder should be created</param>
        public TemporaryDirectory(string path)
        {
            if (String.IsNullOrEmpty(path)) throw new ArgumentNullException("path", "Invalid path given");
            if (Directory.Exists(path)) throw new Exception("Folder already exists");

            Directory.CreateDirectory(path);
            _path = path;
        }

        /// <summary>
        /// Deletes the temporary folder recursively.
        /// </summary>
        public void Dispose()
        {
            Directory.Delete(_path, true);
        }
    }

    /// <summary>
    /// Disposable class that allows operating on a newly created folder at a
    /// specified path, and if necessary temporarily moving the original folder
    /// to a new position.
    /// </summary>
    public class TemporaryReplacement : IDisposable
    {
        private readonly TemporaryDirectory _tempDir;
        private readonly TemporaryMove _backupMove;

        public string Path
        {
            get { return _tempDir.Path; }
        }

        public string BackupPath
        {
            get { return _backupMove.BackupPath; }
        }

        /// <summary>
        /// Applies <see cref="TemporaryMove"/> and <see cref="TemporaryDirectory"/>.
        /// </summary>
        /// <param name="path"></param>
        public TemporaryReplacement(string path)
        {
            _backupMove = new TemporaryMove(path);
            _tempDir = new TemporaryDirectory(path);
        }

        /// <summary>
        /// Applies the Dispose methods of <see cref="TemporaryMove"/> and <see cref="TemporaryDirectory"/>
        /// </summary>
        public void Dispose()
        {
            _tempDir.Dispose();
            _backupMove.Dispose();
        }
    }
}