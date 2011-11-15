/*
 * Copyright 2010 Simon E. Silva Lauinger
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Common;
using Common.Controls;
using Common.Utils;
using Common.Storage;
using ZeroInstall.Model;
using ZeroInstall.Publish.WinForms.Controls;

namespace ZeroInstall.Publish.WinForms.FeedStructure
{
    public partial class RecipeDialog : OKCancelDialog, IDigestProvider
    {
        #region Variables
        /// <summary>
        /// <see cref="Recipe"/> to be edited by this form.
        /// </summary>
        private Recipe _recipe = new Recipe();
        #endregion

        #region Properties
        /// <summary>
        /// <see cref="Recipe"/> to edit by this form.
        /// </summary>
        public Recipe Recipe
        {
            get { return _recipe; }
            set
            {
                _recipe = value ?? new Recipe();
                UpdateFormControls();
                SetStartState();
            }
        }

        /// <summary>
        /// <see cref="ManifestDigest"/> of <see cref="RecipeDialog.Recipe"/>.
        /// </summary>
        public ManifestDigest ManifestDigest { get; private set; }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public RecipeDialog()
        {
            InitializeComponent();
            InsertArchiveTab(0);
        }
        #endregion

        //--------------------//

        #region Control Management
        private void ClearFormControls()
        {
            foreach (TabPage tabPage in tabControlRecipe.TabPages)
            {
                if (tabPage.Name == "tabPageAddNew") continue;
                tabControlRecipe.TabPages.Remove(tabPage);
            }
        }

        private void UpdateFormControls()
        {
            ClearFormControls();

            if (_recipe.Steps.Count == 0)
            {
                var archiveControl = CreateArchiveControl();
                archiveControl.Archive = new Archive();

                var newTabPage = new TabPage("Archive") {UseVisualStyleBackColor = true};
                newTabPage.Controls.Add(archiveControl);
                tabControlRecipe.TabPages.Insert(tabControlRecipe.TabCount - 1, newTabPage);
            }
            else
            {
                foreach (var recipeStep in _recipe.Steps)
                {
                    var archiveControl = CreateArchiveControl();
                    archiveControl.Archive = (Archive)recipeStep;

                    var newTabPage = new TabPage("Archive") {UseVisualStyleBackColor = true};
                    newTabPage.Controls.Add(archiveControl);
                    tabControlRecipe.TabPages.Insert(tabControlRecipe.TabCount - 1, newTabPage);
                }
            }
            tabControlRecipe.SelectedIndex = 0;
        }
        #endregion

        #region Archive Control Events
        /// <summary>
        /// Invoked by every archiveControl on a tabPage when a valid archive was created.
        /// </summary>
        private void ValidArchiveCreated()
        {
            if (!AreAllArchivesValid()) return;
            buttonCreateRecipe.Enabled = true;
        }
        #endregion

        #region Control events
        private void ButtonCreateRecipeClick(object sender, EventArgs e)
        {
            _recipe.Steps.Clear();

            using (var tempDir = new TemporaryDirectory("0install-feed-editor"))
            {
                foreach (TabPage tabPage in tabControlRecipe.TabPages)
                {
                    if (tabPage.Name == "tabPageAddNew") continue;
                    var archiveControl = (ArchiveControl)tabPage.Controls["archiveControl"];

                    string extractedArchiveDir = archiveControl.ExtractedArchivePath;
                    string subfolder = archiveControl.Archive.Extract;
                    var completeSourceDir = Path.Combine(extractedArchiveDir, FileUtils.UnifySlashes(subfolder ?? ""));
                    FileUtils.CopyDirectory(completeSourceDir, tempDir.Path, true, true);
                    _recipe.Steps.Add(archiveControl.Archive);
                }
                try
                {
                    ManifestDigest = ManifestUtils.CreateDigest(this, tempDir.Path);
                }
                    #region Error handling
                catch (UserCancelException)
                {
                    return;
                }
                catch (IOException ex)
                {
                    Msg.Inform(this, ex.Message, MsgSeverity.Error);
                    return;
                }
                #endregion
            }
            buttonOK.Enabled = true;
        }
        #endregion

        #region Helper methodes
        private bool AreAllArchivesValid()
        {
            foreach (TabPage tabPage in tabControlRecipe.TabPages)
            {
                if (tabPage.Name == "tabPageAddNew") continue;
                var control = (ArchiveControl)tabPage.Controls["archiveControl"];
                if (control.ExtractedArchivePath == null) return false;
            }
            return true;
        }
        #endregion

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

            InsertArchiveTab(lastTabIndex);
            tabControlRecipe.SelectedIndex = lastTabIndex;

            SetStartState();
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
            ValidArchiveCreated();
        }

        /// <summary>
        /// Inserts a new <see cref="TabPage"/> with a <see cref="ArchiveControl"/> to <see cref="tabControlRecipe"/> on the given index.
        /// </summary>
        /// <param name="index">The index to insert the <see cref="TabPage"/></param>
        private void InsertArchiveTab(int index)
        {
            var newTabPage = new TabPage("Archive") {UseVisualStyleBackColor = true};
            newTabPage.Controls.Add(CreateArchiveControl());

            tabControlRecipe.TabPages.Insert(index, newTabPage);
        }

        /// <summary>
        /// Creates a new <see cref="ArchiveControl"/> to use on a <see cref="TabPage"/>.
        /// </summary>
        /// <returns></returns>
        private ArchiveControl CreateArchiveControl()
        {
            var newArchiveControl = new ArchiveControl
            {
                Name = "archiveControl",
                Location = new Point(6, 6)
            };
            newArchiveControl.NoValidArchive += delegate
            {
                buttonCreateRecipe.Enabled = false;
                buttonOK.Enabled = false;
            };
            newArchiveControl.ValidArchive += ValidArchiveCreated;
            newArchiveControl.Archive = new Archive();

            return newArchiveControl;
        }
        #endregion

        #region Form state
        private void SetStartState()
        {
            ManifestDigest = new ManifestDigest();
            buttonCreateRecipe.Enabled = false;
            buttonOK.Enabled = false;
        }
        #endregion
    }
}
