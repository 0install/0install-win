// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

namespace ZeroInstall.Commands.WinForms;

/// <summary>
/// Helpers for running 0install-win commands.
/// </summary>
public class CommandUtils
{
    /// <summary>
    /// The base name of the executable.
    /// </summary>
    public const string ExeName = "0install-win";

    /// <summary>
    /// Starts a 0install-win command and does not wait for it to complete.
    /// </summary>
    /// <param name="args">Command name with arguments to execute.</param>
    public static void Start(params string?[] args)
    {
        args = args.WhereNotNull().ToArray();

        Log.Debug($"Starting {ExeName} {args.JoinEscapeArguments()}");

        try
        {
            ProcessUtils.Assembly(ExeName, args).Start();
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
        args = args.WhereNotNull().ToArray();

        Log.Debug($"Running {ExeName} {args.JoinEscapeArguments()}");
        var exitCode = await Task.Run(() => (ExitCode)ProcessUtils.Assembly(ExeName, args).Run());
        Log.Debug($"{ExeName} {args.JoinEscapeArguments()} finished with exit code: {exitCode}");
        return exitCode;
    }
}
