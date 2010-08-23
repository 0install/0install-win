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

        #region Add/Remove Tab logic

        /// <summary>
        /// Adds a new <see cref="TabPage"/> with a <see cref="ArchiveControl"/> left to the last <see cref="TabPage"/> of <see cref="tabControlRecipe"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void TabControlRecipeMouseClick(object sender, MouseEventArgs e)
        {
            int lastTabIndex = tabControlRecipe.TabCount - 1;
            int selectedTabIndex = tabControlRecipe.SelectedIndex;
            if (selectedTabIndex != lastTabIndex) return;

            var newArchiveControl = new ArchiveControl
                                        {
                                            Name = "archiveControl",
                                            Location = new Point(6, 6),
                                        };
            var newTabPage = new TabPage("Archive") { UseVisualStyleBackColor = true };
            newTabPage.Controls.Add(newArchiveControl);

            tabControlRecipe.TabPages.Insert(lastTabIndex, newTabPage);
            tabControlRecipe.SelectedIndex = lastTabIndex;
        }

        /// <summary>
        /// Removes the double clicked <see cref="TabPage"/> on <see cref="tabControlRecipe"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void TabControlRecipeDoubleClick(object sender, EventArgs e)
        {
            int openTabs = tabControlRecipe.TabCount;
            int selectedTabIndex = tabControlRecipe.SelectedIndex;
            int lastTabIndex = tabControlRecipe.TabCount - 1;

            if (openTabs == 2 || selectedTabIndex == lastTabIndex) return;

            tabControlRecipe.TabPages.RemoveAt(selectedTabIndex);
        }

        #endregion
    }
}
