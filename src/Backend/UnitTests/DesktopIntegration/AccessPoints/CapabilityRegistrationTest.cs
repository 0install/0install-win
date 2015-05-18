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

using NUnit.Framework;
using ZeroInstall.Store.Model.Capabilities;

namespace ZeroInstall.DesktopIntegration.AccessPoints
{
    /// <summary>
    /// Contains test methods for <see cref="CapabilityRegistration"/>.
    /// </summary>
    [TestFixture]
    public class CapabilityRegistrationTest
    {
        [Test]
        public void TestGetConflictIDs()
        {
            var capabilityRegistration = new CapabilityRegistration();
            var appEntry = new AppEntry
            {
                CapabilityLists =
                {
                    new CapabilityList {Entries = {new Store.Model.Capabilities.FileType {ID = "test1"}}},
                    new CapabilityList {Entries = {new Store.Model.Capabilities.FileType {ID = "test2"}}}
                }
            };

            CollectionAssert.AreEqual(
                expected: new[] {"progid:test1", "progid:test2"},
                actual: capabilityRegistration.GetConflictIDs(appEntry));
        }
    }
}
