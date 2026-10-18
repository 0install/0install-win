// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System.Diagnostics;

namespace ZeroInstall.Commands.WinForms;

/// <summary>
/// Helpers for running 0install-win commands.
/// </summary>
public static class CommandUtils
{
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
    /// Starts a 0install-win command elevated as Administrator and does not wait for it to complete.
    /// </summary>
    /// <param name="args">Command name with arguments to execute.</param>
    public static void StartAsAdmin(params string?[] args)
    {
        try
        {
            GetStartInfo(args).AsAdmin().Start();
        }
        #region Error handling
        catch (OperationCanceledException) {}
        catch (Exception ex) when (ex is PlatformNotSupportedException or IOException)
        {
            Msg.Inform(null, ex.Message, MsgSeverity.Error);
        }
        #endregion
    }

    private static ProcessStartInfo GetStartInfo(string?[] args)
        => ProcessUtils.Assembly("0install-win", args.WhereNotNull().ToArray());
}
