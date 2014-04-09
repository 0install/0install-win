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

using NanoByte.Common.Storage;
using NUnit.Framework;

namespace ZeroInstall.Store.Trust
{
    /// <summary>
    /// Contains test methods for <see cref="TrustDB"/>.
    /// </summary>
    [TestFixture]
    public class TrustDBTest
    {
        #region Helpers
        /// <summary>
        /// Creates a fictive test <see cref="TrustDB"/>.
        /// </summary>
        private static TrustDB CreateTestTrust()
        {
            return new TrustDB
            {
                Keys = {new Key {Fingerprint = "abc", Domains = {new Domain {Value = "0install.de"}, new Domain {Value = "eicher.net"}}}}
            };
        }
        #endregion

        [Test(Description = "Ensures that methods for adding and removing trusted keys work correctly.")]
        public void TestAddRemoveTrust()
        {
            var trust = new TrustDB();
            Assert.IsFalse(trust.IsTrusted("abc", new Domain("domain")));

            trust.TrustKey("abc", new Domain("domain"));
            CollectionAssert.AreEqual(new[] {new Key {Fingerprint = "abc", Domains = {new Domain("domain")}}}, trust.Keys);
            Assert.IsTrue(trust.IsTrusted("abc", new Domain("domain")));

            trust.UntrustKey("abc", new Domain("domain"));
            Assert.IsFalse(trust.IsTrusted("abc", new Domain("domain")));
        }

        [Test(Description = "Ensures that the class is correctly serialized and deserialized.")]
        public void TestSaveLoad()
        {
            TrustDB trust1 = CreateTestTrust(), trust2;
            Assert.That(trust1, Is.XmlSerializable);
            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                // Write and read file
                trust1.SaveXml(tempFile);
                trust2 = TrustDB.Load(tempFile);
            }

            // Ensure data stayed the same
            Assert.AreEqual(trust1, trust2, "Serialized objects should be equal.");
            Assert.AreEqual(trust1.GetHashCode(), trust2.GetHashCode(), "Serialized objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(trust1, trust2), "Serialized objects should not return the same reference.");
        }

        [Test(Description = "Ensures that the class can be correctly cloned.")]
        public void TestClone()
        {
            var trust1 = CreateTestTrust();
            var trust2 = trust1.Clone();

            // Ensure data stayed the same
            Assert.AreEqual(trust1, trust2, "Cloned objects should be equal.");
            Assert.AreEqual(trust1.GetHashCode(), trust2.GetHashCode(), "Cloned objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(trust1, trust2), "Cloning should not return the same reference.");
        }
    }
}
