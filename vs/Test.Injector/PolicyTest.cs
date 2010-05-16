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

using System;
using NUnit.Framework;

namespace ZeroInstall.Injector
{
    /// <summary>
    /// Contains test methods for <see cref="Policy"/>.
    /// </summary>
    [TestFixture]
    public class PolicyTest
    {
        /// <summary>
        /// Ensures <see cref="Policy.GetSelections"/> and <see cref="Policy.GetLauncher"/> throw exceptions if <see cref="Policy.Solve"/> wasn't called first.
        /// </summary>
        [Test]
        public void TestExceptions()
        {
            var policy = new DefaultPolicy("invalid");
            Assert.Throws<InvalidOperationException>(() => policy.GetSelections(), "GetSelections should depend on Solve being called first");
            Assert.Throws<InvalidOperationException>(() => policy.GetLauncher(), "GetLauncher should depend on Solve being called first");
        }

        /// <summary>
        /// Ensures <see cref="Policy.GetLauncher"/> correctly provides an application that can be launched.
        /// </summary>
        // Test deactivated because it performs network IO and launches an external application
        //[Test]
        public void TestGetLauncher()
        {
            var policy = new DefaultPolicy("http://afb.users.sourceforge.net/zero-install/interfaces/seamonkey2.xml");
            policy.Solve();
            policy.GetLauncher().Run("");
        }
    }
}
