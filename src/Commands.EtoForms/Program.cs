// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using Eto;
using NanoByte.Common.Native;
using NanoByte.Common.Threading;

namespace ZeroInstall.Commands.EtoForms;

public static class Program
{
    internal const string ExeName = "0install-gui";

    [STAThread]
    public static int Main(string[] args)
    {
        ProgramUtils.Init();
        if (WindowsUtils.IsWindows)
            WindowsUtils.SetCurrentProcessAppID(ZeroInstallInstance.IsIntegrated ? "ZeroInstall" : "ZeroInstall.NotIntegrated");
        ErrorReportForm.SetupMonitoring(new("https://0install.de/error-report/"));

        if (TryGetPlatform() is {} platform)
        {
            EtoApp.Run(platform, () =>
            {
                using var handler = new GuiCommandHandler();
                return (int)ProgramUtils.Run(ExeName, args, handler);
            });
        }

        // Fall back to command-line interface if no GUI is available (e.g., no display server)
        using var cliHandler = new CliCommandHandler();
        return (int)ProgramUtils.Run(ExeName, args, cliHandler);
    }

    private static Platform? TryGetPlatform()
    {
        try
        {
            return Platform.Detect;
        }
        #region Error handling
        catch (InvalidOperationException ex)
        {
            Log.Debug("Unable to detect Eto.Forms platform", ex);
            return null;
        }
        #endregion
    }
}
