using System.Windows.Forms;
using Common;

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

        public bool AcceptNewKey(string information)
        {
            return Msg.Ask(null, information, MsgSeverity.Warning, "Accept\nTrust this new key", "Deny\nReject the key and cancel");
        }

        public void StartingDownload(IProgress download)
        {
            Initialize();

            downloadProgressBar.Task = download;
        }

        public void StartingExtraction(IProgress extraction)
        {
            Initialize();

            // ToDo: Implement
        }

        public void StartingManifest(IProgress manifest)
        {
            Initialize();

            // ToDo: Implement
        }
    }
}
