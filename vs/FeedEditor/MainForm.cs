using System;
using System.Windows.Forms;
using Common.Storage;
using ZeroInstall.Backend.Model;

namespace ZeroInstall.FeedEditor
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }


        private void toolStripButtonNew_Click(object sender, EventArgs e)
        {
            propertyGridInterface.SelectedObject = new Interface();
        }

        private void toolStripButtonOpen_Click(object sender, EventArgs e)
        {
            openFileDialog.ShowDialog(this);
        }

        private void toolStripButtonSave_Click(object sender, EventArgs e)
        {
            saveFileDialog.ShowDialog(this);
        }

        private void openFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            propertyGridInterface.SelectedObject = XmlStorage.Load<Interface>(openFileDialog.FileName);
        }

        private void saveFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            XmlStorage.Save<Interface>(saveFileDialog.FileName, (Interface)propertyGridInterface.SelectedObject);
        }
    }
}
