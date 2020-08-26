// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Controls;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using NanoByte.Common.Values;
using ZeroInstall.Commands.Basic;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Model;
using ZeroInstall.Services;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Store;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Trust;
using ZeroInstall.Store.ViewModel;

namespace ZeroInstall.Commands.WinForms
{
    public sealed partial class ConfigDialog : OKCancelDialog
    {
        #region Global
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
            LoadImplementationDirs();

            LoadCatalogSources();

            LoadTrust();
            panelTrustedKeys.Controls.Add(treeViewTrustedKeys);
            treeViewTrustedKeys.CheckedEntriesChanged += treeViewTrustedKeys_CheckedEntriesChanged;

            if (WindowsUtils.IsWindows) LoadLanguages();
            else tabPageLanguage.Visible = false;
        }

        /// <summary>
        /// Switch to a specific tab in the configuration GUI.
        /// </summary>
        /// <param name="configTab">The tab to switch to.</param>
        public void SelectTab(ConfigTab configTab)
        {
            switch (configTab)
            {
                case ConfigTab.Updates:
                    tabOptions.SelectTab(tabPageUpdates);
                    break;
                case ConfigTab.Storage:
                    tabOptions.SelectTab(tabPageStorage);
                    break;
                case ConfigTab.Catalog:
                    tabOptions.SelectTab(tabPageCatalog);
                    break;
                case ConfigTab.Trust:
                    tabOptions.SelectTab(tabPageTrust);
                    break;
                case ConfigTab.Sync:
                    tabOptions.SelectTab(tabPageSync);
                    break;
                case ConfigTab.Language:
                    tabOptions.SelectTab(tabPageLanguage);
                    break;
                case ConfigTab.Advanced:
                    tabOptions.SelectTab(tabPageAdvanced);
                    break;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            SaveImplementationDirs();
            SaveCatalogSources();
            SaveTrust();
            SaveLanguage();
        }
        #endregion

        #region General
        private readonly Config _config;

        private void ConfigToControls()
        {
            (_config.NetworkUse switch
            {
                NetworkLevel.Full => radioNetworkUseFull,
                NetworkLevel.Minimal => radioNetworkUseMinimal,
                NetworkLevel.Offline => radioNetworkUseOffline,
                _ => radioNetworkUseFull
            }).Checked = true;
            radioNetworkUseFull.Enabled = radioNetworkUseMinimal.Enabled = radioNetworkUseMinimal.Enabled = !Config.IsOptionLocked("network_policy");
            timespanFreshness.Value = _config.Freshness;
            checkBoxHelpWithTesting.Checked = _config.HelpWithTesting;
            checkBoxHelpWithTesting.Enabled = !Config.IsOptionLocked("help_with_testing");
            checkBoxAutoApproveKeys.Checked = _config.AutoApproveKeys;
            checkBoxAutoApproveKeys.Enabled = !Config.IsOptionLocked("auto_approve_keys");

            textBoxSyncServer.Uri = _config.SyncServer;
            textBoxSyncServer.Enabled = !Config.IsOptionLocked("sync_server");
            textBoxSyncUsername.Text = _config.SyncServerUsername;
            textBoxSyncUsername.Enabled = !Config.IsOptionLocked("sync_serveR_user");
            textBoxSyncPassword.Text = _config.SyncServerPassword;
            textBoxSyncPassword.Enabled = !Config.IsOptionLocked("sync_server_password");
            textBoxSyncCryptoKey.Text = _config.SyncCryptoKey;
            textBoxSyncCryptoKey.Enabled = !Config.IsOptionLocked("sync_crypto_key");

            propertyGridAdvanced.SelectedObject = _config;
        }

        private void radioNetworkUse_CheckedChanged(object sender, EventArgs e)
        {
            if (radioNetworkUseFull.Checked) _config.NetworkUse = NetworkLevel.Full;
            else if (radioNetworkUseMinimal.Checked) _config.NetworkUse = NetworkLevel.Minimal;
            else if (radioNetworkUseOffline.Checked) _config.NetworkUse = NetworkLevel.Offline;
            propertyGridAdvanced.Refresh();
        }

        private void timespanFreshness_Validated(object sender, EventArgs e)
        {
            _config.Freshness = timespanFreshness.Value;
            propertyGridAdvanced.Refresh();
        }

        private void checkBoxHelpWithTesting_CheckedChanged(object sender, EventArgs e)
        {
            _config.HelpWithTesting = checkBoxHelpWithTesting.Checked;
            propertyGridAdvanced.Refresh();
        }
        #endregion

        #region Storage
        private List<string> _lockedImplDirs;

        private void LoadImplementationDirs()
        {
            var allImplDirs = ImplementationStores.GetDirectories().ToList();
            listBoxImplDirs.Items.AddRange(allImplDirs.Cast<object>().ToArray());

            var userImplDirs = ImplementationStores.GetUserDirectories();
            _lockedImplDirs = allImplDirs.Except(userImplDirs).ToList();
        }

        private void SaveImplementationDirs()
        {
            var allImplDirs = listBoxImplDirs.Items.Cast<string>();
            var userImplDirs = allImplDirs.Except(_lockedImplDirs);
            ImplementationStores.SetUserDirectories(userImplDirs);
        }

        private void listBoxImplDirs_SelectedIndexChanged(object sender, EventArgs e)
        {
            buttonGoToImplDir.Enabled = (listBoxImplDirs.SelectedItems.Count == 1);

            buttonRemoveImplDir.Enabled =
                (listBoxImplDirs.SelectedItems.Count >= 1) &&
                !listBoxImplDirs.SelectedItems.Cast<string>().ToList().ContainsAny(_lockedImplDirs);
        }

        private void buttonGoToImplDir_Click(object sender, EventArgs e)
        {
            try
            {
                ProcessUtils.Start((string)listBoxImplDirs.SelectedItem);
            }
            #region Error handling
            catch (OperationCanceledException)
            {}
            catch (IOException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }
            #endregion
        }

        private void buttonAddImplDir_Click(object sender, EventArgs e)
        {
            if (implDirBrowserDialog.ShowDialog(this) != DialogResult.OK) return;

            listBoxImplDirs.Items.Insert(1, implDirBrowserDialog.SelectedPath);
        }

        private void buttonRemoveImplDir_Click(object sender, EventArgs e)
        {
            var toRemove = listBoxImplDirs.SelectedItems.Cast<string>().ToList();

            if (Msg.YesNo(this, string.Format(Resources.RemoveSelectedEntries, toRemove.Count), MsgSeverity.Warn))
            {
                foreach (string store in toRemove)
                    listBoxImplDirs.Items.Remove(store);
            }
        }
        #endregion

        #region Catalog
        private void LoadCatalogSources()
        {
            listBoxCatalogSources.Items.Clear();
            listBoxCatalogSources.Items.AddRange(CatalogManager.GetSources().Cast<object>().ToArray());
        }

        private void SaveCatalogSources() => CatalogManager.SetSources(
            listBoxCatalogSources.Items.OfType<FeedUri>());

        private void listBoxCatalogSources_SelectedIndexChanged(object sender, EventArgs e)
        {
            buttonGoToCatalogSource.Enabled = (listBoxCatalogSources.SelectedItems.Count == 1);
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
            {
                try
                {
                    ProcessUtils.Start(listBoxCatalogSources.SelectedItem.ToString());
                }
                #region Error handling
                catch (OperationCanceledException)
                {}
                catch (IOException ex)
                {
                    Msg.Inform(this, ex.Message, MsgSeverity.Error);
                }
                #endregion
            }
        }

        private void buttonAddCatalogSource_Click(object sender, EventArgs e)
        {
            string input = InputBox.Show(this, groupCatalogSources.Text, Resources.EnterCatalogUrl);
            if (!string.IsNullOrEmpty(input)) AddCatalogSource(input);
        }

        private void AddCatalogSource(string input)
        {
            try
            {
                var uri = new FeedUri(input);
                if (listBoxCatalogSources.Items.Contains(uri)) return;

                using (var handler = new DialogTaskHandler(this))
                {
                    var services = new ServiceLocator(handler);
                    services.CatalogManager.DownloadCatalog(uri);
                }
                listBoxCatalogSources.Items.Add(uri);

                // Trusted keys may have changed as a side-effect
                LoadTrust();
            }
            #region Error handling
            catch (OperationCanceledException)
            {}
            catch (ArgumentException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }
            catch (UriFormatException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }
            catch (WebException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }
            catch (SignatureException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }
            catch (IOException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }
            catch (InvalidDataException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }
            #endregion
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
        private void LoadTrust()
            => treeViewTrustedKeys.Nodes = TrustDB.LoadSafe().ToNodes();

        private void SaveTrust()
            => treeViewTrustedKeys.Nodes.ToTrustDB().Save(TrustDB.DefaultLocation);

        private void treeViewTrustedKeys_CheckedEntriesChanged(object sender, EventArgs e)
            => buttonRemoveTrustedKey.Enabled = (treeViewTrustedKeys.CheckedEntries.Count != 0);

        private void buttonRemoveTrustedKey_Click(object sender, EventArgs e)
        {
            var checkedNodes = treeViewTrustedKeys.CheckedEntries.ToList();

            if (Msg.YesNo(this, string.Format(Resources.RemoveCheckedKeys, checkedNodes.Count), MsgSeverity.Warn))
            {
                foreach (var node in checkedNodes)
                    treeViewTrustedKeys.Nodes.Remove(node);
            }
        }

        private void checkBoxAutoApproveKeys_CheckedChanged(object sender, EventArgs e)
        {
            _config.AutoApproveKeys = checkBoxAutoApproveKeys.Checked;
            propertyGridAdvanced.Refresh();
        }
        #endregion

        #region Sync
        private void linkSyncAccount_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string syncServer = textBoxSyncServer.Text;
            if (!syncServer.EndsWith("/")) syncServer += "/"; // Ensure the server URI references a directory
            try
            {
                ProcessUtils.Start(syncServer + "account");
            }
            #region Error handling
            catch (OperationCanceledException)
            {}
            catch (IOException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }
            #endregion
        }

        private void buttonSyncCryptoKey_Click(object sender, EventArgs e)
            => Msg.Inform(this, Resources.SyncCryptoKeyDescription, MsgSeverity.Info);

        private void textBoxSyncServer_TextChanged(object sender, EventArgs e)
        {
            _config.SyncServer = !textBoxSyncServer.IsValid || string.IsNullOrEmpty(textBoxSyncServer.Text)
                ? new FeedUri(Config.DefaultSyncServer)
                : new FeedUri(textBoxSyncServer.Uri);
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
        #endregion

        #region Advanced
        private void buttonAdvancedShow_Click(object sender, EventArgs e)
        {
            labelAdvancedWarning.Visible = buttonAdvancedShow.Visible = false;
            propertyGridAdvanced.Visible = true;
        }

        private void propertyGridAdvanced_PropertyValueChanged(object s, PropertyValueChangedEventArgs e) => ConfigToControls();
        #endregion

        #region Language
        private sealed class LanguageWrapper
        {
            public readonly CultureInfo Culture;

            public LanguageWrapper(CultureInfo culture)
            {
                Culture = culture;
            }

            public override string ToString() => Culture.DisplayName;

            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                if (obj == this) return true;
                return obj is LanguageWrapper && Culture.Equals(((LanguageWrapper)obj).Culture);
            }

            public override int GetHashCode() => Culture.GetHashCode();
        }

        private void LoadLanguages()
        {
            comboBoxLanguage.Items.Add(Resources.UseSystemLanguage);
            comboBoxLanguage.Items.AddRange(Languages.AllKnown.Select(x => new LanguageWrapper(x)).Cast<object>().ToArray());
            if (ProgramUtils.UILanguage == null) comboBoxLanguage.SelectedIndex = 0;
            else comboBoxLanguage.SelectedItem = new LanguageWrapper(ProgramUtils.UILanguage);
        }

        private void SaveLanguage()
        {
            var lang = comboBoxLanguage.SelectedItem as LanguageWrapper;
            ProgramUtils.UILanguage = lang?.Culture;
        }
        #endregion
    }
}
