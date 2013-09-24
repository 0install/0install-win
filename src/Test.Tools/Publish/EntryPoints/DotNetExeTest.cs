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
    /// Contains test methods for <see cref="DotNetExe"/>.
    /// </summary>
    [TestFixture(Ignore = true, IgnoreReason = "Inspected Assemblies are not properly unloaded yet")]
    public class DotNetExeTest : TemporayDirectoryTest
    {
        public static readonly DotNetExe ReferenceCandidate = new DotNetExe
        {
            RelativePath = "dotnet20.exe",
            Name = "Hello",
            Description = "a Hello World application",
            Version = new ImplementationVersion("1.0.0"),
            RuntimeVersion = DotNetRuntimeVersion.V20
        };

        [Test]
        public void DotNet20()
        {
            var candidate = new DotNetExe {BaseDirectory = Directory};
            Assert.IsTrue(candidate.Analyze(Directory.DeployFile("dotnet20.exe")));
            Assert.AreEqual(ReferenceCandidate, candidate);
        }

        [Test]
        public void NotDotNet()
        {
            var candidate = new DotNetExe {BaseDirectory = Directory};
            Assert.IsFalse(candidate.Analyze(Directory.DeployFile("sh")));
        }
    }
}
