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
using System.Drawing;
using System.Windows.Forms;
using Common.Collections;
using Common.Controls;
using Common.Utils;
using ZeroInstall.Central.WinForms.Properties;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall.Central.WinForms
{
    /// <summary>
    /// Displays a list of <see cref="AppTile"/>s.
    /// </summary>
    public sealed class AppTileList : UserControl
    {
        #region Variables
        /// <summary>Allows the user to search/filter the <see cref="AppTile"/>s.</summary>
        private readonly HintTextBox _textSearch;

        /// <summary>Displays <see cref="AppTile"/>s in top-bottom list.</summary>
        private readonly FlowLayoutPanel _flowLayout;

        /// <summary>Contains <see cref="_flowLayout"/> and makes it scrollable.</summary>
        private readonly Panel _scrollPanel;

        /// <summary>Maps interface IDs to <see cref="AppTile"/>s.</summary>
        private readonly C5.IDictionary<string, AppTile> _tileDictionary = new C5.HashDictionary<string, AppTile>();

        /// <summary><see langword="true"/> if the last tile used <see cref="TileColorLight"/>; <see langword="false"/> if the last tile used <see cref="TileColorDark"/>.</summary>
        private bool _lastTileLight;

        /// <summary><see cref="AppTile"/>s prepared by <see cref="QueueNewTile"/>, waiting to be added to <see cref="_flowLayout"/>.</summary>
        private readonly List<Control> _appTileQueue = new List<Control>();

        /// <summary>The combined height of all <see cref="AppTile"/>s in <see cref="_appTileQueue"/>.</summary>
        private int _appTileQueueHeight;
        #endregion

        #region Properties
        /// <summary>
        /// The icon cache used by newly created <see cref="AppTile"/>s to retrieve application icons.
        /// </summary>
        [Category("Data"), Description("The icon cache used by newly created AppTiles to retrieve application icons.")]
        [DefaultValue(null)]
        public IIconCache IconCache { get; set; }

        /// <summary>
        /// The light background color (one of two colors the list toggles between) for <see cref="AppTile"/>s.
        /// </summary>
        /// <seealso cref="TileColorDark"/>
        private Color _tileColorLight = Color.White;

        [Category("Appearance"), Description("The light background color (one of two colors the list toggles between) for AppTiles.")]
        [DefaultValue(typeof(Color), "ControlLightLight")]
        public Color TileColorLight { get { return _tileColorLight; } set { _tileColorLight = value; } }

        /// <summary>
        /// The dark background color (one of two colors the list toggles between) for <see cref="AppTile"/>s.
        /// </summary>
        /// <seealso cref="TileColorLight"/>
        private Color _tileColorDark = Color.FromArgb(230, 240, 255);

        [Category("Appearance"), Description("The dark background color (one of two colors the list toggles between) for AppTiles.")]
        [DefaultValue(typeof(Color), "ControlLight")]
        public Color TileColorDark { get { return _tileColorDark; } set { _tileColorDark = value; } }
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

            _textSearch = new HintTextBox
            {
                Dock = DockStyle.Top, Height = 20,
                HintText = Resources.Search, ShowClearButton = true,
                TabIndex = 0
            };
            _textSearch.TextChanged += delegate { RefilterTiles(); };

            _flowLayout = new FlowLayoutPanel
            {
                Location = new Point(0, 0), Size = Size.Empty, Margin = Padding.Empty,
                FlowDirection = FlowDirection.TopDown
            };
            _scrollPanel = new Panel
            {
                Dock = DockStyle.Fill, Margin = Padding.Empty,
                AutoScroll = true, Controls = {_flowLayout},
                TabIndex = 1
            };

            // Must add scroll panel first for docking to work correctly
            Controls.Add(_scrollPanel);
            Controls.Add(_textSearch);

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
        /// Prepares a new application tile to be added to the list. Will be added in bulk when <see cref="AddQueuedTiles"/> is called.
        /// </summary>
        /// <param name="systemWide">Apply operations system-wide instead of just for the current user.</param>
        /// <param name="interfaceID">The interface ID of the application this tile represents.</param>
        /// <param name="appName">The name of the application this tile represents.</param>
        /// <exception cref="C5.DuplicateNotAllowedException">Thrown if the list already contains an <see cref="AppTile"/> with the specified <paramref name="interfaceID"/>.</exception>
        public AppTile QueueNewTile(bool systemWide, string interfaceID, string appName)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            if (appName == null) throw new ArgumentNullException("appName");
            if (_tileDictionary.Contains(interfaceID)) throw new C5.DuplicateNotAllowedException();
            #endregion

            var tile = new AppTile(systemWide, interfaceID, appName, IconCache) {Width = _flowLayout.Width};

            if (StringUtils.Contains(appName, _textSearch.Text))
            {
                _appTileQueueHeight += tile.Height;

                // Alternate between light and dark tiles
                tile.BackColor = _lastTileLight ? TileColorDark : TileColorLight;
                _lastTileLight = !_lastTileLight;
            }
            else tile.Visible = false;

            _appTileQueue.Add(tile);
            _tileDictionary.Add(interfaceID, tile);
            return tile;
        }

        /// <summary>
        /// Adds all new tiles queued by <see cref="QueueNewTile"/> calls.
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
        /// <param name="interfaceID">The interface ID of the application the tile to retrieve represents.</param>
        /// <returns>The requested <see cref="AppTile"/>; <see langword="null"/> if no matching entry was found.</returns>
        public AppTile GetTile(string interfaceID)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            #endregion

            return _tileDictionary.Contains(interfaceID) ? _tileDictionary[interfaceID] : null;
        }

        /// <summary>
        /// Removes an application tile from the list.
        /// </summary>
        /// <param name="tile">The tile to remove.</param>
        /// <remarks>Disposes the <see cref="AppTile"/> (it cannot be reused).</remarks>
        public void RemoveTile(AppTile tile)
        {
            #region Sanity checks
            if (tile == null) throw new ArgumentNullException("tile");
            #endregion

            // Flush queue first, to allow propper recoloring
            AddQueuedTiles();

            _flowLayout.Controls.Remove(tile);
            if (tile.Visible) _flowLayout.Height -= tile.Height;
            _tileDictionary.Remove(tile.InterfaceID);
            tile.Dispose();

            RecolorTiles();
        }

        /// <summary>
        /// Removes an application tile from the list. Does nothing if no matching tile can be found.
        /// </summary>
        /// <param name="interfaceID">The interface ID of the application the tile to remove represents.</param>
        public void RemoveTile(string interfaceID)
        {
            try
            {
                RemoveTile(_tileDictionary[interfaceID]);
            }
            catch (C5.NoSuchItemException)
            {}
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
        /// Show a list of categories of the current tiles.
        /// </summary>
        public void ShowCategories()
        {
            // Accumulate all categories
            var categories = new C5.TreeSet<string>();
            foreach (var tile in _tileDictionary.Values)
                categories.AddAll(tile.Feed.Categories);

            // ToDo: Show category GUI
        }

        /// <summary>
        /// Scrolls the list by a specified <paramref name="delta"/>.
        /// </summary>
        public void PerformScroll(int delta)
        {
            // AutoScrollPosition are inverted by WinForms when set
            _scrollPanel.AutoScrollPosition = new Point(-_scrollPanel.AutoScrollPosition.X, -(_scrollPanel.AutoScrollPosition.Y + delta));
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Applies the search filter to the list of tiles. Should be called after the filter was changed.
        /// </summary>
        private void RefilterTiles()
        {
            _scrollPanel.SuspendLayout();
            _flowLayout.SuspendLayout();

            bool needsRecolor = false;
            foreach (var tile in _tileDictionary.Values)
            {
                // Check if new filter changes visibility
                bool shouldBeVisible = StringUtils.Contains(tile.AppName, _textSearch.Text);
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

            foreach (var tile in EnumerableUtils.OfType<AppTile>(_flowLayout.Controls))
            {
                if (!tile.Visible) continue;

                // Alternate between light and dark tiles
                tile.BackColor = _lastTileLight ? TileColorDark : TileColorLight;
                _lastTileLight = !_lastTileLight;
            }
        }
        #endregion
    }
}
