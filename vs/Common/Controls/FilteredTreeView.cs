using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using Common.Collections;
using Common.Helpers;

namespace Common.Controls
{
    /// <summary>
    /// Displays a list of <see cref="IHighlightable"/> <see cref="INamed"/>s in a <see cref="TreeView"/> with incremental search.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="IHighlightable"/> <see cref="INamed"/> to list.</typeparam>
    [Description("Displays a list of INamed in a TreeView with incremental search.")]
    public sealed partial class FilteredTreeView<T> : UserControl where T : class, INamed, IHighlightable
    {
        #region Events
        /// <summary>
        /// Occurs whenever <see cref="SelectedEntry"/> has been changed.
        /// </summary>
        [Description("Occurs whenever SelectedEntry has been changed.")]
        public event EventHandler SelectedEntryChanged;

        private void OnSelectedEntryChanged()
        {
            if (SelectedEntryChanged != null && !_supressEvents) SelectedEntryChanged(this, EventArgs.Empty);
        }
        #endregion

        #region Variables
        /// <summary>Suppress the execution of <see cref="SelectedEntryChanged"/>.</summary>
        private bool _supressEvents;
        #endregion

        #region Properties
        private INamedCollection<T> _entries;
        /// <summary>
        /// The <see cref="IHighlightable"/> <see cref="INamed"/>s to be listed in the <see cref="TreeView"/>.
        /// </summary>
        [Browsable(false)]
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "This control is supposed to represent a live and mutable collection")]
        public INamedCollection<T> Entries
        {
            get { return _entries; }
            set { _entries = value; UpdateList(); }
        }


        private T _selectedEntry;
        /// <summary>
        /// The <see cref="IHighlightable"/> <see cref="INamed"/> currently selected in the <see cref="TreeView"/>; <see langword="null"/> for no selection.
        /// </summary>
        [Browsable(false)]
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

        private char _seperator = '.';
        /// <summary>
        /// The namespace seperator used in <see cref="INamed.Name"/>. This controls how the tree structure is generated.
        /// </summary>
        [DefaultValue('.')]
        public char Seperator { get { return _seperator; } set { _seperator = value; UpdateList(); } }
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
                    else if (StringHelper.Contains(classEntry.Name, textSearch.Text))
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
            string[] nameSplit = name.Split(_seperator);

            // Start off at the top-level
            TreeNodeCollection subTree = treeView.Nodes;

            string partialName = "";
            for (int i = 0; i < nameSplit.Length - 1; i++)
            {
                partialName += nameSplit[i] + ".";
                // Try to use a pre-existing node for namespace-subtrees, if non-existant create new ones
                TreeNode node = subTree[partialName] ?? subTree.Add(partialName, nameSplit[i]);
                subTree = node.Nodes;
            }

            // Create node storing full name, using last part as visible text
            TreeNode finalNode = subTree.Add(name, nameSplit[nameSplit.Length - 1]);

            // Apply the highlighting color if one is set
            if (entry.HighlightColor != Color.Empty) finalNode.ForeColor = entry.HighlightColor;

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
                if (StringHelper.Contains(node.Text, textSearch.Text)) node.EnsureVisible();

                // Nodes with matches in their full name shall be expanded...
                if (fullNameExpand && StringHelper.Contains(node.Name, textSearch.Text))
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