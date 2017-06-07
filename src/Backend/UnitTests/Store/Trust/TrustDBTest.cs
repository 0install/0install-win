/*
 * Copyright 2010-2016 Bastian Eicher
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
using NanoByte.Common.Storage;
using Xunit;

namespace ZeroInstall.Store.Trust
{
    /// <summary>
    /// Contains test methods for <see cref="TrustDB"/>.
    /// </summary>
    public class TrustDBTest
    {
        /// <summary>
        /// Creates a fictive test <see cref="TrustDB"/>.
        /// </summary>
        private static TrustDB CreateTestTrust() => new TrustDB
        {
            Keys =
            {
                new Key
                {
                    Fingerprint = "abc",
                    Domains =
                    {
                        new Domain {Value = "0install.de"},
                        new Domain {Value = "eicher.net"}
                    }
                }
            }
        };

        [Fact] // Ensures that methods for adding and removing trusted keys work correctly.
        public void TestAddRemoveTrust()
        {
            var trust = new TrustDB();
            trust.IsTrusted("abc", new Domain("domain")).Should().BeFalse();

            trust.TrustKey("abc", new Domain("domain"));
            trust.Keys.Should().Equal(new Key {Fingerprint = "abc", Domains = {new Domain("domain")}});
            trust.IsTrusted("abc", new Domain("domain")).Should().BeTrue();

            trust.UntrustKey("abc", new Domain("domain"));
            trust.IsTrusted("abc", new Domain("domain")).Should().BeFalse();
        }

        [Fact] // Ensures that the class is correctly serialized and deserialized.
        public void TestSaveLoad()
        {
            TrustDB trust1 = CreateTestTrust(), trust2;
            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                // Write and read file
                trust1.SaveXml(tempFile);
                trust2 = XmlStorage.LoadXml<TrustDB>(tempFile);
            }

            // Ensure data stayed the same
            trust2.Should().Be(trust1, because: "Serialized objects should be equal.");
            trust2.GetHashCode().Should().Be(trust1.GetHashCode(), because: "Serialized objects' hashes should be equal.");
            ReferenceEquals(trust1, trust2).Should().BeFalse(because: "Serialized objects should not return the same reference.");
        }

        [Fact] // Ensures that the class can be correctly cloned.
        public void TestClone()
        {
            var trust1 = CreateTestTrust();
            var trust2 = trust1.Clone();

            // Ensure data stayed the same
            trust2.Should().Be(trust1, because: "Cloned objects should be equal.");
            trust2.GetHashCode().Should().Be(trust1.GetHashCode(), because: "Cloned objects' hashes should be equal.");
            trust2.Should().NotBeSameAs(trust1, because: "Cloning should not return the same reference.");
        }
    }
}
