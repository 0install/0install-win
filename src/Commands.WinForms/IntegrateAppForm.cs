// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using NanoByte.Common.Native;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.DesktopIntegration.ViewModel;
using ZeroInstall.Model.Capabilities;

namespace ZeroInstall.Commands.WinForms;

/// <summary>
/// A dialog form used for displaying and applying desktop integration options for applications.
/// </summary>
public sealed partial class IntegrateAppForm : OKCancelDialog
{
    /// <summary>
    /// A View-Model for modifying the current desktop integration state.
    /// </summary>
    private readonly IntegrationState _state;

    /// <summary>
    /// Creates an instance of the form.
    /// </summary>
    /// <param name="state">A View-Model for modifying the current desktop integration state.</param>
    public IntegrateAppForm(IntegrationState state)
    {
        InitializeComponent();
        Font = DefaultFonts.Modern;
        this.PreventPinningIfNotIntegrated();

        _state = state ?? throw new ArgumentNullException(nameof(state));
        Text = string.Format(Resources.Integrate, _state.AppEntry.Name);

        checkBoxAutoUpdate.Checked = _state.AppEntry.AutoUpdate;
        checkBoxCapabilities.Visible = (_state.AppEntry.CapabilityLists.Count != 0);
        checkBoxCapabilities.Checked = _state.CapabilityRegistration;

        SetupCategory(CapabilityRegistration.TagName, checkBoxCapabilities);
        SetupCommandAccessPoints();
        SetupDefaultAccessPoints();
        _standardCategories = CategoryIntegrationManager.StandardCategories.Where(_categories.ContainsKey).ToList();

        _switchToBasicMode?.Invoke();
        _initialCategories.Add(_categories.Where(x => x.Value()).Select(x => x.Key));
    }

    #region Event handlers
    /// <summary>
    /// Integrates all <see cref="Capability"/>s chosen by the user.
    /// </summary>
    private void buttonOK_Click(object sender, EventArgs e)
    {
        if (buttonAdvancedMode.Visible)
        {
            // Apply changes made in "Simple View"
            _switchToAdvancedMode?.Invoke();
        }

        _state.CapabilityRegistration = checkBoxCapabilities.Checked;
        _state.AppEntry.AutoUpdate = checkBoxAutoUpdate.Checked;
    }

    private void buttonHelpCommandAccessPoint_Click(object sender, EventArgs e)
        => Msg.Inform(this, Resources.DataGridCommandAccessPointHelp, MsgSeverity.Info);

    private void buttonHelpDefaultAccessPoint_Click(object sender, EventArgs e)
        => Msg.Inform(this, Resources.DataGridDefaultAccessPointHelp, MsgSeverity.Info);

    private void accessPointDataGrid_DataError(object sender, DataGridViewDataErrorEventArgs e)
    {
        labelLastDataError.Visible = true;
        labelLastDataError.Text = e.Exception.Message;
    }
    #endregion

    //--------------------//

    #region Commmand access points
    /// <summary>
    /// Sets up the UI elements for configuring <see cref="CommandAccessPoint"/>s.
    /// </summary>
    /// <remarks>Users create <see cref="CommandAccessPoint"/>s themselves based on <see cref="EntryPoint"/>s.</remarks>
    private void SetupCommandAccessPoints()
    {
        SetupCommandAccessPoint(MenuEntry.TagName, checkBoxStartMenuSimple, labelStartMenuSimple, _state.MenuEntries, () => Suggest.MenuEntries(_state.Feed));
        SetupCommandAccessPoint(DesktopIcon.TagName, checkBoxDesktopSimple, labelDesktopSimple, _state.DesktopIcons, () => Suggest.DesktopIcons(_state.Feed));
        SetupCommandAccessPoint(SendTo.TagName, checkBoxSendToSimple, labelSendToSimple, _state.SendTo, () => Suggest.SendTo(_state.Feed));
        SetupCommandAccessPoint(AppAlias.TagName, checkBoxAliasesSimple, labelAliasesSimple, _state.Aliases, () => Suggest.Aliases(_state.Feed));
        SetupCommandAccessPoint(AutoStart.TagName, checkBoxAutoStartSimple, labelAutoStartSimple, _state.AutoStarts, () => Suggest.AutoStart(_state.Feed));

        SetupCommandComboBoxes();
        ShowCommandAccessPoints();
    }

    /// <summary>
    /// Hooks up event handlers for a specific category of <see cref="CommandAccessPoint"/>s.
    /// </summary>
    /// <typeparam name="T">The specific kind of <see cref="CommandAccessPoint"/> to handle.</typeparam>
    /// <param name="category">The name of the access point category.</param>
    /// <param name="checkBoxSimple">The simple mode checkbox for this type of <see cref="CommandAccessPoint"/>.</param>
    /// <param name="labelSimple">A simple mode description for this type of <see cref="CommandAccessPoint"/>.</param>
    /// <param name="accessPoints">A list of applied <see cref="CommandAccessPoint"/>.</param>
    /// <param name="getSuggestions">Retrieves a list of default <see cref="CommandAccessPoint"/> suggested by the system.</param>
    private void SetupCommandAccessPoint<T>(string category, CheckBox checkBoxSimple, Label labelSimple, ICollection<T> accessPoints, Func<IEnumerable<T>> getSuggestions)
        where T : CommandAccessPoint
    {
        var suggestions = getSuggestions().ToList();
        if (suggestions is [])
        {
            labelSimple.Visible = checkBoxSimple.Visible = false;
            return;
        }

        SetupCategory(category, checkBoxSimple);
        _switchToBasicMode += () => checkBoxSimple.Checked = accessPoints.Any();
        _switchToAdvancedMode += () =>
        {
            if (checkBoxSimple.Checked)
            {
                if (accessPoints.Count == 0)
                {
                    foreach (var entry in suggestions)
                        accessPoints.Add(entry);
                }
            }
            else accessPoints.Clear();
        };
    }

    /// <summary>
    /// Fills the <see cref="DataGridViewComboBoxColumn"/>s with data from <see cref="Feed.EntryPoints"/> .
    /// </summary>
    private void SetupCommandComboBoxes()
    {
        var commands = _state.Feed.EntryPoints
                             .Select(entryPoint => entryPoint.Command)
                             .Append(Command.NameRun)
                             .WhereNotNull().Distinct()
                             .ToArray<object>();

        dataGridStartMenuColumnCommand.Items.AddRange(commands);
        dataGridDesktopColumnCommand.Items.AddRange(commands);
        dataGridSendToColumnCommand.Items.AddRange(commands);
        dataGridAliasesColumnCommand.Items.AddRange(commands);
        dataGridAutoStartColumnCommand.Items.AddRange(commands);
    }

    /// <summary>
    /// Apply data to DataGrids in bulk for better performance
    /// </summary>
    private void ShowCommandAccessPoints()
    {
        dataGridStartMenu.DataSource = _state.MenuEntries;
        dataGridDesktop.DataSource = _state.DesktopIcons;
        dataGridSendTo.DataSource = _state.SendTo;
        dataGridAliases.DataSource = _state.Aliases;
        dataGridAutoStart.DataSource = _state.AutoStarts;
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
        SetupDefaultAccessPoint(checkBoxFileTypesSimple, labelFileTypesSimple, checkBoxFileTypesAll, _state.FileTypes);
        SetupDefaultAccessPoint(checkBoxUrlProtocolsSimple, labelUrlProtocolsSimple, checkBoxUrlProtocolsAll, _state.UrlProtocols);
        SetupDefaultAccessPoint(checkBoxAutoPlaySimple, labelAutoPlaySimple, checkBoxAutoPlayAll, _state.AutoPlay);
        SetupDefaultAccessPoint(checkBoxContextMenuSimple, labelContextMenuSimple, checkBoxContextMenuAll, _state.ContextMenu);
        SetupDefaultAccessPoint(checkBoxDefaultProgramsSimple, labelDefaultProgramsSimple, checkBoxDefaultProgramsAll, _state.DefaultProgram);
        SetupCategory(DefaultAccessPoint.TagName, _defaultAccessPointCheckBoxes.ToArray());

        // File type associations cannot be set programmatically on Windows 8, so hide the option
        _switchToBasicMode += () =>
        {
            if (WindowsUtils.IsWindows8) labelFileTypesSimple.Visible = checkBoxFileTypesSimple.Visible = false;
        };

        SetDefaultAccessPoints();
    }

    /// <summary>
    /// Hooks up event handlers for <see cref="DefaultAccessPoint"/>s.
    /// </summary>
    /// <typeparam name="T">The specific kind of <see cref="DefaultAccessPoint"/> to handle.</typeparam>
    /// <param name="checkBoxSimple">The simple mode checkbox for this type of <see cref="CommandAccessPoint"/>.</param>
    /// <param name="labelSimple">A simple mode description for this type of <see cref="CommandAccessPoint"/>.</param>
    /// <param name="checkBoxSelectAll">The "select all" checkbox for this type of <see cref="CommandAccessPoint"/>.</param>
    /// <param name="model">A model representing the underlying <see cref="DefaultCapability"/>s and their selection states.</param>
    private void SetupDefaultAccessPoint<T>(CheckBox checkBoxSimple, Label labelSimple, CheckBox checkBoxSelectAll, BindingList<T> model)
        where T : CapabilityModel
    {
        if (!model.Any())
        {
            labelSimple.Visible = checkBoxSimple.Visible = false;
            return;
        }

        _defaultAccessPointCheckBoxes.Add(checkBoxSimple);

        _switchToBasicMode += () => checkBoxSimple.Checked = model.Any(element => element.Use);

        bool suppressEvent = false;
        checkBoxSelectAll.CheckedChanged += delegate { if (!suppressEvent) model.SetAllUse(checkBoxSelectAll.Checked); };
        _switchToAdvancedMode += () =>
        {
            if (!checkBoxSimple.Checked) model.SetAllUse(false);
            else if (!model.Any(element => element.Use)) model.SetAllUse(true);

            suppressEvent = true;
            checkBoxSelectAll.Checked = model.All(element => element.Use);
            suppressEvent = false;
        };
    }

    /// <summary>
    /// Set data in DataGrids in bulk for better performance (remove empty tabs).
    /// </summary>
    private void SetDefaultAccessPoints()
    {
        if (_state.FileTypes.Any()) dataGridFileTypes.DataSource = _state.FileTypes;
        else tabControl.TabPages.Remove(tabPageFileTypes);
        if (_state.UrlProtocols.Any()) dataGridUrlProtocols.DataSource = _state.UrlProtocols;
        else tabControl.TabPages.Remove(tabPageUrlProtocols);
        if (_state.AutoPlay.Any()) dataGridAutoPlay.DataSource = _state.AutoPlay;
        else tabControl.TabPages.Remove(tabPageAutoPlay);
        if (_state.ContextMenu.Any()) dataGridContextMenu.DataSource = _state.ContextMenu;
        else tabControl.TabPages.Remove(tabPageContextMenu);
        if (_state.DefaultProgram.Any()) dataGridDefaultPrograms.DataSource = _state.DefaultProgram;
        else tabControl.TabPages.Remove(tabPageDefaultPrograms);
    }
    #endregion

    #region Basic mode
    /// <summary>
    /// Displays the basic configuration view. Transfers data from models to checkboxes.
    /// </summary>
    /// <remarks>Must be called after form setup.</remarks>
    private Action? _switchToBasicMode;

    private void buttonBasicMode_Click(object sender, EventArgs e)
    {
        _switchToBasicMode?.Invoke();

        buttonAdvancedMode.Visible = panelBasic.Visible = groupBoxCommandLine.Visible = true;
        buttonBasicMode.Visible = checkBoxAutoUpdate.Visible = checkBoxCapabilities.Visible = tabControl.Visible = false;
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
            checkBox.CheckStateChanged += UpdateCommandLine;

        if (checkBoxes.Length != 0)
            _categories.Add(name, () => checkBoxes.All(x => x.Checked));
    }

    /// <summary>
    /// Updates the equivalent command-line for the currently selected categories.
    /// </summary>
    private void UpdateCommandLine(object sender, EventArgs e)
    {
        if (_state.Feed.Uri == null) return;

        var commandLine = new List<string> {"0install", "integrate", _state.Feed.Uri.ToStringRfc()};

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

        textBoxCommandLine.Text = commandLine.JoinEscapeArguments();
    }
    #endregion

    #region Advanced mode
    /// <summary>
    /// Displays the advanced configuration view. Transfers data from checkboxes to models.
    /// </summary>
    /// <remarks>Must be called before form close.</remarks>
    private Action? _switchToAdvancedMode;

    private void buttonAdvancedMode_Click(object sender, EventArgs e)
    {
        _switchToAdvancedMode?.Invoke();

        buttonBasicMode.Visible = checkBoxAutoUpdate.Visible = checkBoxCapabilities.Visible = tabControl.Visible = true;
        buttonAdvancedMode.Visible = panelBasic.Visible = groupBoxCommandLine.Visible = false;
    }
    #endregion
}
