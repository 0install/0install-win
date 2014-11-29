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

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// Contains test methods for <see cref="ManifestDigest"/>.
    /// </summary>
    [TestFixture]
    public class ManifestDigestTest
    {
        /// <summary>
        /// Ensures <see cref="ManifestDigest.ParseID"/> correctly extracts additional digests from ID strings.
        /// </summary>
        [Test]
        public void TestParseID()
        {
            Assert.AreEqual("test", new ManifestDigest("sha1=test").Sha1);
            Assert.AreEqual("test", new ManifestDigest("sha1new=test").Sha1New);
            Assert.AreEqual("test", new ManifestDigest("sha256=test").Sha256);
            Assert.AreEqual("test", new ManifestDigest("sha256new_test").Sha256New);

            // Once a digest value has been set, ID values shall not be able to overwrite it
            var digest = new ManifestDigest("sha1=test");
            digest.ParseID("sha1=test2");
            Assert.AreEqual("test", digest.Sha1);
        }

        /// <summary>
        /// Ensures <see cref="ManifestDigest.PartialEquals"/> correctly compares digests.
        /// </summary>
        [Test]
        public void TestPartialEqual()
        {
            var digest1 = new ManifestDigest(sha1: "test1");
            var digest2 = new ManifestDigest(sha1: "test1", sha1New: "test2");
            Assert.IsTrue(digest1.PartialEquals(digest2));

            digest1 = new ManifestDigest(sha1: "test1");
            digest2 = new ManifestDigest(sha1: "test2");
            Assert.IsFalse(digest1.PartialEquals(digest2));

            digest1 = new ManifestDigest(sha1: "test1");
            digest2 = new ManifestDigest(sha1New: "test2");
            Assert.IsFalse(digest1.PartialEquals(digest2));

            digest1 = new ManifestDigest(sha1New: "test1");
            digest2 = new ManifestDigest(sha256: "test2");
            Assert.IsFalse(digest1.PartialEquals(digest2));
        }
    }
}
