/*
 * Copyright 2006-2013 Bastian Eicher
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
using System.Linq;
using System.Windows.Forms;
using Common.Collections;
using Common.Properties;
using Common.Undo;
using Common.Utils;
using ICSharpCode.TextEditor;

namespace Common.StructureEditor
{
    /// <summary>
    /// A universal editor for hierarchical structures with undo support.
    /// </summary>
    public partial class StructureEditorControl<T> : UserControl
        where T : class, IEquatable<T>, new()
    {
        #region Properties
        private T _target;

        /// <inheritdoc/>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public T Target
        {
            get { return _target; }
            set
            {
                _target = value;
                RebuildTree();
                treeView.SelectedNode = treeView.Nodes[0];
            }
        }

        private CommandManager _commandManager;

        /// <summary>
        /// The undo system to use for editing. Required!
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CommandManager CommandManager
        {
            get { return _commandManager; }
            set
            {
                if (_commandManager != null) _commandManager.Updated -= RebuildTree;
                _commandManager = value;
                if (_commandManager != null) _commandManager.Updated += RebuildTree;
            }
        }
        #endregion

        #region Constructor
        public StructureEditorControl()
        {
            InitializeComponent();
            buttonAdd.Image = Resources.AddButton;
            buttonRemove.Image = Resources.DeleteButton;

            Describe<StructureEditorControl<T>>()
                .AddProperty(x => new PropertyPointer<T>(() => Target, value => Target = value));
        }
        #endregion

        //--------------------//

        #region Undo
        /// <summary>
        /// Passes through to <see cref="Common.Undo.CommandManager.Undo"/> or <see cref="TextEditorControl.Undo"/>.
        /// </summary>
        public void Undo()
        {
            if (xmlEditor.textEditor.EnableUndo) xmlEditor.textEditor.Undo();
            else CommandManager.Undo();
        }

        /// <summary>
        /// Passes through to <see cref="Common.Undo.CommandManager.Redo"/> or <see cref="TextEditorControl.Redo"/>.
        /// </summary>
        public void Redo()
        {
            if (xmlEditor.textEditor.EnableRedo) xmlEditor.textEditor.Redo();
            else CommandManager.Redo();
        }
        #endregion

        #region Describe
        private readonly AggregateDispatcher<object, EntryInfo> _getEntries = new AggregateDispatcher<object, EntryInfo>();
        private readonly AggregateDispatcher<object, ChildInfo> _getPossibleChildren = new AggregateDispatcher<object, ChildInfo>();

        /// <summary>
        /// Adds a <see cref="ContainerDescription{TContainer}"/> used to describe the structure of the data being editing.
        /// </summary>
        /// <typeparam name="TContainer">The type of the container to describe.</typeparam>
        /// <returns>The <see cref="ContainerDescription{TContainer}"/> for use in a "Fluent API" style.</returns>
        public ContainerDescription<TContainer> Describe<TContainer>()
            where TContainer : class
        {
            var description = new ContainerDescription<TContainer>();
            _getEntries.Add<TContainer>(container => description.GetEntrysIn(container).ToList());
            _getPossibleChildren.Add<TContainer>(container => description.GetPossibleChildrenFor(container).ToList());
            return description;
        }
        #endregion

        #region Build nodes
        private Node _reselectNode;

        private void RebuildTree()
        {
            treeView.BeginUpdate();

            treeView.Nodes.Clear();

            _reselectNode = null;
            treeView.Nodes.AddRange(BuildNodes(this));
            _selectedNode = _reselectNode ?? (Node)treeView.Nodes[0];
            treeView.SelectedNode = _selectedNode;
            _selectedNode.Expand();

            treeView.EndUpdate();

            UpdateEditorControl();
        }

        private TreeNode[] BuildNodes(object target)
        {
            var nodes = _getEntries.Dispatch(target).Select(BuildNode);
            return nodes.Cast<TreeNode>().ToArray();
        }

        private Node BuildNode(EntryInfo entry)
        {
            var node = new Node(entry, BuildNodes(entry.Target));
            if (_selectedNode != null && entry.Target == _selectedNode.Entry.Target)
                _reselectNode = node;
            return node;
        }
        #endregion

        #region Selection
        private Node _selectedNode;

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            _selectedNode = (Node)e.Node;
            buttonRemove.Enabled = (e.Node != treeView.Nodes[0]);

            UpdateEditorControl();
            ToXmlString();
        }
        #endregion

        #region Editor control
        private Control _editorControl;

        private void UpdateEditorControl()
        {
            if (_editorControl != null)
            {
                verticalSplitter.Panel2.Controls.Remove(_editorControl);
                _editorControl.Dispose();
            }

            var editorControl = _selectedNode.Entry.GetEditorControl(CommandManager);
            editorControl.Dock = DockStyle.Fill;
            verticalSplitter.Panel2.Controls.Add(editorControl);
            _editorControl = editorControl;
        }
        #endregion

        #region XML
        private void ToXmlString()
        {
            string xmlString = _selectedNode
                .Entry.ToXmlString()
                .GetRightPartAtFirstOccurrence('\n')
                .Replace("\n", Environment.NewLine);

            xmlEditor.SetContent(xmlString, "XML");
        }

        private void xmlEditor_LiveUpdate(string text)
        {
            object newObj = _selectedNode.Entry.FromXmlString(CommandManager, text);
        }
        #endregion

        #region Add/remove
        private void buttonAdd_DropDownOpening(object sender, EventArgs e)
        {
            buttonAdd.DropDownItems.Clear();
            BuildAddDropDownMenu(_selectedNode.Entry.Target);
        }

        private void BuildAddDropDownMenu(object instance)
        {
            foreach (var child in _getPossibleChildren.Dispatch(instance))
            {
                if (child == null) buttonAdd.DropDownItems.Add(new ToolStripSeparator());
                else
                {
                    ChildInfo child1 = child;
                    buttonAdd.DropDownItems.Add(new ToolStripMenuItem(child.Name, null, delegate
                    {
                        object newObj = child1.Create(CommandManager);
                    }));
                }
            }
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            var delete = _selectedNode.Entry.Delete; // Remember target even if selection changes
            treeView.SelectedNode = treeView.SelectedNode.Parent; // Select parent before deleting
            delete(CommandManager);
        }
        #endregion
    }
}
