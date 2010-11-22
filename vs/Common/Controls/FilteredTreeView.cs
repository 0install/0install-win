/*
 * Copyright 2006-2010 Bastian Eicher
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using Common.Collections;
using Common.Utils;

namespace Common.Controls
{
    /// <summary>
    /// Displays a list of <see cref="INamed"/>s objects in a <see cref="TreeView"/> with incremental search.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="INamed"/> object to list.
    /// Special support for types implementing <see cref="IHighlightColor"/> and/or <see cref="IContextMenu"/>,</typeparam>
    [Description("Displays a list of INamed in a TreeView with incremental search.")]
    public sealed partial class FilteredTreeView<T> : UserControl where T : class, INamed
    {
        #region Events
        /// <summary>
        /// Occurs whenever <see cref="SelectedEntry"/> has been changed.
        /// </summary>
        [Description("Occurs whenever SelectedEntry has been changed.")]
        public event EventHandler SelectedEntryChanged;

        private void OnSelectedEntryChanged()
        {
            if (Visible && SelectedEntryChanged != null && !_supressEvents) SelectedEntryChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when the user has confirmed the <see cref="SelectedEntry"/> via double-clicking or pressing Enter.
        /// </summary>
        [Description("Occurs when the user has confirmed the current selection via double-clicking or pressing Enter.")]
        public event EventHandler SelectionConfirmed;

        private void OnSelectionConfirmed()
        {
            // Only confirm if the user actually selected something
            if (_selectedEntry != null && SelectionConfirmed != null) SelectionConfirmed(this, EventArgs.Empty);
        }
        #endregion

        #region Variables
        /// <summary>Suppress the execution of <see cref="SelectedEntryChanged"/>.</summary>
        private bool _supressEvents;
        #endregion

        #region Properties
        /// <summary>
        /// Toggle the visibility of the search box.
        /// </summary>
        [DefaultValue(true), Description("Toggle the visibility of the search box."), Category("Appearance")]
        public bool ShowSearchBox
        {
            get { return textSearch.Visible; }
            set { textSearch.Visible = value; }
        }

        private INamedCollection<T> _entries;
        /// <summary>
        /// The <see cref="INamed"/> (and optionally <see cref="IContextMenu"/>) objects to be listed in the tree.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "This control is supposed to represent a live and mutable collection")]
        public INamedCollection<T> Entries
        {
            get { return _entries; }
            set { _entries = value; UpdateList(); }
        }

        private T _selectedEntry;
        /// <summary>
        /// The <see cref="INamed"/> object currently selected in the <see cref="TreeView"/>; <see langword="null"/> for no selection.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public T SelectedEntry
        {
            get { return _selectedEntry; }
            set
            {
                _selectedEntry = value;
                UpdateList();
                OnSelectedEntryChanged();
            }
        }

        private char _separator = '.';
        /// <summary>
        /// The character used to separate namespaces in the <see cref="INamed.Name"/>s. This controls how the tree structure is generated.
        /// </summary>
        [DefaultValue('.'), Description("The character used to separate namespaces in the Names. This controls how the tree structure is generated.")]
        public char Separator { get { return _separator; } set { _separator = value; UpdateList(); } }
        #endregion

        #region Constructor
        public FilteredTreeView()
        {
            InitializeComponent();
        }
        #endregion

        //--------------------//

        #region Search control
        private void textSearch_TextChanged(object sender, EventArgs e)
        {
            UpdateList();
        }
        #endregion

        #region ListView control
        private void treeView_DoubleClick(object sender, EventArgs e)
        {
            OnSelectionConfirmed();
        }

        private void treeView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) OnSelectionConfirmed();
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string className = treeView.SelectedNode.Name;

            // Don't use the property, to prevent a loop
            _selectedEntry = _entries.Contains(className) ? _entries[className] : null;
            OnSelectedEntryChanged();
        }

        /// <summary>
        /// Updates the filtered <see cref="ListView"/> representation of <see cref="Entries"/>
        /// </summary>
        private void UpdateList()
        {
            // Suppress events to prevent infinite loops
            _supressEvents = true;

            treeView.Nodes.Clear();
            if (_entries != null)
            {
                foreach (T classEntry in _entries)
                {
                    // The currently selected class is always visible
                    if (_selectedEntry != null && classEntry.Name == _selectedEntry.Name) // Only compare name to handle cloned entries
                    {
                        _selectedEntry = classEntry; // Fix problems that might arrise from using clones
                        treeView.SelectedNode = AddTreeNode(classEntry);
                    }
                    // List all nodes if there is no filter
                    else if (string.IsNullOrEmpty(textSearch.Text))
                        AddTreeNode(classEntry);
                    // Only list nodes that match the filter
                    else if (StringUtils.Contains(classEntry.Name, textSearch.Text))
                        AddTreeNode(classEntry);
                }

                // Automatically expand nodes based on the filtering
                if (!string.IsNullOrEmpty(textSearch.Text))
                    FilterNodes(treeView.Nodes, true);
            }

            // Restore events at the end
            _supressEvents = false;
        }
        #endregion

        #region ListView node helper
        /// <summary>
        /// Adds a new node to <see cref="treeView"/>.
        /// </summary>
        /// <param name="entry">The <typeparamref name="T"/> to create the entry for.</param>
        /// <returns>The newly created <see cref="TreeNode"/>.</returns>
        private TreeNode AddTreeNode(T entry)
        {
            // Split into hierarchic namespaces
            string name = entry.Name;
            string[] nameSplit = name.Split(_separator);

            // Start off at the top-level
            TreeNodeCollection subTree = treeView.Nodes;

            // Try to use a pre-existing nodes for namespace-subtrees, if non-existant create new ones
            string partialName = "";
            for (int i = 0; i < nameSplit.Length - 1; i++)
            {
                partialName += nameSplit[i];
                TreeNode node = subTree[partialName] ?? subTree.Add(partialName, nameSplit[i]);
                subTree = node.Nodes;
                partialName += ".";
            }

            // Create node storing full name, using last part as visible text
            TreeNode finalNode = subTree.Add(name, nameSplit[nameSplit.Length - 1]);

            #region Highlight color
            var highlightColorProvider = entry as IHighlightColor;
            if (highlightColorProvider != null && highlightColorProvider.HighlightColor != Color.Empty)
            {
                // Apply the highlighting color if one is set
                finalNode.ForeColor = highlightColorProvider.HighlightColor;
                finalNode.NodeFont = new Font(treeView.Font, FontStyle.Bold);
            }
            #endregion

            #region Context menu
            var contextMenuProvider = entry as IContextMenu;
            if (contextMenuProvider != null)
            {
                var contextMenu = contextMenuProvider.GetContextMenu();
                if (contextMenu != null)
                {
                    // Attach the context menu if one is set
                    finalNode.ContextMenu = contextMenu;

                    // Automatically reflect any changes the context menu may have made
                    foreach (MenuItem menuItem in finalNode.ContextMenu.MenuItems)
                    {
                        menuItem.Click += delegate { UpdateList(); };
                    }
                }
            }
            #endregion

            return finalNode;
        }
        #endregion

        #region ListView filter node helper
        /// <summary>
        /// Automatically expand nodes based on the <see cref="textSearch"/> filtering
        /// </summary>
        /// <param name="subTree">The current <see cref="TreeNodeCollection"/> used in recursion</param>
        /// <param name="fullNameExpand">Shall a search in the full name of a tag allow it to be expanded?</param>
        private void FilterNodes(TreeNodeCollection subTree, bool fullNameExpand)
        {
            foreach (TreeNode node in subTree)
            {
                // Nodes with matches in the last part of their name (the displayed text) shall always be visible
                if (StringUtils.Contains(node.Text, textSearch.Text)) node.EnsureVisible();

                // Nodes with matches in their full name shall be expanded...
                if (fullNameExpand && StringUtils.Contains(node.Name, textSearch.Text))
                {
                    node.EnsureVisible();
                    node.Expand();

                    // ... but not beyond the first recursion level
                    FilterNodes(node.Nodes, false);
                }
                else FilterNodes(node.Nodes, fullNameExpand);
            }
        }
        #endregion
    }
}