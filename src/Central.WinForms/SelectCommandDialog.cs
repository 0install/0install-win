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
using System.Globalization;
using System.Windows.Forms;
using Common;
using Common.Controls;
using ZeroInstall.Central.WinForms.Properties;
using ZeroInstall.Model;

namespace ZeroInstall.Central.WinForms
{
    /// <summary>
    /// A dialog box asking the the user to select an <see cref="Command"/>.
    /// </summary>
    public partial class SelectCommandDialog : OKCancelDialog
    {
        #region Inner classes
        /// <summary>
        /// Wraps <see cref="EntryPoint"/>s so that their ToString methods return localized names.
        /// </summary>
        private class EntryPointWrapper : ToStringWrapper<EntryPoint>
        {
            public EntryPointWrapper(EntryPoint entryPoint) :
                base(entryPoint, () => entryPoint.Names.GetBestLanguage(CultureInfo.CurrentUICulture) ?? entryPoint.Command)
            {}
        }
        #endregion

        #region Constructor
        private SelectCommandDialog()
        {
            InitializeComponent();
        }
        #endregion

        #region Static access
        /// <summary>
        /// Displays an dialog box asking the the user to select an <see cref="Command"/>.
        /// </summary>
        /// <param name="owner">The parent window the displayed window is modal to; may be <see langword="null"/>.</param>
        /// <param name="feed">The <see cref="Feed"/> containing <see cref="EntryPoint"/>s with information about available <see cref="Command"/>s.</param>
        /// <param name="args">Returns additional command-line arguments specified by the user.</param>
        /// <returns>The <see cref="EntryPoint.Command"/> the user selected if she pressed OK; otherwise <see langword="null"/>.</returns>
        public static string ShowDialog(IWin32Window owner, Feed feed, out string args)
        {
            #region Sanity checks
            if (feed == null) throw new ArgumentNullException("feed");
            #endregion

            using (var dialog = new SelectCommandDialog
            {
                Text = string.Format(Resources.SelectCommand, feed.Name),
                ShowInTaskbar = (owner == null)
            })
            {
                // Wrap entry points so that their ToString methods return localized names
                foreach (var entryPoint in feed.EntryPoints)
                    dialog.comboBoxCommand.Items.Add(new EntryPointWrapper(entryPoint));

                // Add default command as a fallback
                if (dialog.comboBoxCommand.Items.Count == 0)
                    dialog.comboBoxCommand.Items.Add(Command.NameRun);

                if (dialog.ShowDialog(owner) == DialogResult.OK)
                {
                    args = dialog.textBoxArgs.Text;
                    return dialog.GetCommand();
                }
                else
                {
                    args = null;
                    return null;
                }
            }
        }

        /// <summary>
        /// Determines the name of the selected command.
        /// </summary>
        private string GetCommand()
        {
            // Differentiate between entry point describing a command and a direct command
            var entryPoint = comboBoxCommand.SelectedItem as EntryPointWrapper;
            return (entryPoint == null) ? comboBoxCommand.Text : entryPoint.Element.Command;
        }
        #endregion

        #region ComboBox
        private void SelectCommandDialog_Load(object sender, EventArgs e)
        {
            if (comboBoxCommand.Items.Count != 0) comboBoxCommand.SelectedIndex = 0;
        }

        private void comboBoxCommand_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Display entry point description
            var entryPoint = comboBoxCommand.SelectedItem as EntryPointWrapper;
            if (entryPoint != null) labelSummary.Text = entryPoint.Element.Summaries.GetBestLanguage(CultureInfo.CurrentUICulture);
        }
        #endregion
    }
}
