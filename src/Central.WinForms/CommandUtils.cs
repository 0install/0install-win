// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System.Diagnostics;
using ZeroInstall.Commands;

namespace ZeroInstall.Central.WinForms;

/// <summary>
/// Helpers for running 0install-win commands.
/// </summary>
public class CommandUtils
{
    private const string ExeName = "0install-win";

    /// <summary>
    /// Starts a 0install-win command and does not wait for it to complete.
    /// </summary>
    /// <param name="args">Command name with arguments to execute.</param>
    public static void Start(params string?[] args)
    {
        try
        {
           GetStartInfo(args).Start();
        }
        #region Error handling
        catch (IOException ex)
        {
            Msg.Inform(null, ex.Message, MsgSeverity.Error);
        }
        #endregion
    }

    /// <summary>
    /// Runs a 0install-win command and waits for it to complete.
    /// </summary>
    /// <param name="args">Command name with arguments to execute.</param>
    /// <returns>The command's exit code.</returns>
    public static async Task<ExitCode> RunAsync(params string?[] args)
    {
        try
        {
            return await Task.Run(() => (ExitCode)GetStartInfo(args).Run());
        }
        #region Error handling
        catch (Exception ex) when (ex is IOException or NotAdminException)
        {
            Msg.Inform(null, ex.Message, MsgSeverity.Error);
            return (ExitCode)(-1);
        }
        #endregion
    }

    private static ProcessStartInfo GetStartInfo(string?[] args)
        => ProcessUtils.Assembly("0install-win", args.WhereNotNull().ToArray());
}
