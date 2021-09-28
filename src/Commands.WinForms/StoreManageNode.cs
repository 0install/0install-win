// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows.Forms;
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
    [SuppressMessage("ReSharper", "AsyncVoidLambda")]
    public sealed class StoreManageNode : INamed, IContextMenu
    {
        #region Dependencies
        /// <summary>
        /// The underlying <see cref="CacheNode"/> containing the cache information.
        /// </summary>
        public CacheNode BackingNode { get; }

        private readonly StoreManageForm _manageForm;

        /// <summary>
        /// Creates a new store management node.
        /// </summary>
        /// <param name="backingNode">The underlying <see cref="CacheNode"/> containing the cache information.</param>
        /// <param name="manageForm">The form hosting the management UI.</param>
        public StoreManageNode(CacheNode backingNode, StoreManageForm manageForm)
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
        public ContextMenuStrip GetContextMenu()
        {
            var menu = new ContextMenuStrip();

            if (BackingNode is StoreNode storeNode)
            {
                if (storeNode.Path != null)
                    menu.Items.Add(Resources.OpenInFileManager, null, delegate { Process.Start(storeNode.Path); });

                if (storeNode is ImplementationNode implementationNode)
                {
                    menu.Items.Add(Resources.Verify, null, async delegate
                    {
                        try
                        {
                            using var handler = new DialogTaskHandler(_manageForm);
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

                        await _manageForm.RefreshListAsync();
                    });
                }
            }

            menu.Items.Add(Resources.Remove, null, async delegate
            {
                using var handler = new DialogTaskHandler(_manageForm);
                if (handler.Ask(Resources.DeleteEntry))
                {
                    try
                    {
                        // ReSharper disable once AccessToDisposedClosure
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

                    await _manageForm.RefreshListAsync();
                }
            });

            return menu;
        }
    }
}
