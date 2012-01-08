/*
 * Copyright 2010-2011 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using Common;
using Common.Storage;
using Common.Streams;
using Common.Tasks;
using Common.Utils;
using Microsoft.CSharp;
using ZeroInstall.Model;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall.DesktopIntegration.Windows
{
    /// <summary>
    /// Utility class for building stub EXEs that execute "0install" commands. Provides persistent local paths.
    /// </summary>
    public static class StubBuilder
    {
        #region Build
        /// <summary>
        /// Builds a stub EXE that executes the "0install run" command.
        /// </summary>
        /// <param name="path">The target path to store the generated EXE file.</param>
        /// <param name="target">The application to be launched via the stub.</param>
        /// <param name="command">The command argument to be passed to the the "0install run" command; may be <see langword="null"/>.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <exception cref="OperationCanceledException">Thrown if the user canceled the task.</exception>
        /// <exception cref="InvalidOperationException">Thrown if there was a compilation error while generating the stub EXE.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem is not permitted.</exception>
        internal static void BuildRunStub(string path, InterfaceFeed target, string command, ITaskHandler handler)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            // Make sure the containing directory exists
            string directory = Path.GetDirectoryName(Path.GetFullPath(path));
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);

            // Build command-line
            string args = "run ";
            if (!string.IsNullOrEmpty(command)) args += "--command=" + StringUtils.EscapeArgument(command) + " ";
            args += StringUtils.EscapeArgument(target.InterfaceID);

            var entryPoint = target.Feed.GetEntryPoint(command ?? Command.NameRun);
            bool needsTerminal = target.Feed.NeedsTerminal || (entryPoint != null && entryPoint.NeedsTerminal);

            // Load the template code and insert variables
            string code = GetEmbeddedResource("Stub.template").Replace("[EXE]", Path.Combine(Locations.InstallBase, needsTerminal ? "0install.exe" : "0install-win.exe").Replace(@"\", @"\\"));
            code = code.Replace("[ARGUMENTS]", EscapeForCode(args));
            code = code.Replace("[TITLE]", EscapeForCode(target.Feed.GetBestName(CultureInfo.CurrentUICulture, command)));

            // Configure the compiler
            var compilerParameters = new CompilerParameters
            {
                GenerateExecutable = true, OutputAssembly = path, IncludeDebugInformation = false, GenerateInMemory = false, TreatWarningsAsErrors = true,
                ReferencedAssemblies = {"System.dll"}
            };
            if (!needsTerminal) compilerParameters.CompilerOptions += " /target:winexe";

            // Set icon if available
            try
            {
                string iconPath = IconCacheProvider.CreateDefault().GetIcon(target.Feed.GetIcon(Icon.MimeTypeIco, command).Location, handler);
                compilerParameters.CompilerOptions += " /win32icon:" + StringUtils.EscapeArgument(iconPath);
            }
            catch (KeyNotFoundException)
            {}

            using (var manifestFile = new TemporaryFile("0install"))
            {
                // Select compiler
                CodeDomProvider compiler;
                if (File.Exists(Environment.ExpandEnvironmentVariables(@"%windir%\Microsoft.NET\Framework\v3.5\csc.exe")))
                { // Use C# v3.5 compiler if available to add Win32 manifest
                    File.WriteAllText(manifestFile.Path, GetEmbeddedResource("Stub.manifest"));
                    compilerParameters.CompilerOptions += " /win32manifest:" + StringUtils.EscapeArgument(manifestFile.Path);
                    compiler = new CSharpCodeProvider(new Dictionary<string, string> {{"CompilerVersion", "v3.5"}});
                }
                else compiler = new CSharpCodeProvider();

                // Run the compilation process and check for errors
                var compilerResults = compiler.CompileAssemblyFromSource(compilerParameters, code);
                if (compilerResults.Errors.HasErrors)
                {
                    var error = compilerResults.Errors[0];
                    throw new InvalidOperationException("Compilation error " + error.ErrorNumber + " in line " + error.Line + "\n" + error.ErrorText);
                }
            }
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Escapes a string so that is safe for substitution inside C# code
        /// </summary>
        private static string EscapeForCode(string value)
        {
            return value.Replace(@"\", @"\\").Replace("\"", "\\\"").Replace("\n", @"\n");
        }

        private static string GetEmbeddedResource(string name)
        {
            var assembly = Assembly.GetAssembly(typeof(StubBuilder));
            using (var stream = assembly.GetManifestResourceStream(typeof(StubBuilder), name))
                return StreamUtils.ReadToString(stream);
        }
        #endregion

        #region Get
        /// <summary>How long to keep reusing existing stubs before rebuilding them.</summary>
        private static readonly TimeSpan _freshness = new TimeSpan(1, 0, 0, 0); // 1 day

        /// <summary>
        /// Uses <see cref="BuildRunStub"/> to build a stub EXE in a well-known location. Future calls with the same arguments will return the same EXE without rebuilding it.
        /// </summary>
        /// <param name="target">The application to be launched via the stub.</param>
        /// <param name="command">The command argument to be passed to the the "0install run" command; may be <see langword="null"/>.</param>
        /// <param name="systemWide">Store the stub in a system-wide directory instead of just for the current user.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <returns>The path to the generated stub EXE.</returns>
        /// <exception cref="OperationCanceledException">Thrown if the user canceled the task.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem is not permitted.</exception>
        public static string GetRunStub(InterfaceFeed target, string command, bool systemWide, ITaskHandler handler)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            string hash = StringUtils.Hash(target.InterfaceID + "#" + command, SHA256.Create());
            string dirPath = Locations.GetIntegrationDirPath("0install.net", systemWide, "desktop-integration", "stubs", hash);

            var entryPoint = target.Feed.GetEntryPoint(command ?? Command.NameRun);
            string exeName = (entryPoint != null && !string.IsNullOrEmpty(entryPoint.BinaryName))
                ? entryPoint.BinaryName
                : ModelUtils.Escape(target.Feed.Name);
            string exePath = Path.Combine(dirPath, exeName + ".exe");

            if (File.Exists(exePath))
            { // Existing stub, ...
                if ((DateTime.UtcNow - File.GetLastWriteTimeUtc(exePath)) > _freshness)
                { // Stale, try to rebuild
                    try
                    {
                        File.Delete(exePath);
                    }
                        #region Error handling
                    catch (IOException ex)
                    {
                        Log.Warn("Unable to replace stale stub: " + exePath + "\n" + ex.Message);
                        return exePath;
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        Log.Warn("Unable to replace stale stub: " + exePath + "\n" + ex.Message);
                        return exePath;
                    }
                    #endregion

                    BuildRunStub(exePath, target, command, handler);
                    return exePath;
                }
                else
                { // Fresh, keep existing
                    return exePath;
                }
            }
            else
            { // No existing stub, build new one
                BuildRunStub(exePath, target, command, handler);
                return exePath;
            }
        }
        #endregion
    }
}
