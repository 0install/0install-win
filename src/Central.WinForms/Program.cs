// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using NanoByte.Common.Native;
using ZeroInstall.Commands;

namespace ZeroInstall.Central.WinForms;

/// <summary>
/// The main GUI for Zero Install, for discovering and installing new applications, managing and launching installed applications, etc.
/// </summary>
public static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread] // Required for WinForms
    private static int Main(string[] args)
    {
        ProgramUtils.Init();
        WindowsUtils.SetCurrentProcessAppID(ZeroInstallInstance.IsIntegrated ? "ZeroInstall" : "ZeroInstall.NotIntegrated");
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        ErrorReportForm.SetupMonitoring(new("https://0install.de/error-report/"));

        try
        {
            Application.Run(new MainForm(machineWide: args.Contains("-m") || args.Contains("--machine")));
        }
        #region Error handling
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException)
        {
            Log.Error("Central startup failed", ex);
            Msg.Inform(null, ex.GetMessageWithInner(), MsgSeverity.Error);
            return -1;
        }
        #endregion

        return 0;
    }
}
