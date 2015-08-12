/*
 * Copyright 2010-2016 Bastian Eicher
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
using System.IO;
using JetBrains.Annotations;
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
        #region Inner classe
        private class EntryPointWrapper
        {
            private readonly Feed _feed;
            private readonly EntryPoint _entryPoint;

            public EntryPointWrapper([NotNull] Feed feed, [NotNull] EntryPoint entryPoint)
            {
                _feed = feed;
                _entryPoint = entryPoint;
            }

            public EntryPointWrapper([NotNull] Feed feed, [NotNull] string commandName)
            {
                _feed = feed;
                _entryPoint = new EntryPoint {Command = commandName};
            }

            [CanBeNull]
            public string GetSummary()
            {
                return _feed.GetBestSummary(CultureInfo.CurrentUICulture, _entryPoint.Command);
            }

            public override string ToString()
            {
                return _feed.GetBestName(CultureInfo.CurrentUICulture, _entryPoint.Command);
            }

            [NotNull]
            public string GetCommand()
            {
                return _entryPoint.Command ?? Command.NameRun;
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

            foreach (var entryPoint in _target.Feed.EntryPoints)
                comboBoxCommand.Items.Add(new EntryPointWrapper(_target.Feed, entryPoint));
            if (comboBoxCommand.Items.Count == 0)
                comboBoxCommand.Items.Add(new EntryPointWrapper(_target.Feed, Command.NameRun));

            comboBoxCommand.SelectedIndex = 0;
        }

        private void comboBoxCommand_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Display entry point description
            var entryPoint = comboBoxCommand.SelectedItem as EntryPointWrapper;
            if (entryPoint != null) labelSummary.Text = entryPoint.GetSummary();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            // Differentiate between entry point describing a command and a direct command
            var entryPoint = comboBoxCommand.SelectedItem as EntryPointWrapper;
            string command = entryPoint?.GetCommand() ?? comboBoxCommand.Text;

            try
            {
                // Cannot use in-process method here because the "args" string needs to be parsed by the operating system
                ProcessUtils.Assembly(Commands.WinForms.Program.ExeName,
                    "run", "--no-wait", "--command", command, _target.Uri.ToStringRfc(), textBoxArgs.Text).Start();
            }
                #region Error handling
            catch (OperationCanceledException)
            {}
            catch (IOException ex)
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
