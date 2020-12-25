// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System.Windows.Forms;
using NanoByte.Common.Controls;
using NanoByte.Common.Native;
using ZeroInstall.Commands;
using ZeroInstall.Commands.WinForms;

ProgramUtils.Init();
WindowsUtils.SetCurrentProcessAppID("ZeroInstall");
Application.EnableVisualStyles();
Application.SetCompatibleTextRenderingDefault(false);
ErrorReportForm.SetupMonitoring(new("https://0install.de/error-report/"));

using var handler = new GuiCommandHandler();
return (int)ProgramUtils.Run("0install-win", args, handler);
