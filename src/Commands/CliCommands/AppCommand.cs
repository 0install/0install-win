// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using JetBrains.Annotations;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Store;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Common base class for commands that manage an <see cref="AppList"/>.
    /// </summary>
    public abstract class AppCommand : IntegrationCommand
    {
        #region Metadata
        /// <inheritdoc/>
        protected override int AdditionalArgsMin => 1;

        /// <inheritdoc/>
        protected override int AdditionalArgsMax => 1;
        #endregion

        /// <inheritdoc/>
        protected AppCommand([NotNull] ICommandHandler handler)
            : base(handler)
        {}

        /// <inheritdoc/>
        public override ExitCode Execute()
        {
            try
            {
                var interfaceUri = GetCanonicalUri(AdditionalArgs[0]);
                using (var integrationManager = new CategoryIntegrationManager(Handler, MachineWide))
                    return ExecuteHelper(integrationManager, interfaceUri);
            }
            finally
            {
                SelfUpdateCheck();
            }
        }

        /// <summary>
        /// Template method that performs the actual operation.
        /// </summary>
        /// <param name="integrationManager">Manages desktop integration operations.</param>
        /// <param name="interfaceUri">The interface for the application to perform the operation on.</param>
        /// <returns>The exit status code to end the process with.</returns>
        protected abstract ExitCode ExecuteHelper([NotNull] ICategoryIntegrationManager integrationManager, [NotNull] FeedUri interfaceUri);
    }
}
