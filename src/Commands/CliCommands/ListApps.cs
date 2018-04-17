// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NanoByte.Common;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// List all current <see cref="AppEntry"/>s in the <see cref="AppList"/>.
    /// </summary>
    public sealed class ListApps : IntegrationCommand
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "list-apps";

        /// <inheritdoc/>
        public override string Description => Resources.DescriptionListApps;

        /// <inheritdoc/>
        public override string Usage => "[PATTERN]";

        /// <inheritdoc/>
        protected override int AdditionalArgsMax => 1;
        #endregion

        /// <inheritdoc/>
        public ListApps([NotNull] ICommandHandler handler)
            : base(handler)
        {}

        /// <inheritdoc/>
        public override ExitCode Execute()
        {
            IEnumerable<AppEntry> apps = AppList.LoadSafe(MachineWide).Entries;
            if (AdditionalArgs.Count > 0) apps = apps.Where(x => x.Name.ContainsIgnoreCase(AdditionalArgs[0]));

            Handler.Output(Resources.MyApps, apps);
            return ExitCode.OK;
        }
    }
}
