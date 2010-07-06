/*
 * Copyright 2010 Bastian Eicher
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

using System.Windows.Forms;
using Common.Controls;

namespace ZeroInstall.Injector.WinForms
{
    /// <summary>
    /// Asks the user for an interface URI.
    /// </summary>
    partial class InterfaceUriDialog : OKCancelDialog
    {
        public InterfaceUriDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Asks the user for an interface URI.
        /// </summary>
        /// <returns>The URI the user entered or <see langword="null"/> if they chose to cancel.</returns>
        public static string GetUri()
        {
            using (var dialog = new InterfaceUriDialog())
            {
                return (dialog.ShowDialog() == DialogResult.OK) ? dialog.textBoxURL.Text : null;
            }
        }
    }
}
