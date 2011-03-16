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
using System.ComponentModel;
using System.Windows.Forms;
using Common.Tasks;

namespace ZeroInstall.Commands.WinForms
{
    /// <summary>
    /// Displays the progress of a <see cref="CommandBase"/> and <see cref="ITask"/>s.
    /// </summary>
    public partial class ProgressForm : Form
    {
        #region Constructor
        /// <summary>
        /// Initializes the form.
        /// </summary>
        public ProgressForm()
        {
            InitializeComponent();
        }
        #endregion
        
        public void Initialize()
        {
            CreateHandle();
            CreateControl();
        }

        #region Task tracking
        /// <summary>
        /// Sets up tracking for a <see cref="ITask"/>. Returns immediately.
        /// </summary>
        /// <param name="task">The task to be tracked. May or may not alreay be running.</param>
        /// <param name="tag">An object used to associate the <paramref name="task"/> with a specific process; may be <see langword="null"/>.</param>
        public void TrackTask(ITask task, object tag)
        {
            labelOperation.Text = task.Name + @"...";
            progressBar.Task = task;
            labelProgress.Task = task;
            buttonCancel.Enabled = true;

            if (notifyIcon.Visible)
                notifyIcon.ShowBalloonTip(5000, "Zero Install", task.Name, ToolTipIcon.None);
        }
        #endregion

        #region Event handling
        private void MainForm_Closing(object sender, CancelEventArgs e)
        {
            if (!buttonCancel.Enabled)
            {
                e.Cancel = true;
                return;
            }

            // Hide UI immediately so user doesn't notice anything freezing
            Hide();

            // Cancel any tasks that may still be running
            if (progressBar.Task != null) progressBar.Task.Cancel();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
        #endregion

        #region Tray icon
        public void ShowTrayIcon(string text)
        {
            notifyIcon.Text = Text;
            notifyIcon.Visible = true;
            notifyIcon.ShowBalloonTip(5000, "Zero Install", text, ToolTipIcon.None);
        }

        public void HideTrayIcon()
        {
            notifyIcon.Visible = false;
        }

        private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            Visible = true;
            WindowState = FormWindowState.Normal;
            HideTrayIcon();
        }

        private void buttonHide_Click(object sender, EventArgs e)
        {
            if (progressBar.Task != null) ShowTrayIcon(progressBar.Task.Name);
            else ShowTrayIcon(Text);
            Visible = false;
        }
        #endregion
    }
}
