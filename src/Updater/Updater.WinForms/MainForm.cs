/*
 * Copyright 2010-2015 Bastian Eicher
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
using System.IO;
using System.Threading;
using System.Windows.Forms;
using NanoByte.Common;
using NanoByte.Common.Controls;
using NanoByte.Common.Native;
using ZeroInstall.Updater.Properties;

namespace ZeroInstall.Updater.WinForms
{
    /// <summary>
    /// Executes and tracks the progress of an <see cref="UpdateProcess"/>.
    /// </summary>
    public sealed partial class MainForm : Form
    {
        /// <summary>The update process to execute and track.</summary>
        private readonly UpdateProcess _updateProcess;

        /// <summary>Indicates whether the updater has alread respawned itself as an administator. This is used to prevent infinite loops.</summary>
        private readonly bool _rerun;

        /// <summary>Indicates whether the updater shall restart the "0install central" GUI after the update.</summary>
        private readonly bool _restartCentral;

        /// <summary>
        /// Creates a new update GUI.
        /// </summary>
        /// <param name="updateProcess">The update process to execute and track.</param>
        /// <param name="rerun">Indicates whether the updater has alread respawned itself as an administator. This is used to prevent infinite loops.</param>
        /// <param name="restartCentral">Indicates whether the updater shall restart the "0install central" GUI after the update.</param>
        public MainForm(UpdateProcess updateProcess, bool rerun, bool restartCentral)
        {
            _updateProcess = updateProcess;
            _rerun = rerun;
            _restartCentral = restartCentral;

            InitializeComponent();
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            WindowsTaskbar.SetProgressState(Handle, WindowsTaskbar.ProgressBarState.Indeterminate);

            backgroundWorker.RunWorkerAsync();
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                _updateProcess.Run(ReportStatus);
            }
            catch (UnauthorizedAccessException ex)
            {
                // Do not try to elevate in an infinite loop
                if (_rerun || WindowsUtils.IsAdministrator) throw;

                Log.Info("Elevation request triggered by:");
                Log.Info(ex);

                ReportStatus(Resources.RerunElevated);
                RerunElevated();

                ReportStatus(Resources.Done);
            }
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                if (e.Error is UnauthorizedAccessException || e.Error is IOException || e.Error is InvalidOperationException)
                { // Expected exception type
                    Log.Error(e.Error);
                    if (Environment.UserName != "SYSTEM") Msg.Inform(this, (e.Error.InnerException ?? e.Error).Message, MsgSeverity.Error);
                }
                else
                { // Unexpected exception type
                    if (Environment.UserName == "SYSTEM") e.Error.Rethrow();
                    else ErrorReportForm.Report(e.Error, new Uri("https://0install.de/error-report/"));
                }
            }

            if (_restartCentral && !_rerun) _updateProcess.RestartCentral();
            Thread.Sleep(2000);
            Close();
        }

        /// <summary>
        /// Changes the current status message displayed to the user.
        /// </summary>
        /// <param name="message">The message to display to the user.</param>
        private void ReportStatus(string message)
        {
            Invoke(new Action(() => labelStatus.Text = message));
        }

        /// <summary>
        /// Reruns the updater using elevated permissions (as administartor).
        /// </summary>
        private void RerunElevated()
        {
            try
            {
                ProcessUtils.Assembly(Program.ExeName, _updateProcess.Source, _updateProcess.NewVersion.ToString(), _updateProcess.Target, "--rerun").AsAdmin().Run();
            }
                #region Error handling
            catch (OperationCanceledException)
            {
                Log.Info("User cancelled elevation");
            }
            catch (PlatformNotSupportedException)
            {
                Log.Error("UAC not available");
            }
            catch (IOException ex)
            {
                Log.Error("Rerunning elevated failed");
                Log.Error(ex);
            }
            #endregion
        }
    }
}
