// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System.Runtime.InteropServices;
using ZeroInstall.DesktopIntegration.ViewModel;
using ZeroInstall.Model.Selection;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Store.Configuration;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.ViewModel;

namespace ZeroInstall.Commands.EtoForms;

/// <summary>
/// Uses <see cref="Eto.Forms"/> to allow users to interact with <see cref="CliCommand"/>s.
/// </summary>
/// <remarks>Requires a running Eto.Forms <see cref="Application"/>. Invoking methods on the UI thread is handled automatically.</remarks>
public sealed partial class GuiCommandHandler : EtoTaskHandlerBase, ICommandHandler
{
    /// <inheritdoc/>
    public bool IsGui => true;

    /// <inheritdoc/>
    public bool Background { get; set; }

    /// <inheritdoc/>
    public FeedUri? FeedUri { get; set; }

    /// <inheritdoc/>
    public override void RunTask(ITask task)
    {
        Log.Debug("Task: " + task.Name);

        IProgress<TaskSnapshot>? TryGetProgress()
        {
            try
            {
                return ShowForm(form => form.AddProgressFor(task));
            }
            #region Error handling
            catch (Exception ex) when (ex is Win32Exception or ExternalException) // Commonly caused by GDI object exhaustion
            {
                Log.Debug($"Problem showing GUI progress control for {task.Name}", ex);
                return null;
            }
            #endregion
        }

        var progress = TryGetProgress();
        task.Run(CancellationToken, CredentialProvider, progress);
        if (progress != null) _form.Post(form => form.RemoveProgressFor(task));
    }

    /// <inheritdoc/>
    public void DisableUI() => _form.SendLow(x => x.Disable());

    /// <inheritdoc/>
    public void CloseUI() => _form.Close();

    /// <inheritdoc/>
    protected override bool AskInteractive(string question, bool defaultAnswer)
    {
        // Treat messages that default to "Yes" as less severe than those that default to "No"
        var severity = defaultAnswer ? MsgSeverity.Info : MsgSeverity.Warn;

        Log.Debug("Question: " + question);
        switch (ShowForm(form => form.AskAsync(question, severity)).Result)
        {
            case DialogResult.Yes:
                Log.Debug("Answer: Yes");
                return true;
            case DialogResult.No:
                Log.Debug("Answer: No");
                return false;
            case DialogResult.Cancel:
            default:
                Log.Debug("Answer: Cancel");
                throw new OperationCanceledException();
        }
    }

    /// <inheritdoc/>
    public override void Output(string title, string message)
    {
        if (Background) ShowNotification(Branding, title, message, wait: true);
        else
        {
            DisableUI();
            base.Output(title, message);
        }
    }

    /// <inheritdoc/>
    public override void Output<T>(string title, IEnumerable<T> data)
    {
        if (Background)
        {
            ShowNotification(Branding, title, string.Join(Environment.NewLine, data.Select(x => x?.ToString() ?? "")), wait: true);
            return;
        }

        switch (data)
        {
            case IEnumerable<SearchResult> results:
                SwitchToDialog(() => new FeedSearchDialog(title, results));
                break;

            case IEnumerable<CacheNode> nodes:
                SwitchToDialog(() => new StoreManageForm(nodes.ToList()));
                break;

            case Config config:
                if (SwitchToDialog(() => new ConfigDialog(config))) config.Save();
                else throw new OperationCanceledException();
                break;

            default:
                DisableUI();
                base.Output(title, data);
                break;
        }
    }

    /// <inheritdoc/>
    public override void Output<T>(string title, NamedCollection<T> data)
    {
        switch (data)
        {
            case NamedCollection<CacheNode> nodes:
                SwitchToDialog(() => new StoreManageForm(nodes));
                break;

            default:
                DisableUI();
                base.Output(title, data);
                break;
        }
    }

    /// <inheritdoc/>
    public void ShowSelections(Selections selections, IFeedManager feedManager)
        => ShowForm(form => form.ShowSelections(selections, feedManager));

    /// <inheritdoc/>
    public void CustomizeSelections(Func<Selections> solveCallback)
    {
        // Show "modify selections" screen and then asynchronously wait until it's done
        _form.Post(form => form.CustomizeSelectionsAsync(solveCallback))
             .Wait(CancellationToken);
    }

    /// <inheritdoc/>
    public void ShowIntegrateApp(IntegrationState state)
    {
        if (!SwitchToDialog(() => new IntegrateAppForm(state)))
            throw new OperationCanceledException();
    }

    /// <inheritdoc/>
    public override void Error(Exception exception)
    {
        DisableUI();
        base.Error(exception);
    }
}
