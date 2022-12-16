// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using NanoByte.Common.Threading;

namespace ZeroInstall;

/// <summary>
/// Uses <see cref="MainForm"/> to show <see cref="ITask"/> progress during the bootstrap process.
/// </summary>
/// <remarks>This class manages a GUI thread with an independent message queue. Invoking methods on the right thread is handled automatically.</remarks>
public class GuiBootstrapHandler : GuiTaskHandlerBase, IBootstrapHandler
{
    private readonly AsyncFormWrapper<MainForm> _wrapper;

    public GuiBootstrapHandler()
    {
        _wrapper = new AsyncFormWrapper<MainForm>(delegate
        {
            var form = new MainForm(CancellationTokenSource);
            form.Show();
            return form;
        });
    }

    public override void Dispose()
    {
        try
        {
            _wrapper.Dispose();
        }
        finally
        {
            base.Dispose();
        }
    }

    /// <inheritdoc/>
    public override void Error(Exception exception)
    {
        _wrapper.SendLow(x => x.Enabled = false);
        base.Error(exception);
    }

    /// <inheritdoc/>
    public bool IsGui => true;

    /// <inheritdoc/>
    public bool Background { get; set; }

    /// <inheritdoc/>
    public override void RunTask(ITask task)
    {
        #region Sanity checks
        if (task == null) throw new ArgumentNullException(nameof(task));
        #endregion

        Log.Debug("Task: " + task.Name);
        var progress = Background ? null : _wrapper.Post(form => form.GetProgressControl(task.Name));
        task.Run(CancellationToken, CredentialProvider, progress);
    }

    /// <inheritdoc/>
    public string? GetCustomPath(bool machineWide, string? currentPath)
    {
        if (!IsInteractive) return currentPath;

        string? result = _wrapper.Post(form => form.GetCustomPath(machineWide, currentPath)).Result;
        CancellationToken.ThrowIfCancellationRequested();
        return result;
    }
}
