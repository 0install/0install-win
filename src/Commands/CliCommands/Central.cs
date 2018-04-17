// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Native;
using ZeroInstall.Commands.Properties;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Opens the central graphical user interface for launching and managing applications.
    /// </summary>
    public sealed class Central : CliCommand
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "central";

        /// <inheritdoc/>
        public override string Description => Resources.DescriptionCentral;

        /// <inheritdoc/>
        public override string Usage => "[OPTIONS]";

        /// <inheritdoc/>
        protected override int AdditionalArgsMax => 0;
        #endregion

        #region State
        private bool _machineWide;

        /// <inheritdoc/>
        public Central([NotNull] ICommandHandler handler)
            : base(handler)
        {
            Options.Add("m|machine", () => Resources.OptionMachine, _ => _machineWide = true);
        }
        #endregion

        /// <inheritdoc/>
        public override ExitCode Execute()
        {
            if (_machineWide && !WindowsUtils.IsAdministrator)
                throw new NotAdminException(Resources.MustBeAdminForMachineWide);

            return (ExitCode)ProcessUtils.Assembly("ZeroInstall").Run();
        }
    }
}
