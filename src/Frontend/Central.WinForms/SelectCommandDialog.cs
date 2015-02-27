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
using System.ComponentModel;
using System.Globalization;
using System.IO;
using NanoByte.Common;
using NanoByte.Common.Controls;
using ZeroInstall.Central.Properties;
using ZeroInstall.Store;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Central.WinForms
{
    /// <summary>
    /// A dialog box asking the the user to select an <see cref="Command"/>.
    /// </summary>
    public partial class SelectCommandDialog : OKCancelDialog
    {
        #region Inner classes
        /// <summary>
        /// Wraps <see cref="Element"/>s so that their <see cref="object.ToString"/> methods return localized names.
        /// </summary>
        private class EntryPointWrapper
        {
            public readonly EntryPoint Element;

            public EntryPointWrapper(EntryPoint element)
            {
                Element = element;
            }

            public override string ToString()
            {
                return Element.Names.GetBestLanguage(CultureInfo.CurrentUICulture) ?? Element.Command;
            }
        }
        #endregion

        private readonly FeedTarget _target;

        /// <summary>
        /// Creates a dialog box for asking the the user to select an <see cref="Command"/>.
        /// </summary>
        /// <param name="target">The application to be launched.</param>
        public SelectCommandDialog(FeedTarget target)
        {
            _target = target;

            InitializeComponent();
        }

        private void SelectCommandDialog_Load(object sender, EventArgs e)
        {
            Text = string.Format(Resources.SelectCommand, _target.Feed.Name);

            // Wrap entry points so that their ToXmlString methods return localized names
            foreach (var entryPoint in _target.Feed.EntryPoints)
                comboBoxCommand.Items.Add(new EntryPointWrapper(entryPoint));

            comboBoxCommand.SelectedIndex = 0;
        }

        private void comboBoxCommand_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Display entry point description
            var entryPoint = comboBoxCommand.SelectedItem as EntryPointWrapper;
            if (entryPoint != null) labelSummary.Text = entryPoint.Element.Summaries.GetBestLanguage(CultureInfo.CurrentUICulture);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            // Differentiate between entry point describing a command and a direct command
            var entryPoint = comboBoxCommand.SelectedItem as EntryPointWrapper;
            string command = (entryPoint == null) ? comboBoxCommand.Text : entryPoint.Element.Command;

            try
            {
                // Cannot use in-process method here because the "args" string needs to be parsed by the operating system
                ProcessUtils.LaunchAssembly(Commands.WinForms.Program.ExeName,
                    "run --no-wait --command " + command.EscapeArgument() + " " + _target.Uri.ToStringRfc().EscapeArgument() +
                    " " + textBoxArgs.Text);
            }
                #region Error handling
            catch (FileNotFoundException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }
            catch (Win32Exception ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }
            #endregion

            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
