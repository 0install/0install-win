// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using ZeroInstall.DesktopIntegration;

namespace ZeroInstall.Central.WinForms;

/// <summary>
/// Displays a list of <see cref="AppTile"/>s.
/// </summary>
public class AppTileList : UserControl
{
    #region Variables
    /// <summary>
    /// Allows the user to search/filter the <see cref="AppTile"/>s.
    /// </summary>
    public readonly HintTextBox TextSearch;

    /// <summary>Allows the user to filter the <see cref="AppTile"/>s by category.</summary>
    private readonly ComboBox _comboCategory;

    /// <summary>Contains <see cref="TextSearch"/> and <see cref="_comboCategory"/>.</summary>
    private readonly Panel _filterPanel;

    /// <summary>Allows the user to include <see cref="AppTile"/>s for applications that require a terminal, reporting how many are hidden.</summary>
    private readonly CheckBox _checkBoxIncludeTerminal;

    /// <summary>Displays <see cref="AppTile"/>s in top-bottom list.</summary>
    private readonly FlowLayoutPanel _flowLayout;

    /// <summary>Contains <see cref="_flowLayout"/> and makes it scrollable.</summary>
    private readonly Panel _scrollPanel;

    /// <summary>Maps interface URIs to <see cref="AppTile"/>s.</summary>
    private readonly Dictionary<FeedUri, AppTile> _tileDictionary = [];

    /// <summary><see cref="AppTile"/>s prepared by <see cref="QueueNewTile"/>, waiting to be added to <see cref="_flowLayout"/>.</summary>
    private readonly List<Control> _appTileQueue = [];

    /// <summary>The category currently selected in <see cref="_comboCategory"/>; <c>null</c> for all categories.</summary>
    /// <remarks>Kept separate from <see cref="_comboCategory"/> so that the selection survives rebuilds of its item list.</remarks>
    private string? _selectedCategory;

    /// <summary><c>true</c> while <see cref="UpdateCategories"/> is rebuilding the items in <see cref="_comboCategory"/>.</summary>
    private bool _updatingCategories;

    /// <summary>Windows message for a character typed on the keyboard.</summary>
    private const int WmChar = 0x0102;
    #endregion

    #region Properties
    /// <summary>
    /// The light background color (one of two colors the list toggles between) for <see cref="AppTile"/>s.
    /// </summary>
    /// <seealso cref="TileColorDark"/>
    [Category("Appearance"), Description("The light background color (one of two colors the list toggles between) for AppTiles.")]
    [DefaultValue(typeof(Color), "Window")]
    public Color TileColorLight { get; set; } = SystemColors.Window;

    /// <summary>
    /// The dark background color (one of two colors the list toggles between) for <see cref="AppTile"/>s.
    /// </summary>
    /// <seealso cref="TileColorLight"/>
    [Category("Appearance"), Description("The dark background color (one of two colors the list toggles between) for AppTiles.")]
    [DefaultValue(typeof(Color), "Control")]
    public Color TileColorDark { get; set; } = SystemColors.Control;

    /// <summary>
    /// Apply operations machine-wide instead of just for the current user.
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool MachineWide { get; set; }

    private bool _showFilters;

    /// <summary>
    /// Show the filters other than the name/summary search.
    /// </summary>
    [Category("Behavior"), Description("Show the filters other than the name/summary search.")]
    [DefaultValue(false)]
    public bool ShowFilters
    {
        get => _showFilters;
        set
        {
            _showFilters = value;
            _comboCategory.Visible = _checkBoxIncludeTerminal.Visible = value;
            UpdateFilterPanelHeight();
            RefilterTiles();
        }
    }
    #endregion

    #region Constructor
    /// <summary>
    /// Creates a new <see cref="AppTile"/> list.
    /// </summary>
    public AppTileList()
    {
        Size = new Size(425, 200);
        AutoScaleDimensions = new SizeF(6F, 13F);
        AutoScaleMode = AutoScaleMode.Font;

        SuspendLayout();

        TextSearch = new HintTextBox
        {
            Dock = DockStyle.Fill,
            HintText = Resources.Search,
            ShowClearButton = true,
            TabIndex = 0
        };
        TextSearch.TextChanged += delegate { RefilterTiles(); };

        // Note: Must set SelectedIndex before subscribing, since the handler uses fields that do not exist yet
        _comboCategory = new ComboBox
        {
            Dock = DockStyle.Right,
            Width = 120,
            DropDownWidth = 200,
            DropDownStyle = ComboBoxStyle.DropDownList,
            MaxDropDownItems = 16,
            AccessibleName = Resources.Categories,
            Visible = false,
            TabIndex = 1
        };
        _comboCategory.Items.Add(new CategoryEntry(null, Resources.AllCategories));
        _comboCategory.SelectedIndex = 0;
        _comboCategory.SelectedIndexChanged += delegate
        {
            if (_updatingCategories) return;
            _selectedCategory = (_comboCategory.SelectedItem as CategoryEntry)?.Name;
            RefilterTiles();
        };

        // Must add fill control first for docking to work correctly
        _filterPanel = new Panel {Dock = DockStyle.Top, TabIndex = 0, Controls = {TextSearch, _comboCategory}};

        // Text boxes and drop-downs force their own font-derived heights
        TextSearch.SizeChanged += delegate { UpdateFilterPanelHeight(); };
        _comboCategory.SizeChanged += delegate { UpdateFilterPanelHeight(); };

        _flowLayout = new FlowLayoutPanel
        {
            Location = new Point(0, 0),
            Size = Size.Empty,
            Margin = Padding.Empty,
            FlowDirection = FlowDirection.TopDown
        };
        _scrollPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            AutoScroll = true,
            Controls = {_flowLayout},
            TabIndex = 1
        };

        // Note: Command-line apps are hidden by default; the hidden count keeps them discoverable
        _checkBoxIncludeTerminal = new CheckBox
        {
            Dock = DockStyle.Bottom,
            Height = 20,
            Padding = new Padding(left: 4, 0, 0, 0),
            UseMnemonic = false, // App counts must not be interpreted as access keys
            Visible = false,
            TabIndex = 2
        };
        _checkBoxIncludeTerminal.CheckedChanged += delegate { RefilterTiles(); };

        // Must add scroll panel first for docking to work correctly
        Controls.Add(_scrollPanel);
        Controls.Add(_filterPanel);
        Controls.Add(_checkBoxIncludeTerminal);

        UpdateFilterPanelHeight();

        Resize += delegate
        {
            _flowLayout.SuspendLayout();
            _flowLayout.Width = _scrollPanel.Width - (int)Math.Round(20 * this.GetScaleFactor().Width);
            foreach (Control control in _flowLayout.Controls)
                control.Width = _flowLayout.Width;
            _flowLayout.ResumeLayout(false);
        };

        ResumeLayout(false);
    }
    #endregion

    #region Access
    /// <summary>
    /// Prepares a new <see cref="AppTile"/> to be added to the list. Will be added in bulk when <see cref="AppTileList.AddQueuedTiles"/> is called.
    /// </summary>
    /// <param name="interfaceUri">The interface URI of the application this tile represents.</param>
    /// <param name="appName">The name of the application this tile represents.</param>
    /// <param name="status">Describes whether the application is listed in the <see cref="AppList"/> and if so whether it is integrated.</param>
    /// <exception cref="InvalidOperationException">The list already contains an <see cref="AppTile"/> with the specified <paramref name="interfaceUri"/>.</exception>
    public AppTile QueueNewTile(FeedUri interfaceUri, string appName, AppTileStatus status)
    {
        #region Sanity checks
        if (interfaceUri == null) throw new ArgumentNullException(nameof(interfaceUri));
        if (appName == null) throw new ArgumentNullException(nameof(appName));
        if (_tileDictionary.ContainsKey(interfaceUri)) throw new InvalidOperationException("Duplicate interface URI");
        #endregion

        var tile = new AppTile(interfaceUri, appName, status, MachineWide) {Width = _flowLayout.Width};
        tile.Hide(); // Shown by RefilterTiles() once Feed data is available

        _appTileQueue.Add(tile);
        _tileDictionary.Add(interfaceUri, tile);
        return tile;
    }

    /// <summary>
    /// Adds all new tiles queued by <see cref="AppTileList.QueueNewTile"/> calls.
    /// </summary>
    public void AddQueuedTiles()
    {
        FlushQueue();
        UpdateCategories();
        RefilterTiles();
    }

    /// <summary>
    /// Moves all tiles queued by <see cref="AppTileList.QueueNewTile"/> calls into <see cref="_flowLayout"/>.
    /// </summary>
    private void FlushQueue()
    {
        if (_appTileQueue.Count == 0) return;

        _flowLayout.Controls.AddRange(_appTileQueue.ToArray());
        _appTileQueue.Clear();
    }


    /// <summary>
    /// Retrieves a specific application tile from the list.
    /// </summary>
    /// <param name="interfaceUri">The interface URI of the application the tile to retrieve represents.</param>
    /// <returns>The requested <see cref="AppTile"/>; <c>null</c> if no matching entry was found.</returns>
    public AppTile? GetTile(FeedUri interfaceUri)
    {
        #region Sanity checks
        if (interfaceUri == null) throw new ArgumentNullException(nameof(interfaceUri));
        #endregion

        return _tileDictionary.TryGetValue(interfaceUri ?? throw new ArgumentNullException(nameof(interfaceUri)), out var tile)
            ? tile
            : null;
    }

    /// <summary>
    /// Removes an application tile from the list. Does nothing if no matching tile can be found.
    /// </summary>
    /// <param name="interfaceUri">The interface URI of the application the tile to remove represents.</param>
    public void RemoveTile(FeedUri interfaceUri)
    {
        if (_tileDictionary.TryGetValue(interfaceUri ?? throw new ArgumentNullException(nameof(interfaceUri)), out var tile))
            RemoveTile(tile);
    }

    /// <summary>
    /// Removes an application tile from the list.
    /// </summary>
    /// <param name="tile">The tile to remove.</param>
    /// <remarks>Disposes the <see cref="AppTile"/> (it cannot be reused).</remarks>
    private void RemoveTile(AppTile tile)
    {
        #region Sanity checks
        if (tile == null) throw new ArgumentNullException(nameof(tile));
        #endregion

        // Flush queue first, to avoid adding the disposed tile later
        FlushQueue();

        _flowLayout.Controls.Remove(tile);
        _tileDictionary.Remove(tile.InterfaceUri);
        tile.Dispose();

        // Note: Deliberately does not call UpdateCategories(), since a changed app is represented as a removal
        // followed by an addition and dropping a category in-between would reset the user's selection.
        RefilterTiles();
    }

    /// <summary>
    /// Removes all application tiles from the list.
    /// </summary>
    public void Clear()
    {
        _appTileQueue.Clear();

        _flowLayout.Controls.Clear();
        _flowLayout.Height = 0;

        _tileDictionary.Clear();

        UpdateCategories();
    }

    /// <summary>
    /// Scrolls the list by a specified <paramref name="delta"/>.
    /// </summary>
    public void PerformScroll(int delta)
        => _scrollPanel.AutoScrollPosition = new Point(-_scrollPanel.AutoScrollPosition.X, -(_scrollPanel.AutoScrollPosition.Y + delta));
    #endregion

    #region Keyboard input
    /// <summary>
    /// Redirects keyboard input to <see cref="TextSearch"/>, so that the user can start typing a search without focusing it first.
    /// </summary>
    /// <param name="character">The character that was typed.</param>
    /// <returns><c>true</c> if the character was consumed and must not be passed on to the focused control.</returns>
    public bool HandleTypedChar(char character)
    {
        switch (character)
        {
            case (char)Keys.Escape:
                // Note: Also applies while TextSearch itself is focused
                if (TextSearch.TextLength == 0 || _comboCategory.DroppedDown) return false;
                TextSearch.Clear();
                TextSearch.Focus();
                return true;

            case (char)Keys.Back:
                if (TextSearch.TextLength == 0 || FocusConsumesTypedChars()) return false;
                TextSearch.Focus();
                TextSearch.Text = TextSearch.Text.Substring(0, TextSearch.TextLength - 1);
                TextSearch.SelectionStart = TextSearch.TextLength;
                return true;

            default:
                // Note: Whitespace would be useless at the start of a search, but activates buttons and check boxes
                if (char.IsControl(character) || char.IsWhiteSpace(character) || FocusConsumesTypedChars()) return false;
                TextSearch.Focus();
                TextSearch.AppendText(character.ToString());
                return true;
        }
    }

    /// <summary>
    /// <see cref="TabControl"/> and similar containers only get key events for controls they contain via this method,
    /// so <see cref="HandleTypedChar"/> must be applied here rather than in a key event handler.
    /// </summary>
    protected override bool ProcessKeyPreview(ref Message m)
        => (m.Msg == WmChar && HandleTypedChar((char)(int)m.WParam))
        || base.ProcessKeyPreview(ref m);

    /// <summary>
    /// Checks whether the control that currently has the keyboard focus handles typed characters itself.
    /// </summary>
    private bool FocusConsumesTypedChars()
        => FindFocused(this) is TextBoxBase or ListControl or UpDownBase or DateTimePicker;

    /// <summary>
    /// Finds the control that currently has the keyboard focus within <paramref name="control"/>; <c>null</c> if the focus is elsewhere.
    /// </summary>
    private static Control? FindFocused(Control control)
        => control.Focused
            ? control
            : control.Controls.OfType<Control>().FirstOrDefault(x => x.ContainsFocus) is {} child
                ? FindFocused(child)
                : null;
    #endregion

    #region Helpers
    /// <summary>
    /// Sets the height of <see cref="_filterPanel"/> to fit its content.
    /// </summary>
    private void UpdateFilterPanelHeight()
        => _filterPanel.Height = Math.Max(TextSearch.Height, _showFilters ? _comboCategory.Height : 0);

    /// <summary>
    /// Checks whether a tile matches the current search and category filters. Ignores <see cref="IsExcludedAsTerminal"/>.
    /// </summary>
    private bool IsMatch(AppTile tile)
        => (tile.AppName.ContainsIgnoreCase(TextSearch.Text) || tile.AppSummary.ContainsIgnoreCase(TextSearch.Text))
        && (_selectedCategory == null || tile.Categories.Contains(_selectedCategory, StringComparer.OrdinalIgnoreCase));

    /// <summary>
    /// Checks whether a tile is held back by <see cref="_checkBoxIncludeTerminal"/>.
    /// </summary>
    /// <remarks>Kept separate from <see cref="IsMatch"/> so that <see cref="RefilterTiles"/> can count what this hides.</remarks>
    private bool IsExcludedAsTerminal(AppTile tile)
        => _showFilters && !_checkBoxIncludeTerminal.Checked && tile.NeedsTerminal;

    /// <summary>
    /// Applies the filters to the list of tiles and recolors them. Should be called after the filters or the tiles were changed.
    /// </summary>
    private void RefilterTiles()
    {
        _scrollPanel.SuspendLayout();
        _flowLayout.SuspendLayout();

        int height = 0;
        bool lastTileLight = false;
        int hidden = 0;
        foreach (var tile in _flowLayout.Controls.OfType<AppTile>())
        {
            bool matches = IsMatch(tile);
            bool visible = matches && !IsExcludedAsTerminal(tile);

            if (visible)
            {
                // Alternate between light and dark tiles
                tile.BackColor = lastTileLight ? TileColorDark : TileColorLight;
                lastTileLight = !lastTileLight;

                height += tile.Height;
            }
            else if (matches) hidden++;

            tile.Visible = visible;
        }
        _flowLayout.Height = height;

        // Report how many tiles the command-line filter is holding back, taking the other filters into account
        _checkBoxIncludeTerminal.Text = (hidden == 0)
            ? Resources.IncludeCommandLineApps
            : string.Format(Resources.IncludeCommandLineAppsHidden, hidden);

        _flowLayout.ResumeLayout();
        _scrollPanel.ResumeLayout();
    }

    /// <summary>
    /// Rebuilds the list of categories offered for filtering based on the categories the tiles actually have.
    /// </summary>
    private void UpdateCategories()
    {
        var categories = _flowLayout.Controls.OfType<AppTile>()
                                    .SelectMany(tile => tile.Categories)
                                    .Distinct(StringComparer.OrdinalIgnoreCase)
                                    .Select(category => new CategoryEntry(category, LocalizeCategory(category)))
                                    // Sort by the localized name, so the drop-down reads alphabetically to the user
                                    .OrderBy(entry => entry.ToString(), StringComparer.CurrentCultureIgnoreCase)
                                    .ToList();

        // Avoid closing an open drop-down and flickering when nothing changed
        if (categories.Select(entry => entry.Name)
                      .SequenceEqual(_comboCategory.Items.Cast<CategoryEntry>().Skip(1).Select(entry => entry.Name), StringComparer.Ordinal)) return;

        _updatingCategories = true;
        _comboCategory.BeginUpdate();
        try
        {
            _comboCategory.Items.Clear();
            _comboCategory.Items.Add(new CategoryEntry(null, Resources.AllCategories));
            foreach (var category in categories)
                _comboCategory.Items.Add(category);

            // Drop a selection that no longer exists
            int index = (_selectedCategory == null)
                ? 0
                : categories.FindIndex(entry => entry.Name == _selectedCategory) + 1; // Offset by the "all categories" entry
            if (index <= 0) _selectedCategory = null;
            _comboCategory.SelectedIndex = Math.Max(index, 0);
        }
        finally
        {
            _comboCategory.EndUpdate();
            _updatingCategories = false;
        }
    }

    /// <summary>
    /// Returns a localized name for a category, falling back to the raw name for categories that have no translation.
    /// </summary>
    /// <remarks>Categories are free-form, so only the well-known freedesktop.org ones can be translated.</remarks>
    private static string LocalizeCategory(string category)
        => Resources.ResourceManager.GetString("Category" + category, Resources.Culture) ?? category;

    /// <summary>
    /// An entry in <see cref="_comboCategory"/>, shown to the user by its localized name but filtered by its raw name.
    /// </summary>
    private sealed class CategoryEntry(string? name, string localizedName)
    {
        /// <summary>The raw name as used in <see cref="Feed.Categories"/>; <c>null</c> for all categories.</summary>
        public string? Name { get; } = name;

        public override string ToString() => localizedName;
    }
    #endregion
}
