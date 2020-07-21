// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Controls;
using NanoByte.Common.Native;
using ZeroInstall.Central.Properties;
using ZeroInstall.Model;

namespace ZeroInstall.Central.WinForms
{
    /// <summary>
    /// A dialog box allowing the user to specify additional options for running a feed.
    /// </summary>
    public partial class SelectCommandDialog : OKCancelDialog
    {
        #region Inner class
        private class EntryPointWrapper
        {
            private readonly Feed _feed;
            private readonly EntryPoint _entryPoint;

            public EntryPointWrapper(Feed feed, EntryPoint entryPoint)
            {
                _feed = feed;
                _entryPoint = entryPoint;
            }

            public EntryPointWrapper(Feed feed, string commandName)
            {
                _feed = feed;
                _entryPoint = new EntryPoint {Command = commandName};
            }

            public string? GetSummary() => _feed.GetBestSummary(CultureInfo.CurrentUICulture, _entryPoint.Command);

            public override string ToString() => _feed.GetBestName(CultureInfo.CurrentUICulture, _entryPoint.Command);

                public string GetCommand() => _entryPoint.Command ?? Command.NameRun;
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
            Text = Resources.Run + @" " + _target.Feed.Name;

            this.CenterOnParent();

            foreach (var entryPoint in _target.Feed.EntryPoints)
                comboBoxCommand.Items.Add(new EntryPointWrapper(_target.Feed, entryPoint));

            if (comboBoxCommand.Items.Count == 0)
                comboBoxCommand.Items.Add(new EntryPointWrapper(_target.Feed, Command.NameRun));

            comboBoxCommand.SelectedIndex = 0;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            try
            {
                ProcessUtils.Assembly(Commands.WinForms.Program.ExeName, GetArgs().ToArray()).Start();
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
            => Close();

        private void UpdateDescription(object sender, EventArgs e)
        {
            labelSummary.Text = (comboBoxCommand.SelectedItem as EntryPointWrapper)?.GetSummary();
            textBoxCommandLine.Text = GetArgs().Except("--no-wait").Prepend("0install").JoinEscapeArguments();
        }

        private IEnumerable<string> GetArgs()
        {
            yield return "run";
            yield return "--no-wait";

            string command = (comboBoxCommand.SelectedItem as EntryPointWrapper)?.GetCommand() ?? comboBoxCommand.Text;
            if (!string.IsNullOrEmpty(command) && command != Command.NameRun)
            {
                yield return "--command";
                yield return command;
            }

            if (checkBoxCustomizeVersion.Checked)
                yield return "--customize";

            if (checkBoxRefresh.Checked)
                yield return "--refresh";

            yield return _target.Uri.ToStringRfc();

            if (!string.IsNullOrEmpty(textBoxArgs.Text))
            {
                foreach (string arg in WindowsUtils.SplitArgs(textBoxArgs.Text))
                    yield return arg;
            }
        }
    }
}
