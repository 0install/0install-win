/*
 * Copyright 2010 Bastian Eicher
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
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using ZeroInstall.Model;

namespace ZeroInstall.Solver
{
    public sealed class PythonSolver : ISolver
    {
        private static string PythonBinary
        {
            get
            {
                return Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar + @"python" + Path.DirectorySeparatorChar + @"python.exe";
            }
        }

        private static string SolverScript
        {
            get
            {
                return Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar + @"python" + Path.DirectorySeparatorChar + @"Scripts" + Path.DirectorySeparatorChar + @"0launch";
            }
        }

        private Selections RunSolver(string arguments)
        {
            var python = new ProcessStartInfo
            {
                FileName = PythonBinary,
                Arguments = SolverScript + " " + arguments,
                CreateNoWindow = true, UseShellExecute = false,
                RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true
            };

            Process process = Process.Start(python);
            if (process == null) throw new Exception();

            process.WaitForExit();
            string output = process.StandardOutput.ReadToEnd();
            string errors = process.StandardError.ReadToEnd();
            return Selections.Parse(output);
        }

        public Selections Solve(Uri feed)
        {
            return RunSolver(string.Format("--get-selections --select-only {0}", feed));
        }

        public Selections Solve(Uri feed, ImplementationVersion notBefore)
        {
            return RunSolver(string.Format("--get-selections --select-only --not-before={0} {1}", notBefore, feed));
        }

        public Selections Solve(Uri feed, string withStore)
        {
            return RunSolver(string.Format("--get-selections --select-only --with-store=\"{0} {1}", withStore, feed));
        }

        public Selections Solve(Uri feed, ImplementationVersion notBefore, string withStore)
        {
            return RunSolver(string.Format("--get-selections --select-only --not-before={0} --with-store=\"{1}\" {2}", notBefore, withStore, feed));
        }
    }
}
