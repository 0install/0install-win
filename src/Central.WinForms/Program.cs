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
        WindowsUtils.SetCurrentProcessAppID("ZeroInstall");
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        ErrorReportForm.SetupMonitoring(new("https://0install.de/error-report/"));
        return Run(args);
    }

    /// <summary>
    /// Runs the application (called by main method or by embedding process).
    /// </summary>
    [STAThread] // Required for WinForms
    public static int Run(string[] args)
    {
        try
        {
            Application.Run(new MainForm(machineWide: args.Contains("-m") || args.Contains("--machine")));
        }
        #region Error handling
        catch (IOException ex)
        {
            Log.Error(ex);
            Msg.Inform(null, ex.Message, MsgSeverity.Error);
            return -1;
        }
        catch (UnauthorizedAccessException ex)
        {
            Log.Error(ex);
            Msg.Inform(null, ex.Message, MsgSeverity.Error);
            return -1;
        }
        catch (InvalidDataException ex)
        {
            Log.Error(ex);
            Msg.Inform(null, ex.Message + (ex.InnerException == null ? "" : Environment.NewLine + ex.InnerException.Message), MsgSeverity.Error);
            return -1;
        }
        #endregion

        return 0;
    }
}
