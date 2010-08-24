using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using Common;
using Common.Helpers;
using ZeroInstall.Injector.Arguments;
using ZeroInstall.Injector.Solver;

namespace ZeroInstall.Injector.WinForms
{
    /// <summary>
    /// Uses GUI message boxes to ask the user questions.
    /// </summary>
    public partial class MainForm : Form, IHandler
    {
        public MainForm()
        {
            Closing += MainForm_Closing;
        }
        
        void MainForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (progressBar.Task != null)
            {
                // Try to cancel the current task instead of closing the window directly
                progressBar.Task.Cancel();
                e.Cancel = true;
            }
            else
            {
                // Stop any solving processes
                Process.GetCurrentProcess().Kill();
            }
        }

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
                catch (SolverException ex)
                {
                    // Handle events coming from a non-UI thread, block caller until user has answered
                    Invoke((SimpleEventHandler)(delegate
                    {
                        Msg.Inform(this, ex.Message, MsgSeverity.Error);
                        Close();
                    }));
                    return;
                }
                #endregion
            }
            else controller.SetSelections(Selections.Load(results.SelectionsFile));

            controller.DownloadUncachedImplementations();

            Invoke((SimpleEventHandler)Close);

            if (!results.DownloadOnly)
            {
                var launcher = controller.GetLauncher();
                launcher.Main = results.Main;
                launcher.Wrapper = results.Wrapper;
                launcher.RunSync(StringHelper.Concatenate(results.AdditionalArgs, " "));
            }
        }

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
