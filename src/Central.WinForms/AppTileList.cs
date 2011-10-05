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

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall.Central.WinForms
{
    /// <summary>
    /// Displays a list of <see cref="AppTile"/>s.
    /// </summary>
    public sealed class AppTileList : UserControl
    {
        #region Variables
        /// <summary>Displays <see cref="AppTile"/>s in top-bottom list.</summary>
        private readonly FlowLayoutPanel _flowLayout;

        /// <summary>Contains <see cref="_flowLayout"/> and makes it scrollable.</summary>
        private readonly Panel _scrollPanel;

        /// <summary><see langword="true"/> if the last <see cref="AppTile"/> used the <see cref="TileColorDark"/>; <see langword="false"/> if the last <see cref="AppTile"/> used the <see cref="TileColorLight"/>.</summary>
        private bool _lastTileDark;
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
        private Color _tileColorLight = Color.LightCyan;

        [Category("Appearance"), Description("The light background color (one of two colors the list toggles between) for AppTiles.")]
        [DefaultValue(typeof(Color), "LightCyan")]
        public Color TileColorLight { get { return _tileColorLight; } set { _tileColorLight = value; } }

        /// <summary>
        /// The dark background color (one of two colors the list toggles between) for <see cref="AppTile"/>s.
        /// </summary>
        /// <seealso cref="TileColorLight"/>
        private Color _tileColorDark = Color.LightBlue;

        [Category("Appearance"), Description("The dark background color (one of two colors the list toggles between) for AppTiles.")]
        [DefaultValue(typeof(Color), "LightBlue")]
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

            _flowLayout = new FlowLayoutPanel
            {
                Location = new Point(0, 0), Margin = Padding.Empty,
                Size = Size.Empty,
                FlowDirection = FlowDirection.TopDown,
            };
            _scrollPanel = new Panel
            {
                Location = new Point(0, 0), Margin = Padding.Empty,
                Dock = DockStyle.Fill,
                AutoScroll = true, Controls = {_flowLayout}
            };
            Controls.Add(_scrollPanel);

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
        public AppTile AddTile(string interfaceID, string appName)
        {
            var tile = new AppTile(interfaceID, appName, IconCache)
            {
                Width = _flowLayout.Width,
                BackColor = (_lastTileDark ? TileColorDark : TileColorLight)
            };
            _lastTileDark = !_lastTileDark;

            _flowLayout.Height += tile.Height;
            _flowLayout.Controls.Add(tile);

            return tile;
        }

        /// <summary>
        /// Removes an application tile from the list.
        /// </summary>
        /// <param name="tile">The tile to remove.</param>
        public void RemoveTile(AppTile tile)
        {
            _flowLayout.Controls.Remove(tile);
            _flowLayout.Height -= tile.Height;
        }
        #endregion
    }
}
