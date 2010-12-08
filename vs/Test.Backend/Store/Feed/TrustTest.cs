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

using Common.Storage;
using NUnit.Framework;

namespace ZeroInstall.Store.Feed
{
    /// <summary>
    /// Contains test methods for <see cref="Trust"/>.
    /// </summary>
    [TestFixture]
    public class TrustTest
    {
        #region Helpers
        /// <summary>
        /// Creates a fictive test <see cref="Trust"/>.
        /// </summary>
        private static Trust CreateTestTrust()
        {
            return new Trust
            {
                Keys = { new Key { Fingerprint = "abc", Domains = { new Domain { Value = "0install.de" }, new Domain { Value = "eicher.net" } } } }
            };
        }
        #endregion

        /// <summary>
        /// Ensures that the class is correctly serialized and deserialized.
        /// </summary>
        [Test]
        public void TestSaveLoad()
        {
            Trust trust1, trust2;
            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                // Write and read file
                trust1 = CreateTestTrust();
                trust1.Save(tempFile.Path);
                trust2 = Trust.Load(tempFile.Path);
            }

            // Ensure data stayed the same
            Assert.AreEqual(trust1, trust2, "Serialized objects should be equal.");
            Assert.AreEqual(trust1.GetHashCode(), trust2.GetHashCode(), "Serialized objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(trust1, trust2), "Serialized objects should not return the same reference.");
        }

        /// <summary>
        /// Ensures that the class can be correctly cloned.
        /// </summary>
        [Test]
        public void TestClone()
        {
            var trust1 = CreateTestTrust();
            var trust2 = trust1.CloneTrust();

            // Ensure data stayed the same
            Assert.AreEqual(trust1, trust2, "Cloned objects should be equal.");
            Assert.AreEqual(trust1.GetHashCode(), trust2.GetHashCode(), "Cloned objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(trust1, trust2), "Cloning should not return the same reference.");
        }
    }
}
