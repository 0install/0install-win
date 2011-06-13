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
using Common.Storage;
using Common.Utils;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Injector;
using ZeroInstall.Model;
using ZeroInstall.DesktopIntegration.Model;
using System.Collections.Generic;
using ZeroInstall.Model.Capabilities;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Integrate an application into the desktop environment.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class IntegrateApp : AppCommand
    {
        #region Constants
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public const string Name = "integrate-app";

        /// <summary>
        /// Indicates that all <see cref="Capability"/>s shall be integrated.
        /// </summary>
        private const string CapabilitiesCategoryName = "capabilities";

        /// <summary>
        /// Indicates that all <see cref="Capability"/>s and <see cref="AccessPoint"/>s shall be integrated.
        /// </summary>
        private const string AllCategoryName = "all";
        #endregion

        #region Variables
        /// <summary>
        /// A list of all integration categories to be added to the already applied ones.
        /// </summary>
        private readonly List<string> _addIntegrations = new List<string>();

        /// <summary>
        /// A list of all integration categories to be removed from the already applied ones.
        /// </summary>
        private readonly List<string> _removeIntegrations = new List<string>();
        #endregion

        #region Properties
        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionIntegrateApp; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public IntegrateApp(Policy policy) : base(policy)
        {
            string categoryList = StringUtils.Concatenate(new[] {CapabilitiesCategoryName, DefaultAccessPoint.CategoryName, IconAccessPoint.CategoryName, AppPath.CategoryName, AllCategoryName}, ", ");

            Options.Add("a|add=", Resources.OptionAppAdd + "\n" + Resources.OptionAppCategory + categoryList + "\n" + string.Format(Resources.OptionAppImplicitCategory, CapabilitiesCategoryName), category => _addIntegrations.Add(category.ToLower()));
            Options.Add("r|remove=", Resources.OptionAppRemove + "\n" + Resources.OptionAppCategory + categoryList, category => _removeIntegrations.Add(category.ToLower()));
        }
        #endregion

        //--------------------//

        #region Execute
        /// <inheritdoc/>
        public override int Execute()
        {
            if (Locations.IsPortable) throw new InvalidOperationException(Resources.NotAvailableInPortableMode);

            return base.Execute();
        }

        /// <inheritdoc/>
        protected override int ExecuteHelper(string interfaceID, Feed feed)
        {
            // ToDo: Move logic into backend
            foreach (var capabilityList in feed.CapabilityLists)
            {
                if (!capabilityList.Architecture.IsCompatible(Architecture.CurrentSystem)) continue;

                foreach (var capability in capabilityList.Entries)
                {
                    var fileType = capability as Model.Capabilities.FileType;
                    if (fileType != null && WindowsUtils.IsWindows)
                        DesktopIntegration.Windows.FileType.Apply(interfaceID, feed, fileType, _addIntegrations.Contains(DefaultAccessPoint.CategoryName), Global);
                }

                WindowsUtils.NotifyAssocChanged();
            }
            return 0;
        }
        #endregion
    }
}
