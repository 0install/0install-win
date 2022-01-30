// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NanoByte.Common;
using NanoByte.Common.Controls;
using ZeroInstall.Central.WinForms.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Model;
using ZeroInstall.Store.Icons;

namespace ZeroInstall.Central.WinForms
{
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

        /// <summary>Displays <see cref="AppTile"/>s in top-bottom list.</summary>
        private readonly FlowLayoutPanel _flowLayout;

        /// <summary>Contains <see cref="_flowLayout"/> and makes it scrollable.</summary>
        private readonly Panel _scrollPanel;

        /// <summary>Maps interface URIs to <see cref="AppTile"/>s.</summary>
        private readonly Dictionary<FeedUri, AppTile> _tileDictionary = new();

        /// <summary><c>true</c> if the last tile used <see cref="TileColorLight"/>; <c>false</c> if the last tile used <see cref="TileColorDark"/>.</summary>
        private bool _lastTileLight;

        /// <summary><see cref="AppTile"/>s prepared by <see cref="QueueNewTile"/>, waiting to be added to <see cref="_flowLayout"/>.</summary>
        private readonly List<Control> _appTileQueue = new();

        /// <summary>The combined height of all <see cref="AppTile"/>s in <see cref="_appTileQueue"/>.</summary>
        private int _appTileQueueHeight;
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
                Dock = DockStyle.Top,
                Height = 20,
                HintText = Resources.Search,
                ShowClearButton = true,
                TabIndex = 0
            };
            TextSearch.TextChanged += delegate { RefilterTiles(); };

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

            // Must add scroll panel first for docking to work correctly
            Controls.Add(_scrollPanel);
            Controls.Add(TextSearch);

            Resize += delegate
            {
                _flowLayout.SuspendLayout();
                _flowLayout.Width = _scrollPanel.Width - (int)(20 * AutoScaleDimensions.Width / 6F);
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
        /// <param name="iconStore">The icon store used by newly created <see cref="AppTile"/>s to retrieve application icons; can be <c>null</c>.</param>
        /// <param name="machineWide">Apply operations machine-wide instead of just for the current user.</param>
        /// <exception cref="InvalidOperationException">The list already contains an <see cref="AppTile"/> with the specified <paramref name="interfaceUri"/>.</exception>
        public AppTile QueueNewTile(FeedUri interfaceUri, string appName, AppTileStatus status, IIconStore? iconStore = null, bool machineWide = false)
        {
            #region Sanity checks
            if (interfaceUri == null) throw new ArgumentNullException(nameof(interfaceUri));
            if (appName == null) throw new ArgumentNullException(nameof(appName));
            if (_tileDictionary.ContainsKey(interfaceUri)) throw new InvalidOperationException("Duplicate interface URI");
            #endregion

            var tile = new AppTile(interfaceUri, appName, status, iconStore, machineWide) {Width = _flowLayout.Width};

            if (appName.ContainsIgnoreCase(TextSearch.Text))
            {
                _appTileQueueHeight += tile.Height;

                // Alternate between light and dark tiles
                tile.BackColor = _lastTileLight ? TileColorDark : TileColorLight;
                _lastTileLight = !_lastTileLight;
            }
            else tile.Visible = false;

            _appTileQueue.Add(tile);
            _tileDictionary.Add(interfaceUri, tile);
            return tile;
        }

        /// <summary>
        /// Adds all new tiles queued by <see cref="AppTileList.QueueNewTile"/> calls.
        /// </summary>
        public void AddQueuedTiles()
        {
            _flowLayout.Height += _appTileQueueHeight;
            _appTileQueueHeight = 0;

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

            // Flush queue first, to allow proper recoloring
            AddQueuedTiles();

            _flowLayout.Controls.Remove(tile);
            if (tile.Visible) _flowLayout.Height -= tile.Height;
            _tileDictionary.Remove(tile.InterfaceUri);
            tile.Dispose();

            RecolorTiles();
        }

        /// <summary>
        /// Removes all application tiles from the list.
        /// </summary>
        public void Clear()
        {
            _appTileQueue.Clear();
            _appTileQueueHeight = 0;

            _flowLayout.Controls.Clear();
            _flowLayout.Height = 0;

            _tileDictionary.Clear();
            _lastTileLight = false;
        }

        /// <summary>
        /// Scrolls the list by a specified <paramref name="delta"/>.
        /// </summary>
        public void PerformScroll(int delta)
            => _scrollPanel.AutoScrollPosition = new Point(-_scrollPanel.AutoScrollPosition.X, -(_scrollPanel.AutoScrollPosition.Y + delta));
        #endregion

        #region Helpers
        /// <summary>
        /// Applies the search filter to the list of tiles. Should be called after the filter was changed.
        /// </summary>
        protected virtual void RefilterTiles()
        {
            _scrollPanel.SuspendLayout();
            _flowLayout.SuspendLayout();

            bool needsRecolor = false;
            foreach (var tile in _tileDictionary.Values)
            {
                // Check if new filter changes visibility
                bool shouldBeVisible = tile.AppName.ContainsIgnoreCase(TextSearch.Text);
                if (tile.Visible != shouldBeVisible)
                {
                    // Update list length
                    if (shouldBeVisible) _flowLayout.Height += tile.Height;
                    else _flowLayout.Height -= tile.Height;

                    tile.Visible = shouldBeVisible;
                    needsRecolor = true;
                }
            }

            if (needsRecolor) RecolorTiles();

            _scrollPanel.ResumeLayout();
            _flowLayout.ResumeLayout();
        }

        /// <summary>
        /// Colors all application tiles in the list. Should be called after one or more tiles were removed.
        /// </summary>
        private void RecolorTiles()
        {
            _lastTileLight = false;

            foreach (var tile in _flowLayout.Controls.OfType<AppTile>().Where(tile => tile.Visible))
            {
                // Alternate between light and dark tiles
                tile.BackColor = _lastTileLight ? TileColorDark : TileColorLight;
                _lastTileLight = !_lastTileLight;
            }
        }
        #endregion
    }
}
