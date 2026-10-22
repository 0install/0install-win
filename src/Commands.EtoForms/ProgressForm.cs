// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using NanoByte.Common.Native;
using ZeroInstall.Model.Preferences;
using ZeroInstall.Model.Selection;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Services.Solvers;

namespace ZeroInstall.Commands.EtoForms;

/// <summary>
/// Displays the progress of a <see cref="CliCommand"/> and <see cref="ITask"/>s.
/// </summary>
public sealed class ProgressForm : Form
{
    #region Init
    private readonly FeedBranding _branding;

    private readonly StackLayout _progressLayout = new() {Orientation = Orientation.Vertical, HorizontalContentAlignment = HorizontalAlignment.Stretch, Spacing = 8};
    private readonly Scrollable _progressPanel;
    private readonly SelectionsControl _selectionsControl = new();
    private readonly Panel _contentPanel;
    private readonly LinkButton _linkPoweredBy = new() {Text = @"Powered by Zero Install", Visible = false};
    private readonly Button _buttonCustomizeSelectionsDone = new() {Text = Commands.Properties.Resources.Done, Visible = false};
    private readonly Button _buttonHide = new() {Text = Commands.Properties.Resources.Hide, ToolTip = Resources.HideToolTip};
    private readonly Button _buttonCancel = new() {Text = Commands.Properties.Resources.Cancel};

    /// <summary>
    /// Creates a new progress tracking window.
    /// </summary>
    /// <param name="branding">Branding to apply to the window.</param>
    /// <param name="cancellationTokenSource">Used to signal when the user wishes to cancel the current process.</param>
    public ProgressForm(FeedBranding branding, CancellationTokenSource cancellationTokenSource)
    {
        _branding = branding ?? throw new ArgumentNullException(nameof(branding));
        _cancellationTokenSource = cancellationTokenSource ?? throw new ArgumentNullException(nameof(cancellationTokenSource));

        Title = branding.Name ?? "Zero Install";
        if (Locations.IsPortable) Title += @" - " + Commands.Properties.Resources.PortableMode;
        if (branding.Icon != null) Icon = branding.Icon;
        Resizable = true;

        var menuItemShow = new ButtonMenuItem {Text = Resources.Show};
        menuItemShow.Click += delegate { Restore(); };
        var menuItemCancel = new ButtonMenuItem {Text = Commands.Properties.Resources.Cancel};
        menuItemCancel.Click += delegate { Cancel(); };
        _trayIcon = new()
        {
            Title = branding.Name ?? "Zero Install",
            Image = branding.Icon,
            // Some platforms (e.g., AppIndicator on Linux) only support menus, not clicks
            Menu = new ContextMenu(menuItemShow, menuItemCancel)
        };
        _trayIcon.Activated += delegate { Restore(); };
        Application.Instance.NotificationActivated += OnNotificationActivated;

        _progressPanel = new() {Content = _progressLayout, Border = BorderType.None, ExpandContentWidth = true, ExpandContentHeight = false};
        _contentPanel = new() {Content = _progressPanel, Padding = 12};

        if (branding.Name != null) _linkPoweredBy.Visible = true;
        _linkPoweredBy.Click += linkPoweredBy_Click;
        _buttonCustomizeSelectionsDone.Click += buttonCustomizeSelectionsDone_Click;
        _buttonHide.Click += buttonHide_Click;
        _buttonCancel.Click += delegate { Cancel(); };

        var layout = new StackLayout {Orientation = Orientation.Vertical, HorizontalContentAlignment = HorizontalAlignment.Stretch};
        if (branding.SplashScreen != null)
            layout.Items.Add(new ImageView {Image = branding.SplashScreen, Height = SplashScreenHeight});
        layout.Items.Add(new StackLayoutItem(_contentPanel, expand: true));
        layout.Items.Add(new StackLayout
        {
            Orientation = Orientation.Horizontal,
            VerticalContentAlignment = VerticalAlignment.Center,
            Padding = new(12, 0, 12, 12),
            Spacing = 6,
            Items = {_linkPoweredBy, _buttonCustomizeSelectionsDone, new StackLayoutItem(null, expand: true), _buttonHide, _buttonCancel}
        });
        Content = layout;

        // Set after Content to avoid flickering
        Maximizable = false;
        if (branding.SplashScreen != null) Closeable = Minimizable = false;

        int extraHeight = branding.SplashScreen == null ? 0 : SplashScreenHeight;
        MinimumSize = new(350, 150 + extraHeight);
        ClientSize = new(560, 288 + extraHeight);

        if (ZeroInstallInstance.IsLibraryMode && branding.AppId != null) this.SetWindowAppID(branding.AppId);
        else this.PreventPinningIfNotIntegrated();
        Load += delegate { this.CenterOnScreen(); };
        Shown += delegate
        {
            Log.Debug("Progress form shown");
            BringToFront();
        };
        Closing += ProgressForm_Closing;
    }

    private const int SplashScreenHeight = 250;
    #endregion

    #region Selections UI
    private Selections? _selections;

    /// <summary>A wait handle to be signaled once the user is satisfied with the <see cref="Selections"/> after <see cref="CustomizeSelectionsAsync"/>.</summary>
    private TaskCompletionSource<bool>? _customizeSelectionsComplete;

    /// <summary>
    /// Shows the user the <see cref="Selections"/> made by the <see cref="ISolver"/>.
    /// Returns immediately.
    /// </summary>
    /// <param name="selections">The <see cref="Selections"/> as provided by the <see cref="ISolver"/>.</param>
    /// <param name="feedManager">The feed manager used to retrieve feeds for additional information about implementations.</param>
    /// <remarks>This must be called on the GUI thread.</remarks>
    public void ShowSelections(Selections selections, IFeedManager feedManager)
    {
        _selections = selections;

        _selectionsControl.SetSelections(selections, feedManager);
        _contentPanel.Content = _selectionsControl;
    }

    /// <summary>
    /// Allows the user to modify the <see cref="InterfacePreferences"/> and rerun the <see cref="ISolver"/> if desired.
    /// </summary>
    /// <param name="solveCallback">Called after <see cref="InterfacePreferences"/> have been changed and the <see cref="ISolver"/> needs to be rerun.</param>
    /// <returns>A task that completes once the user has finished customization the selections.</returns>
    /// <remarks>This must be called on the GUI thread.</remarks>
    public Task CustomizeSelectionsAsync(Func<Selections> solveCallback)
    {
        // Leave tray icon mode
        Restore();

        // Show "modify selections" UI
        _selectionsControl.BeginCustomizeSelections(solveCallback);
        _linkPoweredBy.Visible = false;
        _buttonCustomizeSelectionsDone.Visible = true;
        _buttonCustomizeSelectionsDone.Focus();
        _buttonHide.Visible = false;

        _customizeSelectionsComplete = new();
        return _customizeSelectionsComplete.Task;
    }

    private void buttonCustomizeSelectionsDone_Click(object? sender, EventArgs e)
    {
        _buttonHide.Visible = true;
        _buttonCustomizeSelectionsDone.Visible = false;
        _selectionsControl.EndCustomizeSelections();

        // Signal the waiting thread modification is complete
        _customizeSelectionsComplete?.TrySetResult(true);
    }
    #endregion

    #region Task tracking
    /// <summary>
    /// Adds a GUI element for reporting progress of a task. Multiple tasks may be run in parallel.
    /// </summary>
    /// <param name="task">The task to create the GUI element for.</param>
    /// <returns>The GUI element that the task can send progress reports to.</returns>
    /// <remarks>This must be called on the GUI thread.</remarks>
    public IProgress<TaskSnapshot> AddProgressFor(ITask task)
    {
        if (_selections != null && task.Tag is string tag)
        {
            _contentPanel.Content = _selectionsControl;

            var control = _selectionsControl.TaskControls[tag];
            control.TaskName = task.Name;
            return control;
        }
        else
        {
            _contentPanel.Content = _progressPanel;

            var control = new TaskControl
            {
                TaskName = task.Name,
                Tag = task
            };
            _progressLayout.Items.Add(control);
            return control;
        }
    }

    /// <summary>
    /// Removes a GUI element previously added by <see cref="AddProgressFor"/>.
    /// </summary>
    /// <param name="task">The task the GUI element was created for.</param>
    /// <remarks>This must be called on the GUI thread.</remarks>
    public void RemoveProgressFor(ITask task)
    {
        if (_progressLayout.Items.FirstOrDefault(x => x.Control?.Tag == task) is {} toRemove)
            _progressLayout.Items.Remove(toRemove);

        if (_progressLayout.Items is [] && _selections != null)
            _contentPanel.Content = _selectionsControl;
    }

    /// <summary>
    /// Disables the form but leaves it visible.
    /// </summary>
    public void Disable()
    {
        Enabled = false;

        // Remove leftover progress (e.g., from a failed or canceled task) from the taskbar while an error or output is shown
        if (Loaded && WindowsUtils.IsWindows7)
            WindowsTaskbar.SetProgressState(NativeHandle, WindowsTaskbar.ProgressBarState.NoProgress);
    }
    #endregion

    #region Question
    private string? _pendingQuestion;
    private TaskCompletionSource<DialogResult>? _pendingResult;

    /// <summary>
    /// Asks the user a Yes/No/Cancel question.
    /// </summary>
    /// <param name="question">The question and comprehensive information to help the user make an informed decision.</param>
    /// <param name="severity">The severity/possible impact of the question.</param>
    /// <returns><see cref="DialogResult.Yes"/>, <see cref="DialogResult.No"/> or <see cref="DialogResult.Cancel"/>.</returns>
    public Task<DialogResult> AskAsync(string question, MsgSeverity severity)
    {
        #region Sanity checks
        if (string.IsNullOrEmpty(question)) throw new ArgumentNullException(nameof(question));
        #endregion

        if (Visible)
            return Task.FromResult(Msg.YesNoCancel(this, question, severity));
        else
        {
            _pendingQuestion = question;
            _pendingResult = new();

            ShowTrayIcon(question.GetLeftPartAtFirstOccurrence(Environment.NewLine) + Environment.NewLine + Commands.Properties.Resources.ClickToChoose);

            return _pendingResult.Task;
        }
    }
    #endregion

    #region Tray icon
    private readonly TrayIndicator _trayIcon;

    /// <summary>Identifies <see cref="Notification"/>s shown by this form.</summary>
    private const string NotificationID = "ZeroInstall.ProgressForm";

    /// <summary>
    /// Shows the tray icon.
    /// </summary>
    /// <param name="notificationMessage">An optional notification message to associate with the tray icon.</param>
    /// <remarks>This must be called on the GUI thread.</remarks>
    public void ShowTrayIcon(string? notificationMessage = null)
    {
        _trayIcon.Show();
        if (!string.IsNullOrEmpty(notificationMessage))
        {
            new Notification {ID = NotificationID, Title = Title, Message = notificationMessage}.Show(_trayIcon);
        }
    }

    private void OnNotificationActivated(object? sender, NotificationEventArgs e)
    {
        if (e.ID == NotificationID) Restore();
    }

    /// <summary>
    /// Leaves tray icon mode and asks any question that was deferred while the window was hidden.
    /// </summary>
    private void Restore()
    {
        Show();
        _trayIcon.Hide();

        if (_pendingQuestion != null && _pendingResult != null)
        {
            var (question, result) = (_pendingQuestion, _pendingResult);
            _pendingQuestion = null;
            _pendingResult = null;
            result.SetResult(Msg.YesNoCancel(this, question, MsgSeverity.Warn));
        }
    }

    private void buttonHide_Click(object? sender, EventArgs e)
    {
        ShowTrayIcon();
        Visible = false;
    }
    #endregion

    #region Closing
    /// <summary>Signaled when the user wishes to cancel the current process.</summary>
    private readonly CancellationTokenSource _cancellationTokenSource;

    /// <summary>Indicates that the window is being closed programmatically rather than by the user.</summary>
    private bool _closeRequested;

    /// <inheritdoc/>
    public override void Close()
    {
        _closeRequested = true;
        base.Close();
    }

    private void ProgressForm_Closing(object? sender, CancelEventArgs e)
    {
        if (_closeRequested) return;

        // Never allow the user to directly close the window
        e.Cancel = true;

        // Start proper cancellation instead
        Cancel();
    }

    /// <inheritdoc/>
    protected override void OnClosed(EventArgs e)
    {
        Application.Instance.NotificationActivated -= OnNotificationActivated;
        _trayIcon.Dispose();
        base.OnClosed(e);
    }

    /// <summary>
    /// Hides the window and then starts canceling the current process asynchronously.
    /// </summary>
    private void Cancel()
    {
        Visible = false;
        _trayIcon.Hide();

        _cancellationTokenSource.Cancel();

        // Unblock any waiting thread
        _customizeSelectionsComplete?.TrySetResult(false);
        _pendingQuestion = null;
        _pendingResult?.TrySetResult(DialogResult.Cancel);
        _pendingResult = null;
    }
    #endregion

    #region Link
    private void linkPoweredBy_Click(object? sender, EventArgs e)
    {
        try
        {
            Application.Instance.Open("https://0install.net/");
        }
        #region Error handling
        catch (Exception ex)
        {
            Msg.Inform(this, ex.Message, MsgSeverity.Error);
        }
        #endregion
    }
    #endregion
}
