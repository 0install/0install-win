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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NanoByte.Common;
using NanoByte.Common.Storage;

namespace ZeroInstall.Publish.EntryPoints
{
    /// <summary>
    /// Provides automatic detection of entry point <see cref="Candidate"/>s.
    /// </summary>
    public static class Detection
    {
        private static readonly List<Func<Candidate>> _candidateCreators = new List<Func<Candidate>>
        {
            () => new JavaClass(),
            () => new JavaJar(),
            () => new DotNetExe(),
            () => new WindowsExe(),
            () => new WindowsBatch(),
            () => new MacOSApp(),
            () => new PowerShellScript(),
            () => new PythonScript(),
            () => new PhpScript(),
            () => new PerlScript(),
            () => new RubyScript(),
            () => new BashScript(),
            () => new PosixScript(),
            () => new PosixBinary(),
        };

        /// <summary>
        /// Returns a list of entry point <see cref="Candidate"/>s in a directory.
        /// </summary>
        /// <param name="baseDirectory">The base directory to scan for entry points.</param>
        public static List<Candidate> ListCandidates(DirectoryInfo baseDirectory)
        {
            var candidates = new List<Candidate>();
            baseDirectory.Walk(fileAction: file =>
            {
                // Ignore uninstallers
                if (file.Name.ContainsIgnoreCase("uninstall") || file.Name.ContainsIgnoreCase("unins0")) return;

                var candidate = _candidateCreators.Select(x => x()).FirstOrDefault(x => x.Analyze(baseDirectory, file));
                if (candidate != null) candidates.Add(candidate);
            });
            return candidates;
        }
    }
}
