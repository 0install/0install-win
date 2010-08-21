using System.Windows.Forms;
using Common;
using Common.Helpers;

namespace ZeroInstall.Injector.WinForms
{
    /// <summary>
    /// Uses GUI message boxes to ask the user questions.
    /// </summary>
    public partial class GuiFeedHandler : Form, IHandler
    {
        private bool _initialized;

        private void Initialize()
        {
            if (_initialized) return;

            InitializeComponent();
            Show();

            _initialized = true;
        }

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
                Initialize();

                progressBar.Task = download;
            });
        }

        /// <inheritdoc />
        public void StartingExtraction(IProgress extraction)
        {
            // Handle events coming from a non-UI thread, don't block caller
            BeginInvoke((SimpleEventHandler)delegate
            {
                Initialize();

                progressBar.Task = extraction;
            });
        }

        /// <inheritdoc />
        public void StartingManifest(IProgress manifest)
        {
            // Handle events coming from a non-UI thread, don't block caller
            BeginInvoke((SimpleEventHandler)delegate
            {
                Initialize();

                progressBar.Task = manifest;
            });
        }
    }
}
