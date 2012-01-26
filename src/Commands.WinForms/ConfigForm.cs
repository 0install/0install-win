/*
 * Copyright 2010-2012 Bastian Eicher
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
using System.Windows.Forms;
using Common.Controls;
using Common.Storage;
using ZeroInstall.Commands.WinForms.Properties;
using ZeroInstall.Injector;

namespace ZeroInstall.Commands.WinForms
{
    /// <summary>
    /// A dialog form used for displaying and manipulating <see cref="Config"/> settings.
    /// </summary>
    public partial class ConfigForm : OKCancelDialog
    {
        private ConfigForm(Config config)
        {
            InitializeComponent();
            propertyGrid.SelectedObject = config;

            HandleCreated += delegate { Program.ConfigureTaskbar(this, Text, "Config", "config"); };
        }

        /// <summary>
        /// Displays the form as a modal dialog.
        /// </summary>
        /// <param name="config">The settings to display.</param>
        public static DialogResult ShowDialog(Config config)
        {
            #region Sanity checks
            if (config == null) throw new ArgumentNullException("config");
            #endregion

            using (var form = new ConfigForm(config))
            {
                if (Locations.IsPortable) form.Text += @" - " + Resources.PortableMode;
                return form.ShowDialog();
            }
        }

        private void resetValueMenuItem_Click(object sender, EventArgs e)
        {
            var item = propertyGrid.SelectedGridItem;
            if (item.PropertyDescriptor != null && item.PropertyDescriptor.CanResetValue(item.Parent.Value))
                propertyGrid.ResetSelectedProperty();
        }
    }
}
