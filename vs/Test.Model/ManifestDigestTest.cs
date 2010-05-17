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
            // An empty digest object should extract a sha1 digest
            var digest = new ManifestDigest();
            ManifestDigest.ParseID("sha1=test", ref digest);
            Assert.AreEqual("test", digest.Sha1Old);

            // Once a digest value has been set, ID values shall not be able to overwrite it
            ManifestDigest.ParseID("sha1=test2", ref digest);
            Assert.AreEqual("test", digest.Sha1Old);
        }

        /// <summary>
        /// Ensures <see cref="ManifestDigest.BestDigest"/> correctly identifies the best available algorithm in each situation.
        /// </summary>
        [Test]
        public void TestBestDigest()
        {
            var digest = new ManifestDigest("test1", "test2", "test3");
            Assert.AreEqual("sha256=test3", digest.BestDigest);

            digest = new ManifestDigest("test1", "test2", null);
            Assert.AreEqual("sha1new=test2", digest.BestDigest);

            digest = new ManifestDigest("test1", null, null);
            Assert.AreEqual("sha1=test1", digest.BestDigest);
        }
    }
}
