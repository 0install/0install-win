﻿/*
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

using System.Linq;
using NUnit.Framework;
using ZeroInstall.Model;

namespace ZeroInstall.Services.Solvers
{
    /// <summary>
    /// Contains test methods for <see cref="SolverUtils"/>.
    /// </summary>
    [TestFixture]
    public class SolverUtilsTest
    {
        [Test]
        public void GetEffectiveFillsInDefaultValues()
        {
            Assert.AreEqual(
                expected: new Requirements
                {
                    InterfaceID = "http://test/feed.xml",
                    Command = Command.NameRun,
                    Architecture = Architecture.CurrentSystem
                },
                actual: new Requirements
                {
                    InterfaceID = "http://test/feed.xml"
                }.GetEffective().First());
        }

        [Test]
        public void GetEffectiveHandlesX86OnX64()
        {
            if (Architecture.CurrentSystem.Cpu != Cpu.X64) Assert.Ignore("Can only test on X64 systems");

            CollectionAssert.AreEqual(
                expected: new[]
                {
                    new Requirements
                    {
                        InterfaceID = "http://test/feed.xml",
                        Command = Command.NameRun,
                        Architecture = new Architecture(OS.Linux, Cpu.X64)
                    },
                    new Requirements
                    {
                        InterfaceID = "http://test/feed.xml",
                        Command = Command.NameRun,
                        Architecture = new Architecture(OS.Linux, Cpu.I686)
                    }
                },
                actual: new Requirements
                {
                    InterfaceID = "http://test/feed.xml",
                    Command = Command.NameRun,
                    Architecture = new Architecture(OS.Linux, Cpu.X64)
                }.GetEffective());
        }
    }
}
