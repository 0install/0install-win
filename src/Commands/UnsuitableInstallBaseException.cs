// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using NanoByte.Common.Storage;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Indicates that the current <see cref="Locations.InstallBase"/> is unsuitable for the desired operation.
    /// </summary>
    [Serializable]
    public sealed class UnsuitableInstallBaseException : NotSupportedException
    {
        /// <summary>
        /// <c>true</c> if a machine-wide install location is required; <c>false</c> if a user-specific location will also do.
        /// </summary>
        public bool NeedsMachineWide { get; }

        /// <inheritdoc/>
        public UnsuitableInstallBaseException() {}

        /// <inheritdoc/>
        public UnsuitableInstallBaseException(string message)
            : base(message)
        {}

        /// <summary>
        /// Creates a new unsuitable install base exception.
        /// </summary>
        /// <param name="message">A message describing why the current location in unsuitable.</param>
        /// <param name="needsMachineWide"><c>true</c> if a machine-wide location is required; <c>false</c> if a user-specific location will also do.</param>
        public UnsuitableInstallBaseException(string message, bool needsMachineWide)
            : base(message)
        {
            NeedsMachineWide = needsMachineWide;
        }

        /// <inheritdoc/>
        public UnsuitableInstallBaseException(string message, Exception innerException)
            : base(message, innerException)
        {}

        #region Serialization
        /// <summary>
        /// Deserializes an exception.
        /// </summary>
        private UnsuitableInstallBaseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            #region Sanity checks
            if (info == null) throw new ArgumentNullException(nameof(info));
            #endregion

            NeedsMachineWide = info.GetBoolean("NeedsMachineWide");
        }

        /// <inheritdoc/>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            #region Sanity checks
            if (info == null) throw new ArgumentNullException(nameof(info));
            #endregion

            info.AddValue("NeedsMachineWide", NeedsMachineWide);

            base.GetObjectData(info, context);
        }
        #endregion
    }
}
