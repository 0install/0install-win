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

using FluentAssertions;
using NUnit.Framework;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Publish.EntryPoints
{
    /// <summary>
    /// Contains test methods for <see cref="PosixBinary"/>.
    /// </summary>
    [TestFixture]
    public class PosixBinaryTest : CandidateTest
    {
        public static readonly PosixBinary Reference32 = new PosixBinary {RelativePath = "elf32", Name = "elf32", Architecture = new Architecture(OS.Linux, Cpu.I386)};
        public static readonly PosixBinary Reference64 = new PosixBinary {RelativePath = "elf64", Name = "elf64", Architecture = new Architecture(OS.Linux, Cpu.X64)};

        [Test]
        public void Elf32()
        {
            TestAnalyze(Reference32, executable: true);
        }

        [Test]
        public void Elf64()
        {
            TestAnalyze(Reference64, executable: true);
        }

        [Test]
        public void NotExecutable()
        {
            new PosixBinary().Analyze(baseDirectory: Directory, file: Deploy(Reference32, xbit: false))
                .Should().BeFalse();
        }

        [Test]
        public void NotElf()
        {
            new PosixBinary().Analyze(baseDirectory: Directory, file: Deploy(PosixScriptTest.Reference, xbit: true))
                .Should().BeFalse();
        }
    }
}
