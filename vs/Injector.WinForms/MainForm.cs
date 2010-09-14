using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using Common;
using Common.Helpers;
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
            StartAsyncGui(results.Feed);

            var controller = new Controller(results.Feed, SolverProvider.Default, results.Policy);

            if (results.SelectionsFile == null)
            {
                try { controller.Solve(); }
                #region Error hanlding
                catch (IOException ex)
                {
                    ReportSyncError(ex.Message);
                    return;
                }
                catch (SolverException ex)
                {
                    ReportSyncError(ex.Message);
                    return;
                }
                #endregion
            }
            else controller.SetSelections(Selections.Load(results.SelectionsFile));

            try { controller.DownloadUncachedImplementations(); }
            #region Error hanlding
            catch (IOException ex)
            {
                ReportSyncError(ex.Message);
                return;
            }
            catch (WebException ex)
            {
                ReportSyncError(ex.Message);
                return;
            }
            catch (UserCancelException)
            {
                progressBar.Task = null;
                Invoke((SimpleEventHandler)Close);
                return;
            }
            #endregion

            progressBar.Task = null;
            Invoke((SimpleEventHandler)Close);

            if (!results.DownloadOnly)
            {
                var launcher = controller.GetLauncher();
                launcher.Main = results.Main;
                launcher.Wrapper = results.Wrapper;
                try { launcher.RunSync(StringHelper.Concatenate(results.AdditionalArgs, " ")); }
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
                #endregion
            }
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Runs the GUI in a separate thread.
        /// </summary>
        private void StartAsyncGui(string interfaceID)
        {
            new Thread(delegate()
            {
                InitializeComponent();
                labelName.Text = interfaceID;
                Application.Run(this);
            }).Start();
        }

        /// <summary>
        /// Displays error messages in dialogs synchronous to the main UI.
        /// </summary>
        /// <param name="message">The error message to be displayed.</param>
        private void ReportSyncError(string message)
        {
            // Handle events coming from a non-UI thread, block caller until user has answered
            Invoke((SimpleEventHandler)(delegate
            {
                Msg.Inform(this, message, MsgSeverity.Error);
                Close();
            }));
        }
        #endregion

        #region Hanlder
        /// <inheritdoc />
        public bool AcceptNewKey(string information)
        {
            bool result = false;

            // Handle events coming from a non-UI thread, block caller until user has answered
            Invoke((SimpleEventHandler)(() => result = Msg.Ask(this, information, MsgSeverity.Information, "Accept\nTrust this new key", "Deny\nReject the key and cancel")));

            return result;
        }

        /// <inheritdoc />
        public void StartingDownload(IProgress download)
        {
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
            // Handle events coming from a non-UI thread, don't block caller
            BeginInvoke((SimpleEventHandler)delegate
            {
                labelOperation.Text = "Extracting...";
                progressBar.Task = extraction;
                labelName.Text = extraction.Name;
            });
        }

        /// <inheritdoc />
        public void StartingManifest(IProgress manifest)
        {
            // Handle events coming from a non-UI thread, don't block caller
            BeginInvoke((SimpleEventHandler)delegate
            {
                labelOperation.Text = "Generating manifest...";
                progressBar.Task = manifest;
                labelName.Text = manifest.Name;
            });
        }
        #endregion
    }
}
