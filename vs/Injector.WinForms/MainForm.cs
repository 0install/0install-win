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
        #region Execute
        /// <summary>
        /// Executes the commands specified by the command-line arguments.
        /// </summary>
        /// <param name="results">The parser results to be executed.</param>
        public void Execute(ParseResults results)
        {
            new Thread(delegate()
            {
                InitializeComponent();
                labelName.Text = results.Feed;
                Application.Run(this);
            }).Start();

            var controller = new Controller(results.Feed, SolverProvider.Default, results.Policy);

            if (results.SelectionsFile == null) controller.Solve();
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
        #endregion

        #region Hanlder
        /// <inheritdoc />
        public bool AcceptNewKey(string information)
        {
            return Msg.Ask(null, information, MsgSeverity.Warning, "Accept\nTrust this new key", "Deny\nReject the key and cancel");
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
