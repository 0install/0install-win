using System.Windows.Forms;
using ZeroInstall.Backend.Model;

namespace ZeroInstall.Launchpad
{
    public partial class InterfaceInformationDialog : Form
    {
        public InterfaceInformationDialog(Interface feed)
        {
            InitializeComponent();

            // ToDo
        }

        public static void ShowDialog(IWin32Window owner, Interface feed)
        {
            using (var dialog = new InterfaceInformationDialog(feed))
                dialog.ShowDialog(owner);
        }
    }
}
