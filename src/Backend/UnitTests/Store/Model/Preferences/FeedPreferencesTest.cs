/*
 * Copyright 2010-2015 Bastian Eicher
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

using System;
using FluentAssertions;
using NanoByte.Common.Storage;
using NUnit.Framework;

namespace ZeroInstall.Store.Model.Preferences
{
    /// <summary>
    /// Contains test methods for <see cref="FeedPreferences"/>.
    /// </summary>
    [TestFixture]
    public class FeedPreferencesTest
    {
        #region Helpers
        /// <summary>
        /// Creates a fictive test <see cref="FeedPreferences"/>.
        /// </summary>
        public static FeedPreferences CreateTestFeedPreferences()
        {
            return new FeedPreferences
            {
                LastChecked = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Implementations = {new ImplementationPreferences {ID = "test_id", UserStability = Stability.Testing}}
            };
        }
        #endregion

        [Test(Description = "Ensures that the class is correctly serialized and deserialized.")]
        public void TestSaveLoad()
        {
            FeedPreferences preferences1 = CreateTestFeedPreferences(), preferences2;
            Assert.That(preferences1, Is.XmlSerializable);
            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                // Write and read file
                preferences1.SaveXml(tempFile);
                preferences2 = XmlStorage.LoadXml<FeedPreferences>(tempFile);
            }

            // Ensure data stayed the same
            preferences2.Should().Be(preferences1, because: "Serialized objects should be equal.");
            preferences2.GetHashCode().Should().Be(preferences1.GetHashCode(), because: "Serialized objects' hashes should be equal.");
            preferences2.Should().NotBeSameAs(preferences1, because: "Serialized objects should not return the same reference.");
        }

        [Test(Description = "Ensures that the class can be correctly cloned.")]
        public void TestClone()
        {
            var preferences1 = CreateTestFeedPreferences();
            var preferences2 = preferences1.Clone();

            // Ensure data stayed the same
            preferences2.Should().Be(preferences1, because: "Cloned objects should be equal.");
            preferences2.GetHashCode().Should().Be(preferences1.GetHashCode(), because: "Cloned objects' hashes should be equal.");
            preferences2.Should().NotBeSameAs(preferences1, because: "Cloning should not return the same reference.");
        }

        /// <summary>
        /// Ensures that <see cref="FeedPreferences.Normalize"/> correctly removes superflous entries from <see cref="FeedPreferences.Implementations"/>.
        /// </summary>
        [Test]
        public void TestNormalize()
        {
            var keep = new ImplementationPreferences {ID = "id1", UserStability = Stability.Testing};
            var superflous = new ImplementationPreferences {ID = "id2"};
            var preferences = new FeedPreferences {Implementations = {keep, superflous}};

            preferences.Normalize();
            preferences.Implementations.Should().BeEquivalentTo(keep);
        }

        [Test]
        public void TestGetImplementationPreferences()
        {
            var preferences = new FeedPreferences();
            var prefs1 = preferences["id1"];
            preferences["id1"].Should().BeSameAs(prefs1, because: "Second call with same ID should return same reference");

            var prefs2 = new ImplementationPreferences {ID = "id2"};
            preferences.Implementations.Add(prefs2);
            preferences["id2"].Should().BeSameAs(prefs2, because: "Call with pre-existing ID should return existing reference");

            preferences.Implementations.Should().BeEquivalentTo(prefs1, prefs2);
        }
    }
}
