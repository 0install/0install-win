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
    [TestFixture]
    public class DotNetExeTest : TemporayDirectoryTest
    {
        [Test]
        public void DotNet20()
        {
            var candidate = new DotNetExe { BaseDirectory = Directory };
            Assert.IsTrue(candidate.Analyze(Directory.DeployFile("dotnet20.exe")));
            Assert.AreEqual("Hello", candidate.Name);
            Assert.AreEqual("a Hello World application", candidate.Description);
            Assert.AreEqual(new ImplementationVersion("1.0.0"), candidate.Version);
            Assert.AreEqual("v2.0.50727", candidate.RuntimeVersion);
        }

        [Test]
        public void NotDotNet()
        {
            var candidate = new DotNetExe { BaseDirectory = Directory };
            Assert.IsFalse(candidate.Analyze(Directory.DeployFile("sh")));
        }
    }
}
