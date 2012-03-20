/*
 * Copyright 2010-2012 Bastian Eicher
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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Common;
using Common.Collections;
using Common.Controls;
using Common.Storage;
using Common.Utils;
using ZeroInstall.Central.WinForms.Properties;
using ZeroInstall.Injector;
using ZeroInstall.Injector.Feeds;
using ZeroInstall.Store.Implementation;

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
            groupImplDirs.Enabled = !Locations.IsPortable;

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
                foreach (string implementationDir in StoreProvider.GetImplementationDirs())
                {
                    // Differentiate between directories that can be modified (because they are listed in the user config) and those that cannot
                    if (Array.Exists(userConfig, entry => entry == implementationDir)) listBoxImplDirs.Items.Add(new DirectoryStore(implementationDir)); // DirectoryStore = can be modified
                    else listBoxImplDirs.Items.Add(implementationDir); // Plain string = cannot be modified
                }

                // Read list of trusted keys
                var trustDB = TrustDB.LoadSafe();
                var trustNodes = new NamedCollection<TrustNode>();
                trustDB.Keys.Apply(key => key.Domains.Apply(domain => trustNodes.Add(new TrustNode(key.Fingerprint, domain))));
                _treeViewTrustedKeys.Entries = trustNodes;
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
                Msg.Inform(this, Resources.ProblemLoadingOptions + "\n" + ex.Message, MsgSeverity.Error);
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

                if (!Locations.IsPortable)
                {
                    // Write list of user-configured implementation directories to temporary file first to ensure atomicity
                    string tempPath = _implementationDirsConfigPath + ".new." + Path.GetRandomFileName(); // Append random string for temp file name
                    try
                    {
                        WriteImplDirs(tempPath);
                        FileUtils.Replace(tempPath, _implementationDirsConfigPath);
                    }
                        #region Error handling
                    catch (Exception)
                    {
                        // Clean up failed transactions
                        if (File.Exists(tempPath)) File.Delete(tempPath);
                        throw;
                    }
                    #endregion
                }

                // Write list of trusted keys
                var trustDB = new TrustDB();
                foreach (var trustNode in _treeViewTrustedKeys.Entries)
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

        /// <summary>
        /// Writes the directories listed in <see cref="listBoxImplDirs"/> to a plain-text configuration file.
        /// </summary>
        /// <param name="configPath">The path of the configuration file to write.</param>
        private void WriteImplDirs(string configPath)
        {
            using (var configFile = new StreamWriter(configPath, false, new UTF8Encoding(false)) {NewLine = "\n"})
            {
                foreach (var store in EnumerableUtils.OfType<DirectoryStore>(listBoxImplDirs.Items))
                    configFile.WriteLine(store.DirectoryPath);
            }
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Opens a URL in the system's default browser.
        /// </summary>
        /// <param name="url">The URL to open.</param>
        private void OpenInBrowser(string url)
        {
            try
            {
                Process.Start(url);
            }
                #region Error handling
            catch (FileNotFoundException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }
            catch (Win32Exception ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }
            #endregion
        }
        #endregion

        //--------------------//

        #region Storage buttons
        private void listBoxImplDirs_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Enable go to button if there is exactly one object selected
            buttonGoToImplDir.Enabled = (listBoxImplDirs.SelectedItems.Count == 1);

            // Enable remove button if there is at least one removable object selected
            buttonRemoveImplDir.Enabled = false;
            foreach (var item in listBoxImplDirs.SelectedItems)
            {
                if (item is DirectoryStore)
                {
                    buttonRemoveImplDir.Enabled = true;
                    return;
                }
            }
        }

        private void buttonGoToImplDir_Click(object sender, EventArgs e)
        {
            OpenInBrowser(listBoxImplDirs.SelectedItem.ToString());
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
                    // ToDo: newStore.Audit()
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

            listBoxImplDirs.Items.Add(newStore);
        }

        private void buttonRemoveImplDir_Click(object sender, EventArgs e)
        {
            // Remove all selected items that are DirectoryStores and not plain strings
            var toRemove = new LinkedList<DirectoryStore>();
            foreach (var item in listBoxImplDirs.SelectedItems)
            {
                var store = item as DirectoryStore;
                if (store != null) toRemove.AddLast(store);
            }

            if (!Msg.YesNo(this, string.Format(Resources.RemoveSelectedEntries, toRemove.Count), MsgSeverity.Warn)) return;
            foreach (var store in toRemove) listBoxImplDirs.Items.Remove(store);
        }
        #endregion

        #region Trust buttons
        private void _treeViewTrustedKeys_CheckedEntriesChanged(object sender, EventArgs e)
        {
            // Enable remove button when there are checked elements
            buttonRemoveTrustedKey.Enabled = (_treeViewTrustedKeys.CheckedEntries.Length != 0);
        }

        private void buttonRemoveTrustedKey_Click(object sender, EventArgs e)
        {
            var checkedNodes = _treeViewTrustedKeys.CheckedEntries;
            if (!Msg.YesNo(this, string.Format(Resources.RemoveCheckedKeys, checkedNodes.Length), MsgSeverity.Warn)) return;

            foreach (var node in checkedNodes)
                _treeViewTrustedKeys.Entries.Remove(node);
        }
        #endregion

        #region Sync buttons
        private void textBoxSync_TextChanged(object sender, EventArgs e)
        {
            buttonSyncReset.Enabled = !string.IsNullOrEmpty(textBoxSyncServer.Text) && textBoxSyncServer.IsValid &&
                !string.IsNullOrEmpty(textBoxSyncUsername.Text) && !string.IsNullOrEmpty(textBoxSyncPassword.Text);
        }

        private void linkSyncAccount_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                string syncServer = textBoxSyncServer.Text;
                if (!syncServer.EndsWith("/")) syncServer += "/"; // Ensure the server URI references a directory
                OpenInBrowser(syncServer + "account");
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
                Msg.Inform(this, Resources.ProblemLoadingOptions + "\n" + ex.Message, MsgSeverity.Error);
            }
            #endregion
        }

        private void buttonSyncCryptoKey_Click(object sender, EventArgs e)
        {
            Msg.Inform(this, Resources.SyncCryptoKeyDescription, MsgSeverity.Info);
        }

        private void buttonSyncSetup_Click(object sender, EventArgs e)
        {
            new SyncConfig.SetupWizard().ShowDialog(this);
            LoadConfig();
        }

        private void buttonSyncReset_Click(object sender, EventArgs e)
        {
            SaveConfig();
            new SyncConfig.ResetWizard().ShowDialog(this);
            LoadConfig();
        }
        #endregion

        #region Global buttons
        private void buttonAdvanced_Click(object sender, EventArgs e)
        {
            switch (Msg.YesNoCancel(this, Resources.SaveChanges, MsgSeverity.Info, Resources.SaveChangesYes, Resources.SaveChangesNo))
            {
                case DialogResult.Yes:
                    SaveConfig();
                    break;
                case DialogResult.Cancel:
                    return;
            }

            ProcessUtils.RunAsync(() => Commands.WinForms.Program.Main(new[] {"config"}));
            Close();
        }

        private void buttonApplyOK_Click(object sender, EventArgs e)
        {
            SaveConfig();
        }
        #endregion
    }
}
