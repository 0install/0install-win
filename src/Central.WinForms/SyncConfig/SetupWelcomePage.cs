using System;
using System.Windows.Forms;

namespace ZeroInstall.Central.WinForms.SyncConfig
{
    internal partial class SetupWelcomePage : UserControl
    {
        public event Action<bool> UsedBeforeSelected;

        public SetupWelcomePage()
        {
            InitializeComponent();

            buttonUsedBeforeYes.Click += delegate { UsedBeforeSelected(true); };
            buttonUsedBeforeNo.Click += delegate { UsedBeforeSelected(false); };
        }
    }
}
