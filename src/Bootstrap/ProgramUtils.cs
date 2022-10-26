// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using NanoByte.Common.Net;
using NDesk.Options;
using ZeroInstall.Services.Executors;
using ZeroInstall.Services.Solvers;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Trust;

namespace ZeroInstall;

/// <summary>
/// Provides utility methods for application entry points.
/// </summary>
public static class ProgramUtils
{
    /// <summary>
    /// Common initialization code to be called by every Bootstrap executable right after startup.
    /// </summary>
    public static void Init()
    {
        ProcessUtils.SanitizeEnvironmentVariables();
        NetUtils.ApplyProxy();
    }

    /// <summary>
    /// Executes the bootstrap process controlled by command-line arguments.
    /// </summary>
    /// <param name="args">The command-line arguments to handle.</param>
    /// <param name="handler">A callback object used when the the user needs to be asked questions or informed about download and IO tasks.</param>
    /// <returns>The exit status code to end the process with.</returns>
    public static ExitCode Run(string[] args, IBootstrapHandler handler)
    {
        try
        {
            return new BootstrapProcess(handler).Execute(args);
        }
        #region Error handling
        catch (OperationCanceledException)
        {
            return ExitCode.UserCanceled;
        }
        catch (Exception ex) when (ex is OptionException or FormatException)
        {
            handler.Error(ex);
            return ExitCode.InvalidArguments;
        }
        catch (WebException ex)
        {
            handler.Error(ex);
            return ExitCode.WebError;
        }
        catch (NotSupportedException ex)
        {
            handler.Error(ex);
            return ExitCode.NotSupported;
        }
        catch (IOException ex)
        {
            handler.Error(ex);
            return ExitCode.IOError;
        }
        catch (UnauthorizedAccessException ex)
        {
            handler.Error(ex);
            return ExitCode.AccessDenied;
        }
        catch (InvalidDataException ex)
        {
            handler.Error(ex);
            return ExitCode.InvalidData;
        }
        catch (SignatureException ex)
        {
            handler.Error(ex);
            return ExitCode.InvalidSignature;
        }
        catch (DigestMismatchException ex)
        {
            Log.Info(ex.LongMessage);
            handler.Error(ex);
            return ExitCode.DigestMismatch;
        }
        catch (SolverException ex)
        {
            handler.Error(ex);
            return ExitCode.SolverError;
        }
        catch (ExecutorException ex)
        {
            handler.Error(ex);
            return ExitCode.ExecutorError;
        }
        #endregion
    }
}
