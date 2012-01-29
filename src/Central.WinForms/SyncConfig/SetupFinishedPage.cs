using System.Windows.Forms;
using Common;

namespace ZeroInstall.Central.WinForms.SyncConfig
{
    internal partial class SetupFinishedPage : UserControl
    {
        public event SimpleEventHandler Done;

        public SetupFinishedPage()
        {
            InitializeComponent();
        }

        private void buttonDone_Click(object sender, System.EventArgs e)
        {
            Done();
        }
    }
}
