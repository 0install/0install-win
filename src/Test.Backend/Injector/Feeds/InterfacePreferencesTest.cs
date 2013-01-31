﻿/*
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

using System;
using Common.Storage;
using NUnit.Framework;
using ZeroInstall.Model;

namespace ZeroInstall.Injector.Feeds
{
    /// <summary>
    /// Contains test methods for <see cref="InterfacePreferences"/>.
    /// </summary>
    [TestFixture]
    public class InterfacePreferencesTest
    {
        #region Helpers
        /// <summary>
        /// Creates a fictive test <see cref="InterfacePreferences"/>.
        /// </summary>
        public static InterfacePreferences CreateTestInterfacePreferences()
        {
            return new InterfacePreferences
            {
                Uri = new Uri("http://somedomain/someapp.xml"),
                StabilityPolicy = Stability.Testing,
                Feeds = {new FeedReference {Source = "http://invalid"}}
            };
        }
        #endregion

        [Test(Description = "Ensures that the class is correctly serialized and deserialized.")]
        public void TestSaveLoad()
        {
            InterfacePreferences preferences1 = CreateTestInterfacePreferences(), preferences2;
            Assert.That(preferences1, Is.XmlSerializable);
            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                // Write and read file
                XmlStorage.Save(tempFile.Path, preferences1);
                preferences2 = XmlStorage.Load<InterfacePreferences>(tempFile.Path);
            }

            // Ensure data stayed the same
            Assert.AreEqual(preferences1, preferences2, "Serialized objects should be equal.");
            Assert.AreEqual(preferences1.GetHashCode(), preferences2.GetHashCode(), "Serialized objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(preferences1, preferences2), "Serialized objects should not return the same reference.");
        }

        [Test(Description = "Ensures that the class can be correctly cloned.")]
        public void TestClone()
        {
            var preferences1 = CreateTestInterfacePreferences();
            var preferences2 = preferences1.Clone();

            // Ensure data stayed the same
            Assert.AreEqual(preferences1, preferences2, "Cloned objects should be equal.");
            Assert.AreEqual(preferences1.GetHashCode(), preferences2.GetHashCode(), "Cloned objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(preferences1, preferences2), "Cloning should not return the same reference.");
        }
    }
}
