// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.IO;
using System.Net;
using NDesk.Options;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.Services.Executors;
using ZeroInstall.Services.Solvers;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// An exit code is returned to the original caller after the application terminates, to indicate success or the reason for failure.
    /// </summary>
    public enum ExitCode
    {
        /// <summary>The operation completed without any problems.</summary>
        OK = 0,

        /// <summary>The operation resulted in no changes. This may be due to a problem with the input or simply indicate that the system is already in the desired state.</summary>
        NoChanges = 1,

        /// <summary>There was a network problem. This may be intermittent and resolve itself e.g. when a Wi-Fi connection is restored.</summary>
        /// <seealso cref="WebException"/>
        WebError = 10,

        /// <summary>You have insufficient access rights. This can potentially be fixed by running the command as an Administrator/root. It may also indicate misconfigured file permissions.</summary>
        /// <seealso cref="UnauthorizedAccessException"/>
        AccessDenied = 11,

        /// <summary>There was an IO problem. This encompasses issues such as missing files or insufficient disk space.</summary>
        /// <seealso cref="IOException"/>
        IOError = 12,

        /// <summary>A desktop integration operation could not be completed due to conflicting <see cref="AccessPoint"/>s.</summary>
        /// <seealso cref="ConflictException"/>
        Conflict = 15,

        /// <summary>The <see cref="ISolver"/> was unable to provide <see cref="Selections"/> that fulfill the <see cref="Requirements"/>. This can be caused by a problem with the feed, an impossible request (e.g., non-existing version) or your local configuration.</summary>
        /// <seealso cref="SolverException"/>
        SolverError = 20,

        /// <summary>The <see cref="IExecutor"/> was unable to launch the desired application. This usually indicates a problem with the feed.</summary>
        /// <seealso cref="ExecutorException"/>
        ExecutorError = 21,

        /// <summary>A data file could not be parsed. This encompasses issues such as damaged configuration files or malformed XML documents (e.g. feeds).</summary>
        /// <seealso cref="InvalidDataException"/>
        InvalidData = 25,

        /// <summary>The <see cref="ManifestDigest"/> of an implementation does not match the expected value. This could be caused by a damaged download or an incorrect feed.</summary>
        /// <seealso cref="DigestMismatchException"/>
        DigestMismatch = 26,

        /// <summary>There was a problem with the digital signature of a feed. The signature may be missing, damaged or not trusted for the source the feed came from.</summary>
        /// <seealso cref="SignatureException"/>
        InvalidSignature = 27,

        /// <summary>The operation could not be completed because a feature that is not (yet) supported was requested. Upgrading to a newer version may resolve this issue.</summary>
        /// <seealso cref="NotSupportedException"/>
        NotSupported = 50,

        /// <summary>The command-line arguments passed to the application were invalid.</summary>
        /// <seealso cref="OptionException"/>
        /// <seealso cref="FormatException"/>
        InvalidArguments = 99,

        /// <summary>The user canceled the task.</summary>
        /// <seealso cref="OperationCanceledException"/>
        UserCanceled = 100
    }
}
