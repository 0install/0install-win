using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Storage
{
    /// <summary>
    /// Disposable class that allows operating on a newly created folder at a
    /// specified path, and if necessary temporarily moving the original folder
    /// to a new position.
    /// </summary>
    public class TemporaryDirectoryReplacement : IDisposable
    {
        private readonly TemporaryDirectory _tempDir;
        private readonly TemporaryDirectoryMove _backupMove;

        public string Path
        {
            get { return _tempDir.Path; }
        }

        public string BackupPath
        {
            get { return _backupMove.BackupPath; }
        }

        /// <summary>
        /// Applies <see cref="TemporaryDirectoryMove"/> and <see cref="TemporaryDirectory"/>.
        /// </summary>
        /// <param name="path"></param>
        public TemporaryDirectoryReplacement(string path)
        {
            _backupMove = new TemporaryDirectoryMove(path);
            _tempDir = new TemporaryDirectory(path);
        }

        /// <summary>
        /// Applies the Dispose methods of <see cref="TemporaryDirectoryMove"/> and <see cref="TemporaryDirectory"/>
        /// </summary>
        public void Dispose()
        {
            _tempDir.Dispose();
            _backupMove.Dispose();
        }
    }
}
