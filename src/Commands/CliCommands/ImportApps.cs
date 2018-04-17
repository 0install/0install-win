// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using JetBrains.Annotations;
using NanoByte.Common.Storage;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Imports a set of applications and desktop integrations from an existing <see cref="AppList"/> file.
    /// </summary>
    public class ImportApps : IntegrationCommand
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "import-apps";

        /// <inheritdoc/>
        public override string Description => Resources.DescriptionImportApps;

        /// <inheritdoc/>
        public override string Usage => "APP-LIST-FILE [OPTIONS]";

        /// <inheritdoc/>
        protected override int AdditionalArgsMin => 1;

        /// <inheritdoc/>
        protected override int AdditionalArgsMax => 1;
        #endregion

        #region State
        /// <inheritdoc/>
        public ImportApps([NotNull] ICommandHandler handler)
            : base(handler)
        {
            Options.Add("no-download", () => Resources.OptionNoDownload, _ => NoDownload = true);
        }
        #endregion

        /// <inheritdoc/>
        public override ExitCode Execute()
        {
            var importList = XmlStorage.LoadXml<AppList>(AdditionalArgs[0]);

            using (var integrationManager = new IntegrationManager(Handler, MachineWide))
            {
                foreach (var importEntry in importList.Entries)
                {
                    var interfaceUri = importEntry.InterfaceUri;
                    var appEntry = GetAppEntry(integrationManager, ref interfaceUri);

                    if (importEntry.AccessPoints != null)
                    {
                        var feed = FeedManager[interfaceUri];
                        integrationManager.AddAccessPoints(appEntry, feed, importEntry.AccessPoints.Entries);
                    }
                }
            }

            return ExitCode.OK;
        }
    }
}
