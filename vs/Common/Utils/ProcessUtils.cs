using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Common.Properties;

namespace Common.Utils
{
    public static class ProcessUtils
    {
        #region Run
        /// <summary>
        /// Runs the specified program and waits for it to exit. Terminating this process (the parent) may also terminate the new process (the child).
        /// </summary>
        /// <param name="startInfo">Details about the program to be launched.</param>
        /// <exception cref="Win32Exception">Thrown if the specified executable could not be launched.</exception>
        /// <exception cref="BadImageFormatException">Thrown if the specified executable could is damaged.</exception>
        public static void RunSync(ProcessStartInfo startInfo)
        {
            #region Sanity checks
            if (startInfo == null) throw new ArgumentNullException("startInfo");
            #endregion

            Process.Start(startInfo).WaitForExit();
        }

        /// <summary>
        /// Starts the specified program and immediately returns. Terminating this process (the parent) may also terminate the new process (the child).
        /// </summary>
        /// <param name="startInfo">Details about the program to be launched.</param>
        /// <returns>A handle to the newly launched process.</returns>
        /// <exception cref="Win32Exception">Thrown if the specified executable could not be launched.</exception>
        /// <exception cref="BadImageFormatException">Thrown if the specified executable could is damaged.</exception>
        public static Process RunAsync(ProcessStartInfo startInfo)
        {
            #region Sanity checks
            if (startInfo == null) throw new ArgumentNullException("startInfo");
            #endregion

            return Process.Start(startInfo);
        }

        /// <summary>
        /// Starts the specified program and immediately returns. Terminating this process (the parent) will not affect the new process (the child).
        /// </summary>
        /// <param name="startInfo">Details about the program to be launched.</param>
        /// <exception cref="Win32Exception">Thrown if the specified executable could not be launched.</exception>
        /// <exception cref="BadImageFormatException">Thrown if the specified executable could is damaged.</exception>
        public static void RunDetached(ProcessStartInfo startInfo)
        {
            #region Sanity checks
            if (startInfo == null) throw new ArgumentNullException("startInfo");
            #endregion

            // On Unix-like systems using an external launch helper is required to detach the child process from the parent
            if (MonoUtils.IsUnix) startInfo.UseShellExecute = true;

            RunAsync(startInfo);
        }

        /// <summary>
        /// On Windows runs the specified program and waits for it to exit. On Unix-like systems replaces the currently executing process with a new one.
        /// </summary>
        /// <param name="startInfo">Details about the program to be launched.</param>
        /// <exception cref="Win32Exception">Thrown if the specified executable could not be launched.</exception>
        /// <exception cref="BadImageFormatException">Thrown if the specified executable could is damaged.</exception>
        /// <exception cref="IOException">Thrown if the process could not be replaced.</exception>
        /// <remarks>This method may not return on success. Warning: Any concurrent threads may be terminated!</remarks>
        public static void RunReplace(ProcessStartInfo startInfo)
        {
            #region Sanity checks
            if (startInfo == null) throw new ArgumentNullException("startInfo");
            #endregion

            if (MonoUtils.IsUnix && !startInfo.UseShellExecute)
            {
                if (!string.IsNullOrEmpty(startInfo.WorkingDirectory)) Environment.CurrentDirectory = startInfo.WorkingDirectory;
                MonoUtils.ProcessReplace(startInfo.FileName, startInfo.Arguments, startInfo.EnvironmentVariables);
            }
            else RunSync(startInfo);
        }
        #endregion

        #region Helper applications
        /// <summary>
        /// Attempts to launch a .NET helper assembly in the application's base directory.
        /// </summary>
        /// <param name="assembly">The name of the assembly to launch (without the file ending).</param>
        /// <param name="arguments">The command-line arguments to pass to the assembly.</param>
        /// <exception cref="FileNotFoundException">Thrown if the assembly could not be located.</exception>
        /// <exception cref="Win32Exception">Thrown if there was a problem launching the assembly.</exception>
        public static void LaunchHelperAssembly(string assembly, string arguments)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(assembly)) throw new ArgumentNullException("assembly");
            #endregion

            string appPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assembly + ".exe");
            if (!File.Exists(appPath)) throw new FileNotFoundException(string.Format(Resources.UnableToLocateAssembly, assembly), appPath);

            // Only Windows can directly launch .NET executables, other platforms must run through Mono
            RunDetached(WindowsUtils.IsWindows
                ? new ProcessStartInfo(appPath, arguments)
                : new ProcessStartInfo("mono", "\"" + appPath + "\" " + arguments));
        }
        #endregion
    }
}
