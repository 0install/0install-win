using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Common.Controls;
using Common.Helpers;
using Common.Storage;
using ZeroInstall.Model;
using ZeroInstall.Publish.WinForms.Controls;
using ZeroInstall.Publish.WinForms.Properties;
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
                    CopyDirectory(completeSourceDir, tempDir.Path, true);
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

        #region Helpers

        /// <summary>
        /// Copies the content of a directory to a new location.
        /// </summary>
        /// <param name="sourcePath">The path of source directory. Must exist!</param>
        /// <param name="destinationPath">The path of the target directory. Must not exist!</param>
        /// <param name="overrideFiles">If <see langword="true"/> an existing file in <paramref name="destinationPath"/> will be overriden.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="sourcePath"/> and <paramref name="destinationPath"/> are equal.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown if <paramref name="sourcePath"/> does not exist.</exception>
        public static void CopyDirectory(string sourcePath, string destinationPath, bool overrideFiles)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(sourcePath)) throw new ArgumentNullException("sourcePath");
            if (string.IsNullOrEmpty(destinationPath)) throw new ArgumentNullException("destinationPath");
            if (sourcePath == destinationPath) throw new ArgumentException("sourcePath and destinationPath are equal.");
            if (!Directory.Exists(sourcePath)) throw new DirectoryNotFoundException();
            #endregion

            if (Directory.Exists(destinationPath))
            {
                var lastWriteTime = Directory.GetLastWriteTimeUtc(destinationPath);
                Directory.CreateDirectory(destinationPath);
                Directory.SetLastWriteTimeUtc(destinationPath, lastWriteTime);
            }
            else
            {
                Directory.CreateDirectory(destinationPath);
            }
            foreach (string entry in Directory.GetFileSystemEntries(sourcePath))
            {
                string destinationFilePath = Path.Combine(destinationPath, Path.GetFileName(entry));
                if (Directory.Exists(entry))
                {
                    // Recurse into sub-direcories
                    CopyDirectory(entry, destinationFilePath, overrideFiles);
                }
                else
                {
                    // Copy individual files
                    var lastWriteTime = File.GetLastWriteTimeUtc(destinationFilePath);
                    File.Copy(entry, destinationFilePath, overrideFiles);
                    File.SetLastWriteTimeUtc(destinationFilePath, lastWriteTime);
                }
            }
        }

        #endregion
    }
}
