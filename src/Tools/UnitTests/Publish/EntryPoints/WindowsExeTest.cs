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

using NUnit.Framework;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Publish.EntryPoints
{
    /// <summary>
    /// Contains test methods for <see cref="WindowsExe"/>.
    /// </summary>
    [TestFixture]
    public class WindowsExeTest : CandidateTest
    {
        public static readonly WindowsExe Reference32 = new WindowsExe
        {
            RelativePath = "windows32.exe",
            Architecture = new Architecture(OS.Windows, Cpu.All),
            Name = "Hello",
            Summary = "a Hello World application",
            Version = new ImplementationVersion("1.2.3.0")
        };

        public static readonly WindowsExe Reference64 = new WindowsExe
        {
            RelativePath = "windows64.exe",
            Architecture = new Architecture(OS.Windows, Cpu.X64),
            Name = "Hello",
            Summary = "a Hello World application",
            Version = new ImplementationVersion("1.2.3.0")
        };

        public static readonly WindowsExe ReferenceTerminal = new WindowsExe
        {
            RelativePath = "windows32_terminal.exe",
            Architecture = new Architecture(OS.Windows, Cpu.All),
            Name = "Hello",
            Summary = "a Hello World application",
            Version = new ImplementationVersion("1.2.3.0"),
            NeedsTerminal = true
        };

        [Test]
        public void X86()
        {
            TestAnalyze(Reference32);
        }

        [Test]
        public void X64()
        {
            TestAnalyze(Reference64);
        }

        [Test]
        public void X86Terminal()
        {
            TestAnalyze(ReferenceTerminal);
        }

        [Test]
        public void NotExe()
        {
            var candidate = new WindowsExe();
            Assert.IsFalse(candidate.Analyze(
                baseDirectory: Directory,
                file: Deploy(PosixScriptTest.Reference, xbit: false)));
        }
    }
}
