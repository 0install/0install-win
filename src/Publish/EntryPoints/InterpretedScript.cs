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
using System.IO;
using System.Text;
using Common.Utils;
using ZeroInstall.Model;

namespace ZeroInstall.Publish.EntryPoints
{
    public abstract class InterpretedScript : Candidate
    {
        /// <inheritdoc/>
        internal override bool Analyze(FileInfo file)
        {
            if (!base.Analyze(file)) return false;

            Name = file.Name.Substring(0, file.Name.Length - file.Extension.Length);
            NeedsTerminal = true;
            return true;
        }

        public ImplementationVersion InterpreterVersion { get; set; }

        #region Helpers
        /// <summary>
        /// Determines whether a file is executable and has a shebang line pointing to a specific interpreter.
        /// </summary>
        /// <param name="file">The file to analyze.</param>
        /// <param name="interpreter">The name of the interpreter to search for (e.g. 'python').</param>
        protected bool HasShebang(FileInfo file, string interpreter)
        {
            #region Sanity checks
            if (file == null) throw new ArgumentNullException("file");
            if (string.IsNullOrEmpty(interpreter)) throw new ArgumentNullException("interpreter");
            #endregion

            if (!IsExecutable(file.FullName)) return false;

            string firstLine = file.ReadFirstLine(Encoding.ASCII);
            return
                firstLine.StartsWith("#!/usr/bin/" + interpreter) ||
                firstLine.StartsWith("#!/usr/bin/env " + interpreter);
        }
        #endregion
    }
}
