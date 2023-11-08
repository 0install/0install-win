// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System.Diagnostics;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.ViewModel;

namespace ZeroInstall.Commands.WinForms;

/// <summary>
/// Wraps a <see cref="CacheNode"/> and adds a context menu.
/// </summary>
[SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Justification = "Comparison only used for string sorting in UI lists")]
internal sealed class CacheNodeWithContextMenu : INamed, IContextMenu
{
    private readonly StoreManageForm _form;

    /// <summary>
    /// The underlying <see cref="CacheNode"/> containing the cache information.
    /// </summary>
    public CacheNode InnerNode { get; }

    /// <param name="form">The form this cache node is displayed on.</param>
    /// <param name="innerNode">The underlying <see cref="CacheNode"/> containing the cache information.</param>
    public CacheNodeWithContextMenu(StoreManageForm form, CacheNode innerNode)
    {
        _form = form;
        InnerNode = innerNode;
    }

    /// <summary>
    /// The UI path name of this node.
    /// </summary>
    public string Name { get => InnerNode.Name; set => throw new NotSupportedException(); }

    /// <inheritdoc/>
    public ContextMenuStrip GetContextMenu()
    {
        var menu = new ContextMenuStrip
        {
            Items =
            {
                {
                    Resources.OpenInFileManager, null, delegate
                    {
                        Process.Start("explorer.exe", (InnerNode is FeedNode ? "/select," : "") + InnerNode.Path.EscapeArgument());
                    }
                },
                {
                    Resources.Remove, null, delegate
                    {
                        if (Msg.YesNo(_form, Resources.DeleteEntry, MsgSeverity.Warn))
                        {
                            try { Remove(); }
                            catch (OperationCanceledException) {}
                            _form.RefreshList();
                        }
                    }
                }
            }
        };

        if (InnerNode is ImplementationNode)
        {
            menu.Items.Add(Resources.Verify, null, delegate
            {
                try { Verify(); }
                catch (OperationCanceledException) {}
            });
        }

        return menu;
    }

    public void Remove()
    {
        try
        {
            InnerNode.Remove(_form.Services.FeedCache, _form.Services.ImplementationStore);
        }
        #region Error handling
        catch (Exception ex) when (ex is ImplementationNotFoundException or IOException or UnauthorizedAccessException)
        {
            Msg.Inform(null, ex.Message, MsgSeverity.Error);
        }
        #endregion
    }

    public void Verify()
    {
        try
        {
            (InnerNode as ImplementationNode)?.Verify(_form.Services.ImplementationStore);
        }
        #region Error handling
        catch (Exception ex) when (ex is ImplementationNotFoundException or IOException or UnauthorizedAccessException)
        {
            Msg.Inform(null, ex.Message, MsgSeverity.Warn);
        }
        #endregion
    }
}
