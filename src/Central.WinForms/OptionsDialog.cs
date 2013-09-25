/*
 * Copyright 2010-2013 Bastian Eicher
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Common;
using Common.Collections;
using Common.Controls;
using Common.Storage;
using ZeroInstall.Backend;
using ZeroInstall.Central.Properties;
using ZeroInstall.Store;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Central.WinForms
{
    public partial class OptionsDialog : OKCancelDialog
    {
        #region Variables
        // Don't use WinForms designer for this, since it doesn't understand generics
        private readonly FilteredTreeView<TrustNode> _treeViewTrustedKeys = new FilteredTreeView<TrustNode> {Separator = '#', CheckBoxes = true, Dock = DockStyle.Fill};
        #endregion

        #region Startup
        public OptionsDialog()
        {
            InitializeComponent();

            panelTrustedKeys.Controls.Add(_treeViewTrustedKeys);
            _treeViewTrustedKeys.CheckedEntriesChanged += _treeViewTrustedKeys_CheckedEntriesChanged;
        }

        private void OptionsDialog_Load(object sender, EventArgs e)
        {
            LoadConfig();
        }
        #endregion

        #region Data access
        private readonly string _implementationDirsConfigPath = Locations.GetSaveConfigPath("0install.net", true, "injector", "implementation-dirs");
        private readonly string _catalogSourcesConfigPath = Locations.GetSaveConfigPath("0install.net", true, "catalog-sources");

        private void LoadConfig()
        {
            try
            {
                // Fill fields with data from config
                var config = Config.Load();
                switch (config.NetworkUse)
                {
                    case NetworkLevel.Full:
                        radioNetworkUseFull.Checked = true;
                        break;
                    case NetworkLevel.Minimal:
                        radioNetworkUseMinimal.Checked = true;
                        break;
                    case NetworkLevel.Offline:
                        radioNetworkUseOffline.Checked = true;
                        break;
                }
                checkBoxHelpWithTesting.Checked = config.HelpWithTesting;
                checkBoxAutoApproveKeys.Checked = config.AutoApproveKeys;
                textBoxSyncServer.Uri = config.SyncServer;
                textBoxSyncUsername.Text = config.SyncServerUsername;
                textBoxSyncPassword.Text = config.SyncServerPassword;
                textBoxSyncCryptoKey.Text = config.SyncCryptoKey;

                // List all implementation directories in use
                listBoxImplDirs.Items.Clear();
                var userConfig = File.Exists(_implementationDirsConfigPath) ? File.ReadAllLines(_implementationDirsConfigPath, Encoding.UTF8) : new string[0];
                foreach (string implementationDir in StoreFactory.GetImplementationDirs())
                {
                    // Differentiate between directories that can be modified (because they are listed in the user config) and those that cannot
                    if (userConfig.Contains(implementationDir)) listBoxImplDirs.Items.Add(new DirectoryStore(implementationDir)); // DirectoryStore = can be modified
                    else listBoxImplDirs.Items.Add(implementationDir); // Plain string = cannot be modified
                }

                // List all catalog sources in use
                listBoxCatalogSources.Items.Clear();
                // ReSharper disable once CoVariantArrayConversion
                listBoxCatalogSources.Items.AddRange(CatalogManager.GetCatalogSources());

                // Read list of trusted keys
                var trustDB = TrustDB.LoadSafe();
                var trustNodes = new NamedCollection<TrustNode>();
                trustDB.Keys.Apply(key => key.Domains.Apply(domain => trustNodes.Add(new TrustNode(key.Fingerprint, domain))));
                _treeViewTrustedKeys.Nodes = trustNodes;
            }
                #region Error handling
            catch (IOException ex)
            {
                Msg.Inform(this, Resources.ProblemLoadingOptions + "\n" + ex.Message, MsgSeverity.Error);
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(this, Resources.ProblemLoadingOptions + "\n" + ex.Message, MsgSeverity.Error);
            }
            catch (InvalidDataException ex)
            {
                Msg.Inform(this, Resources.ProblemLoadingOptions + "\n" + ex.Message + (ex.InnerException == null ? "" : "\n" + ex.InnerException.Message), MsgSeverity.Error);
            }
            #endregion
        }

        private void SaveConfig()
        {
            try
            {
                // Write data from fields back to config
                var config = Config.Load();
                if (radioNetworkUseFull.Checked) config.NetworkUse = NetworkLevel.Full;
                if (radioNetworkUseMinimal.Checked) config.NetworkUse = NetworkLevel.Minimal;
                if (radioNetworkUseOffline.Checked) config.NetworkUse = NetworkLevel.Offline;
                config.AutoApproveKeys = checkBoxAutoApproveKeys.Checked;
                config.HelpWithTesting = checkBoxHelpWithTesting.Checked;
                config.SyncServer = textBoxSyncServer.Uri;
                config.SyncServerUsername = textBoxSyncUsername.Text;
                config.SyncServerPassword = textBoxSyncPassword.Text;
                config.SyncCryptoKey = textBoxSyncCryptoKey.Text;
                config.Save();

                // Write list of user implementation directories
                WriteConfigFile(_implementationDirsConfigPath, listBoxImplDirs.Items.OfType<DirectoryStore>());

                // Write list of catalog sources
                WriteConfigFile(_catalogSourcesConfigPath, listBoxCatalogSources.Items.OfType<string>());

                // Write list of trusted keys
                var trustDB = new TrustDB();
                foreach (var trustNode in _treeViewTrustedKeys.Nodes)
                    trustDB.TrustKey(trustNode.Fingerprint, trustNode.Domain);
                trustDB.Save();
            }
                #region Error handling
            catch (IOException ex)
            {
                Msg.Inform(this, Resources.ProblemSavingOptions + "\n" + ex.Message, MsgSeverity.Error);
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(this, Resources.ProblemSavingOptions + "\n" + ex.Message, MsgSeverity.Error);
            }
            catch (InvalidDataException ex)
            {
                Msg.Inform(this, Resources.ProblemSavingOptions + "\n" + ex.Message, MsgSeverity.Error);
            }
            #endregion
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Writes a set of elements to a plain-text config file.
        /// </summary>
        private static void WriteConfigFile<T>(string path, IEnumerable<T> elements)
        {
            using (var atomic = new AtomicWrite(path))
            {
                using (var configFile = new StreamWriter(atomic.WritePath, false, new UTF8Encoding(false)) {NewLine = "\n"})
                {
                    foreach (var element in elements)
                        configFile.WriteLine(element.ToString());
                }
                atomic.Commit();
            }
        }
        #endregion

        //--------------------//

        #region Storage buttons
        private void listBoxImplDirs_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Enable go to button if there is exactly one object selected
            buttonGoToImplDir.Enabled = (listBoxImplDirs.SelectedItems.Count == 1);

            // Enable remove button if there is at least one removable object selected
            buttonRemoveImplDir.Enabled = listBoxImplDirs.SelectedItems.OfType<DirectoryStore>().Any();
        }

        private void buttonGoToImplDir_Click(object sender, EventArgs e)
        {
            Program.OpenInBrowser(this, listBoxImplDirs.SelectedItem.ToString());
        }

        private void buttonAddImplDir_Click(object sender, EventArgs e)
        {
            if (implDirBrowserDialog.ShowDialog(this) != DialogResult.OK) return;

            DirectoryStore newStore;
            try
            {
                newStore = new DirectoryStore(implDirBrowserDialog.SelectedPath);
                if (Directory.GetFileSystemEntries(newStore.DirectoryPath).Length != 0)
                {
                    // TODO: newStore.Audit()
                }
            }
                #region Error handling
            catch (IOException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                return;
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                return;
            }
            #endregion

            // Insert after default location in user profile
            listBoxImplDirs.Items.Insert(1, newStore);
        }

        private void buttonRemoveImplDir_Click(object sender, EventArgs e)
        {
            // Remove all selected items that are DirectoryStores and not plain strings
            var toRemove = listBoxImplDirs.SelectedItems.OfType<DirectoryStore>().ToList();

            if (!Msg.YesNo(this, string.Format(Resources.RemoveSelectedEntries, toRemove.Count), MsgSeverity.Warn)) return;
            foreach (var store in toRemove) listBoxImplDirs.Items.Remove(store);
        }
        #endregion

        #region Catalog buttons
        private void listBoxCatalogSources_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Enable go to button if there is exactly one object selected
            buttonGoToCatalogSource.Enabled = (listBoxCatalogSources.SelectedItems.Count == 1);

            // Enable remove button if there is at least one object selected
            buttonRemoveCatalogSource.Enabled = (listBoxCatalogSources.SelectedItems.Count >= 1);
        }

        private void buttonResetCatalogSources_Click(object sender, EventArgs e)
        {
            if (!Msg.YesNo(this, Resources.RemoveAllEntries, MsgSeverity.Warn)) return;
            listBoxCatalogSources.Items.Clear();
            listBoxCatalogSources.Items.Add(CatalogManager.DefaultSource);
        }

        private void buttonGoToCatalogSource_Click(object sender, EventArgs e)
        {
            Program.OpenInBrowser(this, listBoxCatalogSources.SelectedItem.ToString());
        }

        private void buttonAddCatalogSource_Click(object sender, EventArgs e)
        {
            string newSource = InputBox.Show(this, groupCatalogSources.Text, Resources.EnterCatalogUrl);
            if (!string.IsNullOrEmpty(newSource)) listBoxCatalogSources.Items.Add(newSource);
        }

        private void buttonRemoveCatalogSource_Click(object sender, EventArgs e)
        {
            if (!Msg.YesNo(this, string.Format(Resources.RemoveSelectedEntries, listBoxCatalogSources.SelectedIndices.Count), MsgSeverity.Warn)) return;
            foreach (int i in listBoxCatalogSources.SelectedIndices) listBoxCatalogSources.Items.RemoveAt(i);
        }
        #endregion

        #region Trust buttons
        private void _treeViewTrustedKeys_CheckedEntriesChanged(object sender, EventArgs e)
        {
            // Enable remove button when there are checked elements
            buttonRemoveTrustedKey.Enabled = (_treeViewTrustedKeys.CheckedEntries.Count != 0);
        }

        private void buttonRemoveTrustedKey_Click(object sender, EventArgs e)
        {
            var checkedNodes = _treeViewTrustedKeys.CheckedEntries.ToList();

            if (!Msg.YesNo(this, string.Format(Resources.RemoveCheckedKeys, checkedNodes.Count), MsgSeverity.Warn)) return;
            foreach (var node in checkedNodes.ToList()) _treeViewTrustedKeys.Nodes.Remove(node);
        }
        #endregion

        #region Sync buttons
        private void linkSyncAccount_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                string syncServer = textBoxSyncServer.Text;
                if (!syncServer.EndsWith("/")) syncServer += "/"; // Ensure the server URI references a directory
                Program.OpenInBrowser(this, syncServer + "account");
            }
                #region Error handling
            catch (IOException ex)
            {
                Msg.Inform(this, Resources.ProblemLoadingOptions + "\n" + ex.Message, MsgSeverity.Error);
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(this, Resources.ProblemLoadingOptions + "\n" + ex.Message, MsgSeverity.Error);
            }
            catch (InvalidDataException ex)
            {
                Msg.Inform(this, Resources.ProblemLoadingOptions + "\n" + ex.Message +
                                 (ex.InnerException == null ? "" : "\n" + ex.InnerException.Message), MsgSeverity.Error);
            }
            #endregion
        }

        private void buttonSyncCryptoKey_Click(object sender, EventArgs e)
        {
            Msg.Inform(this, Resources.SyncCryptoKeyDescription, MsgSeverity.Info);
        }
        #endregion

        #region Global buttons
        private void buttonApplyOK_Click(object sender, EventArgs e)
        {
            SaveConfig();
        }
        #endregion
    }
}
