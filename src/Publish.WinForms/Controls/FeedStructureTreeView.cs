using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Common;
using Common.Undo;
using ZeroInstall.Model;
using ZeroInstall.Publish.WinForms.FeedStructure;
using Binding = ZeroInstall.Model.Binding;

namespace ZeroInstall.Publish.WinForms.Controls
{
    public partial class FeedStructureTreeView : TreeView
    {
        #region Variables
        private FeedEditing _feedEditing;
        #endregion

        #region Properties
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public FeedEditing FeedEditing
        {
            get
            {
                return _feedEditing;
            }
            set
            {
                _feedEditing = value;
                StartBuildingTreeNodes();
            }
        }
        #endregion

        #region Contructor
        public FeedStructureTreeView(FeedEditing toUse)
        {
            InitializeComponent();
            Initialize();
            FeedEditing = toUse;
        }

        public FeedStructureTreeView(IContainer container, FeedEditing toUse) : this(toUse)
        {
            container.Add(this);
        }
        #endregion

        #region Initialization
        private void Initialize()
        {
            SetupDoubleClickHooks();
        }

        private void SetupDoubleClickHooks()
        {
            SetupFeedStructureHooks<IElementContainer, Element, Implementation>(implementation => new ImplementationForm { Implementation = implementation }, container => container.Elements);
            SetupFeedStructureHooks<IElementContainer, Element, PackageImplementation>(implementation => new PackageImplementationForm { PackageImplementation = implementation }, container => container.Elements);
            SetupFeedStructureHooks<IElementContainer, Element, Group>(group => new GroupForm { Group = group }, container => container.Elements);

            SetupFeedStructureHooks<IBindingContainer, Binding, EnvironmentBinding>(binding => new EnvironmentBindingForm { EnvironmentBinding = binding }, container => container.Bindings);
            SetupFeedStructureHooks<IBindingContainer, Binding, OverlayBinding>(binding => new OverlayBindingForm { OverlayBinding = binding }, container => container.Bindings);

            SetupFeedStructureHooks<IDependencyContainer, Dependency, Dependency>(dependency => new DependencyForm { Dependency = dependency }, container => container.Dependencies);

            SetupFeedStructureHooks<Element, Command, Command>(command => new CommandForm { Command = command }, element => element.Commands);
            SetupFeedStructureHooks<Command, Runner, Runner>(runner => new RunnerForm { Runner = runner }, command => new PropertyPointer<Runner>(() => command.Runner, newValue => command.Runner = newValue));

            SetupFeedStructureHooks<Implementation, RetrievalMethod, Archive>(archive => new ArchiveForm { Archive = archive }, implementation => implementation.RetrievalMethods);
            SetupFeedStructureHooks<Implementation, RetrievalMethod, Recipe>(recipe => new RecipeForm { Recipe = recipe }, implementation => implementation.RetrievalMethods);
        }
        #endregion

        #region Insert tree nodes
        private void StartBuildingTreeNodes()
        {
            BeginUpdate();
            Nodes.Clear();
            Nodes.Add(BuildTreeNodes(_feedEditing.Feed));
            EndUpdate();

            ExpandAll();
        }

        /// <summary>
        /// Generates a <see cref="TreeNode"/> with child elements representing an object from <see cref="ZeroInstall.Model"/>.
        /// </summary>
        /// <param name="data">The <see cref="ZeroInstall.Model"/> to represent.</param>
        private TreeNode BuildTreeNodes(object data)
        {
            #region Sanity checks
            if (data == null) throw new ArgumentNullException("data");
            #endregion

            var node = new TreeNode(data.ToString()) { Tag = data };
            node.ContextMenuStrip = AddContextMenuStrip(node.Tag);

            node.Nodes.AddRange(BuildTreeNodesHelper<IElementContainer, Element>(data, container => container.Elements));
            node.Nodes.AddRange(BuildTreeNodesHelper<IBindingContainer, Binding>(data, container => container.Bindings));
            node.Nodes.AddRange(BuildTreeNodesHelper<IDependencyContainer, Dependency>(data, container => container.Dependencies));
            node.Nodes.AddRange(BuildTreeNodesHelper<Element, Command>(data, container => container.Commands));
            node.Nodes.AddRange(BuildTreeNodesHelper<Implementation, RetrievalMethod>(data, container => container.RetrievalMethods));

            var command = data as Command;
            if (command != null)
            {
                if (command.WorkingDir != null) node.Nodes.Add(BuildTreeNodes(command.WorkingDir));
                if (command.Runner != null) node.Nodes.Add(BuildTreeNodes(command.Runner));
            }

            return node;
        }

        /// <summary>
        /// Helper method for <see cref="BuildTreeNodes"/> that takes objects of unkown type, checking whether they are containers for specific <see cref="ZeroInstall.Model"/> objects.
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

            var nodes = new C5.LinkedList<TreeNode>();
            foreach (var element in getChildren(elementContainer))
                nodes.Add(BuildTreeNodes(element));
            return nodes.ToArray();
        }

        private ContextMenuStrip AddContextMenuStrip(object data)
        {
            #region Sanity checks
            if(data == null) throw new ArgumentNullException("data");
            #endregion

            var contextMenuStrip = new ContextMenuStrip();
            var menuItem = SetupContextMenuStripHooks<IElementContainer, Element, Group>(data, "Group", container => container.Elements);
            if (menuItem != null) contextMenuStrip.Items.Add(menuItem);
            menuItem = SetupContextMenuStripHooks<IElementContainer, Element, Implementation>(data, "Implementation", container => container.Elements);
            if (menuItem != null) contextMenuStrip.Items.Add(menuItem);
            menuItem = SetupContextMenuStripHooks<IElementContainer, Element, PackageImplementation>(data, "PackageImplementation", container => container.Elements);
            if (menuItem != null) contextMenuStrip.Items.Add(menuItem);
            menuItem = SetupContextMenuStripHooks<IBindingContainer, Binding, EnvironmentBinding>(data, "EnvironmentBinding", container => container.Bindings);
            if (menuItem != null) contextMenuStrip.Items.Add(menuItem);
            menuItem = SetupContextMenuStripHooks<IBindingContainer, Binding, OverlayBinding>(data, "OverlayBinding", container => container.Bindings);
            if (menuItem != null) contextMenuStrip.Items.Add(menuItem);

            menuItem = SetupContextMenuStripHooks<IDependencyContainer, Dependency, Dependency>(data, "Dependency", container => container.Dependencies);
            if (menuItem != null) contextMenuStrip.Items.Add(menuItem);

            menuItem = SetupContextMenuStripHooks<Element, Command, Command>(data, "Command", element => element.Commands);
            if (menuItem != null) contextMenuStrip.Items.Add(menuItem);
            menuItem = SetupContextMenuStripHooks<Command, Runner, Runner>(data, "Runner", command => new PropertyPointer<Runner>(() => command.Runner, newValue => command.Runner = newValue));
            if (menuItem != null) contextMenuStrip.Items.Add(menuItem);

            menuItem = SetupContextMenuStripHooks<Implementation, RetrievalMethod, Archive>(data, "Archive", implementation => implementation.RetrievalMethods);
            if (menuItem != null) contextMenuStrip.Items.Add(menuItem);
            menuItem = SetupContextMenuStripHooks<Implementation, RetrievalMethod, Recipe>(data, "Recipe", implementation => implementation.RetrievalMethods);
            if (menuItem != null) contextMenuStrip.Items.Add(menuItem);

            return contextMenuStrip;
        }
        #endregion

        #region Context menu generation
        private ToolStripItem SetupContextMenuStripHooks<TContainer, TAbstractEntry, TSpecialEntry>(object data, String text, MapAction<TContainer, PropertyPointer<TAbstractEntry>> getPointer)
            where TContainer : class
            where TAbstractEntry : class, ICloneable
            where TSpecialEntry : class, TAbstractEntry, new()
        {
            #region Sanity checks
            if(data == null) throw new ArgumentNullException("data");
            if (text == null) throw new ArgumentNullException("text");
            if (getPointer == null) throw new ArgumentNullException("getPointer");
            #endregion

            if (!(data is TContainer)) return null;
            var container = (TContainer) data;

            var toolStripItem = new ToolStripMenuItem(text, null, delegate
                {
                    _feedEditing.ExecuteCommand(new SetValueCommand<TAbstractEntry>(getPointer(container), new TSpecialEntry()));
                    StartBuildingTreeNodes();
                });

            return toolStripItem;
        }

        private ToolStripItem SetupContextMenuStripHooks<TContainer, TAbstractEntry, TSpecialEntry>(object data, String text, MapAction<TContainer, IList<TAbstractEntry>> getList)
            where TContainer : class
            where TAbstractEntry : class, ICloneable
            where TSpecialEntry : class, TAbstractEntry, new()
        {
            #region Sanity checks
            if (data == null) throw new ArgumentNullException("data");
            if (text == null) throw new ArgumentNullException("text");
            if (getList == null) throw new ArgumentNullException("getList");
            #endregion

            if (!(data is TContainer)) return null;
            var container = (TContainer)data;

            var toolStripItem = new ToolStripMenuItem(text, null, delegate
                {
                    _feedEditing.ExecuteCommand(new AddToCollection<TAbstractEntry>(getList(container), new TSpecialEntry()));
                    StartBuildingTreeNodes();
                });

            return toolStripItem;
        }
        #endregion

        #region Click events
        private void SetupFeedStructureHooks<TContainer, TAbstractEntry, TSpecialEntry>(MapAction<TSpecialEntry, Form> getEditDialog, MapAction<TContainer, PropertyPointer<TAbstractEntry>> getPointer)
            where TContainer : class
            where TAbstractEntry : class, ICloneable
            where TSpecialEntry : class, TAbstractEntry, new()
        {
            #region Sanity checks
            if (getEditDialog == null) throw new ArgumentNullException("getEditDialog");
            if (getPointer == null) throw new ArgumentNullException("getPointer");
            #endregion

            #region Edit dialog
            NodeMouseDoubleClick += (sender, nodeArgs) =>
            {
                // Type must match exactly
                if (nodeArgs.Node.Tag.GetType() != typeof(TSpecialEntry)) return;

                var entry = (TSpecialEntry)nodeArgs.Node.Tag;
                var parent = nodeArgs.Node.Parent.Tag as TContainer;
                if (parent != null)
                {
                    // Clone entry for undoable modification
                    var clonedEntry = (TSpecialEntry)entry.Clone();

                    using (var dialog = getEditDialog(clonedEntry))
                    {
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            _feedEditing.ExecuteCommand(new SetValueCommand<TAbstractEntry>(getPointer(parent), clonedEntry));
                        }
                    }
                }
            };
            #endregion
        }

        private void SetupFeedStructureHooks<TContainer, TAbstractEntry, TSpecialEntry>(MapAction<TSpecialEntry, Form> getEditDialog, MapAction<TContainer, IList<TAbstractEntry>> getList)
            where TContainer : class
            where TAbstractEntry : class, ICloneable
            where TSpecialEntry : class, TAbstractEntry, new()
        {
            #region Sanity checks
            if (getEditDialog == null) throw new ArgumentNullException("getEditDialog");
            if (getList == null) throw new ArgumentNullException("getList");
            #endregion

            #region Edit dialog
            NodeMouseDoubleClick += (sender, nodeArgs) =>
            {
                // Type must match exactly
                if (nodeArgs.Node.Tag.GetType() != typeof(TSpecialEntry)) return;

                var entry = (TSpecialEntry)nodeArgs.Node.Tag;
                var parent = nodeArgs.Node.Parent.Tag as TContainer;
                if (parent != null)
                {
                    // Clone entry for undoable modification
                    var clonedEntry = (TSpecialEntry)entry.Clone();

                    using (var dialog = getEditDialog(clonedEntry))
                    {
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            // Prepare a list of 1 to 3 commands to be executed as a single transaction
                            var commandList = new List<IUndoCommand>(3)
                            {
                                // Replace original entry with cloned and modified one
                                new SetInList<TAbstractEntry>(getList(parent), entry, clonedEntry)
                            };

                            #region Update manifest digest
                            var digestProvider = dialog as IDigestProvider;
                            var implementation = parent as ImplementationBase;
                            if (digestProvider != null && implementation != null)
                            {
                                // ToDo: Warn when changing an existing digest

                                // Set the ManifestDigest entry
                                commandList.Add(new SetValueCommand<ManifestDigest>(
                                    new PropertyPointer<ManifestDigest>(() => implementation.ManifestDigest, newValue => implementation.ManifestDigest = newValue),
                                    digestProvider.ManifestDigest));

                                // Set the implementation ID unless its already something custom
                                if (string.IsNullOrEmpty(implementation.ID) || implementation.ID.StartsWith("sha1=new"))
                                {
                                    commandList.Add(new SetValueCommand<string>(
                                        new PropertyPointer<string>(() => implementation.ID, newValue => implementation.ID = newValue),
                                        "sha1new=" + digestProvider.ManifestDigest.Sha1New));
                                }
                            }
                            #endregion

                            // Execute the transaction
                            _feedEditing.ExecuteCommand(new CompositeCommand(commandList));
                        }
                    }
                }
            };
            #endregion
        }
        #endregion

        #region Drag & Drop
        //TODO Add drag and drop code here
        #endregion
    }
}
