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
        /// Runs the specified program and waits for it to exit.
        /// </summary>
        /// <param name="startInfo">Details about the program to be launched.</param>
        /// <exception cref="Win32Exception">Thrown if the specified executable could not be launched.</exception>
        /// <exception cref="BadImageFormatException">Thrown if the specified executable could is damaged.</exception>
        public static void RunSync(ProcessStartInfo startInfo)
        {
            Process.Start(startInfo).WaitForExit();
        }

        /// <summary>
        /// Starts the specified program and immediately returns. On Unix-like systems terminating this process (the parent) will also terminate the new process (the child).
        /// </summary>
        /// <param name="startInfo">Details about the program to be launched.</param>
        /// <returns>A handle to the newly launched process.</returns>
        /// <exception cref="Win32Exception">Thrown if the specified executable could not be launched.</exception>
        /// <exception cref="BadImageFormatException">Thrown if the specified executable could is damaged.</exception>
        public static Process RunAsync(ProcessStartInfo startInfo)
        {
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
            if (MonoUtils.IsUnix && !startInfo.UseShellExecute)
            {
                Environment.CurrentDirectory = startInfo.WorkingDirectory;
                MonoUtils.ProcessDetach(startInfo.FileName, startInfo.Arguments, startInfo.EnvironmentVariables);
            }
            else RunAsync(startInfo);
        }

        /// <summary>
        /// On Windows runs the specified program and waits for it to exit. On Unix-like systems replaces the currently executing process with a new one.
        /// </summary>
        /// <param name="startInfo">Details about the program to be launched.</param>
        /// <exception cref="Win32Exception">Thrown if the specified executable could not be launched.</exception>
        /// <exception cref="BadImageFormatException">Thrown if the specified executable could is damaged.</exception>
        public static void RunReplace(ProcessStartInfo startInfo)
        {
            if (MonoUtils.IsUnix && !startInfo.UseShellExecute)
            {
                Environment.CurrentDirectory = startInfo.WorkingDirectory;
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
