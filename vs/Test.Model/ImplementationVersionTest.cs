/*
 * Copyright 2010 Bastian Eicher
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

namespace ZeroInstall.Model
{
    /// <summary>
    /// Contains test methods for <see cref="ImplementationVersion"/>.
    /// </summary>
    [TestFixture]
    public class ImplementationVersionTest
    {
        /// <summary>
        /// Ensures the <see cref="ImplementationVersion.TryCreate"/> correctly handles valid inputs and rejects invalid ones.
        /// </summary>
        [Test]
        public void TestTryCreate()
        {
            var validVersions = new[] {"0.1", "1", "1.0", "1.1", "1.1-", "1.2-pre", "1.2-pre1", "1.2-rc1", "1.2", "1.2-0", "1.2--0", "1.2-post", "1.2-post1-pre", "1.2-post1", "1.2.1-pre", "1.2.1.4", "1.2.2", "1.2.10", "3"};
            foreach (string version in validVersions)
            {
                ImplementationVersion result;
                Assert.IsTrue(ImplementationVersion.TryCreate(version, out result), version);
                Assert.AreEqual(version, result.ToString());
            }

            var invalidVersions = new[] {"", "a", "pre-1", "1.0-1post"};
            foreach (string version in invalidVersions)
            {
                ImplementationVersion result;
                Assert.IsFalse(ImplementationVersion.TryCreate(version, out result), version);
            }
        }

        /// <summary>
        /// Ensures <see cref="ImplementationVersion"/> objects are sorted correctly.
        /// </summary>
        [Test]
        public void TestSort()
        {
            var sortedVersions = new[] { "0.1", "1", "1.0", "1.1", "1.2-pre", "1.2-pre1", "1.2-rc1", "1.2", "1.2-0", "1.2-post", "1.2-post1-pre", "1.2-post1", "1.2.1-pre", "1.2.1.4", "1.2.2", "1.2.10", "3" };
            for (int i = 1; i < sortedVersions.Length; i++)
            {
                var v1 = new ImplementationVersion(sortedVersions[i - 1]);
                var v2 = new ImplementationVersion(sortedVersions[i]);
                Assert.IsTrue(v1 < v2);
                Assert.IsTrue(v2 > v1);
                Assert.IsFalse(v1 > v2);
                Assert.IsFalse(v2 < v1);
            }
        }
    }
}
