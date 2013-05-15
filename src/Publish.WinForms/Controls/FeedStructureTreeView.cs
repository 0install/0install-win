/*
 * Copyright 2011-2013 Bastian Eicher, Simon E. Silva Lauinger
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
using System.Linq;
using System.Windows.Forms;
using Common;
using Common.Controls;
using Common.Utils;
using ZeroInstall.Model;
using Binding = ZeroInstall.Model.Binding;

namespace ZeroInstall.Publish.WinForms.Controls
{
    public partial class FeedStructureTreeView : TreeView
    {
        #region Properties
        private FeedEditing _feedEditing;

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public FeedEditing FeedEditing
        {
            get { return _feedEditing; }
            set
            {
                _feedEditing = value;
                if (_feedEditing != null) RebuildTreeNodes();
            }
        }
        #endregion

        #region Contructor
        public FeedStructureTreeView(IContainer container)
        {
            #region Sanity checks
            if (container == null) throw new ArgumentNullException("container");
            #endregion

            container.Add(this);

            InitializeComponent();
            SetupNodeBindings();
        }
        #endregion

        #region Tree node building
        private void RebuildTreeNodes()
        {
            BeginUpdate();
            Nodes.Clear();
            Nodes.Add(GetTreeNodes(_feedEditing.Feed));
            EndUpdate();

            ExpandAll();
        }

        /// <summary>
        /// Generates a <see cref="TreeNode"/> with child elements representing an object from <see cref="ZeroInstall.Model"/>.
        /// </summary>
        /// <param name="data">The <see cref="ZeroInstall.Model"/> to represent.</param>
        private TreeNode GetTreeNodes(object data)
        {
            #region Sanity checks
            if (data == null) throw new ArgumentNullException("data");
            #endregion

            var node = new TreeNode(data.ToString()) {Tag = data};

            node.Nodes.AddRange(BuildTreeNodesHelper<IElementContainer, Element>(data, container => container.Elements));
            node.Nodes.AddRange(BuildTreeNodesHelper<IBindingContainer, Binding>(data, container => container.Bindings));
            node.Nodes.AddRange(BuildTreeNodesHelper<IDependencyContainer, Dependency>(data, container => container.Dependencies));
            node.Nodes.AddRange(BuildTreeNodesHelper<Element, Command>(data, container => container.Commands));
            node.Nodes.AddRange(BuildTreeNodesHelper<Implementation, RetrievalMethod>(data, container => container.RetrievalMethods));

            var command = data as Command;
            if (command != null)
            {
                if (command.WorkingDir != null) node.Nodes.Add(GetTreeNodes(command.WorkingDir));
                if (command.Runner != null) node.Nodes.Add(GetTreeNodes(command.Runner));
            }

            return node;
        }

        /// <summary>
        /// Helper method for <see cref="GetTreeNodes"/> that takes objects of unkown type, checking whether they are containers for specific <see cref="ZeroInstall.Model"/> objects.
        /// </summary>
        /// <typeparam name="TContainer">A type that contains <typeparamref name="TEntry"/> child elements.</typeparam>
        /// <typeparam name="TEntry">The type of elements contained within <typeparamref name="TContainer"/>s.</typeparam>
        /// <param name="data">An object potentially of the type <typeparamref name="TContainer"/>.</param>
        /// <param name="getChildren">A delegate describing how to get a collection of <typeparamref name="TEntry"/>s from a <typeparamref name="TContainer"/>.</param>
        /// <returns>An array of <see cref="TreeNode"/>s representing the <typeparamref name="TEntry"/>s in <paramref name="data"/> if any.</returns>
        private TreeNode[] BuildTreeNodesHelper<TContainer, TEntry>(object data, MapAction<TContainer, IEnumerable<TEntry>> getChildren)
            where TContainer : class
            where TEntry : class
        {
            var elementContainer = data as TContainer;
            if (elementContainer == null) return new TreeNode[0];

            return getChildren(elementContainer).Select(GetTreeNodes).ToArray();
        }
        #endregion

        #region Node bindings
        //_feedEditing.ExecuteCommand(new SetValueCommand<TAbstractEntry>(getPointer(container), new TSpecialEntry()));
        //_feedEditing.ExecuteCommand(new AddToCollection<TAbstractEntry>(getList(container), new TSpecialEntry()));
        //RebuildTreeNodes();
        

        private void SetupNodeBindings()
        {
            //SetupListNodeBinding<IElementContainer, Element, Implementation, ImplementationDialog>(container => container.Elements);
            SetupListNodeBinding<IElementContainer, Element, PackageImplementation>(container => container.Elements);
            //SetupListNodeBinding<IElementContainer, Element, Group, GroupDialog>(container => container.Elements);

            SetupListNodeBinding<IBindingContainer, Binding, EnvironmentBinding>(container => container.Bindings);
            SetupListNodeBinding<IBindingContainer, Binding, OverlayBinding>(container => container.Bindings);
            SetupListNodeBinding<IBindingContainer, Binding, ExecutableInVar>(container => container.Bindings);
            SetupListNodeBinding<IBindingContainer, Binding, ExecutableInPath>(container => container.Bindings);

            SetupListNodeBinding<IDependencyContainer, Dependency, Dependency>(container => container.Dependencies);

            SetupListNodeBinding<Element, Command, Command>(element => element.Commands);
            SetupPropertyNodeBinding<Command, Runner>(command => new PropertyPointer<Runner>(() => command.Runner, newValue => command.Runner = newValue));

            //SetupListNodeBinding<Implementation, RetrievalMethod, Archive, ArchiveDialog>(implementation => implementation.RetrievalMethods);
            //SetupListNodeBinding<Implementation, RetrievalMethod, Recipe, RecipeDialog>(implementation => implementation.RetrievalMethods);
        }

        private void SetupPropertyNodeBinding<TContainer, TEntry, TEditor>(MapAction<TContainer, PropertyPointer<TEntry>> getPointer)
            where TContainer : class
            where TEntry : class, ICloneable, new()
            where TEditor : class, IEntryEditor<TEntry>, new()
        {
            NodeMouseDoubleClick += delegate(object sender, TreeNodeMouseClickEventArgs nodeArgs)
            {
                // Type must match exactly
                if (nodeArgs.Node.Tag.GetType() != typeof(TEntry)) return;

                nodeArgs.Node.Toggle();
                var entry = (TEntry)nodeArgs.Node.Tag;
                var parent = nodeArgs.Node.Parent.Tag as TContainer;
                if (parent != null)
                {
                    // Clone entry for undoable modification
                    var clonedEntry = (TEntry)entry.Clone();

                    //using (var editor = new TEditor())
                    //{
                    //    if (editor.ShowDialog(this, clonedEntry) == DialogResult.OK)
                    //    {
                    //        _feedEditing.ExecuteCommand(new SetValueCommand<TEntry>(getPointer(parent), clonedEntry));
                    //        RebuildTreeNodes();
                    //    }
                    //}
                }
            };
        }

        private void SetupPropertyNodeBinding<TContainer, TEntry>(MapAction<TContainer, PropertyPointer<TEntry>> getPointer)
            where TContainer : class
            where TEntry : class, ICloneable, new()
        {
            SetupPropertyNodeBinding<TContainer, TEntry, EditorDialog<TEntry>>(getPointer);
        }

        private void SetupListNodeBinding<TContainer, TAbstractEntry, TSpecialEntry, TEditor>(MapAction<TContainer, IList<TAbstractEntry>> getList)
            where TContainer : class
            where TAbstractEntry : class, ICloneable
            where TSpecialEntry : class, TAbstractEntry, new()
            where TEditor : class, IEntryEditor<TSpecialEntry>, new()
        {
            NodeMouseDoubleClick += delegate(object sender, TreeNodeMouseClickEventArgs nodeArgs)
            {
                // Type must match exactly
                if (nodeArgs.Node.Tag.GetType() != typeof(TSpecialEntry)) return;

                nodeArgs.Node.Toggle();
                var entry = (TSpecialEntry)nodeArgs.Node.Tag;
                var parent = nodeArgs.Node.Parent.Tag as TContainer;
                if (parent != null)
                {
                    // Clone entry for undoable modification
                    var clonedEntry = (TSpecialEntry)entry.Clone();

                    //using (var editor = new TEditor())
                    //{
                    //    if (editor.ShowDialog(this, clonedEntry) == DialogResult.OK)
                    //    {
                    //        // Prepare a list of 1 to 3 commands to be executed as a single transaction
                    //        var commandList = new List<IUndoCommand>(3)
                    //        {
                    //            // Replace original entry with cloned and modified one
                    //            new SetInList<TAbstractEntry>(getList(parent), entry, clonedEntry)
                    //        };

                    //        #region Update manifest digest
                    //        var digestProvider = editor as IDigestProvider;
                    //        var implementation = parent as ImplementationBase;
                    //        if (digestProvider != null && implementation != null)
                    //        {
                    //            // ToDo: Warn when changing an existing digest

                    //            // Set the ManifestDigest entry
                    //            commandList.Add(new SetValueCommand<ManifestDigest>(
                    //                new PropertyPointer<ManifestDigest>(() => implementation.ManifestDigest, newValue => implementation.ManifestDigest = newValue),
                    //                digestProvider.ManifestDigest));

                    //            // Set the implementation ID unless its already something custom
                    //            if (string.IsNullOrEmpty(implementation.ID) || implementation.ID.StartsWith("sha1=new"))
                    //            {
                    //                commandList.Add(new SetValueCommand<string>(
                    //                    new PropertyPointer<string>(() => implementation.ID, newValue => implementation.ID = newValue),
                    //                    "sha1new=" + digestProvider.ManifestDigest.Sha1New));
                    //            }
                    //        }
                    //        #endregion

                    //        // Execute the transaction
                    //        _feedEditing.ExecuteCommand(new CompositeCommand(commandList));
                    //        RebuildTreeNodes();
                    //    }
                    //}
                }
            };
        }

        private void SetupListNodeBinding<TContainer, TAbstractEntry, TSpecialEntry>(MapAction<TContainer, IList<TAbstractEntry>> getList)
            where TContainer : class
            where TAbstractEntry : class, ICloneable
            where TSpecialEntry : class, TAbstractEntry, new()
        {
            SetupListNodeBinding<TContainer, TAbstractEntry, TSpecialEntry, EditorDialog<TSpecialEntry>>(getList);
        }
        #endregion
    }
}
