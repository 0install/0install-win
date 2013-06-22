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
        private CommandManager<T> _commandManager;

        /// <summary>
        /// The undo system to use for editing. Required!
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CommandManager<T> CommandManager
        {
            get { return _commandManager; }
            set
            {
                if (_commandManager != null) _commandManager.Updated -= RebuildTree;
                _commandManager = value;
                if (_commandManager != null) _commandManager.Updated += RebuildTree;
                RebuildTree();
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
                .AddProperty(x => new PropertyPointer<T>(() => CommandManager.Target, value => CommandManager.Target = value));
        }
        #endregion

        //--------------------//

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
        private void RebuildTree()
        {
            Node reselectNode = null;
            Func<object, TreeNode[]> buildNodes = null;
            buildNodes = target =>
            {
                var nodes = _getEntries.Dispatch(target).Select(entry =>
                {
                    var node = new Node(entry, buildNodes(entry.Target));
                    if (entry.Target == _selectedTarget) reselectNode = node;
                    return node;
                });
                return nodes.Cast<TreeNode>().ToArray();
            };

            treeView.BeginUpdate();
            treeView.Nodes.Clear();
            treeView.Nodes.AddRange(buildNodes(this));
            treeView.SelectedNode = reselectNode ?? (Node)treeView.Nodes[0];
            treeView.SelectedNode.Expand();
            treeView.EndUpdate();
        }
        #endregion

        #region Undo
        /// <summary>
        /// Passes through to <see cref="Common.Undo.CommandManager.Undo"/> or <see cref="TextEditorControl.Undo"/>.
        /// </summary>
        public void Undo()
        {
            if (xmlEditor.TextEditor.EnableUndo) xmlEditor.TextEditor.Undo();
            else CommandManager.Undo();
        }

        /// <summary>
        /// Passes through to <see cref="Common.Undo.CommandManager.Redo"/> or <see cref="TextEditorControl.Redo"/>.
        /// </summary>
        public void Redo()
        {
            if (xmlEditor.TextEditor.EnableRedo) xmlEditor.TextEditor.Redo();
            else CommandManager.Redo();
        }
        #endregion

        //--------------------//

        #region Add/remove
        private void buttonAdd_DropDownOpening(object sender, EventArgs e)
        {
            buttonAdd.DropDownItems.Clear();
            BuildAddDropDownMenu(SelectedNode.Entry.Target);
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
                        var command = child1.Create();
                        _selectedTarget = command.Value;
                        CommandManager.ExecuteCommand(command);
                    }));
                }
            }
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            var delete = SelectedNode.Entry.Delete; // Remember target even if selection changes
            treeView.SelectedNode = treeView.SelectedNode.Parent; // Select parent before deleting
            delete(CommandManager);
        }
        #endregion

        #region Selection
        private object _selectedTarget;
        private object _editingTarget;
        private object _xmlTarget;

        private Node SelectedNode { get { return (Node)treeView.SelectedNode; } }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            buttonRemove.Enabled = (treeView.Nodes.Count > 0 && e.Node != treeView.Nodes[0]);
            _selectedTarget = SelectedNode.Entry.Target;

            if (_selectedTarget == _editingTarget) _editorControl.Refresh();
            else
            {
                UpdateEditorControl();
                _editingTarget = _selectedTarget;
            }

            if (_selectedTarget != _xmlTarget) ToXmlString();
            _xmlTarget = null;
        }

        private Control _editorControl;

        private void UpdateEditorControl()
        {
            var editorControl = SelectedNode.Entry.GetEditorControl(CommandManager);
            editorControl.Dock = DockStyle.Fill;
            verticalSplitter.Panel2.Controls.Add(editorControl);

            if (_editorControl != null)
            {
                verticalSplitter.Panel2.Controls.Remove(_editorControl);
                _editorControl.Dispose();
            }
            _editorControl = editorControl;
        }

        private void ToXmlString()
        {
            string xmlString = SelectedNode.Entry.ToXmlString().GetRightPartAtFirstOccurrence('\n');
            xmlEditor.SetContent(xmlString, "XML");
        }

        private void xmlEditor_ContentChanged(string text)
        {
            var command = SelectedNode.Entry.FromXmlString(text);
            if (command == null) return;
            _xmlTarget = _selectedTarget = command.Value;
            CommandManager.ExecuteCommand(command);
            xmlEditor.TextEditor.Document.UndoStack.ClearAll();
        }
        #endregion
    }
}
