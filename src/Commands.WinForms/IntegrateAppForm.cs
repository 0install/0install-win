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

namespace ZeroInstall.Commands.WinForms
{
    /// <summary>
    /// A dialog form used for displaying and applying desktop integration options for applications.
    /// </summary>
    public partial class IntegrateAppForm : OKCancelDialog
    {
        private readonly IIntegrationManager _integrationManager;
        private readonly string _interfaceID;

        private IntegrateAppForm(IIntegrationManager integrationManager, string interfaceID)
        {
            InitializeComponent();

            _integrationManager = integrationManager;
            _interfaceID = interfaceID;
        }

        /// <summary>
        /// Displays the form as a modal dialog.
        /// </summary>
        /// <param name="integrationManager">The integration manager used to apply selected integration options.</param>
        /// <param name="interfaceID">The interface of the application to be integrated.</param>
        public static void ShowDialog(IIntegrationManager integrationManager, string interfaceID)
        {
            #region Sanity checks
            if (integrationManager == null) throw new ArgumentNullException("integrationManager");
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            #endregion

            using (var form = new IntegrateAppForm(integrationManager, interfaceID))
                form.ShowDialog();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            // ToDo: Apply integrations
        }
    }
}
