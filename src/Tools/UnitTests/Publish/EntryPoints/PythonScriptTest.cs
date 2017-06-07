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

using FluentAssertions;
using Xunit;

namespace ZeroInstall.Publish.EntryPoints
{
    /// <summary>
    /// Contains test methods for <see cref="PythonScript"/>.
    /// </summary>
    public class PythonScriptTest : CandidateTest
    {
        public static readonly PythonScript Reference = new PythonScript {RelativePath = "python", Name = "python", NeedsTerminal = true};
        public static readonly PythonScript ReferenceWithExtension = new PythonScript {RelativePath = "python.py", Name = "python", NeedsTerminal = true};
        public static readonly PythonScript ReferenceWithExtensionWindows = new PythonScript {RelativePath = "python.pyw", Name = "python", NeedsTerminal = false};

        [Fact]
        public void NoExtension()
        {
            TestAnalyze(Reference, executable: true);
        }

        [Fact]
        public void NotExecutable()
        {
            new PythonScript().Analyze(baseDirectory: Directory, file: Deploy(Reference, xbit: false))
                .Should().BeFalse();
        }

        [Fact]
        public void WithExtension()
        {
            TestAnalyze(ReferenceWithExtension);
        }

        [Fact]
        public void WithExtensionWindows()
        {
            TestAnalyze(ReferenceWithExtensionWindows);
        }
    }
}
