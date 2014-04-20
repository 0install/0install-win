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
using NUnit.Framework;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// Contains test methods for <see cref="Architecture"/>.
    /// </summary>
    [TestFixture]
    public class ArchitectureTest
    {
        /// <summary>
        /// Ensures the <see cref="ValueType"/> constructor correctly handles valid inputs.
        /// </summary>
        [Test]
        public void TestConstructor()
        {
            Assert.AreEqual(new Architecture(OS.All, Cpu.All), new Architecture("*-*"));
            Assert.AreEqual(new Architecture(OS.Linux, Cpu.All), new Architecture("Linux-*"));
            Assert.AreEqual(new Architecture(OS.All, Cpu.I686), new Architecture("*-i686"));
            Assert.AreEqual(new Architecture(OS.Linux, Cpu.I686), new Architecture("Linux-i686"));
        }

        /// <summary>
        /// Ensures <see cref="Architecture.IsCompatible(ZeroInstall.Store.Model.Architecture)"/> correctly determines which kinds of packages can run on which machines.
        /// </summary>
        [Test]
        public void TestIsCompatible()
        {
            // Exact matches
            Assert.IsTrue(new Architecture(OS.Windows, Cpu.I486).IsCompatible(new Architecture(OS.Windows, Cpu.I486)));
            Assert.IsTrue(new Architecture(OS.Linux, Cpu.I586).IsCompatible(new Architecture(OS.Linux, Cpu.I586)));
            Assert.IsTrue(new Architecture(OS.MacOSX, Cpu.I686).IsCompatible(new Architecture(OS.MacOSX, Cpu.I686)));

            // OS wildcards
            Assert.IsTrue(new Architecture(OS.All, Cpu.I486).IsCompatible(new Architecture(OS.Windows, Cpu.I486)));
            Assert.IsTrue(new Architecture(OS.All, Cpu.I586).IsCompatible(new Architecture(OS.Linux, Cpu.I586)));
            Assert.IsTrue(new Architecture(OS.All, Cpu.I686).IsCompatible(new Architecture(OS.MacOSX, Cpu.I686)));

            // OS mismatches
            Assert.IsFalse(new Architecture(OS.Windows, Cpu.I486).IsCompatible(new Architecture(OS.Linux, Cpu.I486)));
            Assert.IsFalse(new Architecture(OS.Linux, Cpu.I486).IsCompatible(new Architecture(OS.Windows, Cpu.I486)));
            Assert.IsFalse(new Architecture(OS.MacOSX, Cpu.I686).IsCompatible(new Architecture(OS.Linux, Cpu.I686)));
            Assert.IsFalse(new Architecture(OS.Linux, Cpu.I686).IsCompatible(new Architecture(OS.MacOSX, Cpu.I686)));

            // OS supersets
            Assert.IsTrue(new Architecture(OS.Windows, Cpu.I486).IsCompatible(new Architecture(OS.Cygwin, Cpu.I486)));
            Assert.IsFalse(new Architecture(OS.Cygwin, Cpu.I486).IsCompatible(new Architecture(OS.Windows, Cpu.I486)));
            Assert.IsTrue(new Architecture(OS.Darwin, Cpu.I686).IsCompatible(new Architecture(OS.MacOSX, Cpu.I686)));
            Assert.IsFalse(new Architecture(OS.MacOSX, Cpu.I686).IsCompatible(new Architecture(OS.Darwin, Cpu.I686)));
            Assert.IsTrue(new Architecture(OS.Posix, Cpu.I686).IsCompatible(new Architecture(OS.Linux, Cpu.I686)));
            Assert.IsTrue(new Architecture(OS.Posix, Cpu.I686).IsCompatible(new Architecture(OS.Solaris, Cpu.I686)));
            Assert.IsTrue(new Architecture(OS.Posix, Cpu.I686).IsCompatible(new Architecture(OS.FreeBsd, Cpu.I686)));
            Assert.IsTrue(new Architecture(OS.Posix, Cpu.I686).IsCompatible(new Architecture(OS.Darwin, Cpu.I686)));
            Assert.IsTrue(new Architecture(OS.Posix, Cpu.I686).IsCompatible(new Architecture(OS.MacOSX, Cpu.I686)));
            Assert.IsTrue(new Architecture(OS.Posix, Cpu.I686).IsCompatible(new Architecture(OS.Posix, Cpu.I686)));
            Assert.IsFalse(new Architecture(OS.Posix, Cpu.I686).IsCompatible(new Architecture(OS.Windows, Cpu.I686)));

            // CPU wildcards
            Assert.IsTrue(new Architecture(OS.Windows, Cpu.All).IsCompatible(new Architecture(OS.Windows, Cpu.I486)));
            Assert.IsTrue(new Architecture(OS.Linux, Cpu.All).IsCompatible(new Architecture(OS.Linux, Cpu.I586)));
            Assert.IsTrue(new Architecture(OS.MacOSX, Cpu.All).IsCompatible(new Architecture(OS.MacOSX, Cpu.I686)));

            // CPU mismatches
            Assert.IsFalse(new Architecture(OS.MacOSX, Cpu.I686).IsCompatible(new Architecture(OS.MacOSX, Cpu.Ppc)));
            Assert.IsFalse(new Architecture(OS.Linux, Cpu.Ppc).IsCompatible(new Architecture(OS.Linux, Cpu.I586)));

            // x86-series backwards-compatibility
            Assert.IsTrue(new Architecture(OS.Linux, Cpu.I386).IsCompatible(new Architecture(OS.Linux, Cpu.I686)));
            Assert.IsFalse(new Architecture(OS.Linux, Cpu.I686).IsCompatible(new Architecture(OS.Linux, Cpu.I386)));

            // 32bit/64bit exclusion
            Assert.IsFalse(new Architecture(OS.Linux, Cpu.I386).IsCompatible(new Architecture(OS.Linux, Cpu.X64)));
            Assert.IsFalse(new Architecture(OS.Linux, Cpu.X64).IsCompatible(new Architecture(OS.Linux, Cpu.I686)));
            Assert.IsFalse(new Architecture(OS.MacOSX, Cpu.Ppc).IsCompatible(new Architecture(OS.MacOSX, Cpu.Ppc64)));
            Assert.IsFalse(new Architecture(OS.MacOSX, Cpu.Ppc64).IsCompatible(new Architecture(OS.MacOSX, Cpu.Ppc)));
        }
    }
}
