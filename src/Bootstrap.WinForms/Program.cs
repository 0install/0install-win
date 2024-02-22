// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

ProgramUtils.Init();
Application.EnableVisualStyles();
Application.SetCompatibleTextRenderingDefault(false);

ErrorReportForm.SetupMonitoring(new("https://0install.de/error-report/"));

using var handler = new GuiBootstrapHandler();

try
{
    return (int)ProgramUtils.Run(args, handler);
}
#region Error handling
catch (Win32Exception ex) // Commonly caused by GDI object exhaustion
{
    Msg.Inform(null, ex.Message, MsgSeverity.Error);
    return -1;
}
#endregion
