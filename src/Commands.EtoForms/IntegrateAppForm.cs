// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using ZeroInstall.DesktopIntegration;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.DesktopIntegration.ViewModel;
using ZeroInstall.Model.Capabilities;
using Binding = Eto.Forms.Binding;
using Command = ZeroInstall.Model.Command;
using ContextMenu = ZeroInstall.DesktopIntegration.AccessPoints.ContextMenu;

namespace ZeroInstall.Commands.EtoForms;

/// <summary>
/// A dialog form used for displaying and applying desktop integration options for applications.
/// </summary>
public sealed class IntegrateAppForm : Dialog<bool>
{
    /// <summary>
    /// A View-Model for modifying the current desktop integration state.
    /// </summary>
    private readonly IntegrationState _state;

    private readonly CheckBox _checkBoxAutoUpdate = new() {Text = Resources.IncludeInUpdateAll, ToolTip = Resources.IncludeInUpdateAllToolTip};
    private readonly CheckBox _checkBoxCapabilities = new() {Text = Resources.RegisterCapabilities, ToolTip = Resources.RegisterCapabilitiesToolTip};
    private readonly TabControl _tabControl = new();
    private readonly TableLayout _tableBasic = new() {Spacing = new(12, 6)};
    private readonly TextArea _textBoxCommandLine = new() {ReadOnly = true, Wrap = true, Height = 50};
    private readonly Button _buttonAdvancedMode = new() {Text = Resources.AdvancedSettings};
    private readonly Button _buttonBasicMode = new() {Text = Resources.BasicSettings};
    private readonly Panel _panelMode = new();
    private readonly Control _panelBasic, _panelAdvanced;

    /// <summary>The commands available for <see cref="CommandAccessPoint"/>s.</summary>
    private readonly string[] _commands;

    /// <summary>
    /// Creates an instance of the form.
    /// </summary>
    /// <param name="state">A View-Model for modifying the current desktop integration state.</param>
    public IntegrateAppForm(IntegrationState state)
    {
        _state = state ?? throw new ArgumentNullException(nameof(state));

        Title = string.Format(Commands.Properties.Resources.Integrate, _state.AppEntry.Name);
        Padding = 10;
        Resizable = true;
        MinimumSize = new(520, 380);
        this.SetClientSize(new(640, 440));
        this.PreventPinningIfNotIntegrated();

        _checkBoxAutoUpdate.Checked = _state.AppEntry.AutoUpdate;
        _checkBoxCapabilities.Visible = (_state.AppEntry.CapabilityLists.Count != 0);
        _checkBoxCapabilities.Checked = _state.CapabilityRegistration;

        _commands = _state.Feed.EntryPoints
                          .Select(entryPoint => entryPoint.Command)
                          .Append(Command.NameRun)
                          .WhereNotNull().Distinct()
                          .ToArray();

        SetupCategory(CapabilityRegistration.TagName, _checkBoxCapabilities);
        SetupCommandAccessPoints();
        SetupDefaultAccessPoints();
        _standardCategories = CategoryIntegrationManager.StandardCategories.Where(_categories.ContainsKey).ToList();

        _tableBasic.Rows.Add(new TableRow {ScaleHeight = true});
        _panelBasic = new StackLayout
        {
            Orientation = Orientation.Vertical,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Spacing = 8,
            Items =
            {
                new StackLayoutItem(_tableBasic, expand: true),
                new GroupBox {Text = Resources.EquivalentCommandLine, Padding = 6, Content = _textBoxCommandLine},
                new StackLayoutItem(_buttonAdvancedMode, HorizontalAlignment.Left)
            }
        };
        _panelAdvanced = new StackLayout
        {
            Orientation = Orientation.Vertical,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Spacing = 8,
            Items =
            {
                new StackLayout {Orientation = Orientation.Horizontal, Spacing = 12, Items = {_checkBoxAutoUpdate, _checkBoxCapabilities}},
                new StackLayoutItem(_tabControl, expand: true),
                new StackLayoutItem(_buttonBasicMode, HorizontalAlignment.Left)
            }
        };
        Content = _panelMode;

        _buttonAdvancedMode.Click += buttonAdvancedMode_Click;
        _buttonBasicMode.Click += buttonBasicMode_Click;

        var buttonOK = new Button {Text = NanoByte.Common.Properties.Resources.OK};
        buttonOK.Click += buttonOK_Click;
        var buttonCancel = new Button {Text = Commands.Properties.Resources.Cancel};
        buttonCancel.Click += delegate { Close(false); };
        PositiveButtons.Add(buttonOK);
        NegativeButtons.Add(buttonCancel);
        DefaultButton = buttonOK;
        AbortButton = buttonCancel;

        ShowBasicMode();
        _initialCategories.Add(_categories.Where(x => x.Value()).Select(x => x.Key));
        UpdateCommandLine(null, EventArgs.Empty);
    }

    #region Event handlers
    /// <summary>
    /// Integrates all <see cref="Capability"/>s chosen by the user.
    /// </summary>
    private void buttonOK_Click(object? sender, EventArgs e)
    {
        if (_panelMode.Content == _panelBasic)
        {
            // Apply changes made in "Simple View"
            _switchToAdvancedMode?.Invoke();
        }

        _state.CapabilityRegistration = _checkBoxCapabilities.Checked == true;
        _state.AppEntry.AutoUpdate = _checkBoxAutoUpdate.Checked == true;

        Close(true);
    }

    private void buttonHelpCommandAccessPoint_Click(object? sender, EventArgs e)
        => Msg.Inform(this, Commands.Properties.Resources.DataGridCommandAccessPointHelp, MsgSeverity.Info);

    private void buttonHelpDefaultAccessPoint_Click(object? sender, EventArgs e)
        => Msg.Inform(this, Commands.Properties.Resources.DataGridDefaultAccessPointHelp, MsgSeverity.Info);
    #endregion

    //--------------------//

    #region Helpers
    /// <summary>
    /// Adds a checkbox with a description to the basic mode view.
    /// </summary>
    private CheckBox AddBasicCheckBox(string text, string description)
    {
        var checkBox = new CheckBox {Text = text};
        _tableBasic.Rows.Add(new TableRow(checkBox, new TableCell(new Label {Text = description, Wrap = WrapMode.Word, VerticalAlignment = VerticalAlignment.Center}.ReflowOnResize(), scaleWidth: true)));
        return checkBox;
    }

    private static GridColumn TextColumn<T>(string headerText, Func<T, string?> getValue, Action<T, string?>? setValue = null, bool expand = false)
        => new()
        {
            HeaderText = headerText,
            Expand = expand,
            Editable = setValue != null,
            DataCell = new TextBoxCell {Binding = Binding.Delegate(getValue, setValue)}
        };

    private static Button HelpButton(EventHandler<EventArgs> onClick)
    {
        var button = new Button {Text = "?", Width = 30};
        button.Click += onClick;
        return button;
    }

    /// <summary>
    /// Forces a <see cref="GridView"/> to reload its data, e.g. after items were added or removed.
    /// </summary>
    private static void Reload<T>(GridView grid, IEnumerable<T> dataStore)
    {
        grid.DataStore = null;
        grid.DataStore = dataStore.Cast<object>().ToList();
    }
    #endregion

    #region Commmand access points
    /// <summary>
    /// Sets up the UI elements for configuring <see cref="CommandAccessPoint"/>s.
    /// </summary>
    /// <remarks>Users create <see cref="CommandAccessPoint"/>s themselves based on <see cref="EntryPoint"/>s.</remarks>
    private void SetupCommandAccessPoints()
    {
        SetupCommandAccessPoint(MenuEntry.TagName, Resources.StartMenu, Resources.StartMenuDescription, _state.MenuEntries, () => Suggest.MenuEntries(_state.Feed),
            TextColumn<MenuEntry>(Resources.ColumnCategory, x => x.Category, (x, value) => x.Category = value));
        SetupCommandAccessPoint(DesktopIcon.TagName, Resources.Desktop, Resources.DesktopDescription, _state.DesktopIcons, () => Suggest.DesktopIcons(_state.Feed));
        SetupCommandAccessPoint(SendTo.TagName, Resources.SendTo, Resources.SendToDescription, _state.SendTo, () => Suggest.SendTo(_state.Feed));
        SetupCommandAccessPoint(AppAlias.TagName, Resources.Aliases, Resources.AliasesDescription, _state.Aliases, () => Suggest.Aliases(_state.Feed));
        SetupCommandAccessPoint(AutoStart.TagName, Resources.AutoStart, Resources.AutoStartDescription, _state.AutoStarts, () => Suggest.AutoStart(_state.Feed));
    }

    /// <summary>
    /// Sets up the UI elements for a specific category of <see cref="CommandAccessPoint"/>s.
    /// </summary>
    /// <typeparam name="T">The specific kind of <see cref="CommandAccessPoint"/> to handle.</typeparam>
    /// <param name="category">The name of the access point category.</param>
    /// <param name="title">The title for the tab and simple mode checkbox.</param>
    /// <param name="description">A simple mode description for this type of <see cref="CommandAccessPoint"/>.</param>
    /// <param name="accessPoints">A list of applied <see cref="CommandAccessPoint"/>.</param>
    /// <param name="getSuggestions">Retrieves a list of default <see cref="CommandAccessPoint"/> suggested by the system.</param>
    /// <param name="additionalColumns">Additional columns to display for this type of <see cref="CommandAccessPoint"/>.</param>
    private void SetupCommandAccessPoint<T>(string category, string title, string description, BindingList<T> accessPoints, Func<IEnumerable<T>> getSuggestions, params GridColumn[] additionalColumns)
        where T : CommandAccessPoint, new()
    {
        var grid = new GridView {GridLines = GridLines.Both, AllowMultipleSelection = true, BackgroundColor = SystemColors.ControlBackground};
        grid.Columns.Add(TextColumn<T>(Resources.ColumnName,x => x.Name, (x, value) => x.Name = value, expand: true));
        foreach (var column in additionalColumns) grid.Columns.Add(column);
        grid.Columns.Add(new GridColumn
        {
            HeaderText = Resources.ColumnCommand,
            Editable = true,
            DataCell = new ComboBoxCell
            {
                DataStore = _commands,
                Binding = Binding.Delegate<T, object?>(x => x.Command, (x, value) => x.Command = value as string)
            }
        });
        Reload(grid, accessPoints);

        var buttonAdd = new Button {Text = Resources.Add};
        buttonAdd.Click += delegate
        {
            accessPoints.Add(new T {Command = Command.NameRun});
            Reload(grid, accessPoints);
            grid.SelectedRow = accessPoints.Count - 1;
        };
        var buttonRemove = new Button {Text = Resources.Remove, Enabled = false};
        buttonRemove.Click += delegate
        {
            foreach (var accessPoint in grid.SelectedItems.OfType<T>().ToList())
                accessPoints.Remove(accessPoint);
            Reload(grid, accessPoints);
        };
        grid.SelectionChanged += delegate { buttonRemove.Enabled = grid.SelectedItems.Any(); };

        _tabControl.Pages.Add(new TabPage
        {
            Text = title,
            Padding = 6,
            Content = new StackLayout
            {
                Orientation = Orientation.Vertical,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Spacing = 6,
                Items =
                {
                    new StackLayoutItem(grid, expand: true),
                    new StackLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 6,
                        Items = {HelpButton(buttonHelpCommandAccessPoint_Click), new StackLayoutItem(null, expand: true), buttonAdd, buttonRemove}
                    }
                }
            }
        });

        var suggestions = getSuggestions().ToList();
        if (suggestions is []) return;

        var checkBoxSimple = AddBasicCheckBox(title, description);
        SetupCategory(category, checkBoxSimple);
        _switchToBasicMode += () => checkBoxSimple.Checked = accessPoints.Any();
        _switchToAdvancedMode += () =>
        {
            if (checkBoxSimple.Checked == true)
            {
                if (accessPoints.Count == 0)
                {
                    foreach (var entry in suggestions)
                        accessPoints.Add(entry);
                }
            }
            else accessPoints.Clear();

            Reload(grid, accessPoints);
        };
    }
    #endregion

    #region Default access points
    private readonly List<CheckBox> _defaultAccessPointCheckBoxes = [];

    /// <summary>
    /// Sets up the UI elements for configuring <see cref="DefaultAccessPoint"/>s.
    /// </summary>
    /// <remarks>Users enable or disable predefined <see cref="DefaultAccessPoint"/>s based on <see cref="DefaultCapability"/>s.</remarks>
    private void SetupDefaultAccessPoints()
    {
        SetupDefaultAccessPoint(Resources.FileTypes, Resources.FileTypesDescription, _state.FileTypes, Resources.ColumnDefault,
            TextColumn<FileTypeModel>(Resources.ColumnDescription, x => x.Description),
            TextColumn<FileTypeModel>(Resources.ColumnExtensions, x => x.Extensions, expand: true));
        SetupDefaultAccessPoint(Resources.UrlProtocols, Resources.UrlProtocolsDescription, _state.UrlProtocols, Resources.ColumnDefault,
            TextColumn<UrlProtocolModel>(Resources.ColumnDescription, x => x.Description),
            TextColumn<UrlProtocolModel>(Resources.ColumnProtocols, x => x.KnownPrefixes, expand: true));
        SetupDefaultAccessPoint(Resources.AutoPlay, Resources.AutoPlayDescription, _state.AutoPlay, Resources.ColumnDefault,
            TextColumn<AutoPlayModel>(Resources.ColumnDescription, x => x.Description),
            TextColumn<AutoPlayModel>(Resources.ColumnEvents, x => x.Events, expand: true));
        SetupDefaultAccessPoint(Resources.ContextMenu, Resources.ContextMenuDescription, _state.ContextMenu, Resources.ColumnEnabled,
            TextColumn<ContextMenuModel>(Resources.ColumnEntryName, x => x.Name, expand: true));
        SetupDefaultAccessPoint(Resources.DefaultPrograms, Resources.DefaultProgramsDescription, _state.DefaultProgram, Resources.ColumnDefault,
            TextColumn<DefaultProgramModel>(Resources.ColumnDescription, x => x.Description),
            TextColumn<DefaultProgramModel>(Resources.ColumnService, x => x.Service, expand: true));
        SetupCategory(DefaultAccessPoint.TagName, _defaultAccessPointCheckBoxes.ToArray());
    }

    /// <summary>
    /// Sets up the UI elements for a specific kind of <see cref="DefaultAccessPoint"/>s.
    /// </summary>
    /// <typeparam name="T">The specific kind of <see cref="DefaultAccessPoint"/> to handle.</typeparam>
    /// <param name="title">The title for the tab and simple mode checkbox.</param>
    /// <param name="description">A simple mode description for this type of <see cref="DefaultAccessPoint"/>.</param>
    /// <param name="model">A model representing the underlying <see cref="DefaultCapability"/>s and their selection states.</param>
    /// <param name="useHeaderText">The header text for the column that controls <see cref="CapabilityModel.Use"/>.</param>
    /// <param name="columns">The columns to display in addition to <see cref="CapabilityModel.Use"/>.</param>
    private void SetupDefaultAccessPoint<T>(string title, string description, BindingList<T> model, string useHeaderText, params GridColumn[] columns)
        where T : CapabilityModel
    {
        // Hide empty categories
        if (!model.Any()) return;

        var grid = new GridView {GridLines = GridLines.Both, BackgroundColor = SystemColors.ControlBackground};
        grid.Columns.Add(new GridColumn
        {
            HeaderText = useHeaderText,
            Editable = true,
            DataCell = new CheckBoxCell {Binding = Binding.Delegate<T, bool?>(x => x.Use, (x, value) => x.Use = value == true)}
        });
        foreach (var column in columns) grid.Columns.Add(column);
        Reload(grid, model);

        var checkBoxSelectAll = new CheckBox {Text = Resources.SelectAll};
        bool suppressEvent = false;
        checkBoxSelectAll.CheckedChanged += delegate
        {
            if (suppressEvent) return;
            model.SetAllUse(checkBoxSelectAll.Checked == true);
            Reload(grid, model);
        };

        _tabControl.Pages.Add(new TabPage
        {
            Text = title,
            Padding = 6,
            Content = new StackLayout
            {
                Orientation = Orientation.Vertical,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Spacing = 6,
                Items =
                {
                    new StackLayoutItem(grid, expand: true),
                    new StackLayout
                    {
                        Orientation = Orientation.Horizontal,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        Spacing = 6,
                        Items = {HelpButton(buttonHelpDefaultAccessPoint_Click), checkBoxSelectAll}
                    }
                }
            }
        });

        var checkBoxSimple = AddBasicCheckBox(title, description);
        _defaultAccessPointCheckBoxes.Add(checkBoxSimple);

        _switchToBasicMode += () => checkBoxSimple.Checked = model.Any(element => element.Use);
        _switchToAdvancedMode += () =>
        {
            if (checkBoxSimple.Checked != true) model.SetAllUse(false);
            else if (!model.Any(element => element.Use)) model.SetAllUse(true);
            Reload(grid, model);

            suppressEvent = true;
            checkBoxSelectAll.Checked = model.All(element => element.Use);
            suppressEvent = false;
        };
    }
    #endregion

    #region Basic mode
    /// <summary>
    /// Transfers data from models to checkboxes.
    /// </summary>
    private Action? _switchToBasicMode;

    private void buttonBasicMode_Click(object? sender, EventArgs e) => ShowBasicMode();

    /// <summary>
    /// Displays the basic configuration view.
    /// </summary>
    private void ShowBasicMode()
    {
        _switchToBasicMode?.Invoke();
        _panelMode.Content = _panelBasic;
    }

    /// <summary>
    /// A map from category names to functions indicating whether that category is currently selected.
    /// </summary>
    private readonly Dictionary<string, Func<bool>> _categories = [];

    /// <summary>
    /// The <see cref="CategoryIntegrationManager.StandardCategories"/> that are also in <see cref="_categories"/>.
    /// </summary>
    private readonly IReadOnlyCollection<string> _standardCategories;

    /// <summary>
    /// The categories that where selected when the window was loaded.
    /// </summary>
    private readonly List<string> _initialCategories = [];

    /// <summary>
    /// Registers the association between a category and one or more checkboxes.
    /// </summary>
    private void SetupCategory(string name, params CheckBox[] checkBoxes)
    {
        foreach (var checkBox in checkBoxes)
            checkBox.CheckedChanged += UpdateCommandLine;

        if (checkBoxes.Length != 0)
            _categories.Add(name, () => checkBoxes.All(x => x.Checked == true));
    }

    /// <summary>
    /// Updates the equivalent command-line for the currently selected categories.
    /// </summary>
    private void UpdateCommandLine(object? sender, EventArgs e)
    {
        // Skip updates during initial setup
        if (_standardCategories == null) return;

        var commandLine = new List<string>();

        if (_state.Feed.Uri is {} uri)
            commandLine.Add(["0install", "integrate", uri.ToStringRfc()]);

        if (_categories.All(x => x.Value()))
            commandLine.Add("--add-all");
        else if (_categories.All(x => !x.Value()))
            commandLine.Add("--remove-all");
        else
        {
            var selectedCategories = _categories.Where(x => x.Value()).Select(x => x.Key).ToList();
            if (_standardCategories.All(selectedCategories.Contains))
            {
                commandLine.Add("--add-standard");
                selectedCategories.Remove(_standardCategories);
            }
            commandLine.Add(selectedCategories.Select(x => "--add=" + x));

            var notSelectedCategories = _categories.Where(x => !x.Value()).Select(x => x.Key);
            commandLine.Add(notSelectedCategories.Where(_initialCategories.Contains).Select(x => "--remove=" + x));
        }

        _textBoxCommandLine.Text = commandLine.JoinEscapeArguments();
    }
    #endregion

    #region Advanced mode
    /// <summary>
    /// Transfers data from checkboxes to models.
    /// </summary>
    /// <remarks>Must be called before form close.</remarks>
    private Action? _switchToAdvancedMode;

    private void buttonAdvancedMode_Click(object? sender, EventArgs e)
    {
        _switchToAdvancedMode?.Invoke();
        _panelMode.Content = _panelAdvanced;
    }
    #endregion
}
