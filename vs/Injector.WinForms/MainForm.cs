using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using Common;
using Common.Utils;
using ZeroInstall.Injector.Arguments;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Injector.WinForms
{
    /// <summary>
    /// Uses GUI message boxes to ask the user questions.
    /// </summary>
    public partial class MainForm : Form, IHandler
    {
        #region Events
        void MainForm_Closing(object sender, CancelEventArgs e)
        {
            if (progressBar.Task != null)
            {
                // Try to cancel the current task instead of closing the window directly
                progressBar.Task.Cancel();
                e.Cancel = true;
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Silently answer all questions with "No".
        /// </summary>
        public bool Batch { get; set; }
        #endregion

        #region Constructor
        public MainForm()
        {
            Closing += MainForm_Closing;
        }
        #endregion

        //--------------------//

        #region Execute
        /// <summary>
        /// Executes the commands specified by the command-line arguments.
        /// </summary>
        /// <param name="results">The parser results to be executed.</param>
        public void Execute(ParseResults results)
        {
            RunGuiAsync(results.Feed);

            var controller = new Controller(results.Feed, SolverProvider.Default, results.Policy);

            if (results.SelectionsFile == null)
            {
                try { controller.Solve(); }
                #region Error hanlding
                catch (IOException ex)
                {
                    ReportErrorSync(ex.Message);
                    return;
                }
                catch (SolverException ex)
                {
                    ReportErrorSync(ex.Message);
                    return;
                }
                #endregion
            }
            else controller.SetSelections(Selections.Load(results.SelectionsFile));

            try { controller.DownloadUncachedImplementations(); }
            #region Error hanlding
            catch (UserCancelException)
            {
                progressBar.Task = null;
                Invoke((SimpleEventHandler)Close);
                return;
            }
            catch (WebException ex)
            {
                ReportErrorSync(ex.Message);
                return;
            }
            catch (IOException ex)
            {
                ReportErrorSync(ex.Message);
                return;
            }
            catch (UnauthorizedAccessException ex)
            {
                ReportErrorSync(ex.Message);
                return;
            }
            catch (DigestMismatchException ex)
            {
                ReportErrorSync(ex.Message);
                return;
            }
            #endregion
            
            CloseSync();

            if (!results.DownloadOnly)
            {
                var launcher = controller.GetLauncher();
                launcher.Main = results.Main;
                launcher.Wrapper = results.Wrapper;

                var startInfo = launcher.GetStartInfo(StringUtils.Concatenate(results.AdditionalArgs, " "));
                try
                {
                    if (results.NoWait) ProcessUtils.RunDetached(startInfo);
                    else ProcessUtils.RunReplace(startInfo);
                }
                #region Error hanlding
                catch (ImplementationNotFoundException ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    return;
                }
                catch (MissingMainException ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    return;
                }
                catch (Win32Exception ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    return;
                }
                catch (BadImageFormatException ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    return;
                }
                catch (IOException ex)
                {
                    Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    return;
                }
                #endregion
            }
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Runs the GUI in a separate thread.
        /// </summary>
        private void RunGuiAsync(string interfaceID)
        {
            new Thread(delegate()
            {
                InitializeComponent();
                labelName.Text = interfaceID;
                Application.Run(this);
            }).Start();
        }

        /// <summary>
        /// Safely closes the window running in the GUI thread.
        /// </summary>
        private void CloseSync()
        {
            progressBar.Task = null;

            // Wait until the GUI is actually up and running
            while (!IsHandleCreated) Thread.Sleep(0);

            Invoke((SimpleEventHandler)Close);
        }

        /// <summary>
        /// Displays error messages in dialogs synchronous to the main UI.
        /// </summary>
        /// <param name="message">The error message to be displayed.</param>
        private void ReportErrorSync(string message)
        {
            // Wait until the GUI is actually up and running
            while (!IsHandleCreated) Thread.Sleep(0);

            // Handle events coming from a non-UI thread, block caller until user has answered
            Invoke((SimpleEventHandler)(delegate
            {
                Msg.Inform(this, message, MsgSeverity.Error);

                progressBar.Task = null;
                Close();
            }));
        }
        #endregion

        #region Handler
        /// <inheritdoc />
        public bool AcceptNewKey(string information)
        {
            if (Batch) return false;

            bool result = false;

            // Wait until the GUI is actually up and running
            while (!IsHandleCreated) Thread.Sleep(0);

            // Handle events coming from a non-UI thread, block caller until user has answered
            Invoke((SimpleEventHandler)(() => result = Msg.Ask(this, information, MsgSeverity.Information, "Accept\nTrust this new key", "Deny\nReject the key and cancel")));

            return result;
        }

        /// <inheritdoc />
        public void StartingDownload(IProgress download)
        {
            // Wait until the GUI is actually up and running
            while (!IsHandleCreated) Thread.Sleep(0);

            // Handle events coming from a non-UI thread, don't block caller
            BeginInvoke((SimpleEventHandler)delegate
            {
                labelOperation.Text = "Downloading...";
                progressBar.Task = download;
                labelName.Text = download.Name;
            });
        }

        /// <inheritdoc />
        public void StartingExtraction(IProgress extraction)
        {
            // Wait until the GUI is actually up and running
            while (!IsHandleCreated) Thread.Sleep(0);

            // Handle events coming from a non-UI thread, don't block caller
            BeginInvoke((SimpleEventHandler)delegate
            {
                labelOperation.Text = "Extracting...";
                progressBar.Task = extraction;
                labelName.Text = ""; // Nobody cares about random file names
            });
        }

        /// <inheritdoc />
        public void StartingManifest(IProgress manifest)
        {
            // Wait until the GUI is actually up and running
            while (!IsHandleCreated) Thread.Sleep(0);

            // Handle events coming from a non-UI thread, don't block caller
            BeginInvoke((SimpleEventHandler)delegate
            {
                labelOperation.Text = "Generating manifest...";
                progressBar.Task = manifest;
                labelName.Text = ""; // Nobody cares about random file names
            });
        }
        #endregion

        private void buttonCancel_Click(object sender, System.EventArgs e)
        {
            // Only allow to cancel once
            buttonCancel.Enabled = false;

            // MainForm_Closing will end the current  progressBar.Task
            Close();
        }
    }
}
