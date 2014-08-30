/*
 * Copyright 2010-2014 Bastian Eicher
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
using NanoByte.Common;
using NanoByte.Common.Controls;
using NanoByte.Common.Storage;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Store;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Trust;
using ZeroInstall.Store.ViewModel;

namespace ZeroInstall.Commands.WinForms
{
    public sealed partial class ConfigDialog : OKCancelDialog
    {
        #region Startup
        // Don't use WinForms designer for this, since it doesn't understand generics
        // ReSharper disable once InconsistentNaming
        private readonly FilteredTreeView<TrustNode> treeViewTrustedKeys = new FilteredTreeView<TrustNode> {Separator = '\\', CheckBoxes = true, Dock = DockStyle.Fill};

        public ConfigDialog(Config config)
        {
            InitializeComponent();

            if (Locations.IsPortable) Text += @" - " + Resources.PortableMode;
            HandleCreated += delegate { Program.ConfigureTaskbar(this, Text, subCommand: ".Config", arguments: Configure.Name); };

            _config = config;
            ConfigToControls();
            LoadAdditionalConfig();

            panelTrustedKeys.Controls.Add(treeViewTrustedKeys);
            treeViewTrustedKeys.CheckedEntriesChanged += treeViewTrustedKeys_CheckedEntriesChanged;
        }
        #endregion

        #region Config
        private readonly Config _config;

        private void ConfigToControls()
        {
            switch (_config.NetworkUse)
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
            checkBoxHelpWithTesting.Checked = _config.HelpWithTesting;
            checkBoxAutoApproveKeys.Checked = _config.AutoApproveKeys;

            textBoxSyncServer.Uri = _config.SyncServer;
            textBoxSyncUsername.Text = _config.SyncServerUsername;
            textBoxSyncPassword.Text = _config.SyncServerPassword;
            textBoxSyncCryptoKey.Text = _config.SyncCryptoKey;

            propertyGridAdvanced.SelectedObject = _config;
        }

        private void radioNetworkUse_CheckedChanged(object sender, EventArgs e)
        {
            if (radioNetworkUseFull.Checked) _config.NetworkUse = NetworkLevel.Full;
            else if (radioNetworkUseMinimal.Checked) _config.NetworkUse = NetworkLevel.Minimal;
            else if (radioNetworkUseOffline.Checked) _config.NetworkUse = NetworkLevel.Offline;
            propertyGridAdvanced.Refresh();
        }

        private void checkBoxHelpWithTesting_CheckedChanged(object sender, EventArgs e)
        {
            _config.HelpWithTesting = checkBoxHelpWithTesting.Checked;
            propertyGridAdvanced.Refresh();
        }

        private void checkBoxAutoApproveKeys_CheckedChanged(object sender, EventArgs e)
        {
            _config.AutoApproveKeys = checkBoxAutoApproveKeys.Checked;
            propertyGridAdvanced.Refresh();
        }

        private void textBoxSyncServer_TextChanged(object sender, EventArgs e)
        {
            if (textBoxSyncServer.IsValid) _config.SyncServer = textBoxSyncServer.Uri;
            propertyGridAdvanced.Refresh();
        }

        private void textBoxSyncUsername_TextChanged(object sender, EventArgs e)
        {
            _config.SyncServerUsername = textBoxSyncUsername.Text;
            propertyGridAdvanced.Refresh();
        }

        private void textBoxSyncPassword_TextChanged(object sender, EventArgs e)
        {
            _config.SyncServerPassword = textBoxSyncPassword.Text;
            propertyGridAdvanced.Refresh();
        }

        private void textBoxSyncCryptoKey_TextChanged(object sender, EventArgs e)
        {
            _config.SyncCryptoKey = textBoxSyncCryptoKey.Text;
            propertyGridAdvanced.Refresh();
        }

        private void propertyGridAdvanced_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            ConfigToControls();
        }
        #endregion

        #region Additional config
        private void buttonOK_Click(object sender, EventArgs e)
        {
            SaveAdditionalConfig();
        }

        private readonly string _implementationDirsConfigPath = Locations.GetSaveConfigPath("0install.net", true, "injector", "implementation-dirs");
        private readonly string _catalogSourcesConfigPath = Locations.GetSaveConfigPath("0install.net", true, "catalog-sources");

        private void LoadAdditionalConfig()
        {
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
            treeViewTrustedKeys.Nodes = TrustDB.LoadSafe().ToNodes();
        }

        private void SaveAdditionalConfig()
        {
            // Write list of user implementation directories
            WriteConfigFile(_implementationDirsConfigPath, listBoxImplDirs.Items.OfType<DirectoryStore>());

            // Write list of catalog sources
            WriteConfigFile(_catalogSourcesConfigPath, listBoxCatalogSources.Items.OfType<string>());

            // Write list of trusted keys
            treeViewTrustedKeys.Nodes.ToTrustDB().Save();
        }

        /// <summary>
        /// Writes a set of elements to a plain-text config file.
        /// </summary>
        private static void WriteConfigFile<T>(string path, IEnumerable<T> elements)
        {
            using (var atomic = new AtomicWrite(path))
            {
                using (var configFile = new StreamWriter(atomic.WritePath, false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)) {NewLine = "\n"})
                {
                    foreach (var element in elements)
                        configFile.WriteLine(element.ToString());
                }
                atomic.Commit();
            }
        }
        #endregion

        //--------------------//

        #region Storage
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

        #region Catalog
        private void listBoxCatalogSources_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Enable go to button if there is exactly one object selected
            buttonGoToCatalogSource.Enabled = (listBoxCatalogSources.SelectedItems.Count == 1);

            // Enable remove button if there is at least one object selected
            buttonRemoveCatalogSource.Enabled = (listBoxCatalogSources.SelectedItems.Count >= 1);
        }

        private void buttonResetCatalogSources_Click(object sender, EventArgs e)
        {
            if (Msg.YesNo(this, Resources.ResetList, MsgSeverity.Warn))
            {
                listBoxCatalogSources.Items.Clear();
                listBoxCatalogSources.Items.Add(CatalogManager.DefaultSource);
            }
        }

        private void buttonGoToCatalogSource_Click(object sender, EventArgs e)
        {
            if (listBoxCatalogSources.SelectedItem != null)
                Program.OpenInBrowser(this, listBoxCatalogSources.SelectedItem.ToString());
        }

        private void buttonAddCatalogSource_Click(object sender, EventArgs e)
        {
            string newSource = InputBox.Show(this, groupCatalogSources.Text, Resources.EnterCatalogUrl);
            if (!string.IsNullOrEmpty(newSource)) listBoxCatalogSources.Items.Add(newSource);
        }

        private void buttonRemoveCatalogSource_Click(object sender, EventArgs e)
        {
            if (Msg.YesNo(this, string.Format(Resources.RemoveSelectedEntries, listBoxCatalogSources.SelectedItems.Count), MsgSeverity.Warn))
            {
                foreach (var item in listBoxCatalogSources.SelectedItems.Cast<object>().ToList())
                    listBoxCatalogSources.Items.Remove(item);
            }
        }
        #endregion

        #region Trust
        private void treeViewTrustedKeys_CheckedEntriesChanged(object sender, EventArgs e)
        {
            // Enable remove button when there are checked elements
            buttonRemoveTrustedKey.Enabled = (treeViewTrustedKeys.CheckedEntries.Count != 0);
        }

        private void buttonRemoveTrustedKey_Click(object sender, EventArgs e)
        {
            // Copy to prevent enumerator from becoming invalid due to removals
            var checkedNodes = treeViewTrustedKeys.CheckedEntries.ToList();

            if (Msg.YesNo(this, string.Format(Resources.RemoveCheckedKeys, checkedNodes.Count), MsgSeverity.Warn))
            {
                foreach (var node in checkedNodes)
                    treeViewTrustedKeys.Nodes.Remove(node);
            }
        }
        #endregion

        #region Sync
        private void linkSyncAccount_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string syncServer = textBoxSyncServer.Text;
            if (!syncServer.EndsWith("/")) syncServer += "/"; // Ensure the server URI references a directory
            Program.OpenInBrowser(this, syncServer + "account");
        }

        private void buttonSyncCryptoKey_Click(object sender, EventArgs e)
        {
            Msg.Inform(this, Resources.SyncCryptoKeyDescription, MsgSeverity.Info);
        }
        #endregion

        #region Advanced
        private void buttonAdvancedShow_Click(object sender, EventArgs e)
        {
            labelAdvancedWarning.Visible = buttonAdvancedShow.Visible = false;
            propertyGridAdvanced.Visible = true;
        }
        #endregion
    }
}
