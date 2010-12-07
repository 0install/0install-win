/*
 * Copyright 2010 Bastian Eicher
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
using System.Windows.Forms;
using Common.Collections;
using Common.Controls;
using ZeroInstall.Model;
using ZeroInstall.Store.Feed;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Store.Management.WinForms
{
    public sealed partial class MainForm : Form
    {
        #region Variables
        // Don't use WinForms designer for this, since it doesn't understand generics
        private readonly FilteredTreeView<StoreNode> _treeView = new FilteredTreeView<StoreNode> { Separator = '#', Dock = DockStyle.Fill };
        #endregion

        #region Constructor
        public MainForm()
        {
            InitializeComponent();

            _treeView.Entries = GetStoreNodes();
            panelTreeView.Controls.Add(_treeView);
        }
        #endregion

        private static INamedCollection<StoreNode> GetStoreNodes()
        {
            // ToDo: Add exception handling

            var nodes = new NamedCollection<StoreNode>();

            var feeds = new FeedCache().GetAll();

            foreach (var feed in feeds)
            {
                feed.Simplify();
                nodes.Add(new InterfaceNode(feed));
            }

            foreach (var storeEntry in StoreProvider.Default.ListAll())
            {
                foreach (var feed in feeds)
                {
                    var implementation = feed.GetImplementation(new ManifestDigest(storeEntry));
                    if (implementation != null)
                    {
                        nodes.Add(new ImplementationNode(feed, implementation));
                        break;
                    }
                }
            }

            return nodes;
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            _treeView.Entries = GetStoreNodes();
        }
    }
}
