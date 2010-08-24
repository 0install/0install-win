using System;
using System.Drawing;
using System.Windows.Forms;
using Common.Controls;
using Common.Helpers;
using Common.Storage;
using ZeroInstall.Model;
using ZeroInstall.Publish.WinForms.Controls;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Publish.WinForms.FeedStructure
{
    public partial class RecipeForm : OKCancelDialog
    {
        #region Attributes

        /// <summary>
        /// <see cref="Recipe"/> to edit by this form.
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
        /// <see cref="ManifestDigest"/> of <see cref="RecipeForm.Recipe"/>.
        /// </summary>
        public ManifestDigest ManifestDigest { get; private set; }

        #endregion

        #region Initialization

        public RecipeForm()
        {
            InitializeComponent();
            InsertArchiveTab(0);
        }

        #endregion

        #region Control Management

        private void ClearFormControls()
        {
            foreach (TabPage tabPage in tabControlRecipe.TabPages)
            {
                if (tabPage.Name == "tabPageAddNew") continue;
                tabControlRecipe.TabPages.Remove(tabPage);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "dummy")]
        private void UpdateFormControls()
        {
            ClearFormControls();
            IntPtr dummy = tabControlRecipe.Handle;
            if (_recipe.Steps.Count == 0)
            {
                var archiveControl = CreateArchiveControl();
                archiveControl.Archive = new Archive();

                var newTabPage = new TabPage("Archive") { UseVisualStyleBackColor = true };
                newTabPage.Controls.Add(archiveControl);
                tabControlRecipe.TabPages.Insert(tabControlRecipe.TabCount - 1, newTabPage);
            }
            else
            {
                foreach (var recipeStep in _recipe.Steps)
                {
                    var archiveControl = CreateArchiveControl();
                    archiveControl.Archive = (Archive) recipeStep;

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

            using(var tempDir = new TemporaryDirectory())
            {
                foreach (TabPage tabPage in tabControlRecipe.TabPages)
                {
                    if (tabPage.Name == "tabPageAddNew") continue;
                    var archiveControl = (ArchiveControl) tabPage.Controls["archiveControl"];

                    string extractedArchiveDir = archiveControl.ExtractedArchivePath;
                    string subfolder = archiveControl.Archive.Extract;
                    var completeSourceDir = extractedArchiveDir + StringHelper.UnifySlashes(subfolder);
                    FileHelper.CopyDirectory(completeSourceDir, tempDir.Path, true);
                    _recipe.Steps.Add(archiveControl.Archive);
                }
                ManifestDigest = Manifest.CreateDigest(tempDir.Path, null);
            }
            buttonOK.Enabled = true;
        }

        #endregion

        #region Helper methodes

        private bool AreAllArchivesValid()
        {
            foreach (TabPage tabPage in tabControlRecipe.TabPages)
            {
                if(tabPage.Name == "tabPageAddNew") continue;
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
            var newTabPage = new TabPage("Archive") { UseVisualStyleBackColor = true };
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
            newArchiveControl.NoValidArchive += delegate { buttonCreateRecipe.Enabled = false;
                                                             buttonOK.Enabled = false; };
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
