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

using System.Linq;
using Common;
using NUnit.Framework;
using ZeroInstall.Model;

namespace ZeroInstall.Publish.EntryPoints
{
    /// <summary>
    /// Contains test methods for <see cref="Detection"/>.
    /// </summary>
    [TestFixture]
    public class DetectionTest : TemporayDirectoryTest
    {
        [Test]
        public void DetectElfVaraiants()
        {
            Directory.DeployFile("elf32", true);
            Directory.DeployFile("elf64", true);
            Directory.DeployFile("elfbroken", true);

            var candidates = Detection.ListCandidates(Directory).ToList();
            CollectionAssert.AreEquivalent(
                new Candidate[]
                {
                    new PosixBinary {RelativePath = "elf32", Name = "elf32", Architecture = new Architecture(OS.Linux, Cpu.I386)},
                    new PosixBinary {RelativePath = "elf64", Name = "elf64", Architecture = new Architecture(OS.Linux, Cpu.X64)}
                },
                candidates);
        }
    }
}
