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
    /// Contains test methods for <see cref="DotNetExe"/>.
    /// </summary>
    [TestFixture]
    public class DotNetExeTest : CandidateTest
    {
        public static readonly DotNetExe Reference = new DotNetExe
        {
            RelativePath = "dotnet.exe",
            Architecture = new Architecture(OS.All, Cpu.All),
            Name = "Hello",
            Summary = "a Hello World application",
            Version = new ImplementationVersion("1.2.3.0")
        };

        public static readonly DotNetExe Reference64 = new DotNetExe
        {
            RelativePath = "dotnet64.exe",
            Architecture = new Architecture(OS.All, Cpu.X64),
            Name = "Hello",
            Summary = "a Hello World application",
            Version = new ImplementationVersion("1.2.3.0")
        };

        public static readonly DotNetExe ReferenceTerminal = new DotNetExe
        {
            RelativePath = "dotnet_terminal.exe",
            Architecture = new Architecture(OS.All, Cpu.All),
            Name = "Hello",
            Summary = "a Hello World application",
            Version = new ImplementationVersion("1.2.3.0"),
            NeedsTerminal = true
        };

        [Test]
        public void AnyCpu()
        {
            TestAnalyze(Reference);
        }

        [Test]
        public void X64()
        {
            TestAnalyze(Reference64);
        }

        [Test]
        public void Terminal()
        {
            TestAnalyze(ReferenceTerminal);
        }

        [Test]
        public void NotDotNet()
        {
            new DotNetExe().Analyze(baseDirectory: Directory, file: Deploy(WindowsExeTest.Reference32, xbit: false))
                .Should().BeFalse();
        }
    }
}
