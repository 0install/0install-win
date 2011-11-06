/*
 * Copyright 2010-2011 Bastian Eicher
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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Common.Collections;
using Common.Controls;
using Common.Utils;
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
        #endregion

        #region Properties
        /// <summary>
        /// The icon cache used by newly created <see cref="AppTile"/>s to retreive application icons.
        /// </summary>
        [Category("Data"), Description("The icon cache used by newly created AppTiles to retreive application icons.")]
        [DefaultValue(null)]
        public IIconCache IconCache { get; set; }

        /// <summary>
        /// The light background color (one of two colors the list toggles between) for <see cref="AppTile"/>s.
        /// </summary>
        /// <seealso cref="TileColorDark"/>
        private Color _tileColorLight = SystemColors.ControlLightLight;

        [Category("Appearance"), Description("The light background color (one of two colors the list toggles between) for AppTiles.")]
        [DefaultValue(typeof(Color), "ControlLightLight")]
        public Color TileColorLight { get { return _tileColorLight; } set { _tileColorLight = value; } }

        /// <summary>
        /// The dark background color (one of two colors the list toggles between) for <see cref="AppTile"/>s.
        /// </summary>
        /// <seealso cref="TileColorLight"/>
        private Color _tileColorDark = SystemColors.ControlLight;

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

            SuspendLayout();

            _textSearch = new HintTextBox
            {
                Dock = DockStyle.Top, Height = 20,
                HintText = "Search", ShowClearButton = true,
                TabIndex = 0
            };
            _textSearch.TextChanged += delegate { FilterTiles(); };

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

                _flowLayout.Width = _scrollPanel.Width - 20;
                foreach (Control control in _flowLayout.Controls)
                    control.Width = _flowLayout.Width;

                _flowLayout.ResumeLayout(false);
            };

            ResumeLayout(false);
        }
        #endregion

        #region Access
        /// <summary>
        /// Adds a new application tile to the list.
        /// </summary>
        /// <param name="interfaceID">The interface ID of the application this tile represents.</param>
        /// <param name="appName">The name of the application this tile represents.</param>
        /// <exception cref="C5.DuplicateNotAllowedException">Thrown if the list already contains an <see cref="AppTile"/> with the specified <paramref name="interfaceID"/>.</exception>
        public AppTile AddTile(string interfaceID, string appName)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            if (appName == null) throw new ArgumentNullException("appName");
            if (_tileDictionary.Contains(interfaceID)) throw new C5.DuplicateNotAllowedException();
            #endregion

            var tile = new AppTile(interfaceID, appName, IconCache)
            {
                Width = _flowLayout.Width,
                // Alternate between light and dark tiles
                BackColor = _lastTileLight ? TileColorDark : TileColorLight
            };
            _lastTileLight = !_lastTileLight;

            _flowLayout.Height += tile.Height;
            _flowLayout.Controls.Add(tile);

            _tileDictionary.Add(interfaceID, tile);
            FilterTiles();
            return tile;
        }

        /// <summary>
        /// Retreives a specific application tile from the list.
        /// </summary>
        /// <param name="interfaceID">The interface ID of the application the tile to retreive represents.</param>
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
        public void RemoveTile(AppTile tile)
        {
            #region Sanity checks
            if (tile == null) throw new ArgumentNullException("tile");
            #endregion

            _flowLayout.Controls.Remove(tile);
            _flowLayout.Height -= tile.Height;

            _tileDictionary.Remove(tile.InterfaceID);

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
            _flowLayout.Controls.Clear();
            _flowLayout.Height = 0;

            _tileDictionary.Clear();
            _lastTileLight = false;
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Applies a search filter to the list of tiles.
        /// </summary>
        private void FilterTiles()
        {
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
