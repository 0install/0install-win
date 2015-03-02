/*
 * Copyright 2010-2015 Bastian Eicher
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
using JetBrains.Annotations;
using NanoByte.Common.Storage;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;

namespace ZeroInstall.Commands.FrontendCommands
{
    /// <summary>
    /// Imports a set of applications and desktop integrations from an existing <see cref="AppList"/> file.
    /// </summary>
    [CLSCompliant(false)]
    public class ImportApps : IntegrationCommand
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "import-apps";

        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionImportApps; } }

        /// <inheritdoc/>
        protected override string Usage { get { return "APP-LIST-FILE [OPTIONS]"; } }

        /// <inheritdoc/>
        protected override int AdditionalArgsMin { get { return 1; } }

        /// <inheritdoc/>
        protected override int AdditionalArgsMax { get { return 1; } }
        #endregion

        #region State
        /// <inheritdoc/>
        public ImportApps([NotNull] ICommandHandler handler) : base(handler)
        {
            Options.Add("no-download", () => Resources.OptionNoDownload, _ => NoDownload = true);
        }
        #endregion

        /// <inheritdoc/>
        public override int Execute()
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
                        var feed = FeedManager.GetFeed(interfaceUri);
                        integrationManager.AddAccessPoints(appEntry, feed, importEntry.AccessPoints.Entries);
                    }
                }
            }

            return 0;
        }
    }
}
