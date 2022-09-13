// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using NanoByte.Common.Native;

namespace ZeroInstall;

/// <summary>
/// The main GUI for the Bootstrapper.
/// </summary>
public sealed partial class MainForm : Form
{
    private readonly CancellationTokenSource _cancellationTokenSource;

    public MainForm(CancellationTokenSource cancellationTokenSource)
    {
        _cancellationTokenSource = cancellationTokenSource;

        InitializeComponent();
        Font = DefaultFonts.Modern;
        HandleCreated += delegate { WindowsTaskbar.PreventPinning(Handle); };

        if (EmbeddedConfig.Load() is {AppName: not null} embeddedConfig)
        {
            Text = embeddedConfig.AppName;
            labelAppName.Text = embeddedConfig.AppName;
            Size += new Size(0, 50).ApplyScale(this);
        }
    }

    public IProgress<TaskSnapshot> GetProgressControl(string taskName)
    {
        #region Sanity checks
        if (string.IsNullOrEmpty(taskName)) throw new ArgumentNullException(nameof(taskName));
        #endregion

        taskControl.TaskName = taskName;
        taskControl.Show();
        return taskControl;
    }

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        // Never allow the user to directly close the window
        e.Cancel = true;

        // Start proper cancellation instead
        Cancel();
    }

    /// <summary>
    /// Hides the window and then starts canceling the current process asynchronously.
    /// </summary>
    private void Cancel()
    {
        Hide();

        _cancellationTokenSource.Cancel();
    }
}
