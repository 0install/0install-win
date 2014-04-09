/*
 * Copyright 2006-2014 Bastian Eicher
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NanoByte.Common.Collections;
using NanoByte.Common.Properties;
using NanoByte.Common.Utils;

namespace NanoByte.Common.Controls
{
    /// <summary>
    /// Displays a list of <see cref="INamed{T}"/>s objects in a <see cref="TreeView"/> with incremental search.
    /// An automatic hierachy is generated based on a <see cref="Separator"/> character.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="INamed{T}"/> object to list.
    /// Special support for types implementing <see cref="IHighlightColor"/> and/or <see cref="IContextMenu"/>.</typeparam>
    [Description("Displays a list of INamed in a TreeView with incremental search."), System.Runtime.InteropServices.GuidAttribute("5065F310-D0B3-4AD3-BBE5-B41D00D5F036")]
    public sealed partial class FilteredTreeView<T> : UserControl where T : class, INamed<T>
    {
        #region Events
        /// <summary>
        /// Occurs whenever <see cref="SelectedEntry"/> has been changed.
        /// </summary>
        [Description("Occurs whenever SelectedEntry has been changed.")]
        public event EventHandler SelectedEntryChanged;

        private void OnSelectedEntryChanged()
        {
            if (!_supressEvents && Visible && SelectedEntryChanged != null) SelectedEntryChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when the user has confirmed the <see cref="SelectedEntry"/> via double-clicking or pressing Enter.
        /// </summary>
        [Description("Occurs when the user has confirmed the current selection via double-clicking or pressing Enter.")]
        public event EventHandler SelectionConfirmed;

        private void OnSelectionConfirmed()
        {
            // Only confirm if the user actually selected something
            if (!_supressEvents && Visible && SelectionConfirmed != null && _selectedEntry != null) SelectionConfirmed(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs whenever the content of <see cref="CheckedEntries"/> has changed.
        /// </summary>
        [Description("Occurs whenever the content of CheckedEntries has changed.")]
        public event EventHandler CheckedEntriesChanged;

        private void OnCheckedEntriesChanged()
        {
            if (!_supressEvents && Visible && CheckedEntriesChanged != null) CheckedEntriesChanged(this, EventArgs.Empty);
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
        public bool ShowSearchBox { get { return textSearch.Visible; } set { textSearch.Visible = value; } }

        private NamedCollection<T> _nodes;

        /// <summary>
        /// The <see cref="INamed{T}"/> (and optionally <see cref="IContextMenu"/>) objects to be listed in the tree.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "This control is supposed to represent a live and mutable collection")]
        public NamedCollection<T> Nodes
        {
            get { return _nodes; }
            set
            {
                // Keep track of any changes within the collection
                if (_nodes != null) _nodes.CollectionChanged -= UpdateList;
                _nodes = value;
                if (_nodes != null) _nodes.CollectionChanged += UpdateList;

                _checkedEntries.Clear();
                UpdateList();
            }
        }

        private T _selectedEntry;

        /// <summary>
        /// The <see cref="INamed{T}"/> object currently selected in the <see cref="TreeView"/>; <see langword="null"/> for no selection.
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

        private readonly HashSet<T> _checkedEntries = new HashSet<T>();

        /// <summary>
        /// Returns an array of all <see cref="INamed{T}"/> objects currently marked with a check box.
        /// </summary>
        /// <see cref="CheckBoxes"/>
        public ICollection<T> CheckedEntries { get { return _checkedEntries.ToList(); } }

        private char _separator = '.';

        /// <summary>
        /// The character used to separate namespaces in the <see cref="INamed{T}.Name"/>s. This controls how the tree structure is generated.
        /// </summary>
        [DefaultValue('.'), Description("The character used to separate namespaces in the Names. This controls how the tree structure is generated.")]
        public char Separator
        {
            get { return _separator; }
            set
            {
                _separator = value;
                UpdateList();
            }
        }

        /// <summary>
        /// Controls whether check boxes are displayed for every entry.
        /// </summary>
        /// <see cref="CheckedEntries"/>
        [DefaultValue(false), Description("Controls whether check boxes are displayed for every entry.")]
        public bool CheckBoxes { get { return treeView.CheckBoxes; } set { treeView.CheckBoxes = value; } }
        #endregion

        #region Constructor
        public FilteredTreeView()
        {
            InitializeComponent();
            textSearch.HintText = Resources.Search;
        }
        #endregion

        //--------------------//

        #region Search control
        private void textSearch_TextChanged(object sender, EventArgs e)
        {
            UpdateList();
        }
        #endregion

        #region TreeView control
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
            string name = treeView.SelectedNode.Name;

            // Don't use the property, to prevent a loop
            _selectedEntry = _nodes.Contains(name) ? _nodes[name] : null;
            OnSelectedEntryChanged();
        }

        /// <summary>
        /// Updates the filtered <see cref="TreeView"/> representation of <see cref="Nodes"/>
        /// </summary>
        private void UpdateList(object sender = null)
        {
            // Suppress events to prevent infinite loops
            _supressEvents = true;

            treeView.Nodes.Clear();
            if (_nodes != null)
            {
                foreach (T entry in _nodes)
                {
                    // The currently selected entry and checked entries are always visible
                    // Note: Compare name to handle cloned entries
                    if ((_selectedEntry != null && entry.Name == _selectedEntry.Name) || _checkedEntries.Contains(entry))
                    {
                        _selectedEntry = entry; // Fix problems that might arrise from using clones
                        treeView.SelectedNode = AddTreeNode(entry);
                    }
                        // List all nodes if there is no filter
                    else if (string.IsNullOrEmpty(textSearch.Text))
                        AddTreeNode(entry);
                        // Only list nodes that match the filter
                    else if (entry.Name.ContainsIgnoreCase(textSearch.Text))
                        AddTreeNode(entry);
                }

                // Automatically expand nodes based on the filtering
                if (!string.IsNullOrEmpty(textSearch.Text))
                    ExpandNodes(treeView.Nodes, fullNameExpand: true);
            }

            // Restore events at the end
            _supressEvents = false;
        }
        #endregion

        #region TreeView node helper
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
            if (_checkedEntries.Contains(entry)) finalNode.Checked = true;

            #region Highlight color
            // ReSharper disable SuspiciousTypeConversion.Global
            // ReSharper disable ExpressionIsAlwaysNull
            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            // ReSharper disable HeuristicUnreachableCode
            var highlightColorProvider = entry as IHighlightColor;
            if (highlightColorProvider != null && highlightColorProvider.HighlightColor != Color.Empty)
            {
                // Apply the highlighting color if one is set
                finalNode.ForeColor = highlightColorProvider.HighlightColor;
                finalNode.NodeFont = new Font(treeView.Font, FontStyle.Bold);
            }
            // ReSharper restore HeuristicUnreachableCode
            // ReSharper restore ConditionIsAlwaysTrueOrFalse
            // ReSharper restore ExpressionIsAlwaysNull
            // ReSharper restore SuspiciousTypeConversion.Global
            #endregion

            #region Context menu
            var contextMenuProvider = entry as IContextMenu;
            if (contextMenuProvider != null)
            {
                // Attach the context menu if one is set
                var contextMenu = contextMenuProvider.GetContextMenu();
                if (contextMenu != null) finalNode.ContextMenu = contextMenu;
            }
            #endregion

            return finalNode;
        }
        #endregion

        #region TreeView expand node helper
        /// <summary>
        /// Automatically expand nodes based on the <see cref="textSearch"/> filtering
        /// </summary>
        /// <param name="subTree">The current <see cref="TreeNodeCollection"/> used in recursion</param>
        /// <param name="fullNameExpand">Shall a search for the full name of a tag allow it to be expanded?</param>
        private void ExpandNodes(TreeNodeCollection subTree, bool fullNameExpand)
        {
            foreach (TreeNode node in subTree)
            {
                // Checked nodes and nodes with matches in the last part of their name (the displayed text) shall always be visible
                if (node.Checked || node.Text.ContainsIgnoreCase(textSearch.Text)) node.EnsureVisible();

                // Parent nodes with full name match shall be expanded
                if (fullNameExpand && node.Name.ContainsIgnoreCase(textSearch.Text))
                {
                    node.EnsureVisible();
                    node.Expand();

                    // ... but not beyond the first recursion level
                    ExpandNodes(node.Nodes, fullNameExpand: false);
                }
                else ExpandNodes(node.Nodes, fullNameExpand: fullNameExpand);
            }
        }
        #endregion

        #region Checkbox control
        private void treeView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            // Checking a parent will check all its children
            foreach (TreeNode node in e.Node.Nodes)
                node.Checked = e.Node.Checked;

            // Maintain a list of currently checked bottom-level entries
            if (Nodes.Contains(e.Node.Name))
            {
                T entry = Nodes[e.Node.Name];
                if (e.Node.Checked) _checkedEntries.Add(entry);
                else _checkedEntries.Remove(entry);
                OnCheckedEntriesChanged();
            }
        }
        #endregion
    }
}
