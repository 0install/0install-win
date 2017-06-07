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
using ZeroInstall.Store.Model;

namespace ZeroInstall.Publish.EntryPoints
{
    /// <summary>
    /// Contains test methods for <see cref="PosixScript"/>.
    /// </summary>
    public class PosixScriptTest : CandidateTest
    {
        public static readonly PosixScript Reference = new PosixScript
        {
            RelativePath = "sh",
            Architecture = new Architecture(OS.Posix, Cpu.All),
            Name = "sh",
            NeedsTerminal = true
        };

        [Fact]
        public void Sh()
        {
            TestAnalyze(Reference, executable: true);
        }

        [Fact]
        public void NotExecutable()
        {
            new PosixScript().Analyze(baseDirectory: Directory, file: Deploy(Reference, xbit: false))
                .Should().BeFalse();
        }

        [Fact]
        public void NoShebang()
        {
            new PosixScript().Analyze(baseDirectory: Directory, file: Deploy(PosixBinaryTest.Reference32, xbit: true))
                .Should().BeFalse();
        }
    }
}
