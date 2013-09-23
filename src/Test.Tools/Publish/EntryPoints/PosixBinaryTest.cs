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

using Common;
using NUnit.Framework;
using ZeroInstall.Model;

namespace ZeroInstall.Publish.EntryPoints
{
    /// <summary>
    /// Contains test methods for <see cref="PosixBinary"/>.
    /// </summary>
    [TestFixture]
    public class PosixBinaryTest : TemporayDirectoryTest
    {
        [Test]
        public void Elf32()
        {
            var candidate = new PosixBinary { BaseDirectory = Directory };
            Assert.IsTrue(candidate.Analyze(Directory.DeployFile("elf32", executable: true)));
            Assert.AreEqual("elf32", candidate.Name);
            Assert.AreEqual(new Architecture(OS.Linux, Cpu.I386), candidate.Architecture);
        }

        [Test]
        public void Elf64()
        {
            var candidate = new PosixBinary { BaseDirectory = Directory };
            Assert.IsTrue(candidate.Analyze(Directory.DeployFile("elf64", executable: true)));
            Assert.AreEqual("elf64", candidate.Name);
            Assert.AreEqual(new Architecture(OS.Linux, Cpu.X64), candidate.Architecture);
        }

        [Test]
        public void ElfBroken()
        {
            var candidate = new PosixBinary { BaseDirectory = Directory };
            Assert.IsFalse(candidate.Analyze(Directory.DeployFile("elfbroken", executable: true)));
        }

        [Test]
        public void NotElf()
        {
            var candidate = new PosixBinary { BaseDirectory = Directory };
            Assert.IsFalse(candidate.Analyze(Directory.DeployFile("sh", executable: true)));
        }

        [Test]
        public void NotExecutable()
        {
            var candidate = new PosixBinary { BaseDirectory = Directory };
            Assert.IsFalse(candidate.Analyze(Directory.DeployFile("elf32")));
        }

    }
}
