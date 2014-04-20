/*
 * Copyright 2010-2014 Bastian Eicher
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using NanoByte.Common.Utils;

namespace ZeroInstall.Publish.EntryPoints
{
    /// <summary>
    /// Provides automatic detection of entry point <see cref="Candidate"/>s.
    /// </summary>
    public static class Detection
    {
        #region Candidate types
        private static readonly List<Func<Candidate>> _newCandidates = new List<Func<Candidate>>();

        /// <summary>
        /// Instantiates a set of new <see cref="Candidate"/>s, one instance for each <see cref="Register{T}"/>ed type.
        /// </summary>
        /// <param name="baseDirectory">The <see cref="Candidate.BaseDirectory"/> to set for all <see cref="Candidate"/>s.</param>
        private static IEnumerable<Candidate> NewCandidates(DirectoryInfo baseDirectory)
        {
            return _newCandidates.Select(newCandidate =>
            {
                var candidate = newCandidate();
                candidate.BaseDirectory = baseDirectory;
                return candidate;
            });
        }

        /// <summary>
        /// Registers <typeparamref name="T"/> as a <see cref="Candidate"/> subtype.
        /// </summary>
        private static void Register<T>() where T : Candidate, new()
        {
            _newCandidates.Add(() => new T());
        }

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static Detection()
        {
            Register<JavaClass>();
            Register<JavaJar>();
            Register<DotNetExe>();
            Register<WindowsExe>();
            Register<WindowsBatch>();
            Register<PowerShellScript>();
            Register<PythonScript>();
            Register<PhpScript>();
            Register<PerlScript>();
            Register<RubyScript>();
            Register<PosixScript>();
            Register<MacOSApp>();
            Register<PosixBinary>();
        }
        #endregion

        public static IEnumerable<Candidate> ListCandidates(DirectoryInfo baseDirectory)
        {
            var files = new List<FileInfo>();
            baseDirectory.Walk(fileAction: files.Add);

            // Find the first (if any) matching Candidate type for each file
            return files.SelectMany(file => NewCandidates(baseDirectory).Where(file.Matches).Take(1));
        }

        /// <summary>
        /// Calls <see cref="Candidate.Analyze"/> for the <paramref name="file"/>.
        /// </summary>
        private static bool Matches(this FileInfo file, Candidate candidate)
        {
            return candidate.Analyze(file);
        }
    }
}
