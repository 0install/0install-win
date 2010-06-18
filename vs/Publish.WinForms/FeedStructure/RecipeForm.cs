using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ZeroInstall.Publish.WinForms.FeedStructure
{
    public partial class RecipeForm : Form
    {
        public RecipeForm()
        {
            InitializeComponent();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {

        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Owner.Enabled = true;
            Close();
            Dispose();
        }

        private void RecipeForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Owner.Enabled = true;
        }
    }
}
