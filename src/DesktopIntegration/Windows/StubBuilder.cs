/*
 * Copyright 2010-2013 Bastian Eicher
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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using Common;
using Common.Storage;
using Common.Tasks;
using Common.Utils;
using Microsoft.CSharp;
using ZeroInstall.DesktopIntegration.Properties;
using ZeroInstall.Model;
using ZeroInstall.Store.Icons;

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
        /// <param name="needsTerminal"><see langword="true"/> to build a CLI stub, <see langword="false"/> to build a GUI stub.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <exception cref="OperationCanceledException">Thrown if the user canceled the task.</exception>
        /// <exception cref="InvalidOperationException">Thrown if there was a compilation error while generating the stub EXE.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem is not permitted.</exception>
        internal static void BuildRunStub(string path, InterfaceFeed target, string command, bool needsTerminal, ITaskHandler handler)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            // Make sure the containing directory exists
            string directory = Path.GetDirectoryName(Path.GetFullPath(path));
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);

            // Build command-line
            string args = needsTerminal ? "" : "run --no-wait ";
            if (!string.IsNullOrEmpty(command)) args += "--command=" + command.EscapeArgument() + " ";
            args += target.InterfaceID.EscapeArgument();

            // Load the template code and insert variables
            string code = GetEmbeddedResource("stub.template.cs").Replace("[EXE]", Path.Combine(Locations.InstallBase, needsTerminal ? "0launch.exe" : "0install-win.exe").Replace(@"\", @"\\"));
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
            var icon = target.Feed.GetIcon(Icon.MimeTypeIco, command);
            if (icon != null)
            {
                string iconPath = IconCacheProvider.GetInstance().GetIcon(icon.Href, handler);
                compilerParameters.CompilerOptions += " /win32icon:" + iconPath.EscapeArgument();
            }

            using (var manifestFile = new TemporaryFile("0install"))
            {
                File.WriteAllText(manifestFile, GetEmbeddedResource("Stub.manifest"));

                // Run the compilation process and check for errors
                var compiler = GetCSharpCompiler(compilerParameters, manifestFile);
                var compilerResults = compiler.CompileAssemblyFromSource(compilerParameters, code);
                if (compilerResults.Errors.HasErrors)
                {
                    var error = compilerResults.Errors[0];
                    throw new InvalidOperationException("Compilation error " + error.ErrorNumber + " in line " + error.Line + "\n" + error.ErrorText);
                }
            }
        }

        /// <summary>
        /// Detects the best possible C# compiler version and instantiates it.
        /// </summary>
        /// <param name="compilerParameters">The compiler parameters to be used. Version-specific options may be set.</param>
        /// <param name="manifestFilePath">The path of an assembly file to be added to compiled binaries if possible.</param>
        private static CodeDomProvider GetCSharpCompiler(CompilerParameters compilerParameters, string manifestFilePath)
        {
            if (Environment.Version.Major == 4)
            { // C# 4.0 (.NET 4.0/4.5)
                compilerParameters.CompilerOptions += " /win32manifest:" + manifestFilePath.EscapeArgument();
                return new CSharpCodeProvider();
            }
            else if (WindowsUtils.HasNetFxVersion(WindowsUtils.NetFx35))
            { // C# 3.0 (.NET 3.5)
                compilerParameters.CompilerOptions += " /win32manifest:" + manifestFilePath.EscapeArgument();
                return NewCSharpCodeProviderEx(WindowsUtils.NetFx35);
            }
            else
            { // C# 2.0 (.NET 2.0/3.0)
                return new CSharpCodeProvider();
            }
        }

        /// <summary>
        /// Instantiates a post-v2.0 C# compiler in a 2.0 runtime environment.
        /// </summary>
        /// <param name="version">The full .NET version number including the leading "v". Use predefined constants when possible.</param>
        /// <remarks>Extracted to a separate method in case this is older than C# 2.0 SP2 and the required constructor is therefore missing.</remarks>
        [SuppressMessage("Microsoft.Portability", "CA1903:UseOnlyApiFromTargetedFramework", MessageId = "Microsoft.CSharp.CSharpCodeProvider.#.ctor(System.Collections.Generic.IDictionary`2<System.String,System.String>)", Justification = "Will only be called on post-2.0 .NET versions")]
        private static CodeDomProvider NewCSharpCodeProviderEx(string version)
        {
            return new CSharpCodeProvider(new Dictionary<string, string> {{"CompilerVersion", version}});
        }

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
            // Determine whether the target is a CLI or GUI app
            var entryPoint = target.Feed.GetEntryPoint(command ?? Command.NameRun);
            bool needsTerminal = target.Feed.NeedsTerminal || (entryPoint != null && entryPoint.NeedsTerminal);

            BuildRunStub(path, target, command, needsTerminal, handler);
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
                return stream.ReadToString();
        }
        #endregion

        #region Get
        /// <summary>
        /// Uses <see cref="BuildRunStub(string,InterfaceFeed,string,bool,ITaskHandler)"/> to build a stub EXE in a well-known location. Future calls with the same arguments will return the same EXE without rebuilding it.
        /// </summary>
        /// <param name="target">The application to be launched via the stub.</param>
        /// <param name="command">The command argument to be passed to the the "0install run" command; may be <see langword="null"/>.</param>
        /// <param name="machineWide">Store the stub in a machine-wide directory instead of just for the current user.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <returns>The path to the generated stub EXE.</returns>
        /// <exception cref="OperationCanceledException">Thrown if the user canceled the task.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="InvalidOperationException">Thrown if write access to the filesystem is not permitted.</exception>
        public static string GetRunStub(InterfaceFeed target, string command, bool machineWide, ITaskHandler handler)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            string hash = (target.InterfaceID + "#" + command).Hash(SHA256.Create());
            string dirPath = Locations.GetIntegrationDirPath("0install.net", machineWide, "desktop-integration", "stubs", hash);

            var entryPoint = target.Feed.GetEntryPoint(command ?? Command.NameRun);
            string exeName = (entryPoint != null)
                ? entryPoint.BinaryName ?? entryPoint.Command
                : ModelUtils.Escape(target.Feed.Name);
            string exePath = Path.Combine(dirPath, exeName + ".exe");

            if (File.Exists(exePath))
            { // Existing stub, ...
                // ToDo: Find better rebuild discriminator
                if (File.GetLastWriteTime(exePath) < Process.GetCurrentProcess().StartTime)
                { // Built before current process, try to rebuild
                    try
                    {
                        File.Delete(exePath);
                    }
                        #region Error handling
                    catch (IOException ex)
                    {
                        Log.Warn(string.Format(Resources.UnableToReplaceStub, exePath));
                        Log.Warn(ex);
                        return exePath;
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        Log.Warn(string.Format(Resources.UnableToReplaceStub, exePath));
                        Log.Warn(ex);
                        return exePath;
                    }
                    #endregion

                    BuildRunStub(exePath, target, command, handler);
                    return exePath;
                }
                else
                { // Built during (probably by) current process, keep existing
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
