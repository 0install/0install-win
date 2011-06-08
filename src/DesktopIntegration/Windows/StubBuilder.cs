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
using System.Reflection;
using Common.Streams;
using Common.Utils;

namespace ZeroInstall.DesktopIntegration.Windows
{
    /// <summary>
    /// Utility class for building stub EXEs.
    /// </summary>
    public static class StubBuilder
    {
        /// <summary>
        /// Builds a stub EXE that executes the "0install run" command.
        /// </summary>
        /// <param name="path">The target path to store the generated EXE file.</param>
        /// <param name="icon">The path to the icon to use for the generated EXE file.</param>
        /// <param name="interfaceID">The interface to be passed to the "0install run" command.</param>
        /// <param name="command">The command argument to be passed to the the "0install run" command; may be <see langword="null"/>.</param>
        /// <param name="useGui">Set to <see langword="true"/> to use "0install-win" instead of "0install".</param>
        /// <exception cref="InvalidOperationException">Thrown if there was a compilation error whil generating the stub EXE.</exception>
        public static void BuildRunStub(string path, string icon, string interfaceID, string command, bool useGui)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (string.IsNullOrEmpty(icon)) throw new ArgumentNullException("icon");
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            #endregion

            string args = string.IsNullOrEmpty(command)
                ? "run " + interfaceID
                : "run --command=" + command + " " + interfaceID;

            // Load the template code and insert variables
            string code = GetEmbeddedResource("Stub.template").Replace("[EXE]", useGui ? "0install-win.exe" : "0install.exe").Replace("[ARGUMENTS]", args);

            // Configure the compiler
            var compilerParameters = new CompilerParameters
            {
                GenerateExecutable = true, OutputAssembly = path, IncludeDebugInformation = false, GenerateInMemory = false, TreatWarningsAsErrors = true,
                CompilerOptions = "/win32icon:" + StringUtils.EscapeWhitespace(icon),
                ReferencedAssemblies = {"System.dll"}
            };
            if (useGui) compilerParameters.CompilerOptions += " /target:winexe";

            // Runt the compilation process and check for errors
            var compilerResults = CodeDomProvider.CreateProvider("CSharp").CompileAssemblyFromSource(compilerParameters, code);
            if (compilerResults.Errors.HasErrors)
            {
                var error = compilerResults.Errors[0];
                throw new InvalidOperationException("Compilation error " + error.ErrorNumber + " in line " + error.Line + "\n" + error.ErrorText);
            }
        }

        private static string GetEmbeddedResource(string name)
        {
            var assembly = Assembly.GetAssembly(typeof(StubBuilder));
            using (var stream = assembly.GetManifestResourceStream(typeof(StubBuilder), name))
                return StreamUtils.ReadToString(stream);
        }
    }
}
