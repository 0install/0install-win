// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

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
    #region Constructor
    /// <summary>
    /// Creates a new progress tracking window.
    /// </summary>
    /// <param name="branding">Branding to apply to the window.</param>
    /// <param name="cancellationTokenSource">Used to signal when the user wishes to cancel the current process.</param>
    public ProgressForm(FeedBranding branding, CancellationTokenSource cancellationTokenSource)
    {
        _cancellationTokenSource = cancellationTokenSource ?? throw new ArgumentNullException(nameof(cancellationTokenSource));

        InitializeComponent();
        MinimumSize = new Size(350, 150).Multiply(this.GetDpiScale());
        buttonCustomizeSelectionsDone.Text = Resources.Done;
        buttonHide.Text = Resources.Hide;
        buttonCancel.Text = Resources.Cancel;

        Text = branding.Title;
        if (branding.Icon != null) Icon = branding.Icon;
        if (branding.SplashScreen != null)
        {
            ShowIcon = false;
            pictureBoxSplashScreen.Visible = true;
            pictureBoxSplashScreen.Image = branding.SplashScreen;
            var offset = new Size(width: 0, height: pictureBoxSplashScreen.Height);
            MinimumSize += offset;
            selectionsControl.Location += offset;
            selectionsControl.Size -= offset;
        }

        notifyIcon.Text = Text;
        notifyIcon.Icon = Icon;

        Shown += delegate { this.SetForegroundWindow(); };
    }
    #endregion

    //--------------------//

    #region Selections UI
    /// <summary>Indicates whether <see cref="selectionsControl"/> is intended to be visible or not. Will work even if the form itself is invisible (tray icon mode).</summary>
    private bool _selectionsShown;

    /// <summary>A wait handle to be signaled once the user is satisfied with the <see cref="Selections"/> after <see cref="CustomizeSelectionsAsync"/>.</summary>
    private TaskCompletionSource<bool>? _customizeSelectionsComplete;

    /// <summary>
    /// Shows the user the <see cref="Selections"/> made by the <see cref="ISolver"/>.
    /// Returns immediately.
    /// </summary>
    /// <param name="selections">The <see cref="Selections"/> as provided by the <see cref="ISolver"/>.</param>
    /// <param name="feedManager">The feed manager used to retrieve feeds for additional information about implementations.</param>
    /// <exception cref="InvalidOperationException">The value is set from a thread other than the UI thread.</exception>
    /// <remarks>This method must not be called from a background thread.</remarks>
    public void ShowSelections(Selections selections, IFeedManager feedManager)
    {
        #region Sanity checks
        if (selections == null) throw new ArgumentNullException(nameof(selections));
        if (feedManager == null) throw new ArgumentNullException(nameof(feedManager));
        if (InvokeRequired) throw new InvalidOperationException("Method called from a non UI thread.");
        #endregion

        panelProgress.Visible = false;
        _selectionsShown = selectionsControl.Visible = true;
        selectionsControl.SetSelections(selections, feedManager);
    }

    /// <summary>
    /// Allows the user to modify the <see cref="InterfacePreferences"/> and rerun the <see cref="ISolver"/> if desired.
    /// </summary>
    /// <param name="solveCallback">Called after <see cref="InterfacePreferences"/> have been changed and the <see cref="ISolver"/> needs to be rerun.</param>
    /// <exception cref="InvalidOperationException">The value is set from a thread other than the UI thread.</exception>
    /// <returns>A task that completes once the user has finished customization the selections.</returns>
    /// <remarks>This method must not be called from a background thread.</remarks>
    public async Task CustomizeSelectionsAsync(Func<Selections> solveCallback)
    {
        #region Sanity checks
        if (solveCallback == null) throw new ArgumentNullException(nameof(solveCallback));
        if (InvokeRequired) throw new InvalidOperationException("Method called from a non UI thread.");
        #endregion

        Visible = true;
        notifyIcon.Visible = false;

        // Show "modify selections" UI
        selectionsControl.BeginCustomizeSelections(solveCallback);
        buttonCustomizeSelectionsDone.Visible = true;
        buttonCustomizeSelectionsDone.Focus();
        buttonHide.Visible = false;

        _customizeSelectionsComplete = new();
        await _customizeSelectionsComplete.Task;
    }

    private void buttonCustomizeSelectionsDone_Click(object sender, EventArgs e)
    {
        buttonHide.Visible = true;
        buttonCustomizeSelectionsDone.Visible = false;
        selectionsControl.EndCustomizeSelections();

        // Signal the waiting thread modification is complete
        _customizeSelectionsComplete?.SetResult(true);
    }
    #endregion

    #region Task tracking
    /// <summary>
    /// Adds a GUI element for reporting progress of a generic <see cref="ITask"/>. Should only be one running at a time.
    /// </summary>
    /// <param name="taskName">The name of the task to be tracked.</param>
    /// <remarks>This method must not be called from a background thread.</remarks>
    public IProgress<TaskSnapshot> AddProgressControl(string taskName)
    {
        #region Sanity checks
        if (string.IsNullOrEmpty(taskName)) throw new ArgumentNullException(nameof(taskName));
        if (InvokeRequired) throw new InvalidOperationException("Method called from a non UI thread.");
        #endregion

        panelProgress.Visible = true;

        // Hide other stuff
        if (_selectionsShown) selectionsControl.Visible = false;

        var taskControl = new TaskControl
        {
            TaskName = taskName,
            Dock = DockStyle.Top
        };
        panelProgress.Controls.Add(taskControl);
        return taskControl;
    }

    /// <summary>
    /// Adds a GUI element for reporting progress of a <see cref="ITask"/> for a specific implementation. May run multiple in parallel.
    /// </summary>
    /// <param name="taskName">The name of the task to be tracked.</param>
    /// <param name="tag">A digest used to associate the task with a specific implementation.</param>
    /// <remarks>This method must not be called from a background thread.</remarks>
    public IProgress<TaskSnapshot> AddProgressControl(string taskName, string tag)
    {
        #region Sanity checks
        if (string.IsNullOrEmpty(taskName)) throw new ArgumentNullException(nameof(taskName));
        if (InvokeRequired) throw new InvalidOperationException("Method called from a non UI thread.");
        #endregion

        // Hide other stuff
        panelProgress.Visible = false;

        if (_selectionsShown)
        {
            var control = selectionsControl.TaskControls[tag];
            control.TaskName = taskName;
            return control;
        }
        else return AddProgressControl(taskName);
    }

    /// <summary>
    /// Removes a progress control from the GUI.
    /// </summary>
    public void RemoveProgressControl(IProgress<TaskSnapshot> control)
    {
        panelProgress.Controls.Remove((Control)control);

        if (panelProgress.Controls.Count == 0 && _selectionsShown)
        {
            panelProgress.Visible = false;
            selectionsControl.Visible = true;
        }
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
    /// <summary>
    /// Shows the tray icon.
    /// </summary>
    /// <param name="notificationMessage">An optional notification message to associate with the tray icon.</param>
    /// <remarks>This method must not be called from a background thread.</remarks>
    public void ShowTrayIcon(string? notificationMessage = null)
    {
        #region Sanity checks
        if (InvokeRequired) throw new InvalidOperationException("Method called from a non UI thread.");
        #endregion

        notifyIcon.Visible = true;
        if (!string.IsNullOrEmpty(notificationMessage)) notifyIcon.ShowBalloonTip(7500, Text, notificationMessage, ToolTipIcon.Info);
    }

    private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
    {
        Visible = true;
        notifyIcon.Visible = false;
    }

    private void notifyIcon_BalloonTipClicked(object sender, EventArgs e)
    {
        Visible = true;
        notifyIcon.Visible = false;
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
        notifyIcon.Visible = false;

        _cancellationTokenSource.Cancel();

        // Unblock any waiting thread
        _customizeSelectionsComplete?.TrySetResult(false);
    }
    #endregion
}
