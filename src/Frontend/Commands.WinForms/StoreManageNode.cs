/*
 * Copyright 2010-2016 Bastian Eicher
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
        public CacheNode BackingNode { get; private set; }

        private readonly StoreManageForm _manageForm;

        /// <summary>
        /// Creates a new store management node.
        /// </summary>
        /// <param name="backingNode">The underlying <see cref="CacheNode"/> containing the cache information.</param>
        /// <param name="manageForm">The form hosting the management UI.</param>
        public StoreManageNode([NotNull] CacheNode backingNode, [NotNull] StoreManageForm manageForm)
        {
            #region Sanity checks
            if (backingNode == null) throw new ArgumentNullException(nameof(backingNode));
            if (manageForm == null) throw new ArgumentNullException(nameof(manageForm));
            #endregion

            BackingNode = backingNode;
            _manageForm = manageForm;
        }
        #endregion

        /// <summary>
        /// The UI path name of this node.
        /// </summary>
        public string Name { get { return BackingNode.Name; } set { BackingNode.Name = value; } }

        /// <inheritdoc/>
        public ContextMenu GetContextMenu()
        {
            var menu = new List<MenuItem>();

            var storeNode = BackingNode as StoreNode;
            if (storeNode != null)
            {
                if (storeNode.Path != null)
                    menu.Add(new MenuItem(Resources.OpenInFileManager, delegate { ProcessUtils.Start(storeNode.Path); }));

                var implementationNode = storeNode as ImplementationNode;
                if (implementationNode != null)
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
