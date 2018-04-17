// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows.Forms;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Controls;
using NanoByte.Common.Tasks;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Store.ViewModel;

namespace ZeroInstall.Commands.WinForms
{
    /// <summary>
    /// Wraps a <see cref="CacheNode"/> to add a context menu.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Justification = "Comparison only used for string sorting in UI lists")]
    public sealed class StoreManageNode : INamed<StoreManageNode>, IContextMenu
    {
        #region Dependencies
        /// <summary>
        /// The underlying <see cref="CacheNode"/> containing the cache information.
        /// </summary>
        [NotNull]
        public CacheNode BackingNode { get; }

        private readonly StoreManageForm _manageForm;

        /// <summary>
        /// Creates a new store management node.
        /// </summary>
        /// <param name="backingNode">The underlying <see cref="CacheNode"/> containing the cache information.</param>
        /// <param name="manageForm">The form hosting the management UI.</param>
        public StoreManageNode([NotNull] CacheNode backingNode, [NotNull] StoreManageForm manageForm)
        {
            BackingNode = backingNode ?? throw new ArgumentNullException(nameof(backingNode));
            _manageForm = manageForm ?? throw new ArgumentNullException(nameof(manageForm));
        }
        #endregion

        /// <summary>
        /// The UI path name of this node.
        /// </summary>
        public string Name { get => BackingNode.Name; set => BackingNode.Name = value; }

        /// <inheritdoc/>
        public ContextMenu GetContextMenu()
        {
            var menu = new List<MenuItem>();

            if (BackingNode is StoreNode storeNode)
            {
                if (storeNode.Path != null)
                    menu.Add(new MenuItem(Resources.OpenInFileManager, delegate { ProcessUtils.Start(storeNode.Path); }));

                if (storeNode is ImplementationNode implementationNode)
                {
                    using (var handler = new DialogTaskHandler(_manageForm))
                    {
                        menu.Add(new MenuItem(Resources.Verify, delegate
                        {
                            try
                            {
                                implementationNode.Verify(handler);
                            }
                            #region Error handling
                            catch (OperationCanceledException)
                            {}
                            catch (IOException ex)
                            {
                                Msg.Inform(null, ex.Message, MsgSeverity.Warn);
                            }
                            catch (UnauthorizedAccessException ex)
                            {
                                Msg.Inform(null, ex.Message, MsgSeverity.Warn);
                            }
                            #endregion

                            _manageForm.RefreshList();
                        }));
                    }
                }
            }

            menu.Add(new MenuItem(Resources.Remove, delegate
            {
                using (var handler = new DialogTaskHandler(_manageForm))
                {
                    if (handler.Ask(Resources.DeleteEntry))
                    {
                        try
                        {
                            handler.RunTask(new SimpleTask(Resources.DeletingImplementations, () => BackingNode.Delete(handler)));
                        }
                        #region Error handling
                        catch (KeyNotFoundException ex)
                        {
                            Msg.Inform(null, ex.Message, MsgSeverity.Error);
                        }
                        catch (IOException ex)
                        {
                            Msg.Inform(null, ex.Message, MsgSeverity.Error);
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            Msg.Inform(null, ex.Message, MsgSeverity.Error);
                        }
                        #endregion

                        _manageForm.RefreshList();
                    }
                }
            }));

            return new ContextMenu(menu.ToArray());
        }

        #region Comparison
        /// <inheritdoc/>
        public int CompareTo(StoreManageNode other)
        {
            #region Sanity checks
            if (other == null) throw new ArgumentNullException(nameof(other));
            #endregion

            return string.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
        }
        #endregion
    }
}
