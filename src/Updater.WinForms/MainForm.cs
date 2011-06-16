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
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Common;
using Common.Controls;
using Common.Utils;
using ZeroInstall.Updater.WinForms.Properties;

namespace ZeroInstall.Updater.WinForms
{
    /// <summary>
    /// Executes and tracks the progress of an <see cref="UpdateProcess"/>.
    /// </summary>
    public sealed partial class MainForm : Form
    {
        #region Variables
        /// <summary>The update process to execute and track.</summary>
        private readonly UpdateProcess _updateProcess;

        /// <summary>Indicates whether the updater has alread respawned itself as an administator. Used to prevent infinite loops.</summary>
        private readonly bool _rerun;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new update GUI.
        /// </summary>
        /// <param name="updateProcess">The update process to execute and track.</param>
        /// <param name="rerun">Indicates whether the updater has alread respawned itself as an administator. Used to prevent infinite loops.</param>
        public MainForm(UpdateProcess updateProcess, bool rerun)
        {
            _updateProcess = updateProcess;
            _rerun = rerun;

            InitializeComponent();
        }
        #endregion

        #region Startup
        private void MainForm_Shown(object sender, EventArgs e)
        {
            WindowsUtils.SetProgressState(TaskbarProgressBarState.Indeterminate, Handle);

            backgroundWorker.RunWorkerAsync();
        }
        #endregion

        //--------------------//

        #region Background worker
        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            SetStatus(Resources.MutexWait);
            _updateProcess.MutexWait();

            try
            {
                SetStatus(Resources.CopyFiles);
                _updateProcess.CopyFiles();

                SetStatus(Resources.DeleteFiles);
                _updateProcess.DeleteFiles();

                SetStatus(Resources.RunNgen);
                _updateProcess.RunNgen();
                
                SetStatus(Resources.UpdateRegistry);
                _updateProcess.UpdateRegistry();
            }
            catch (UnauthorizedAccessException)
            {
                if (_rerun || WindowsUtils.IsAdministrator) throw;

                SetStatus(Resources.RerunElevated);
                RerunElevated();
            }

            SetStatus(Resources.Done);
            _updateProcess.Done();
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                if (e.Error is IOException || e.Error is UnauthorizedAccessException)
                    Msg.Inform(this, e.Error.Message, MsgSeverity.Error);
                else ErrorReportForm.Report(e.Error, new Uri("http://0install.de/error-report/"));
            }

            if (!_rerun) RestoreGui();
            Thread.Sleep(2000);
            Close();
        }

        /// <summary>
        /// Changes the current status message displayed to the user.
        /// </summary>
        /// <param name="message">The message to display to the user.</param>
        private void SetStatus(string message)
        {
            Invoke((SimpleEventHandler)(() => statusLabel.Text = message));
        }
        #endregion

        #region Spawn processes
        /// <summary>
        /// Reruns the updater using elevated permissions (as administartor).
        /// </summary>
        private void RerunElevated()
        {
            try
            {
                var startInfo = new ProcessStartInfo(Application.ExecutablePath, StringUtils.ConcatenateEscape(new[] {_updateProcess.Source, _updateProcess.NewVersion.ToString(), _updateProcess.Target, "--rerun"})) {Verb = "runas"};
                Process.Start(startInfo).WaitForExit();
            }
            catch (Win32Exception)
            {}
        }

        /// <summary>
        /// Restores Zero Install's main GUI.
        /// </summary>
        private void RestoreGui()
        {
            var startInfo = new ProcessStartInfo(Path.Combine(_updateProcess.Target, "ZeroInstall.exe"))
            {
                ErrorDialog = true,
                ErrorDialogParentHandle = Handle
            };
            Process.Start(startInfo);
        }
        #endregion
    }
}
