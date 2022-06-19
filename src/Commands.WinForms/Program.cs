// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using NanoByte.Common.Native;
using ZeroInstall.Commands;
using ZeroInstall.Commands.WinForms;

ProgramUtils.Init();
WindowsUtils.SetCurrentProcessAppID(ZeroInstallInstance.IsIntegrated ? "ZeroInstall" : "ZeroInstall.NotIntegrated");
Application.EnableVisualStyles();
Application.SetCompatibleTextRenderingDefault(false);
ErrorReportForm.SetupMonitoring(new("https://0install.de/error-report/"));

using var handler = new GuiCommandHandler();
return (int)ProgramUtils.Run("0install-win", args, handler);
