// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using NanoByte.Common;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Service.Properties;

namespace ZeroInstall.Store.Service
{
    /// <summary>
    /// Models a cache directory that stores <see cref="Implementation"/>s using ACLs and impersonation to ensure security in IPC scenarios.
    /// </summary>
    /// <remarks>The represented store data is mutable but the class itself is immutable.</remarks>
    public class SecureStore : DiskImplementationStore, IEquatable<SecureStore>
    {
        #region Variables
        /// <summary>The identity the service was launched with.</summary>
        private readonly WindowsIdentity _serviceIdentity;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new store using a specific path to a cache directory.
        /// </summary>
        /// <param name="path">A fully qualified directory path. The directory will be created if it doesn't exist yet.</param>
        /// <param name="serviceIdentity">The identity the service was launched with.</param>
        /// <exception cref="IOException">The directory <paramref name="path"/> could not be created or the underlying filesystem can not store file-changed times accurate to the second.</exception>
        /// <exception cref="UnauthorizedAccessException">Creating the directory <paramref name="path"/> is not permitted.</exception>
        public SecureStore(string path, WindowsIdentity serviceIdentity)
            : base(path)
        {
            _serviceIdentity = serviceIdentity ?? throw new ArgumentNullException(nameof(serviceIdentity));

            Log.Info("Using implementation directory: " + path);
        }
        #endregion

        //--------------------//

        #region Temp dir
        /// <inheritdoc/>
        protected override string GetTempDir()
        {
            var callingIdentity = WindowsIdentity.GetCurrent();
            Debug.Assert(callingIdentity != null);
            Debug.Assert(callingIdentity.User != null);

            using (_serviceIdentity.Impersonate()) // Use system rights instead of calling user
            {
                try
                {
                    string path = base.GetTempDir();

                    // Give the calling user write access
                    var acl = Directory.GetAccessControl(path);
                    acl.CanonicalizeAcl();
                    acl.AddAccessRule(new FileSystemAccessRule(callingIdentity.User, FileSystemRights.Modify, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                    Directory.SetAccessControl(path, acl);
                    return path;
                }
                #region Error handling
                catch (Exception ex)
                {
                    Log.Error(string.Format(Resources.FailedToCreateTempDir, callingIdentity.Name, DirectoryPath) + Environment.NewLine + ex.Message);
                    throw;
                }
                #endregion
            }
        }

        /// <inheritdoc/>
        protected override void DeleteTempDir(string path)
        {
            var callingIdentity = WindowsIdentity.GetCurrent();
            Debug.Assert(callingIdentity != null);

            using (_serviceIdentity.Impersonate()) // Use system rights instead of calling user
            {
                try
                {
                    var directory = new DirectoryInfo(path);
                    if (directory.Exists)
                    {
                        try
                        {
                            directory.ResetAcl();
                        }
                        catch (IndexOutOfRangeException)
                        {
                            // Workaround for .NET 2.0 bug
                        }
                        directory.Delete(true);
                    }
                }
                #region Error handling
                catch (Exception ex)
                {
                    Log.Error(string.Format(Resources.FailedToRemoveTempDir, callingIdentity.Name, DirectoryPath) + Environment.NewLine + ex.Message);
                    throw;
                }
                #endregion
            }
        }
        #endregion

        #region Verify directory
        /// <inheritdoc/>
        protected override string VerifyAndAdd(string tempID, ManifestDigest expectedDigest, ITaskHandler handler)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            #endregion

            var callingIdentity = WindowsIdentity.GetCurrent();
            Debug.Assert(callingIdentity != null);

            using (_serviceIdentity.Impersonate()) // Use system rights instead of calling user
            {
                try
                {
                    var tempDirectory = new DirectoryInfo(Path.Combine(DirectoryPath, tempID));
                    try
                    {
                        handler.RunTask(new SimpleTask(Resources.SettingFilePermissions, tempDirectory.ResetAcl) {Tag = expectedDigest});
                    }
                    catch (IndexOutOfRangeException)
                    {
                        // Workaround for .NET 2.0 bug
                    }

                    string result = base.VerifyAndAdd(tempID, expectedDigest, handler);
                    Log.Info(string.Format(Resources.SuccessfullyAddedImplementation, callingIdentity.Name, expectedDigest.AvailableDigests.FirstOrDefault(), DirectoryPath));
                    return result;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Log.Warn(string.Format(Resources.FailedToAddImplementation, callingIdentity.Name, expectedDigest.AvailableDigests.FirstOrDefault(), DirectoryPath) + Environment.NewLine + ex.Message);
                    throw;
                }
            }
        }
        #endregion

        #region Remove
        /// <inheritdoc/>
        public override bool Remove(ManifestDigest manifestDigest, ITaskHandler handler)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            #endregion

            if (!Contains(manifestDigest)) return false;
            if (!WindowsUtils.IsAdministrator) throw new NotAdminException(Resources.MustBeAdminToRemove);

            var callingIdentity = WindowsIdentity.GetCurrent();
            Debug.Assert(callingIdentity != null);

            using (_serviceIdentity.Impersonate()) // Use system rights instead of calling user
            {
                bool removed;
                try
                {
                    removed = base.Remove(manifestDigest, handler);
                }
                #region Error handling
                catch (Exception)
                {
                    Log.Warn(string.Format(Resources.FailedToRemoveImplementation, callingIdentity.Name, manifestDigest, DirectoryPath));
                    throw;
                }
                #endregion

                if (removed) Log.Info(string.Format(Resources.SuccessfullyRemovedImplementation, callingIdentity.Name, manifestDigest, DirectoryPath));
                return removed;
            }
        }
        #endregion

        #region Nop
        /// <summary>
        /// Does nothing. Should be handled by an administrator directly instead of using the service.
        /// </summary>
        public override void Verify(ManifestDigest manifestDigest, ITaskHandler handler) {}

        /// <summary>
        /// Does nothing. Should be handled by an administrator directly instead of using the service.
        /// </summary>
        public override long Optimise(ITaskHandler handler) => 0;
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the Store in the form "SecureStore: DirectoryPath". Not safe for parsing!
        /// </summary>
        public override string ToString() => "SecureStore: " + DirectoryPath;
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(SecureStore other) => base.Equals(other);

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj == this) return true;
            return obj.GetType() == typeof(SecureStore) && Equals((SecureStore)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode() => base.GetHashCode();
        #endregion
    }
}
