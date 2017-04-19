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
using NUnit.Framework;

namespace ZeroInstall.Store.Model.Preferences
{
    /// <summary>
    /// Contains test methods for <see cref="InterfacePreferences"/>.
    /// </summary>
    [TestFixture]
    public class InterfacePreferencesTest
    {
        /// <summary>
        /// Creates a fictive test <see cref="InterfacePreferences"/>.
        /// </summary>
        public static InterfacePreferences CreateTestInterfacePreferences() => new InterfacePreferences
        {
            Uri = new FeedUri("http://somedomain/someapp.xml"),
            StabilityPolicy = Stability.Testing,
            Feeds = {new FeedReference {Source = new FeedUri("http://invalid/")}}
        };

        [Test(Description = "Ensures that the class is correctly serialized and deserialized.")]
        public void TestSaveLoad()
        {
            InterfacePreferences preferences1 = CreateTestInterfacePreferences(), preferences2;
            Assert.That(preferences1, Is.XmlSerializable);
            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                // Write and read file
                preferences1.SaveXml(tempFile);
                preferences2 = XmlStorage.LoadXml<InterfacePreferences>(tempFile);
            }

            // Ensure data stayed the same
            preferences2.Should().Be(preferences1, because: "Serialized objects should be equal.");
            preferences2.GetHashCode().Should().Be(preferences1.GetHashCode(), because: "Serialized objects' hashes should be equal.");
            preferences2.Should().NotBeSameAs(preferences1, because: "Serialized objects should not return the same reference.");
        }

        [Test(Description = "Ensures that the class can be correctly cloned.")]
        public void TestClone()
        {
            var preferences1 = CreateTestInterfacePreferences();
            var preferences2 = preferences1.Clone();

            // Ensure data stayed the same
            preferences2.Should().Be(preferences1, because: "Cloned objects should be equal.");
            preferences2.GetHashCode().Should().Be(preferences1.GetHashCode(), because: "Cloned objects' hashes should be equal.");
            ReferenceEquals(preferences1, preferences2).Should().BeFalse(because: "Cloning should not return the same reference.");
        }
    }
}
