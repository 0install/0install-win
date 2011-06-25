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
using System.IO;
using System.Net;
using System.Reflection;
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
    /// Utility class for building stub EXEs that execute "0install" commands.
    /// </summary>
    public static class StubProvider
    {
        #region Build
        /// <summary>
        /// Builds a stub EXE that executes the "0install run" command.
        /// </summary>
        /// <param name="path">The target path to store the generated EXE file.</param>
        /// <param name="target">The application to be laucnhed via the stub.</param>
        /// <param name="command">The command argument to be passed to the the "0install run" command; may be <see langword="null"/>.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <exception cref="UserCancelException">Thrown if the user canceled the task.</exception>
        /// <exception cref="InvalidOperationException">Thrown if there was a compilation error while generating the stub EXE.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        public static void BuildRunStub(string path, InterfaceFeed target, string command, ITaskHandler handler)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            // Make sure the containing directory exists
            string directory = Path.GetDirectoryName(Path.GetFullPath(path));
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);

            string args = string.IsNullOrEmpty(command)
                ? "run " + target.InterfaceID
                : "run --command=" + StringUtils.EscapeWhitespace(command) + " " + StringUtils.EscapeWhitespace(target.InterfaceID);

            // Load the template code and insert variables
            string code = GetEmbeddedResource("Stub.template").Replace("[EXE]", target.Feed.NeedsTerminal ? "0install.exe" : "0install-win.exe");
            code = code.Replace("[ARGUMENTS]", EscapeForCode(args));
            code = code.Replace("[TITLE]", EscapeForCode(target.Feed.Name));

            // Configure the compiler
            var compilerParameters = new CompilerParameters
            {
                GenerateExecutable = true, OutputAssembly = path, IncludeDebugInformation = false, GenerateInMemory = false, TreatWarningsAsErrors = true,
                ReferencedAssemblies = {"System.dll"}
            };
            if (!target.Feed.NeedsTerminal) compilerParameters.CompilerOptions += " /target:winexe";

            // Find the first suitable icon
            var suitableIcons = target.Feed.Icons.FindAll(icon => icon.MimeType == Icon.MimeTypeIco && icon.Location != null);
            if (!suitableIcons.IsEmpty)
            {
                string iconPath = IconCacheProvider.CreateDefault().GetIcon(suitableIcons.First.Location, handler);
                compilerParameters.CompilerOptions += " /win32icon:" + StringUtils.EscapeWhitespace(iconPath);
            }

            using (var manifestFile = new TemporaryFile("0install"))
            {
                // Select compiler
                CodeDomProvider compiler;
                if (File.Exists(Environment.ExpandEnvironmentVariables(@"%windir%\Microsoft.NET\Framework\v3.5\csc.exe")))
                { // Use C# v3.5 compiler if available to add Win32 manifest
                    File.WriteAllText(manifestFile.Path, GetEmbeddedResource("Stub.manifest"));
                    compilerParameters.CompilerOptions += " /win32manifest:" + StringUtils.EscapeWhitespace(manifestFile.Path);
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

        /// <summary>
        /// Escapes a string so that is safe for substitution inside C# code
        /// </summary>
        private static string EscapeForCode(string value)
        {
            return value.Replace(@"\", @"\\").Replace("\"", "\\\"").Replace("\n", @"\n");
        }

        private static string GetEmbeddedResource(string name)
        {
            var assembly = Assembly.GetAssembly(typeof(StubProvider));
            using (var stream = assembly.GetManifestResourceStream(typeof(StubProvider), name))
                return StreamUtils.ReadToString(stream);
        }
        #endregion

        #region Get
        /// <summary>
        /// Uses <see cref="BuildRunStub"/> to build a stub EXE in a well-known location. Future calls with the same arguments will return the same EXE without rebuilding it.
        /// </summary>
        /// <param name="target">The application to be laucnhed via the stub.</param>
        /// <param name="command">The command argument to be passed to the the "0install run" command; may be <see langword="null"/>.</param>
        /// <param name="postfix">A postfix with which the EXE name must end; may be <see langword="null"/>.</param>
        /// <param name="systemWide">Apply the configuration system-wide instead of just for the current user.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <returns>The path to the generated stub EXE.</returns>
        /// <exception cref="UserCancelException">Thrown if the user canceled the task.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        public static string GetRunStub(InterfaceFeed target, string command, string postfix, bool systemWide, ITaskHandler handler)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            string stubDirPath = Locations.GetIntegrationDirPath("0install.net", systemWide, "desktop-integration", "stubs");
            string exeName = target.Feed.Name;
            if (!string.IsNullOrEmpty(command)) exeName += "_" + command;
            exeName += "_" + ModelUtils.HashID(target.InterfaceID) + ".exe";
            string exePath = Path.Combine(stubDirPath, exeName) + postfix;

            // Return an existing stub or build a new one
            if (!File.Exists(exePath)) BuildRunStub(exePath, target, command, handler);
            return exePath;
        }
        #endregion
    }
}
