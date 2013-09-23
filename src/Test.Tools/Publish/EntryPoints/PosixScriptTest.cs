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
    /// Contains test methods for <see cref="PosixScript"/>.
    /// </summary>
    [TestFixture]
    public class PosixScriptTest : TemporayDirectoryTest
    {
        [Test]
        public void Sh()
        {
            var candidate = new PosixScript { BaseDirectory = Directory };
            Assert.IsTrue(candidate.Analyze(Directory.DeployFile("sh", executable: true)));
            Assert.AreEqual(candidate.Name, "sh");
            Assert.IsTrue(candidate.NeedsTerminal);
        }
    }
}
