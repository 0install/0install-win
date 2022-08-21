// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System.Runtime.CompilerServices;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using NanoByte.Common.Native;
using NanoByte.Common.Threading;
using ZeroInstall.DesktopIntegration.ViewModel;
using ZeroInstall.Model.Selection;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Store.Configuration;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementations;

namespace ZeroInstall.Commands.WinForms;

/// <summary>
/// Uses <see cref="System.Windows.Forms"/> to allow users to interact with <see cref="CliCommand"/>s.
/// </summary>
/// <remarks>This class manages a GUI thread with an independent message queue. Invoking methods on the right thread is handled automatically.</remarks>
public sealed class GuiCommandHandler : GuiTaskHandlerBase, ICommandHandler
{
    #region Resources
    private readonly Lazy<FeedBranding> _branding;
    private FeedBranding Branding => _branding.Value;

    private readonly AsyncFormWrapper<ProgressForm> _wrapper;

    public GuiCommandHandler()
    {
        _branding = new(() => new(FeedUri));

        _wrapper = new AsyncFormWrapper<ProgressForm>(delegate
        {
            var form = new ProgressForm(Branding, CancellationTokenSource);
            if (Background) form.ShowTrayIcon();
            else form.Show();
            return form;
        });
    }

    public override void Dispose()
    {
        try
        {
            _wrapper.Dispose();
            if (_branding.IsValueCreated) Branding.Dispose();
        }
        finally
        {
            base.Dispose();
        }
    }
    #endregion

    /// <inheritdoc/>
    public bool IsGui => true;

    /// <inheritdoc/>
    public bool Background { get; set; }

    /// <inheritdoc/>
    public FeedUri? FeedUri { get; set; }

    #region Task tracking
    /// <inheritdoc/>
    public override void RunTask(ITask task)
    {
        #region Sanity checks
        if (task == null) throw new ArgumentNullException(nameof(task));
        #endregion

        Log.Debug("Task: " + task.Name);

        var progress = _wrapper.Post(form => form.AddProgressControl(task.Name, task.Tag as string));
        task.Run(CancellationToken, CredentialProvider, progress);
        _wrapper.Post(form => form.RemoveProgressControl(progress));
    }
    #endregion

    #region UI control
    /// <inheritdoc/>
    public void DisableUI()
    {
        _wrapper.SendLow(x => x.Enabled = false);
    }

    /// <inheritdoc/>
    public void CloseUI() => _wrapper.Close();
    #endregion

    #region Question
    /// <inheritdoc/>
    protected override bool AskInteractive(string question, bool defaultAnswer)
    {
        #region Sanity checks
        if (question == null) throw new ArgumentNullException(nameof(question));
        #endregion

        // Treat messages that default to "Yes" as less severe than those that default to "No"
        var severity = defaultAnswer ? MsgSeverity.Info : MsgSeverity.Warn;

        Log.Debug("Question: " + question);
        switch (_wrapper.Post(form => form.AskAsync(question, severity)).Result)
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
    #endregion

    #region Selections UI
    /// <inheritdoc/>
    public void ShowSelections(Selections selections, IFeedManager feedManager)
    {
        #region Sanity checks
        if (selections == null) throw new ArgumentNullException(nameof(selections));
        if (feedManager == null) throw new ArgumentNullException(nameof(feedManager));
        #endregion

        _wrapper.Post(form => form.ShowSelections(selections, feedManager));
    }

    /// <inheritdoc/>
    public void CustomizeSelections(Func<Selections> solveCallback)
    {
        #region Sanity checks
        if (solveCallback == null) throw new ArgumentNullException(nameof(solveCallback));
        #endregion

        // Show "modify selections" screen and then asynchronously wait until it's done
        _wrapper.Post(form =>
        {
            // Leave tray icon mode
            form.Show();
            Application.DoEvents();

            return form.CustomizeSelectionsAsync(solveCallback);
        }).Wait(CancellationToken);
    }
    #endregion

    #region Output
    /// <inheritdoc/>
    public override void Output(string title, string message)
    {
        DisableUI();

        if (Background) ShowNotification(title, message);
        else base.Output(title, message);
    }

    /// <inheritdoc/>
    public override void Output<T>(string title, IEnumerable<T> data)
    {
        DisableUI();

        if (Background)
        {
            string message = StringUtils.Join(Environment.NewLine, data.Select(x => x?.ToString() ?? ""));
            ShowNotification(title, message);
        }
        else
        {
            switch (data)
            {
                case IEnumerable<SearchResult> results:
                    ThreadUtils.RunSta(() =>
                    {
                        using var dialog = new FeedSearchDialog(title, results);
                        dialog.ShowDialog();
                    });
                    break;

                case Config config:
                    ThreadUtils.RunSta(() =>
                    {
                        using var dialog = new ConfigDialog(config);
                        if (dialog.ShowDialog() == DialogResult.OK) config.Save();
                        else throw new OperationCanceledException();
                    });
                    break;

                default:
                    base.Output(title, data);
                    break;
            }
        }
    }

    /// <inheritdoc/>
    public override void Output<T>(string title, NamedCollection<T> data)
    {
        DisableUI();

        base.Output(title, data);
    }

    /// <summary>
    /// Displays a notification message detached from the main GUI. Will stick around even after the process ends.
    /// </summary>
    /// <param name="title">The title of the message.</param>
    /// <param name="message">The message text.</param>
    /// <param name="icon">The icon to display next to the notification.</param>
    private void ShowNotification(string title, string message, ToolTipIcon icon = ToolTipIcon.None)
    {
        void Classic()
            => new NotifyIcon {Visible = true, Text = Branding.Name ?? "Zero Install", Icon = Branding.Icon}
               .ShowBalloonTip(10000, title, message, icon);

        void Modern(string appId)
        {
            try
            {
                ShowNotificationModern(title, message, appId);
            }
            catch
            {
                Classic();
            }
        }

        if (WindowsUtils.IsWindows10)
        {
            if (ZeroInstallInstance.IsIntegrated)
                Modern(appId: "ZeroInstall");
            else if (ZeroInstallInstance.IsLibraryMode && !string.IsNullOrEmpty(Branding.AppId))
                Modern(Branding.AppId);
            else Classic();
        }
        else Classic();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ShowNotificationModern(string title, string message, string appId)
    {
        var doc = new XmlDocument();

        XmlElement Element(string tagName, IEnumerable<IXmlNode>? children = null, IDictionary<string, string>? attributes = null, string? innerText = null)
        {
            var element = doc.CreateElement(tagName);
            foreach (var child in children ?? Enumerable.Empty<IXmlNode>())
                element.AppendChild(child);
            foreach ((string key, string value) in attributes ?? new Dictionary<string, string>())
                element.SetAttribute(key, value);
            if (innerText != null) element.InnerText = innerText;
            return element;
        }

        doc.AppendChild(Element("toast", new[]
        {
            Element("visual", new[]
            {
                Element("binding", new[]
                {
                    Element("text", innerText: title),
                    Element("text", innerText: message)
                }, new Dictionary<string, string> {["template"] = "ToastGeneric"})
            })
        }));

        ToastNotificationManager.CreateToastNotifier(appId)
                                .Show(new ToastNotification(doc));
    }

    /// <inheritdoc/>
    public override void Error(Exception exception)
    {
        DisableUI();

        base.Error(exception);
    }
    #endregion

    #region Dialogs
    /// <inheritdoc/>
    public void ShowIntegrateApp(IntegrationState state)
    {
        #region Sanity checks
        if (state == null) throw new ArgumentNullException(nameof(state));
        #endregion

        // The progress form and integration form take turns in being visible
        Background = true;
        var result = _wrapper.Post(new IntegrateAppForm(state).ShowDialog);
        Background = false;

        if (result == DialogResult.OK) _wrapper.Post(form => form.Show());
        else throw new OperationCanceledException();
    }

    /// <inheritdoc/>
    public void ManageStore(IImplementationStore store, IFeedCache feedCache)
    {
        #region Sanity checks
        if (store == null) throw new ArgumentNullException(nameof(store));
        if (feedCache == null) throw new ArgumentNullException(nameof(feedCache));
        #endregion

        ThreadUtils.RunSta(() =>
        {
            using var form = new StoreManageForm(store, feedCache);
            form.ShowDialog();
        });
    }
    #endregion

    #region Log handler
    /// <summary>
    /// Outputs <see cref="Log"/> messages as balloon tips based on their <see cref="LogSeverity"/> and the current <see cref="Verbosity"/> level.
    /// </summary>
    /// <param name="severity">The type/severity of the entry.</param>
    /// <param name="message">The message text of the entry.</param>
    /// <param name="exception">An optional exception associated with the entry.</param>
    protected override void LogHandler(LogSeverity severity, string message, Exception? exception)
    {
        base.LogHandler(severity, message, exception);

        // Avoid dead-lock
        if (!_branding.IsValueCreated) return;

        string title = Branding.Name ?? "Zero Install";
        switch (severity)
        {
            case LogSeverity.Debug when Verbosity >= Verbosity.Debug:
                ShowNotification(title, message);
                break;
            case LogSeverity.Info when Verbosity >= Verbosity.Verbose:
                ShowNotification(title, message, ToolTipIcon.Info);
                break;
            case LogSeverity.Warn:
                ShowNotification(title, message, ToolTipIcon.Warning);
                break;
            case LogSeverity.Error:
                ShowNotification(title, message, ToolTipIcon.Error);
                break;
        }
    }
    #endregion
}
