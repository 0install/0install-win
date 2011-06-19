/*
 * Copyright 2010-2011 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using Common;
using Common.Storage;
using Common.Utils;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.Injector;
using ZeroInstall.Model;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Create an alias for a <see cref="Run"/> command.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class AddAlias : IntegrationCommand
    {
        #region Constants
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public const string Name = "add-alias";
        #endregion

        #region Properties
        /// <inheritdoc/>
        protected override string Usage { get { return "ALIAS [INTERFACE [COMMAND]]"; } }

        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionAddAlias; } }

        /// <inheritdoc/>
        public override string ActionTitle { get { return Resources.ActionAppCommand; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public AddAlias(Policy policy) : base(policy)
        {
            // ToDo: Add options
        }
        #endregion

        //--------------------//

        #region Execute
        /// <inheritdoc/>
        public override int Execute()
        {
            #region Sanity checks
            if (!IsParsed) throw new InvalidOperationException(Resources.NotParsed);
            if (AdditionalArgs.Count == 0) throw new OptionException(Resources.MissingArguments, "");
            if (AdditionalArgs.Count > 3) throw new OptionException(Resources.TooManyArguments, "");
            #endregion

            if (Locations.IsPortable) throw new NotSupportedException(Resources.NotAvailableInPortableMode);

            // Get the alias data
            var alias = new AppAlias {Name = AdditionalArgs[0]};
            string interfaceID = ModelUtils.CanonicalID(StringUtils.UnescapeWhitespace(AdditionalArgs[1]));
            if (AdditionalArgs.Count >= 3) alias.Command = AdditionalArgs[2];

            if (SystemWide && WindowsUtils.IsWindows && !WindowsUtils.IsAdministrator)
                return RerunAsAdmin();

            Policy.Handler.ShowProgressUI(Cancel);
            CacheFeed(interfaceID);
            bool stale;
            var feed = Policy.FeedManager.GetFeed(interfaceID, Policy, out stale);

            if (Canceled) throw new UserCancelException();

            // Apply the new alias
            var integrationManager = new IntegrationManager(SystemWide);
            integrationManager.AddAccessPoints(new InterfaceFeed(interfaceID, feed), new AccessPoint[] {alias}, Policy.Handler);

            // Show a "integration complete" message (but not in batch mode, since it is too unimportant)
            if (!Policy.Handler.Batch) Policy.Handler.Output(Resources.DesktopIntegration, string.Format(Resources.DesktopIntegrationDone, feed.Name));
            return 0;
        }
        #endregion
    }
}
