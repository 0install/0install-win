/*
 * Copyright 2010-2014 Bastian Eicher
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
using System.Net;
using JetBrains.Annotations;
using NanoByte.Common.Tasks;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Store;

namespace ZeroInstall.Commands.FrontendCommands
{
    /// <summary>
    /// Add an application to the <see cref="AppList"/>.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class AddApp : AppCommand
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "add";

        /// <summary>The alternative name of this command as used in command-line arguments in lower-case.</summary>
        public const string AltName = "add-app";

        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionAddApp; } }

        /// <inheritdoc/>
        protected override string Usage { get { return "[OPTIONS] INTERFACE"; } }
        #endregion

        #region State
        /// <inheritdoc/>
        public AddApp([NotNull] ICommandHandler handler) : base(handler)
        {
            Options.Add("no-download", () => Resources.OptionNoDownload, _ => NoDownload = true);
        }
        #endregion

        /// <inheritdoc/>
        protected override int ExecuteHelper(ICategoryIntegrationManager integrationManager, FeedUri interfaceUri)
        {
            #region Sanity checks
            if (integrationManager == null) throw new ArgumentNullException("integrationManager");
            if (interfaceUri == null) throw new ArgumentNullException("interfaceUri");
            #endregion

            try
            {
                CreateAppEntry(integrationManager, ref interfaceUri);
                return 0;
            }
                #region Error handling
            catch (WebException)
            {
                // WebException is a subclass of InvalidOperationException but we don't want to catch it here
                throw;
            }
            catch (InvalidOperationException ex)
            { // Application already in AppList
                Handler.OutputLow(Resources.DesktopIntegration, ex.Message);
                return 1;
            }
            #endregion
        }
    }
}
