/*
 * Copyright 2010-2016 Bastian Eicher
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
using System.ComponentModel;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using NanoByte.Common.Storage;
using ZeroInstall.Store;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Publish.EntryPoints
{
    /// <summary>
    /// A plain text script that is executed by a runtime interpreter.
    /// </summary>
    public abstract class InterpretedScript : Candidate
    {
        /// <inheritdoc/>
        internal override bool Analyze(DirectoryInfo baseDirectory, FileInfo file)
        {
            if (!base.Analyze(baseDirectory, file)) return false;

            Name = file.Name.Substring(0, file.Name.Length - file.Extension.Length);
            NeedsTerminal = true;
            return true;
        }

        /// <summary>
        /// The interface URI of the interpreter to run the script.
        /// </summary>
        protected abstract FeedUri InterpreterInterface { get; }

        /// <summary>
        /// The range of versions of the script interpreter supported by the application.
        /// </summary>
        [Category("Details (Script)"), DisplayName(@"Interpreter versions"), Description("The range of versions of the script interpreter supported by the application.")]
        [DefaultValue("")]
        [UsedImplicitly, CanBeNull]
        public VersionRange InterpreterVersions { get; set; }

        /// <inheritdoc/>
        public override Command CreateCommand()
        {
            return new Command
            {
                Name = CommandName,
                Path = RelativePath,
                Runner = new Runner {InterfaceUri = InterpreterInterface, Versions = InterpreterVersions}
            };
        }

        #region Helpers
        /// <summary>
        /// Determines whether a file is executable and has a shebang line pointing to a specific interpreter.
        /// </summary>
        /// <param name="file">The file to analyze.</param>
        /// <param name="interpreter">The name of the interpreter to search for (e.g. 'python').</param>
        protected bool HasShebang([NotNull] FileInfo file, [NotNull, Localizable(false)] string interpreter)
        {
            #region Sanity checks
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (string.IsNullOrEmpty(interpreter)) throw new ArgumentNullException(nameof(interpreter));
            #endregion

            if (!IsExecutable(file.FullName)) return false;

            string firstLine = file.ReadFirstLine(Encoding.ASCII);
            if (string.IsNullOrEmpty(firstLine)) return false;
            return
                firstLine.StartsWith(@"#!/usr/bin/" + interpreter) ||
                firstLine.StartsWith(@"#!/usr/bin/env " + interpreter);
        }
        #endregion

        #region Equality
        protected bool Equals(InterpretedScript other)
        {
            if (other == null) return false;
            return base.Equals(other) &&
                   Equals(InterpreterVersions, other.InterpreterVersions);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj == this) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((InterpretedScript)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (InterpreterVersions?.GetHashCode() ?? 0);
            }
        }
        #endregion
    }
}
