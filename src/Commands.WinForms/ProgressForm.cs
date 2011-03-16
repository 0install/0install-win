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
using Common;
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
        
        #region Handler
        /// <summary>
        /// Sets up tracking for a <see cref="ITask"/>. Returns immediately.
        /// </summary>
        /// <param name="task">The task to be tracked. May or may not alreay be running.</param>
        /// <param name="tag">An object used to associate the <paramref name="task"/> with a specific process; may be <see langword="null"/>.</param>
        public void TrackTask(ITask task, object tag)
        {
            // ToDo: Use tag

            // Handle events coming from a non-UI thread, don't block caller
            BeginInvoke((SimpleEventHandler)delegate
            {
                labelOperation.Text = task.Name + @"...";
                progressBar.Task = task;
                labelProgress.Task = task;
                buttonCancel.Enabled = true;
            });
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
            Visible = false;

            // Cancel any tasks that may still be running
            if (progressBar.Task != null) progressBar.Task.Cancel();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
        #endregion
    }
}
