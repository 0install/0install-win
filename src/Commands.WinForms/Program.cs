// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using Eto;
using NanoByte.Common.Native;
using NanoByte.Common.Threading;
using ZeroInstall.Commands;

namespace ZeroInstall.Commands.WinForms;

public static class Program
{
    /// <remarks>Explicit Main method instead of top-level statements, since the Eto.Forms message loop running on the main thread requires STA.</remarks>
    [STAThread]
    public static void Main(string[] args)
    {
        ProgramUtils.Init();
        WindowsUtils.SetCurrentProcessAppID(ZeroInstallInstance.IsIntegrated ? "ZeroInstall" : "ZeroInstall.NotIntegrated");
        ErrorReportForm.SetupMonitoring(new("https://0install.de/error-report/"));

        // Same size as buttons created with the Windows Forms designer
        Eto.WinForms.Forms.Controls.ButtonHandler.DefaultMinimumSize = new(85, 23);

        // Buttons created in code only use the themed background if their parent has the default background color, which is not the case, e.g., on tab pages
        Style.Add<Eto.WinForms.Forms.Controls.ButtonHandler>(null, handler => handler.Control.UseVisualStyleBackColor = true);

        EtoApp.Run(new Eto.WinForms.Platform(), () =>
        {
            using var handler = new WindowsGuiCommandHandler();
            return (int)ProgramUtils.Run("0install-win", args, handler);
        });
    }
}
