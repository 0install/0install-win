// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System.Collections.ObjectModel;
using NanoByte.Common.Native;
using NanoByte.Common.Values;
using ZeroInstall.Model.Trust;
using ZeroInstall.Services;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Store.Configuration;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Trust;
using ZeroInstall.Store.ViewModel;
using Binding = Eto.Forms.Binding;

namespace ZeroInstall.Commands.EtoForms;

/// <summary>
/// Dialog for modifying the Zero Install <see cref="Config"/> and related settings.
/// </summary>
public sealed class ConfigDialog : Dialog<bool>
{
    #region Global
    private readonly TabControl _tabOptions = new();

    public ConfigDialog(Config config)
    {
        Title = Resources.ConfigTitle;
        if (Locations.IsPortable) Title += @" - " + Commands.Properties.Resources.PortableMode;
        Padding = 10;
        Resizable = true;
        MinimumSize = new(450, 380);
        this.SetClientSize(new(540, 440));
        this.PreventPinningIfNotIntegrated();

        _config = config;

        var tabPageUpdates = new TabPage {Text = Resources.TabUpdates, Padding = 8, Content = BuildUpdatesTab()};
        var tabPageStorage = new TabPage {Text = Resources.TabStorage, Padding = 8, Content = BuildStorageTab()};
        var tabPageCatalog = new TabPage {Text = Resources.TabCatalog, Padding = 8, Content = BuildCatalogTab()};
        var tabPageTrust = new TabPage {Text = Resources.TabTrust, Padding = 8, Content = BuildTrustTab()};
        var tabPageSync = new TabPage {Text = Resources.TabSync, Padding = 8, Content = BuildSyncTab()};
        var tabPageLanguage = new TabPage {Text = Resources.TabLanguage, Padding = 8, Content = BuildLanguageTab()};
        var tabPageAdvanced = new TabPage {Text = Resources.TabAdvanced, Padding = 8, Content = BuildAdvancedTab()};
        _tabOptions.Pages.Add(tabPageUpdates);
        _tabOptions.Pages.Add(tabPageStorage);
        _tabOptions.Pages.Add(tabPageCatalog);
        _tabOptions.Pages.Add(tabPageTrust);
        _tabOptions.Pages.Add(tabPageSync);
        if (WindowsUtils.IsWindows) _tabOptions.Pages.Add(tabPageLanguage);
        _tabOptions.Pages.Add(tabPageAdvanced);
        Content = _tabOptions;

        ConfigToControls();

        var allImplDirs = ImplementationStores.GetDirectories().ToList();
        foreach (string dir in allImplDirs) _implDirs.Add(dir);
        _lockedImplDirs = allImplDirs.Except(ImplementationStores.GetUserDirectories()).ToList();

        LoadCatalogSources();

        LoadTrust();

        if (WindowsUtils.IsWindows) LoadLanguages();

        _tabOptions.SelectedPage = config.InitialTab switch
        {
            ConfigTab.Updates => tabPageUpdates,
            ConfigTab.Storage => tabPageStorage,
            ConfigTab.Catalog => tabPageCatalog,
            ConfigTab.Trust => tabPageTrust,
            ConfigTab.Sync => tabPageSync,
            ConfigTab.Language when WindowsUtils.IsWindows => tabPageLanguage,
            ConfigTab.Advanced => tabPageAdvanced,
            _ => tabPageUpdates
        };

        var buttonOK = new Button {Text = NanoByte.Common.Properties.Resources.OK};
        buttonOK.Click += buttonOK_Click;
        var buttonCancel = new Button {Text = Commands.Properties.Resources.Cancel};
        buttonCancel.Click += delegate { Close(false); };
        PositiveButtons.Add(buttonOK);
        NegativeButtons.Add(buttonCancel);
        DefaultButton = buttonOK;
        AbortButton = buttonCancel;
    }

    private void buttonOK_Click(object? sender, EventArgs e)
    {
        _propertyEditorAdvanced.CommitPendingChanges();

        SaveImplementationDirs();
        SaveCatalogSources();
        SaveTrust();
        SaveLanguage();

        Close(true);
    }

    /// <summary>
    /// Creates a list with buttons to the right of it.
    /// </summary>
    private static Control ListWithButtons(GridView grid, params Button[] buttons)
    {
        var buttonStack = new StackLayout {Orientation = Orientation.Vertical, HorizontalContentAlignment = HorizontalAlignment.Stretch, Spacing = 6};
        foreach (var button in buttons) buttonStack.Items.Add(button);

        return new TableLayout
        {
            Spacing = new(6, 0),
            Rows = {new TableRow(new TableCell(grid, scaleWidth: true), buttonStack)}
        };
    }

    /// <summary>
    /// Creates a single-column list for displaying entries.
    /// </summary>
    private static GridView ListGrid(object dataStore)
        => new()
        {
            ShowHeader = false,
            AllowMultipleSelection = true,
            BackgroundColor = SystemColors.ControlBackground,
            Columns = {new GridColumn {Expand = true, DataCell = new TextBoxCell {Binding = Binding.Delegate<object, string>(x => x is FeedUri uri ? uri.ToStringRfc() : x.ToString() ?? "")}}},
            DataStore = (IEnumerable<object>)dataStore
        };

    private static Button Button(string text, EventHandler<EventArgs> onClick, bool enabled = true)
    {
        var button = new Button {Text = text, Enabled = enabled};
        button.Click += onClick;
        return button;
    }
    #endregion

    #region General
    private readonly Config _config;

    private readonly RadioButtonList _radioNetworkUse = new()
    {
        Orientation = Orientation.Vertical,
        Spacing = new(0, 4),
        DataStore = new[] {Resources.NetworkUseFull, Resources.NetworkUseMinimal, Resources.NetworkUseOffline}
    };

    private readonly TimeSpanControl _timespanFreshness = new();
    private readonly CheckBox _checkBoxHelpWithTesting = new() {Text = Resources.HelpWithTesting};

    private Control BuildUpdatesTab()
    {
        _radioNetworkUse.SelectedIndexChanged += radioNetworkUse_SelectedIndexChanged;
        _timespanFreshness.ValueChanged += timespanFreshness_ValueChanged;
        _checkBoxHelpWithTesting.CheckedChanged += checkBoxHelpWithTesting_CheckedChanged;

        return new StackLayout
        {
            Orientation = Orientation.Vertical,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Spacing = 8,
            Items =
            {
                new GroupBox {Text = Resources.NetworkUse, Padding = 6, Content = _radioNetworkUse},
                new GroupBox {Text = Resources.Freshness, Padding = 6, Content = _timespanFreshness},
                new GroupBox {Text = Resources.Stability, Padding = 6, Content = _checkBoxHelpWithTesting}
            }
        };
    }

    private void ConfigToControls()
    {
        _radioNetworkUse.SelectedIndex = _config.NetworkUse switch
        {
            NetworkLevel.Full => 0,
            NetworkLevel.Minimal => 1,
            NetworkLevel.Offline => 2,
            _ => 0
        };
        _radioNetworkUse.Enabled = !Config.IsOptionLocked("network_use");
        _timespanFreshness.Value = _config.Freshness;
        _timespanFreshness.Enabled = !Config.IsOptionLocked("freshness");
        _checkBoxHelpWithTesting.Checked = _config.HelpWithTesting;
        _checkBoxHelpWithTesting.Enabled = !Config.IsOptionLocked("help_with_testing");
        _checkBoxAutoApproveKeys.Checked = _config.AutoApproveKeys;
        _checkBoxAutoApproveKeys.Enabled = !Config.IsOptionLocked("auto_approve_keys");

        _textBoxSyncServer.Text = _config.SyncServer?.ToStringRfc() ?? "";
        _textBoxSyncServer.Enabled = !Config.IsOptionLocked("sync_server");
        _textBoxSyncUsername.Text = _config.SyncServerUsername;
        _textBoxSyncUsername.Enabled = !Config.IsOptionLocked("sync_server_user");
        _textBoxSyncPassword.Text = _config.SyncServerPassword;
        _textBoxSyncPassword.Enabled = !Config.IsOptionLocked("sync_server_pw");
        _textBoxSyncCryptoKey.Text = _config.SyncCryptoKey;
        _textBoxSyncCryptoKey.Enabled = !Config.IsOptionLocked("sync_crypto_key");

        if (_propertyEditorAdvanced.SelectedObject == null) _propertyEditorAdvanced.SelectedObject = _config;
        else _propertyEditorAdvanced.RefreshValues();
    }

    private void radioNetworkUse_SelectedIndexChanged(object? sender, EventArgs e)
    {
        _config.NetworkUse = _radioNetworkUse.SelectedIndex switch
        {
            1 => NetworkLevel.Minimal,
            2 => NetworkLevel.Offline,
            _ => NetworkLevel.Full
        };
        _propertyEditorAdvanced.RefreshValues();
    }

    private void timespanFreshness_ValueChanged(object? sender, EventArgs e)
    {
        _config.Freshness = _timespanFreshness.Value;
        _propertyEditorAdvanced.RefreshValues();
    }

    private void checkBoxHelpWithTesting_CheckedChanged(object? sender, EventArgs e)
    {
        _config.HelpWithTesting = _checkBoxHelpWithTesting.Checked == true;
        _propertyEditorAdvanced.RefreshValues();
    }
    #endregion

    #region Storage
    private readonly ObservableCollection<string> _implDirs = [];
    private readonly List<string> _lockedImplDirs;
    private GridView _gridImplDirs = default!;
    private Button _buttonGoToImplDir = default!, _buttonRemoveImplDir = default!;

    private Control BuildStorageTab()
    {
        _gridImplDirs = ListGrid(_implDirs);
        _gridImplDirs.SelectionChanged += gridImplDirs_SelectionChanged;
        _buttonGoToImplDir = Button(Resources.GoTo, buttonGoToImplDir_Click, enabled: false);
        _buttonRemoveImplDir = Button(Resources.Remove, buttonRemoveImplDir_Click, enabled: false);

        return new GroupBox
        {
            Text = Resources.ImplDirs,
            Padding = 6,
            Content = ListWithButtons(_gridImplDirs, Button(Resources.Add, buttonAddImplDir_Click), _buttonRemoveImplDir, _buttonGoToImplDir)
        };
    }

    private void SaveImplementationDirs()
    {
        var userImplDirs = _implDirs.Except(_lockedImplDirs);
        ImplementationStores.SetUserDirectories(userImplDirs);
    }

    private void gridImplDirs_SelectionChanged(object? sender, EventArgs e)
    {
        var selected = _gridImplDirs.SelectedItems.Cast<string>().ToList();
        _buttonGoToImplDir.Enabled = (selected.Count == 1);
        _buttonRemoveImplDir.Enabled = (selected.Count >= 1) && !selected.ContainsAny(_lockedImplDirs);
    }

    private void buttonGoToImplDir_Click(object? sender, EventArgs e)
    {
        if (_gridImplDirs.SelectedItem is string path) CommandUtils.Open(this, path);
    }

    private void buttonAddImplDir_Click(object? sender, EventArgs e)
    {
        using var dialog = new SelectFolderDialog {Title = Resources.SelectImplDir};
        if (dialog.ShowDialog(this) != DialogResult.Ok) return;

        _implDirs.Insert(Math.Min(1, _implDirs.Count), dialog.Directory);
    }

    private void buttonRemoveImplDir_Click(object? sender, EventArgs e)
    {
        var toRemove = _gridImplDirs.SelectedItems.Cast<string>().ToList();

        if (Msg.YesNo(this, string.Format(Commands.Properties.Resources.RemoveSelectedEntries, toRemove.Count), MsgSeverity.Warn))
        {
            foreach (string store in toRemove)
                _implDirs.Remove(store);
        }
    }
    #endregion

    #region Catalog
    private readonly ObservableCollection<FeedUri> _catalogSources = [];
    private GridView _gridCatalogSources = default!;
    private Button _buttonGoToCatalogSource = default!, _buttonRemoveCatalogSource = default!;

    private Control BuildCatalogTab()
    {
        _gridCatalogSources = ListGrid(_catalogSources);
        _gridCatalogSources.SelectionChanged += gridCatalogSources_SelectionChanged;
        _buttonGoToCatalogSource = Button(Resources.GoTo, buttonGoToCatalogSource_Click, enabled: false);
        _buttonRemoveCatalogSource = Button(Resources.Remove, buttonRemoveCatalogSource_Click, enabled: false);

        return new GroupBox
        {
            Text = Resources.CatalogSources,
            Padding = 6,
            Content = ListWithButtons(_gridCatalogSources,
                Button(Resources.Add, buttonAddCatalogSource_Click), _buttonRemoveCatalogSource, _buttonGoToCatalogSource,
                Button(Resources.Reset, buttonResetCatalogSources_Click))
        };
    }

    private void LoadCatalogSources()
    {
        _catalogSources.Clear();
        foreach (var source in CatalogManager.GetSources(preferMachineWide: false))
            _catalogSources.Add(source);
    }

    private void SaveCatalogSources() => CatalogManager.SetSources(_catalogSources);

    private void gridCatalogSources_SelectionChanged(object? sender, EventArgs e)
    {
        int count = _gridCatalogSources.SelectedItems.Count();
        _buttonGoToCatalogSource.Enabled = (count == 1);
        _buttonRemoveCatalogSource.Enabled = (count >= 1);
    }

    private void buttonResetCatalogSources_Click(object? sender, EventArgs e)
    {
        if (Msg.YesNo(this, Commands.Properties.Resources.ResetList, MsgSeverity.Warn))
        {
            _catalogSources.Clear();
            _catalogSources.Add(CatalogManager.DefaultSource);
        }
    }

    private void buttonGoToCatalogSource_Click(object? sender, EventArgs e)
    {
        if (_gridCatalogSources.SelectedItem is FeedUri uri) CommandUtils.Open(this, uri.ToStringRfc());
    }

    private void buttonAddCatalogSource_Click(object? sender, EventArgs e)
    {
        string? input = InputBox.Show(this, Resources.CatalogSources, Commands.Properties.Resources.EnterCatalogUrl);
        if (!string.IsNullOrEmpty(input)) AddCatalogSource(input);
    }

    private void AddCatalogSource(string input)
    {
        try
        {
            var uri = new FeedUri(input);
            if (_catalogSources.Contains(uri)) return;

            using (var handler = new EtoDialogTaskHandler(this))
            {
                var services = new ServiceProvider(handler);
                services.CatalogManager.DownloadCatalog(uri);
            }
            _catalogSources.Add(uri);

            // Trusted keys may have changed as a side-effect
            LoadTrust();
        }
        #region Error handling
        catch (OperationCanceledException)
        {}
        catch (Exception ex) when (ex is ArgumentException or UriFormatException or WebException or SignatureException or IOException or NotSupportedException or InvalidDataException)
        {
            Msg.Inform(this, ex.Message, MsgSeverity.Error);
        }
        #endregion
    }

    private void buttonRemoveCatalogSource_Click(object? sender, EventArgs e)
    {
        var toRemove = _gridCatalogSources.SelectedItems.Cast<FeedUri>().ToList();

        if (Msg.YesNo(this, string.Format(Commands.Properties.Resources.RemoveSelectedEntries, toRemove.Count), MsgSeverity.Warn))
        {
            foreach (var item in toRemove)
                _catalogSources.Remove(item);
        }
    }
    #endregion

    #region Trust
    private readonly FilteredTreeView<TrustNode> _treeViewTrustedKeys = new() {CheckBoxes = true};
    private readonly CheckBox _checkBoxAutoApproveKeys = new() {Text = Resources.AutoApproveKeys};
    private Button _buttonRemoveTrustedKey = default!;

    private Control BuildTrustTab()
    {
        _treeViewTrustedKeys.CheckedEntriesChanged += treeViewTrustedKeys_CheckedEntriesChanged;
        _checkBoxAutoApproveKeys.CheckedChanged += checkBoxAutoApproveKeys_CheckedChanged;
        _buttonRemoveTrustedKey = Button(Resources.Remove, buttonRemoveTrustedKey_Click, enabled: false);

        return new StackLayout
        {
            Orientation = Orientation.Vertical,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Spacing = 8,
            Items =
            {
                new StackLayoutItem(new GroupBox
                {
                    Text = Resources.TrustedKeys,
                    Padding = 6,
                    Content = new StackLayout
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalContentAlignment = HorizontalAlignment.Stretch,
                        Spacing = 6,
                        Items =
                        {
                            new StackLayoutItem(_treeViewTrustedKeys, expand: true),
                            new StackLayoutItem(_buttonRemoveTrustedKey, HorizontalAlignment.Right)
                        }
                    }
                }, expand: true),
                _checkBoxAutoApproveKeys
            }
        };
    }

    private void LoadTrust()
        => _treeViewTrustedKeys.Nodes = TrustDB.LoadSafe().ToNodes();

    private void SaveTrust()
        => _treeViewTrustedKeys.Nodes!.ToTrustDB().Save();

    private void treeViewTrustedKeys_CheckedEntriesChanged(object? sender, EventArgs e)
        => _buttonRemoveTrustedKey.Enabled = (_treeViewTrustedKeys.CheckedEntries.Count != 0);

    private void buttonRemoveTrustedKey_Click(object? sender, EventArgs e)
    {
        var checkedNodes = _treeViewTrustedKeys.CheckedEntries.ToList();

        if (Msg.YesNo(this, string.Format(Commands.Properties.Resources.RemoveCheckedKeys, checkedNodes.Count), MsgSeverity.Warn))
        {
            foreach (var node in checkedNodes)
                _treeViewTrustedKeys.Nodes!.Remove(node);
        }
    }

    private void checkBoxAutoApproveKeys_CheckedChanged(object? sender, EventArgs e)
    {
        _config.AutoApproveKeys = _checkBoxAutoApproveKeys.Checked == true;
        _propertyEditorAdvanced.RefreshValues();
    }
    #endregion

    #region Sync
    private readonly TextBox _textBoxSyncServer = new();
    private readonly TextBox _textBoxSyncUsername = new();
    private readonly PasswordBox _textBoxSyncPassword = new();
    private readonly PasswordBox _textBoxSyncCryptoKey = new();

    private Control BuildSyncTab()
    {
        _textBoxSyncServer.TextChanged += textBoxSyncServer_TextChanged;
        _textBoxSyncUsername.TextChanged += textBoxSyncUsername_TextChanged;
        _textBoxSyncPassword.TextChanged += textBoxSyncPassword_TextChanged;
        _textBoxSyncCryptoKey.TextChanged += textBoxSyncCryptoKey_TextChanged;

        var linkSyncAccount = new LinkButton {Text = Resources.ManageAccount};
        linkSyncAccount.Click += linkSyncAccount_Click;

        static Label Label(string text) => new() {Text = text, VerticalAlignment = VerticalAlignment.Center};

        return new TableLayout
        {
            Spacing = new(6, 6),
            Rows =
            {
                new TableRow(Label(Resources.SyncServer), new TableCell(_textBoxSyncServer, scaleWidth: true), linkSyncAccount),
                new TableRow(Label(Resources.SyncUsername), _textBoxSyncUsername),
                new TableRow(Label(Resources.SyncPassword), _textBoxSyncPassword),
                new TableRow(Label(Resources.SyncCryptoKey), _textBoxSyncCryptoKey, new StackLayout {Orientation = Orientation.Horizontal, Items = {Button("?", buttonSyncCryptoKey_Click)}}),
                new TableRow {ScaleHeight = true}
            }
        };
    }

    private void linkSyncAccount_Click(object? sender, EventArgs e)
    {
        string syncServer = _textBoxSyncServer.Text;
        if (!syncServer.EndsWith("/")) syncServer += "/"; // Ensure the server URI references a directory
        CommandUtils.Open(this, syncServer + "account");
    }

    private void buttonSyncCryptoKey_Click(object? sender, EventArgs e)
        => Msg.Inform(this, Commands.Properties.Resources.SyncCryptoKeyDescription, MsgSeverity.Info);

    private void textBoxSyncServer_TextChanged(object? sender, EventArgs e)
    {
        try
        {
            _config.SyncServer = new(_textBoxSyncServer.Text);
            _textBoxSyncServer.TextColor = SystemColors.ControlText;
        }
        catch (UriFormatException)
        {
            _config.SyncServer = new(Config.DefaultSyncServer);
            _textBoxSyncServer.TextColor = Colors.Red;
        }
        _propertyEditorAdvanced.RefreshValues();
    }

    private void textBoxSyncUsername_TextChanged(object? sender, EventArgs e)
    {
        _config.SyncServerUsername = _textBoxSyncUsername.Text;
        _propertyEditorAdvanced.RefreshValues();
    }

    private void textBoxSyncPassword_TextChanged(object? sender, EventArgs e)
    {
        _config.SyncServerPassword = _textBoxSyncPassword.Text;
        _propertyEditorAdvanced.RefreshValues();
    }

    private void textBoxSyncCryptoKey_TextChanged(object? sender, EventArgs e)
    {
        _config.SyncCryptoKey = _textBoxSyncCryptoKey.Text;
        _propertyEditorAdvanced.RefreshValues();
    }
    #endregion

    #region Advanced
    private readonly PropertyEditor _propertyEditorAdvanced = new() {Visible = false};

    private Control BuildAdvancedTab()
    {
        _propertyEditorAdvanced.PropertyValueChanged += propertyEditorAdvanced_PropertyValueChanged;

        var labelAdvancedWarning = new Label {Text = Resources.AdvancedWarning, Wrap = WrapMode.Word}.ReflowOnResize();
        var buttonAdvancedShow = new Button {Text = Resources.ShowAdvanced};
        buttonAdvancedShow.Click += delegate
        {
            labelAdvancedWarning.Visible = buttonAdvancedShow.Visible = false;
            _propertyEditorAdvanced.Visible = true;
        };

        return new TableLayout
        {
            Spacing = new(0, 8),
            Rows =
            {
                labelAdvancedWarning,
                new TableRow(new TableLayout(new TableRow(buttonAdvancedShow, null))),
                new TableRow(_propertyEditorAdvanced) {ScaleHeight = true}
            }
        };
    }

    private void propertyEditorAdvanced_PropertyValueChanged(object? sender, EventArgs e) => ConfigToControls();
    #endregion

    #region Language
    private sealed record LanguageWrapper(CultureInfo Culture)
    {
        public override string ToString() => Culture.DisplayName;
    }

    private readonly DropDown _comboBoxLanguage = new();

    private Control BuildLanguageTab()
        => new TableLayout
        {
            Spacing = new(0, 8),
            Rows =
            {
                _comboBoxLanguage,
                new Label {Text = Resources.LanguageInfo, Wrap = WrapMode.Word}.ReflowOnResize(),
                new TableRow {ScaleHeight = true}
            }
        };

    private void LoadLanguages()
    {
        var items = new List<object> {Commands.Properties.Resources.UseSystemLanguage};
        items.AddRange(Languages.AllKnown.Select(x => new LanguageWrapper(x)));
        _comboBoxLanguage.DataStore = items;

        _comboBoxLanguage.SelectedIndex = ProgramUtils.UILanguage is {} language
            ? Math.Max(0, items.IndexOf(new LanguageWrapper(language)))
            : 0;
    }

    private void SaveLanguage()
    {
        if (!WindowsUtils.IsWindows) return;

        var lang = _comboBoxLanguage.SelectedValue as LanguageWrapper;
        ProgramUtils.UILanguage = lang?.Culture;
    }
    #endregion
}
