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
using System.Windows.Forms;
using JetBrains.Annotations;
using NanoByte.Common.Native;
using NanoByte.Common.Tasks;

namespace ZeroInstall
{
    /// <summary>
    /// The main GUI for the Bootstrapper.
    /// </summary>
    public partial class MainForm : Form
    {
        private readonly CancellationTokenSource _cancellationTokenSource;

        public MainForm([NotNull] CancellationTokenSource cancellationTokenSource)
        {
            _cancellationTokenSource = cancellationTokenSource;
            InitializeComponent();

            switch (EmbeddedConfig.Instance.AppMode)
            {
                case BootstrapMode.Run:
                    labelLoading.Text = $"Preparing to run {EmbeddedConfig.Instance.AppName}...";
                    break;

                case BootstrapMode.Integrate:
                    labelLoading.Text = $"Preparing to integrate {EmbeddedConfig.Instance.AppName}...";
                    break;
            }

            HandleCreated += delegate
            {
                this.EnableWindowDrag();
                labelBorder.EnableWindowDrag();
                pictureBoxLogo.EnableWindowDrag();
            };
        }

        public IProgress<TaskSnapshot> GetProgressControl([NotNull] string taskName)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(taskName)) throw new ArgumentNullException(nameof(taskName));
            #endregion

            taskControl.TaskName = taskName;
            taskControl.Visible = true;
            return taskControl;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Never allow the user to directly close the window
            e.Cancel = true;

            // Start proper cancellation instead
            Cancel();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Cancel();
        }

        /// <summary>
        /// Hides the window and then starts canceling the current process asynchronously.
        /// </summary>
        private void Cancel()
        {
            Hide();

            _cancellationTokenSource.Cancel();
        }
    }
}
