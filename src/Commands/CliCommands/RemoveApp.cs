// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Store;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Remove an application from the <see cref="AppList"/> and undoes any desktop environment integration.
    /// </summary>
    public sealed class RemoveApp : AppCommand
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "remove";

        /// <summary>The alternative name of this command as used in command-line arguments in lower-case.</summary>
        public const string AltName = "remove-app";

        /// <summary>Another alternative name of this command as used in command-line arguments in lower-case.</summary>
        public const string AltName2 = "destory";

        /// <inheritdoc/>
        public override string Description => Resources.DescriptionRemoveApp;

        /// <inheritdoc/>
        public override string Usage => "[OPTIONS] (PET-NAME|INTERFACE)";
        #endregion

        /// <inheritdoc/>
        public RemoveApp([NotNull] ICommandHandler handler)
            : base(handler)
        {}

        /// <inheritdoc/>
        protected override ExitCode ExecuteHelper(ICategoryIntegrationManager integrationManager, FeedUri interfaceUri)
        {
            #region Sanity checks
            if (interfaceUri == null) throw new ArgumentNullException(nameof(interfaceUri));
            if (integrationManager == null) throw new ArgumentNullException(nameof(integrationManager));
            #endregion

            try
            {
                integrationManager.RemoveApp(integrationManager.AppList[interfaceUri]);
            }
            #region Sanity checks
            catch (KeyNotFoundException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(ex.Message, ex);
            }
            #endregion

            return ExitCode.OK;
        }
    }
}
