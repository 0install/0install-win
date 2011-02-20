/*
 * Copyright 2010-2011 Bastian Eicher
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
using NUnit.Framework;

namespace ZeroInstall.Injector
{
    /// <summary>
    /// Contains test methods for <see cref="Preferences"/>.
    /// </summary>
    [TestFixture]
    public class PreferencesTest
    {
        #region Helpers
        /// <summary>
        /// Creates test <see cref="Preferences"/>.
        /// </summary>
        public static Preferences CreateTestPreferences()
        {
            return new Preferences {NetworkLevel = NetworkLevel.Minimal, Freshness = new TimeSpan(12, 0, 0), HelpWithTesting = true};
        }
        #endregion

        [Test(Description = "Ensures that the class can be correctly cloned.")]
        public void TestClone()
        {
            var preferences1 = CreateTestPreferences();
            var preferences2 = preferences1.ClonePreferences();

            // Ensure data stayed the same
            Assert.AreEqual(preferences1, preferences2, "Cloned objects should be equal.");
            Assert.AreEqual(preferences1.GetHashCode(), preferences2.GetHashCode(), "Cloned objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(preferences1, preferences2), "Cloning should not return the same reference.");
        }
    }
}
