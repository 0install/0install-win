// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using NanoByte.Common;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Services.Solvers;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Preferences;
using ZeroInstall.Store.Model.Selection;
using CancellationTokenSource = NanoByte.Common.Tasks.CancellationTokenSource;

namespace ZeroInstall.Commands.WinForms
{
    /// <summary>
    /// Displays the progress of a <see cref="CliCommand"/> and <see cref="ITask"/>s.
    /// </summary>
    public partial class ProgressForm : Form
    {
        #region Constructor
        /// <summary>
        /// Creates a new progress tracking form.
        /// </summary>
        /// <param name="cancellationTokenSource">Used to signal when the user wishes to cancel the current process.</param>
        public ProgressForm(CancellationTokenSource cancellationTokenSource)
        {
            _cancellationTokenSource = cancellationTokenSource ?? throw new ArgumentNullException(nameof(cancellationTokenSource));

            InitializeComponent();

            buttonCustomizeSelectionsDone.Text = Resources.Done;
            buttonHide.Text = Resources.Hide;
            buttonCancel.Text = Resources.Cancel;

            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            if (Locations.IsPortable) Text += @" - " + Resources.PortableMode;

            Shown += delegate { this.SetForegroundWindow(); };
        }
        #endregion

        //--------------------//

        #region Selections UI
        /// <summary>Indicates whether <see cref="selectionsControl"/> is intended to be visible or not. Will work even if the form itself is invisible (tray icon mode).</summary>
        private bool _selectionsShown;

        /// <summary>A wait handle to be signaled once the user is satisfied with the <see cref="Selections"/> after <see cref="CustomizeSelections"/>.</summary>
        private EventWaitHandle _customizeSelectionsWaitHandle;

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

            taskControl.Visible = false;
            _selectionsShown = selectionsControl.Visible = true;
            selectionsControl.SetSelections(selections, feedManager);
        }

        /// <summary>
        /// Allows the user to modify the <see cref="InterfacePreferences"/> and rerun the <see cref="ISolver"/> if desired.
        /// Returns immediately.
        /// </summary>
        /// <param name="solveCallback">Called after <see cref="InterfacePreferences"/> have been changed and the <see cref="ISolver"/> needs to be rerun.</param>
        /// <param name="waitHandle">A wait handle to be signaled once the user is satisfied with the <see cref="Selections"/>.</param>
        /// <exception cref="InvalidOperationException">The value is set from a thread other than the UI thread.</exception>
        /// <remarks>This method must not be called from a background thread.</remarks>
        public void CustomizeSelections(Func<Selections> solveCallback, EventWaitHandle waitHandle)
        {
            #region Sanity checks
            if (solveCallback == null) throw new ArgumentNullException(nameof(solveCallback));
            if (InvokeRequired) throw new InvalidOperationException("Method called from a non UI thread.");
            #endregion

            Visible = true;
            notifyIcon.Visible = false;

            _customizeSelectionsWaitHandle = waitHandle ?? throw new ArgumentNullException(nameof(waitHandle));

            // Show "modify selections" UI
            selectionsControl.BeginCustomizeSelections(solveCallback);
            buttonCustomizeSelectionsDone.Visible = true;
            buttonCustomizeSelectionsDone.Focus();
            buttonHide.Visible = false;
        }

        private void buttonCustomizeSelectionsDone_Click(object sender, EventArgs e)
        {
            buttonHide.Visible = true;
            buttonCustomizeSelectionsDone.Visible = false;
            selectionsControl.EndCustomizeSelections();

            // Signal the waiting thread modification is complete
            if (_customizeSelectionsWaitHandle != null)
            {
                _customizeSelectionsWaitHandle.Set();
                _customizeSelectionsWaitHandle = null;
            }
        }
        #endregion

        #region Task tracking
        /// <summary>
        /// Gets a GUI element for reporting progress of a generic <see cref="ITask"/>. Should only be one running at a time.
        /// </summary>
        /// <param name="taskName">The name of the task to be tracked.</param>
        /// <remarks>This method must not be called from a background thread.</remarks>
        public IProgress<TaskSnapshot> GetProgressControl(string taskName)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(taskName)) throw new ArgumentNullException(nameof(taskName));
            if (InvokeRequired) throw new InvalidOperationException("Method called from a non UI thread.");
            #endregion

            taskControl.Visible = true;

            // Hide other stuff
            if (_selectionsShown) selectionsControl.Hide();

            taskControl.TaskName = taskName;
            return taskControl;
        }

        /// <summary>
        /// Gets a GUI element for reporting progress of a <see cref="ITask"/> for a specific implementation. May run multiple in parallel.
        /// </summary>
        /// <param name="taskName">The name of the task to be tracked.</param>
        /// <param name="tag">A digest used to associate the task with a specific implementation.</param>
        /// <remarks>This method must not be called from a background thread.</remarks>
        public IProgress<TaskSnapshot> GetProgressControl(string taskName, ManifestDigest tag)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(taskName)) throw new ArgumentNullException(nameof(taskName));
            if (InvokeRequired) throw new InvalidOperationException("Method called from a non UI thread.");
            #endregion

            // Hide other stuff
            taskControl.Hide();

            if (_selectionsShown)
            {
                var control = selectionsControl.TaskControls[tag];
                control.TaskName = taskName;
                return control;
            }
            else return GetProgressControl(taskName);
        }

        /// <summary>
        /// Restores the UI activated by <see cref="ShowSelections"/> and hidden by <see cref="GetProgressControl(string)"/> after an <see cref="ITask.Run"/> completes.
        /// </summary>
        public void RestoreSelections()
        {
            if (_selectionsShown)
            {
                taskControl.Visible = false;
                selectionsControl.Visible = true;
            }
        }
        #endregion

        #region Question
        private string _pendingQuestion;
        private Future<DialogResult> _pendingResult;

        /// <summary>
        /// Asks the user a Yes/No/Cancel question.
        /// </summary>
        /// <param name="question">The question and comprehensive information to help the user make an informed decision.</param>
        /// <param name="severity">The severity/possible impact of the question.</param>
        /// <returns><c>true</c> if the user answered with 'Yes'; <c>false</c> if the user answered with 'No'.</returns>
        /// <exception cref="OperationCanceledException">The user selected 'Cancel'.</exception>
        public Future<DialogResult> Ask(string question, MsgSeverity severity)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(question)) throw new ArgumentNullException(nameof(question));
            #endregion

            if (Visible)
                return Msg.YesNoCancel(this, question, severity);
            else
            {
                _pendingQuestion = question;
                _pendingResult = new Future<DialogResult>();

                ShowTrayIcon(question.GetLeftPartAtFirstOccurrence(Environment.NewLine) + Environment.NewLine + Resources.ClickToChoose, ToolTipIcon.Info);

                return _pendingResult;
            }
        }

        private void ProgressForm_Shown(object sender, EventArgs e)
        {
            if (_pendingQuestion != null)
            {
                _pendingResult.Set(Msg.YesNoCancel(this, _pendingQuestion, MsgSeverity.Warn));
                _pendingQuestion = null;
            }
        }
        #endregion

        #region Tray icon
        /// <summary>
        /// Shows the tray icon with an associated balloon message.
        /// </summary>
        /// <param name="information">The balloon message text.</param>
        /// <param name="messageType">The type icon to display next to the balloon message.</param>
        /// <exception cref="InvalidOperationException">The value is set from a thread other than the UI thread.</exception>
        /// <remarks>This method must not be called from a background thread.</remarks>
        public void ShowTrayIcon(string information = null, ToolTipIcon messageType = ToolTipIcon.None)
        {
            #region Sanity checks
            if (InvokeRequired) throw new InvalidOperationException("Method called from a non UI thread.");
            #endregion

            notifyIcon.Visible = true;
            if (!string.IsNullOrEmpty(information)) notifyIcon.ShowBalloonTip(7500, "Zero Install", information, messageType);
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
            Hide();

            _cancellationTokenSource.Cancel();

            // Unblock any waiting thread
            _customizeSelectionsWaitHandle?.Set();
        }

        /// <summary>
        /// Hides the form and the tray icon.
        /// </summary>
        /// <exception cref="InvalidOperationException">The value is set from a thread other than the UI thread.</exception>
        /// <remarks>This method must not be called from a background thread.</remarks>
        public new void Hide()
        {
            #region Sanity checks
            if (InvokeRequired) throw new InvalidOperationException("Method called from a non UI thread.");
            #endregion

            Visible = false;
            notifyIcon.Visible = false;
        }
        #endregion
    }
}
