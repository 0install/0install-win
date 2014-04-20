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
using System.Windows.Forms;
using NanoByte.Common.Controls;
using NanoByte.Common.Storage;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Store;

namespace ZeroInstall.Commands.WinForms
{
    /// <summary>
    /// A dialog form used for displaying and manipulating <see cref="Config"/> settings.
    /// </summary>
    public sealed class ConfigForm : EditorDialog<Config>
    {
        private ConfigForm()
        {
            Load += delegate { Text = @"Zero Install " + Resources.Configuration + (Locations.IsPortable ? @" - " + Resources.PortableMode : ""); };
            HandleCreated += delegate { Program.ConfigureTaskbar(this, Text, "Config", "config"); };
        }

        /// <summary>
        /// Displays a dialog for editing <see cref="Config"/>.
        /// </summary>
        /// <param name="config"></param>
        /// <returns>Save changes if <see langword="true"/>; discard changes if <see langword="false"/>.</returns>
        public static bool Edit(Config config)
        {
            #region Sanity checks
            if (config == null) throw new ArgumentNullException("config");
            #endregion

            using (var form = new ConfigForm())
                return (form.ShowDialog(config) == DialogResult.OK);
        }
    }
}
