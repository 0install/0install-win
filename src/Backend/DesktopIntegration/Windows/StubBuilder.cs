/*
 * Copyright 2010-2015 Bastian Eicher
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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NanoByte.Common.Streams;
using NanoByte.Common.Tasks;
using ZeroInstall.DesktopIntegration.Properties;
using ZeroInstall.Store;
using ZeroInstall.Store.Icons;
using ZeroInstall.Store.Model;

namespace ZeroInstall.DesktopIntegration.Windows
{
    /// <summary>
    /// Utility class for building stub EXEs that execute "0install" commands. Provides persistent local paths.
    /// </summary>
    public static class StubBuilder
    {
        #region Get
        /// <summary>
        /// Builds a stub EXE in a well-known location. Future calls with the same arguments will return the same EXE.
        /// </summary>
        /// <param name="target">The application to be launched via the stub.</param>
        /// <param name="command">The command argument to be passed to the the "0install run" command; can be <see langword="null"/>.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <param name="machineWide">Store the stub in a machine-wide directory instead of just for the current user.</param>
        /// <returns>The path to the generated stub EXE.</returns>
        /// <exception cref="OperationCanceledException">The user canceled the task.</exception>
        /// <exception cref="InvalidOperationException">There was a compilation error while generating the stub EXE.</exception>
        /// <exception cref="IOException">A problem occurs while writing to the filesystem.</exception>
        /// <exception cref="WebException">A problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="InvalidOperationException">Write access to the filesystem is not permitted.</exception>
        public static string GetRunStub(this FeedTarget target, [CanBeNull] string command, [NotNull] ITaskHandler handler, bool machineWide = false)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            var entryPoint = target.Feed.GetEntryPoint(command);
            string exeName = (entryPoint != null)
                ? entryPoint.BinaryName ?? entryPoint.Command
                : FeedUri.Escape(target.Feed.Name);
            bool needsTerminal = (entryPoint != null && entryPoint.NeedsTerminal);

            string hash = (target.Uri + "#" + command).Hash(SHA256.Create());
            string path = Path.Combine(Locations.GetIntegrationDirPath("0install.net", machineWide, "desktop-integration", "stubs", hash), exeName + ".exe");

            target.CreateOrUpdateRunStub(path, command, needsTerminal, handler);
            return path;
        }

        /// <summary>
        /// Creates a new or updates an existing stub EXE that executes the "0install run" command.
        /// </summary>
        /// <seealso cref="BuildRunStub"/>
        /// <param name="target">The application to be launched via the stub.</param>
        /// <param name="path">The target path to store the generated EXE file.</param>
        /// <param name="command">The command argument to be passed to the the "0install run" command; can be <see langword="null"/>.</param>
        /// <param name="needsTerminal"><see langword="true"/> to build a CLI stub, <see langword="false"/> to build a GUI stub.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <exception cref="OperationCanceledException">The user canceled the task.</exception>
        /// <exception cref="InvalidOperationException">There was a compilation error while generating the stub EXE.</exception>
        /// <exception cref="IOException">A problem occurs while writing to the filesystem.</exception>
        /// <exception cref="WebException">A problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the filesystem is not permitted.</exception>
        private static void CreateOrUpdateRunStub(this FeedTarget target, [NotNull] string path, [CanBeNull] string command, bool needsTerminal, [NotNull] ITaskHandler handler)
        {
            if (File.Exists(path))
            { // Existing stub
                // TODO: Find better rebuild discriminator
                if (File.GetLastWriteTime(path) < Process.GetCurrentProcess().StartTime)
                { // Outdated, try to rebuild
                    try
                    {
                        File.Delete(path);
                    }
                        #region Error handling
                    catch (IOException ex)
                    {
                        Log.Warn(string.Format(Resources.UnableToReplaceStub, path));
                        Log.Warn(ex);
                        return;
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        Log.Warn(string.Format(Resources.UnableToReplaceStub, path));
                        Log.Warn(ex);
                        return;
                    }
                    #endregion

                    target.BuildRunStub(path, handler, needsTerminal, command);
                }
            }
            else
            { // No existing stub, build new one
                target.BuildRunStub(path, handler, needsTerminal, command);
            }
        }
        #endregion

        #region Build
        /// <summary>
        /// Builds a stub EXE that executes the "0install run" command.
        /// </summary>
        /// <param name="target">The application to be launched via the stub.</param>
        /// <param name="path">The target path to store the generated EXE file.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <param name="needsTerminal"><see langword="true"/> to build a CLI stub, <see langword="false"/> to build a GUI stub.</param>
        /// <param name="command">The command argument to be passed to the the "0install run" command; can be <see langword="null"/>.</param>
        /// <exception cref="OperationCanceledException">The user canceled the task.</exception>
        /// <exception cref="InvalidOperationException">There was a compilation error while generating the stub EXE.</exception>
        /// <exception cref="IOException">A problem occurs while writing to the filesystem.</exception>
        /// <exception cref="WebException">A problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the filesystem is not permitted.</exception>
        internal static void BuildRunStub(this FeedTarget target, [NotNull] string path, [NotNull] ITaskHandler handler, bool needsTerminal, [CanBeNull] string command = null)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            var compilerParameters = new CompilerParameters
            {
                GenerateExecutable = true,
                OutputAssembly = path,
                IncludeDebugInformation = false,
                GenerateInMemory = false,
                TreatWarningsAsErrors = true,
                ReferencedAssemblies = {"System.dll"}
            };

            if (!needsTerminal) compilerParameters.CompilerOptions += " /target:winexe";

            var icon = target.Feed.GetIcon(Icon.MimeTypeIco, command);
            if (icon != null)
            {
                string iconPath = IconCacheProvider.GetInstance().GetIcon(icon.Href, handler);
                compilerParameters.CompilerOptions += " /win32icon:" + iconPath.EscapeArgument();
            }

            compilerParameters.CompileCSharp(
                GetRunStubCode(target, needsTerminal, command),
                typeof(StubBuilder).GetEmbeddedString("Stub.manifest"));
        }

        /// <summary>
        /// Generates the C# to be compiled for the stub EXE.
        /// </summary>
        /// <param name="target">The application to be launched via the stub.</param>
        /// <param name="needsTerminal"><see langword="true"/> to build a CLI stub, <see langword="false"/> to build a GUI stub.</param>
        /// <param name="command">The command argument to be passed to the the "0install run" command; can be <see langword="null"/>.</param>
        /// <returns>Generated C# code.</returns>
        private static string GetRunStubCode(FeedTarget target, bool needsTerminal, [CanBeNull] string command = null)
        {
            // Build command-line
            string args = needsTerminal ? "" : "run --no-wait ";
            if (!string.IsNullOrEmpty(command)) args += "--command " + command.EscapeArgument() + " ";
            args += target.Uri.ToStringRfc().EscapeArgument();

            // Load the template code and insert variables
            var code = typeof(StubBuilder).GetEmbeddedString("stub.template.cs")
                .Replace("[EXE]", Path.Combine(Locations.InstallBase, needsTerminal ? "0launch.exe" : "0install-win.exe")
                    .Replace(@"\", @"\\"));
            code = code.Replace("[ARGUMENTS]", EscapeForCode(args));
            code = code.Replace("[TITLE]", EscapeForCode(target.Feed.GetBestName(CultureInfo.CurrentUICulture, command)));
            return code;
        }

        /// <summary>
        /// Escapes a string so that is safe for substitution inside C# code
        /// </summary>
        private static string EscapeForCode(string value)
        {
            return value.Replace(@"\", @"\\").Replace("\"", "\\\"").Replace("\n", @"\n");
        }
        #endregion
    }
}
