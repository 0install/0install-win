using System;
using System.IO;
using Common.Helpers;
using Common.Storage;

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
            if (String.IsNullOrEmpty(path)) throw new ArgumentNullException("path", @"null or empty string passed to TemporaryMove");
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
                if(Directory.Exists(OriginalPath))
                    Directory.Delete(OriginalPath, true);
                Directory.Move(BackupPath, OriginalPath);
            }
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