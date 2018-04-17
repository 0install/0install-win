// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Globalization;
using System.IO;
using System.Net;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Native;
using NanoByte.Common.Net;
using NanoByte.Common.Storage;
using NanoByte.Common.Values;
using NDesk.Options;
using ZeroInstall.Commands.CliCommands;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Services.Executors;
using ZeroInstall.Services.Solvers;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Provides utility methods for application entry points.
    /// </summary>
    public static class ProgramUtils
    {
        /// <summary>
        /// Indicates whether the application is running from an implementation cache.
        /// </summary>
        public static bool IsRunningFromCache => StoreUtils.DetectImplementationPath(Locations.InstallBase) != null;

        /// <summary>
        /// Indicates whether the application is running from a user-specific location.
        /// </summary>
        public static bool IsRunningFromPerUserDir => Locations.InstallBase.StartsWith(Locations.HomeDir);

        /// <summary>
        /// The current UI language; <c>null</c> to use system default.
        /// </summary>
        /// <remarks>This value is only used on Windows and is stored in the Registry. For non-Windows platforms use the <c>LC_*</c> environment variables instead.</remarks>
        [CanBeNull]
        public static CultureInfo UILanguage
        {
            get
            {
                string language = RegistryUtils.GetSoftwareString("Zero Install", "Language");
                if (!string.IsNullOrEmpty(language))
                {
                    try
                    {
                        return Languages.FromString(language);
                    }
                    catch (ArgumentException ex)
                    {
                        Log.Warn(ex);
                    }
                }
                return null;
            }
            set => RegistryUtils.SetSoftwareString("Zero Install", "Language", value?.ToString() ?? "");
        }

        /// <summary>
        /// Common initialization code to be called by every Frontend executable right after startup.
        /// </summary>
        public static void Init()
        {
            // Encode installation path into mutex name to allow instance detection during updates
            string mutexName = "mutex-" + Locations.InstallBase.GetHashCode();
            if (AppMutex.Probe(mutexName + "-update")) Environment.Exit(999);
            AppMutex.Create(mutexName);

            if (WindowsUtils.IsWindows && UILanguage != null) Languages.SetUI(UILanguage);

            ProcessUtils.SanitizeEnvironmentVariables();
            NetUtils.ApplyProxy();
            NetUtils.ConfigureTls();
        }

        /// <summary>
        /// The EXE name for the Command GUI best suited for the current system; <c>null</c> if no GUI subsystem is running.
        /// </summary>
        [CanBeNull]
        public static readonly string GuiAssemblyName =
            WindowsUtils.IsWindows && WindowsUtils.IsInteractive ? "0install-win" : null;

        private const string
            RegKeyFSPolicyMachine = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\FileSystem",
            RegKeyFSPolicyUser = @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy Objects\{B0D05113-7B6B-4D69-81E2-8E8836775C9C}Machine\System\CurrentControlSet\Control\FileSystem",
            RegValueNameLongPaths = "LongPathsEnabled";

        /// <summary>
        /// Parses command-line arguments and performs the indicated action. Performs error handling.
        /// </summary>
        /// <param name="exeName">The name of the executable to use as a reference in help messages and self-invokation.</param>
        /// <param name="args">The arguments to be processed.</param>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or informed about download and IO tasks.</param>
        /// <returns>The exit status code to end the process with. Cast to <see cref="int"/> to return from a Main method.</returns>
        public static ExitCode Run([NotNull] string exeName, [NotNull] string[] args, [NotNull] ICommandHandler handler)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(exeName)) throw new ArgumentNullException(nameof(exeName));
            if (args == null) throw new ArgumentNullException(nameof(args));
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            #endregion

            try
            {
                var command = CommandFactory.CreateAndParse(args, handler);
                return command.Execute();
            }
            #region Error handling
            catch (OperationCanceledException)
            {
                return ExitCode.UserCanceled;
            }
            catch (NeedGuiException ex)
            {
                if (GuiAssemblyName != null)
                {
                    Log.Info("Switching to GUI");
                    handler.DisableUI();
                    try
                    {
                        return (ExitCode)ProcessUtils.Assembly(GuiAssemblyName, args).Run();
                    }
                    catch (IOException ex2)
                    {
                        handler.Error(ex2);
                        return ExitCode.IOError;
                    }
                    catch (NotAdminException ex2)
                    {
                        handler.Error(ex2);
                        return ExitCode.AccessDenied;
                    }
                }
                else
                {
                    handler.Error(ex);
                    return ExitCode.NotSupported;
                }
            }
            catch (NotAdminException ex)
            {
                if (WindowsUtils.HasUac)
                {
                    Log.Info("Elevating to admin");
                    handler.DisableUI();
                    try
                    {
                        return (ExitCode)ProcessUtils.Assembly(GuiAssemblyName ?? exeName, args).AsAdmin().Run();
                    }
                    catch (PlatformNotSupportedException ex2)
                    {
                        handler.Error(ex2);
                        return ExitCode.NotSupported;
                    }
                    catch (IOException ex2)
                    {
                        handler.Error(ex2);
                        return ExitCode.IOError;
                    }
                    catch (NotAdminException ex2)
                    {
                        handler.Error(ex2);
                        return ExitCode.AccessDenied;
                    }
                    catch (OperationCanceledException)
                    {
                        return ExitCode.UserCanceled;
                    }
                }
                else
                {
                    handler.Error(ex);
                    return ExitCode.AccessDenied;
                }
            }
            catch (UnsuitableInstallBaseException ex)
            {
                if (WindowsUtils.IsWindows)
                {
                    try
                    {
                        var result = TryRunOtherInstance(exeName, args, handler, ex.NeedsMachineWide);
                        if (result.HasValue) return result.Value;
                        else if (handler.Ask(Resources.AskDeployZeroInstall + Environment.NewLine + ex.Message,
                            defaultAnswer: true, alternateMessage: ex.Message))
                        {
                            var deployArgs = new[] {MaintenanceMan.Name, MaintenanceMan.Deploy.Name, "--batch"};
                            if (ex.NeedsMachineWide) deployArgs = deployArgs.Append("--machine");
                            var deployResult = Run(exeName, deployArgs, handler);
                            if (deployResult == ExitCode.OK)
                            {
                                result = TryRunOtherInstance(exeName, args, handler, ex.NeedsMachineWide);
                                if (result.HasValue) return result.Value;
                                else throw new IOException("Unable to find newly installed instance.");
                            }
                            else return deployResult;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        return ExitCode.UserCanceled;
                    }
                    catch (IOException ex2)
                    {
                        handler.Error(ex2);
                        return ExitCode.IOError;
                    }
                    catch (NotAdminException ex2)
                    {
                        handler.Error(ex2);
                        return ExitCode.AccessDenied;
                    }
                }
                else handler.Error(ex);

                return ExitCode.NotSupported;
            }
            catch (OptionException ex)
            {
                handler.Error(new OptionException(ex.Message + Environment.NewLine + string.Format(Resources.TryHelp, exeName), ex.OptionName));
                return ExitCode.InvalidArguments;
            }
            catch (FormatException ex)
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
            catch (PathTooLongException ex) when (
                WindowsUtils.IsWindows10Redstone &&
                Environment.Version >= new Version(4, 6, 2) &&
                RegistryUtils.GetDword(RegKeyFSPolicyUser, RegValueNameLongPaths, defaultValue: RegistryUtils.GetDword(RegKeyFSPolicyMachine, RegValueNameLongPaths)) != 1)
            {
                string message = ex.Message + @" " + Resources.SuggestLongPath;
                if (handler.Ask(message + @" " + Resources.AskTryNow, defaultAnswer: false, alternateMessage: message))
                {
                    try
                    {
                        RegistryUtils.SetDword(WindowsUtils.IsAdministrator ? RegKeyFSPolicyMachine : RegKeyFSPolicyUser, RegValueNameLongPaths, 1);
                        return (ExitCode)ProcessUtils.Assembly(exeName, args).Run();
                    }
                    catch (PlatformNotSupportedException ex2)
                    {
                        handler.Error(ex2);
                        return ExitCode.NotSupported;
                    }
                    catch (IOException ex2)
                    {
                        handler.Error(ex2);
                        return ExitCode.IOError;
                    }
                    catch (NotAdminException ex2)
                    {
                        handler.Error(ex2);
                        return ExitCode.AccessDenied;
                    }
                    catch (OperationCanceledException)
                    {
                        return ExitCode.UserCanceled;
                    }
                }

                handler.Error(ex);
                return ExitCode.IOError;
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
            catch (ConflictException ex)
            {
                handler.Error(ex);
                return ExitCode.Conflict;
            }
            #endregion

            finally
            {
                handler.CloseUI();
            }
        }

        /// <summary>
        /// Tries to run a command in another instance of Zero Install deployed on this system.
        /// </summary>
        /// <param name="exeName">The name of the executable to call in the target instance.</param>
        /// <param name="args">The arguments to pass to the target instance.</param>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or informed about download and IO tasks.</param>
        /// <param name="needsMachineWide"><c>true</c> if a machine-wide install location is required; <c>false</c> if a user-specific location will also do.</param>
        /// <returns>The exit code returned by the other instance; <c>null</c> if no other instance could be found.</returns>
        /// <exception cref="IOException">There was a problem launching the target instance.</exception>
        /// <exception cref="NotAdminException">The target process requires elevation.</exception>
        private static ExitCode? TryRunOtherInstance([NotNull] string exeName, [NotNull] string[] args, [NotNull] ICommandHandler handler, bool needsMachineWide)
        {
            string installLocation = FindOtherInstance(needsMachineWide);
            if (installLocation == null) return null;

            Log.Warn("Redirecting to instance at " + installLocation);
            handler.DisableUI();
            return (ExitCode)ProcessUtils.Assembly(Path.Combine(installLocation, exeName), args).Run();
        }

        /// <summary>
        /// Tries to find another instance of Zero Install deployed on this system.
        /// </summary>
        /// <param name="needsMachineWide"><c>true</c> if a machine-wide install location is required; <c>false</c> if a user-specific location will also do.</param>
        /// <returns>The installation directory of another instance of Zero Install; <c>null</c> if none was found.</returns>
        [CanBeNull]
        public static string FindOtherInstance(bool needsMachineWide = true)
        {
            if (!WindowsUtils.IsWindows) return null;

            string installLocation = RegistryUtils.GetSoftwareString("Zero Install", "InstallLocation");
            if (string.IsNullOrEmpty(installLocation)) return null;
            if (installLocation == Locations.InstallBase) return null;
            if (needsMachineWide && installLocation.StartsWith(Locations.HomeDir)) return null;
            if (!File.Exists(Path.Combine(installLocation, "0install.exe"))) return null;
            return installLocation;
        }
    }
}
