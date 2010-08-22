using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Common.Controls;
using ZeroInstall.Publish.WinForms.Controls;

namespace ZeroInstall.Publish.WinForms.FeedStructure
{
    public partial class RecipeForm : OKCancelDialog
    {
        public RecipeForm()
        {
            InitializeComponent();
        }

        private void TabControlRecipeMouseClick(object sender, MouseEventArgs e)
        {
            int lastTabIndex = tabControlRecipe.TabCount - 1;
            if (tabControlRecipe.SelectedIndex != lastTabIndex) return;

            var newArchiveControl = new ArchiveControl();
            newArchiveControl.Name = "archiveControl" + lastTabIndex;
            newArchiveControl.Location = new Point(6, 6);

            var newTabPage = new TabPage("Archive");
            newTabPage.Controls.Add(newArchiveControl);

            tabControlRecipe.TabPages.Insert(lastTabIndex, newTabPage);
            tabControlRecipe.SelectedIndex = lastTabIndex;
        }
    }
}
