// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System.Diagnostics;
using System.Security;
using NanoByte.Common.Native;
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
#if NETFRAMEWORK
            if (RegistryUtils.GetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full", "Release") < 461808)
                throw new IOException("Please download and install .NET Framework 4.7.2 or later: https://dotnet.microsoft.com/en-us/download/dotnet-framework");
#endif

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
        catch (NotAdminException) when (WindowsUtils.HasUac && handler.IsGui)
        {
            Log.Info("Elevating to admin");
            try
            {
                return (ExitCode)GetStartInfo(args).AsAdmin().Run();
            }
            catch (IOException ex)
            {
                handler.Error(ex);
                return ExitCode.IOError;
            }
            catch (OperationCanceledException)
            {
                return ExitCode.UserCanceled;
            }
        }
        catch (NotSupportedException ex)
        {
            handler.Error(ex);
            return ExitCode.NotSupported;
        }
        catch (Exception ex) when (ex is IOException or Win32Exception)
        {
            handler.Error(ex);
            return ExitCode.IOError;
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or SecurityException)
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

    public static ProcessStartInfo GetStartInfo(params string[] args)
        => ProcessUtils.Assembly(Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule!.FileName), args);
}
