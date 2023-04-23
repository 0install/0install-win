// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using NanoByte.Common.Native;
using NanoByte.Common.Streams;

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
        pictureBoxSplashScreen.BackgroundImage = Image.FromStream(typeof(MainForm).GetEmbeddedStream("SplashScreen.png"));
        HandleCreated += delegate { WindowsTaskbar.PreventPinning(Handle); };

        Text = string.Format(LocalizableStrings.Title, BootstrapConfig.Instance.AppName ?? "Zero Install");
        buttonContinue.Text = LocalizableStrings.Continue;
        buttonCancel.Text = LocalizableStrings.Cancel;
        groupPath.Text = string.Format(LocalizableStrings.DestinationFolder, BootstrapConfig.Instance.AppName ?? "Zero Install");
        buttonChangePath.Text = LocalizableStrings.Change;

        if (BootstrapConfig.Instance.AppName is {} appName)
        {
            labelAppName.Text = appName;
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

    private bool _machineWide;
    private readonly TaskCompletionSource<string?> _customPathResult = new();

    public Task<string?> GetCustomPath(bool machineWide, string? currentPath)
    {
        _machineWide = machineWide;

        folderBrowserDialog.SelectedPath = currentPath;
        folderBrowserDialog.Description = string.Format(LocalizableStrings.ChoosePath, BootstrapConfig.Instance.AppName ?? "Zero Install apps");
        UpdatePath();

        return _customPathResult.Task;
    }

    private void UpdatePath(bool visible = true)
    {
        textPath.Text = string.IsNullOrEmpty(folderBrowserDialog.SelectedPath)
            ? Locations.GetCacheDirPath(".", _machineWide) + "\\..."
            : folderBrowserDialog.SelectedPath;
        groupPath.Visible = buttonContinue.Visible = buttonCancel.Visible = visible;
        if (visible) buttonContinue.Focus();
    }

    private void buttonChangePath_Click(object sender, EventArgs e)
    {
        while (true)
        {
            if (folderBrowserDialog.ShowDialog(this) != DialogResult.OK) break;
            if (IsPathOK()) break;
            else folderBrowserDialog.SelectedPath = null;
        }

        UpdatePath();
    }

    private bool IsPathOK()
    {
        try
        {
            if (FileUtils.DetermineTimeAccuracy(folderBrowserDialog.SelectedPath) != 0)
            {
                Log.Error($"Time accuracy at '{folderBrowserDialog.SelectedPath}' is insufficient; probably FAT32 filesystem");
                Msg.Inform(this, string.Format(LocalizableStrings.FolderNotNtfs, folderBrowserDialog.SelectedPath), MsgSeverity.Warn);
                return false;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex);
            Msg.Inform(this, ex.Message, MsgSeverity.Error);
            return false;
        }

        return Directory.GetFileSystemEntries(folderBrowserDialog.SelectedPath).Length == 0
            || Msg.OkCancel(this, string.Format(LocalizableStrings.FolderNotEmpty, folderBrowserDialog.SelectedPath), MsgSeverity.Warn, LocalizableStrings.UseAnyway, LocalizableStrings.ChooseDifferent);

    }

    private void buttonContinue_Click(object sender, EventArgs e)
    {
        UpdatePath(visible: false);
        _customPathResult.SetResult(folderBrowserDialog.SelectedPath);
    }

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        e.Cancel = true;
        Cancel();
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
        Cancel();
    }

    private void Cancel()
    {
        Hide();
        _cancellationTokenSource.Cancel();

        // Unblock pending callbacks
        _customPathResult.TrySetResult(null);
    }
}
