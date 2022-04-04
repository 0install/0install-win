// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

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

        try
        {
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }
        catch (ArgumentException) // Running from network path, can't extract icon
        {}

        var embeddedConfig = EmbeddedConfig.Load();
        if (embeddedConfig.AppName != null)
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
