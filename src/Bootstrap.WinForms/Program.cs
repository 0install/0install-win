// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System.Windows.Forms;
using NanoByte.Common.Controls;
using ZeroInstall;

ProgramUtils.Init();
Application.EnableVisualStyles();
Application.SetCompatibleTextRenderingDefault(false);

ErrorReportForm.SetupMonitoring(new("https://0install.de/error-report/"));

using var handler = new GuiTaskHandler();
return (int)ProgramUtils.Run(args, handler, gui: true);
