// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System.Diagnostics;
using NanoByte.Common.Native;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.ViewModel;

namespace ZeroInstall.Commands.EtoForms;

/// <summary>
/// Wraps a <see cref="CacheNode"/> and adds a context menu.
/// </summary>
/// <param name="form">The form this cache node is displayed on.</param>
/// <param name="innerNode">The underlying <see cref="CacheNode"/> containing the cache information.</param>
[SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Justification = "Comparison only used for string sorting in UI lists")]
internal sealed class CacheNodeWithContextMenu(StoreManageForm form, CacheNode innerNode) : INamed, IContextMenu
{
    /// <summary>
    /// The underlying <see cref="CacheNode"/> containing the cache information.
    /// </summary>
    public CacheNode InnerNode { get; } = innerNode;

    /// <summary>
    /// The UI path name of this node.
    /// </summary>
    public string Name { get => InnerNode.Name; set => throw new NotSupportedException(); }

    /// <inheritdoc/>
    public ContextMenu GetContextMenu()
    {
        var openItem = new ButtonMenuItem {Text = Commands.Properties.Resources.OpenInFileManager};
        openItem.Click += delegate { OpenInFileManager(); };

        var removeItem = new ButtonMenuItem {Text = Commands.Properties.Resources.Remove};
        removeItem.Click += delegate
        {
            if (Msg.YesNo(form, Commands.Properties.Resources.DeleteEntry, MsgSeverity.Warn))
            {
                try { Remove(); }
                catch (OperationCanceledException) {}
                form.RefreshList();
            }
        };

        var menu = new ContextMenu(openItem, removeItem);

        if (InnerNode is ImplementationNode)
        {
            var verifyItem = new ButtonMenuItem {Text = Commands.Properties.Resources.Verify};
            verifyItem.Click += delegate
            {
                try { Verify(); }
                catch (OperationCanceledException) {}
            };
            menu.Items.Add(verifyItem);
        }

        return menu;
    }

    private void OpenInFileManager()
    {
        if (WindowsUtils.IsWindows)
        {
            try
            {
                Process.Start("explorer.exe", (InnerNode is FeedNode ? "/select," : "") + InnerNode.Path.EscapeArgument());
            }
            #region Error handling
            catch (Exception ex)
            {
                Msg.Inform(form, ex.Message, MsgSeverity.Error);
            }
            #endregion
        }
        else
        {
            // Feeds are files, so open the containing directory instead
            CommandUtils.Open(form, InnerNode is FeedNode ? System.IO.Path.GetDirectoryName(InnerNode.Path) ?? InnerNode.Path : InnerNode.Path);
        }
    }

    public void Remove()
    {
        try
        {
            InnerNode.Remove(form.Services.FeedCache, form.Services.ImplementationStore);
        }
        #region Error handling
        catch (Exception ex) when (ex is ImplementationNotFoundException or IOException or UnauthorizedAccessException)
        {
            Msg.Inform(form, ex.Message, MsgSeverity.Error);
        }
        #endregion
    }

    public void Verify()
    {
        try
        {
            (InnerNode as ImplementationNode)?.Verify(form.Services.ImplementationStore);
        }
        #region Error handling
        catch (Exception ex) when (ex is ImplementationNotFoundException or IOException or UnauthorizedAccessException)
        {
            Msg.Inform(form, ex.Message, MsgSeverity.Warn);
        }
        #endregion
    }
}
