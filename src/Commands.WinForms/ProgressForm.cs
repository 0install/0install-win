// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System.Diagnostics;
using NanoByte.Common.Native;
using ZeroInstall.Model.Preferences;
using ZeroInstall.Model.Selection;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Services.Solvers;

namespace ZeroInstall.Commands.WinForms;

/// <summary>
/// Displays the progress of a <see cref="CliCommand"/> and <see cref="ITask"/>s.
/// </summary>
public sealed partial class ProgressForm : Form
{
    #region Init
    private readonly FeedBranding _branding;

    /// <summary>
    /// Creates a new progress tracking window.
    /// </summary>
    /// <param name="branding">Branding to apply to the window.</param>
    /// <param name="cancellationTokenSource">Used to signal when the user wishes to cancel the current process.</param>
    public ProgressForm(FeedBranding branding, CancellationTokenSource cancellationTokenSource)
    {
        _branding = branding ?? throw new ArgumentNullException(nameof(branding));
        _cancellationTokenSource = cancellationTokenSource ?? throw new ArgumentNullException(nameof(cancellationTokenSource));

        _trayIcon = new()
        {
            Text = branding.Name ?? "Zero Install",
            Icon = branding.Icon
        };
        _trayIcon.BalloonTipClicked += trayIcon_BalloonTipClicked;
        _trayIcon.MouseClick += trayIcon_MouseClick;

        StartPosition = FormStartPosition.CenterScreen;

        Shown += delegate
        {
            Log.Debug("Progress form shown");
            this.SetForegroundWindow();
        };
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        if (ZeroInstallInstance.IsLibraryMode && _branding.AppId != null)
            WindowsTaskbar.SetWindowAppID(Handle, _branding.AppId);
        else if (!ZeroInstallInstance.IsIntegrated)
            WindowsTaskbar.PreventPinning(Handle);
    }

    protected override void SetVisibleCore(bool value)
    {
        if (value && components == null)
        {
            Log.Debug("Initializing progress form");
            InitializeComponent();
            Font = DefaultFonts.Modern;

            try
            {
                MinimumSize = new Size(350, 150).ApplyScale(this);
            }
            #region Error handling
            catch (ArgumentException ex)
            {
                Log.Debug(ex);
            }
            #endregion

            buttonCustomizeSelectionsDone.Text = Resources.Done;
            buttonHide.Text = Resources.Hide;
            buttonCancel.Text = Resources.Cancel;

            Text = _branding.Name ?? "Zero Install";
            if (Locations.IsPortable) Text += @" - " + Resources.PortableMode;
            if (_branding.Icon != null) Icon = _branding.Icon;
            if (_branding.Name != null) linkPoweredBy.Show();
            if (_branding.SplashScreen != null)
            {
                ControlBox = false;
                pictureBoxSplashScreen.Show();
                pictureBoxSplashScreen.Image = _branding.SplashScreen;

                try
                {
                    var offset = new Size(width: 0, height: pictureBoxSplashScreen.Bottom);
                    MinimumSize += offset;
                    panelProgress.Location += offset;
                    panelProgress.Size -= offset;
                    selectionsControl.Location += offset;
                    selectionsControl.Size -= offset;
                }
                #region Error handling
                catch (ArgumentException ex)
                {
                    Log.Debug(ex);
                }
                #endregion
            }

            ShowSelectionsControls();

            foreach (var (task, progress) in _deferredProgress)
                progress.SetTarget(AddTaskControl(task));
            _deferredProgress.Clear();
        }

        base.SetVisibleCore(value);
    }
    #endregion

    #region Selections UI
    private Selections? _selections;
    private IFeedManager? _feedManager;

    /// <summary>A wait handle to be signaled once the user is satisfied with the <see cref="Selections"/> after <see cref="CustomizeSelectionsAsync"/>.</summary>
    private TaskCompletionSource<bool>? _customizeSelectionsComplete;

    /// <summary>
    /// Shows the user the <see cref="Selections"/> made by the <see cref="ISolver"/>.
    /// Returns immediately.
    /// </summary>
    /// <param name="selections">The <see cref="Selections"/> as provided by the <see cref="ISolver"/>.</param>
    /// <param name="feedManager">The feed manager used to retrieve feeds for additional information about implementations.</param>
    /// <exception cref="InvalidOperationException">The value is set from a thread other than the UI thread.</exception>
    /// <remarks>This must be called on the GUI thread.</remarks>
    public void ShowSelections(Selections selections, IFeedManager feedManager)
    {
        _selections = selections;
        _feedManager = feedManager;
        if (components != null) ShowSelectionsControls();
    }

    private void ShowSelectionsControls()
    {
        if (_selections == null || _feedManager == null) return;

        panelProgress.Hide();
        selectionsControl.Show();
        selectionsControl.SetSelections(_selections, _feedManager);
    }

    /// <summary>
    /// Allows the user to modify the <see cref="InterfacePreferences"/> and rerun the <see cref="ISolver"/> if desired.
    /// </summary>
    /// <param name="solveCallback">Called after <see cref="InterfacePreferences"/> have been changed and the <see cref="ISolver"/> needs to be rerun.</param>
    /// <exception cref="InvalidOperationException">The value is set from a thread other than the UI thread.</exception>
    /// <returns>A task that completes once the user has finished customization the selections.</returns>
    /// <remarks>This must be called on the GUI thread.</remarks>
    public async Task CustomizeSelectionsAsync(Func<Selections> solveCallback)
    {
        Visible = true;
        _trayIcon.Visible = false;

        // Show "modify selections" UI
        selectionsControl.BeginCustomizeSelections(solveCallback);
        linkPoweredBy.Hide();
        buttonCustomizeSelectionsDone.Show();
        buttonCustomizeSelectionsDone.Focus();
        buttonHide.Hide();

        _customizeSelectionsComplete = new();
        await _customizeSelectionsComplete.Task;
    }

    private void buttonCustomizeSelectionsDone_Click(object sender, EventArgs e)
    {
        buttonHide.Show();
        buttonCustomizeSelectionsDone.Hide();
        selectionsControl.EndCustomizeSelections();

        // Signal the waiting thread modification is complete
        _customizeSelectionsComplete?.SetResult(true);
    }
    #endregion

    #region Task tracking
    private readonly List<(ITask task, DeferredProgress<TaskSnapshot> progress)> _deferredProgress = new();

    /// <summary>
    /// Adds a GUI element for reporting progress of a task. Multiple tasks may be run in parallel.
    /// </summary>
    /// <param name="task">The task to create the GUI element for.</param>
    /// <returns>The GUI element that the task can send progress reports to.</returns>
    /// <remarks>This must be called on the GUI thread.</remarks>
    public IProgress<TaskSnapshot> AddProgressFor(ITask task)
    {
        if (components == null)
        {
            var progress = new DeferredProgress<TaskSnapshot>();
            _deferredProgress.Add((task, progress));
            return progress;
        }

        var control = AddTaskControl(task);
        control.Refresh();
        return control;
    }

    private TaskControl AddTaskControl(ITask task)
    {
        if (_selections != null && task.Tag is string tag)
        {
            panelProgress.Hide();

            var control = selectionsControl.TaskControls[tag];
            control.TaskName = task.Name;
            return control;
        }
        else
        {
            selectionsControl.Hide();
            panelProgress.Show();

            var control = new TaskControl
            {
                TaskName = task.Name,
                Dock = DockStyle.Top,
                Tag = task
            };
            panelProgress.Controls.Add(control);
            panelProgress.Controls.SetChildIndex(control, 0);
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
        _deferredProgress.RemoveAll(x => x.task == task);
        if (components == null) return;

        if (panelProgress.Controls.Cast<Control>().FirstOrDefault(x => x.Tag == task) is {} toRemove)
            panelProgress.Controls.Remove(toRemove);

        if (panelProgress.Controls.Count == 0 && _selections != null)
        {
            panelProgress.Hide();
            selectionsControl.Show();
        }
    }

    /// <summary>
    /// Disables the form but leaves it visible.
    /// </summary>
    public void Disable()
    {
        Enabled = false;
        WindowsTaskbar.SetProgressState(Handle, WindowsTaskbar.ProgressBarState.NoProgress);
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
    /// <returns><c>true</c> if the user answered with 'Yes'; <c>false</c> if the user answered with 'No'.</returns>
    /// <exception cref="OperationCanceledException">The user selected 'Cancel'.</exception>
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

            ShowTrayIcon(question.GetLeftPartAtFirstOccurrence(Environment.NewLine) + Environment.NewLine + Resources.ClickToChoose);

            return _pendingResult!.Task;
        }
    }

    private void ProgressForm_VisibleChanged(object sender, EventArgs e)
    {
        if (Visible && _pendingQuestion != null && _pendingResult != null)
        {
            _pendingResult.SetResult(Msg.YesNoCancel(this, _pendingQuestion, MsgSeverity.Warn));
            _pendingQuestion = null;
            _pendingResult = null;
        }
    }
    #endregion

    #region Tray icon
    private readonly NotifyIcon _trayIcon;

    /// <summary>
    /// Shows the tray icon.
    /// </summary>
    /// <param name="notificationMessage">An optional notification message to associate with the tray icon.</param>
    /// <remarks>This must be called on the GUI thread.</remarks>
    public void ShowTrayIcon(string? notificationMessage = null)
    {
        _trayIcon.Visible = true;
        if (!string.IsNullOrEmpty(notificationMessage))
            _trayIcon.ShowBalloonTip(7500, Text, notificationMessage, ToolTipIcon.Info);
    }

    private void trayIcon_MouseClick(object sender, MouseEventArgs e)
    {
        Visible = true;
        _trayIcon.Visible = false;
    }

    private void trayIcon_BalloonTipClicked(object sender, EventArgs e)
    {
        Visible = true;
        _trayIcon.Visible = false;
    }

    private void buttonHide_Click(object sender, EventArgs e)
    {
        ShowTrayIcon();
        Visible = false;
    }
    #endregion

    #region Closing
    /// <summary>Signaled when the user wishes to cancel the current process.</summary>
    private readonly CancellationTokenSource _cancellationTokenSource;

    private void ProgressForm_FormClosing(object sender, CancelEventArgs e)
    {
        // Never allow the user to directly close the window
        e.Cancel = true;

        // Start proper cancellation instead
        Cancel();
    }

    private void buttonCancel_Click(object sender, EventArgs e)
        => Cancel();

    /// <summary>
    /// Hides the window and then starts canceling the current process asynchronously.
    /// </summary>
    private void Cancel()
    {
        Visible = false;
        _trayIcon.Visible = false;

        _cancellationTokenSource.Cancel();

        // Unblock any waiting thread
        _customizeSelectionsComplete?.TrySetResult(false);
    }
    #endregion

    #region Link
    private void linkPoweredBy_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        try
        {
            Process.Start("https://0install.net/");
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
