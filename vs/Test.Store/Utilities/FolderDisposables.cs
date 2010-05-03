using System;

namespace ZeroInstall.Store.Utilities
{
    /// <summary>
    /// Disposable class that allows operating on a newly created folder at a
    /// specified path, and if necessary temporarily moving the original folder
    /// to a new position.
    /// </summary>
    public class DirectoryReplacement : IDisposable
    {
        private readonly string _path, _backup;

        public string Path
        {
            get { return _path; }
        }

        public string Backup
        {
            get { return _backup; }
        }

        public DirectoryReplacement(string path)
        {
            if (System.IO.Directory.Exists(path))
            {
                string inexistantPath = DirectoryHelper.FindInexistantPath(path);
                System.IO.Directory.Move(path, inexistantPath);
                _backup = inexistantPath;
            }
            System.IO.Directory.CreateDirectory(path);
            _path = path;
        }

        public void Dispose()
        {
            System.IO.Directory.Delete(_path, recursive: true);
            if (!String.IsNullOrEmpty(_backup))
            {
                System.IO.Directory.Move(_backup, _path);
            }
        }
    }

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

        public TemporaryMove(string path)
        {
            if (System.IO.Directory.Exists(path))
            {
                string inexistantPath = DirectoryHelper.FindInexistantPath(path);
                System.IO.Directory.Move(path, inexistantPath);
                _originalPath = path;
                _movedPath = inexistantPath;
            }
        }

        public void Dispose()
        {
            if (!String.IsNullOrEmpty(_originalPath))
            {
                System.IO.Directory.Delete(_originalPath, recursive: true);
                System.IO.Directory.Move(_movedPath, _originalPath);
            }
        }
    }
}