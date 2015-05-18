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

using NUnit.Framework;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// Contains test methods for <see cref="Architecture"/>.
    /// </summary>
    [TestFixture]
    public class ArchitectureTest
    {
        [Test]
        public void TestConstructor()
        {
            Assert.AreEqual(new Architecture(OS.All, Cpu.All), new Architecture("*-*"));
            Assert.AreEqual(new Architecture(OS.Linux, Cpu.All), new Architecture("Linux-*"));
            Assert.AreEqual(new Architecture(OS.All, Cpu.I686), new Architecture("*-i686"));
            Assert.AreEqual(new Architecture(OS.Linux, Cpu.I686), new Architecture("Linux-i686"));
        }

        [Test]
        public void TestIsCompatible()
        {
            Assert.IsTrue(new Architecture(OS.Windows, Cpu.I486).IsCompatible(new Architecture(OS.Windows, Cpu.I486)));
            Assert.IsTrue(new Architecture(OS.Linux, Cpu.I586).IsCompatible(new Architecture(OS.Linux, Cpu.I586)));
            Assert.IsTrue(new Architecture(OS.MacOSX, Cpu.I686).IsCompatible(new Architecture(OS.MacOSX, Cpu.I686)));

            Assert.IsFalse(new Architecture(OS.Windows, Cpu.I486).IsCompatible(new Architecture(OS.Linux, Cpu.I486)));
            Assert.IsFalse(new Architecture(OS.Windows, Cpu.I486).IsCompatible(new Architecture(OS.Linux, Cpu.Ppc)));
        }

        [Test]
        public void TestIsCompatibleOS()
        {
            // Wildcard
            Assert.IsTrue(OS.All.IsCompatible(OS.Windows));
            Assert.IsTrue(OS.All.IsCompatible(OS.Linux));
            Assert.IsTrue(OS.All.IsCompatible(OS.MacOSX));

            // Mismatch
            Assert.IsFalse(OS.Windows.IsCompatible(OS.Linux));
            Assert.IsFalse(OS.Linux.IsCompatible(OS.Windows));
            Assert.IsFalse(OS.MacOSX.IsCompatible(OS.Linux));
            Assert.IsFalse(OS.Linux.IsCompatible(OS.MacOSX));

            // Superset
            Assert.IsTrue(OS.Windows.IsCompatible(OS.Cygwin));
            Assert.IsFalse(OS.Cygwin.IsCompatible(OS.Windows));
            Assert.IsTrue(OS.Darwin.IsCompatible(OS.MacOSX));
            Assert.IsFalse(OS.MacOSX.IsCompatible(OS.Darwin));
            Assert.IsTrue(OS.Posix.IsCompatible(OS.Linux));
            Assert.IsTrue(OS.Posix.IsCompatible(OS.Solaris));
            Assert.IsTrue(OS.Posix.IsCompatible(OS.FreeBsd));
            Assert.IsTrue(OS.Posix.IsCompatible(OS.Darwin));
            Assert.IsTrue(OS.Posix.IsCompatible(OS.MacOSX));
            Assert.IsTrue(OS.Posix.IsCompatible(OS.Posix));
            Assert.IsFalse(OS.Posix.IsCompatible(OS.Windows));
        }

        [Test]
        public void TestIsCompatibleCpu()
        {
            // Wildcard
            Assert.IsTrue(Cpu.All.IsCompatible(Cpu.I486));
            Assert.IsTrue(Cpu.All.IsCompatible(Cpu.I586));
            Assert.IsTrue(Cpu.All.IsCompatible(Cpu.I686));

            // Mismatch
            Assert.IsFalse(Cpu.I686.IsCompatible(Cpu.Ppc));
            Assert.IsFalse(Cpu.Ppc.IsCompatible(Cpu.I586));

            // x86-series backwards-compatibility
            Assert.IsTrue(Cpu.I386.IsCompatible(Cpu.I686));
            Assert.IsFalse(Cpu.I686.IsCompatible(Cpu.I386));

            // 32bit/64bit exclusion
            Assert.IsFalse(Cpu.I386.IsCompatible(Cpu.X64));
            Assert.IsFalse(Cpu.X64.IsCompatible(Cpu.I686));
            Assert.IsFalse(Cpu.Ppc.IsCompatible(Cpu.Ppc64));
            Assert.IsFalse(Cpu.Ppc64.IsCompatible(Cpu.Ppc));
        }
    }
}
