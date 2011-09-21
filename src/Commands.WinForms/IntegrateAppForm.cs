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
using Common.Controls;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.Model;

namespace ZeroInstall.Commands.WinForms
{
    /// <summary>
    /// A dialog form used for displaying and applying desktop integration options for applications.
    /// </summary>
    public partial class IntegrateAppForm : OKCancelDialog
    {
        private readonly IIntegrationManager _integrationManager;
        private readonly InterfaceFeed _target;

        private IntegrateAppForm(IIntegrationManager integrationManager, InterfaceFeed target)
        {
            InitializeComponent();

            _integrationManager = integrationManager;
            _target = target;
        }

        /// <summary>
        /// Displays the form as a modal dialog.
        /// </summary>
        /// <param name="integrationManager">The integration manager used to apply selected integration options.</param>
        /// <param name="target">The application to be integrated.</param>
        public static void ShowDialog(IIntegrationManager integrationManager, InterfaceFeed target)
        {
            #region Sanity checks
            if (integrationManager == null) throw new ArgumentNullException("integrationManager");
            #endregion

            using (var form = new IntegrateAppForm(integrationManager, target))
                form.ShowDialog();
        }

        private void IntegrateAppForm_Load(object sender, EventArgs e)
        {
            foreach (var capabilityList in _target.Feed.CapabilityLists.FindAll(list => list.Architecture.IsCompatible(Architecture.CurrentSystem)))
            {
                foreach (var capability in capabilityList.Entries)
                {
                    // ToDo: Advertise access points based on capabilities
                }
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            // Hide so that the underlying progress tracker is visible
            Hide();

            // ToDo: Apply integrations
            //_integrationManager.RemoveAccessPoints(_target.InterfaceID, new AccessPoint[] {...});
            //_integrationManager.AddAccessPoints(_target, new AccessPoint[] {...});
        }
    }
}
