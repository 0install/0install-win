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

namespace ZeroInstall.Publish.EntryPoints
{
    /// <summary>
    /// Contains test methods for <see cref="PythonScript"/>.
    /// </summary>
    [TestFixture]
    public class PythonScriptTest : TemporayDirectoryTest
    {
        [Test]
        public void WithExtension()
        {
            var candidate = new PythonScript {BaseDirectory = Directory};
            Assert.IsTrue(candidate.Analyze(Directory.DeployFile("python.py")));
            Assert.AreEqual(candidate.Name, "python");
            Assert.IsTrue(candidate.NeedsTerminal);
        }

        [Test]
        public void WithExtensionWindows()
        {
            var candidate = new PythonScript {BaseDirectory = Directory};
            Assert.IsTrue(candidate.Analyze(Directory.DeployFile("python.pyw")));
            Assert.AreEqual(candidate.Name, "python");
            Assert.IsFalse(candidate.NeedsTerminal);
        }

        [Test]
        public void NoExtension()
        {
            var candidate = new PythonScript {BaseDirectory = Directory};
            Assert.IsTrue(candidate.Analyze(Directory.DeployFile("python", executable: true)));
            Assert.AreEqual(candidate.Name, "python");
            Assert.IsTrue(candidate.NeedsTerminal);
        }
    }
}
