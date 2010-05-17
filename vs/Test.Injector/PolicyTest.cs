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
using System.Collections.Generic;
using Common.Storage;
using NUnit.Framework;
using ZeroInstall.DownloadBroker;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Store.Interface;

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
            var policy = Policy.CreateDefault("invalid");
            Assert.Throws<InvalidOperationException>(() => policy.GetSelections(), "GetSelections should depend on Solve being called first");
            Assert.Throws<InvalidOperationException>(() => policy.GetLauncher(), "GetLauncher should depend on Solve being called first");
        }

        /// <summary>
        /// Ensures that <see cref="Policy.ListUncachedImplementations"/> correctly finds <see cref="Implementation"/>s not cached in a <see cref="IStore"/>.
        /// </summary>
        // Test deactivated because it performs network IO
        //[Test]
        public void TestGetUncachedImplementations()
        {
            // Look inside a temporary (empty) store
            IEnumerable<Implementation> implementations;
            using (var temp = new TemporaryDirectory())
            {
                var policy = new Policy("http://afb.users.sourceforge.net/zero-install/interfaces/seamonkey2.xml", SolverFactory.Default, new InterfaceCache(), new Fetcher(new DirectoryStore(temp.Path)));
                policy.Solve();
                implementations = policy.ListUncachedImplementations();
            }

            // Check the first (and only) entry of the "missing list" is the correct implementation
            var enumerator = implementations.GetEnumerator();
            Assert.IsTrue(enumerator.MoveNext(), "An least one Implementation should be uncached.");
            Assert.AreEqual("sha1new=91dba493cc1ff911df9860baebb6136be7341d38", enumerator.Current.ManifestDigest.BestDigest, "The actual Implementation should have the same digest as the selection information.");
        }

        /// <summary>
        /// Ensures <see cref="Policy.GetSelections"/> correctly provides results from a <see cref="ZeroInstall.Injector.Solver"/>.
        /// </summary>
        // Test deactivated because it performs network IO
        //[Test]
        public void TestGetSelections()
        {
            Policy policy = Policy.CreateDefault("http://afb.users.sourceforge.net/zero-install/interfaces/seamonkey2.xml");
            policy.Solve();
            Assert.AreEqual(policy.GetSelections().InterfaceString, "http://afb.users.sourceforge.net/zero-install/interfaces/seamonkey2.xml");
        }

        /// <summary>
        /// Ensures <see cref="Policy.GetLauncher"/> correctly provides an application that can be launched.
        /// </summary>
        // Test deactivated because it performs network IO and launches an external application
        //[Test]
        public void TestGetLauncher()
        {
            Policy policy = Policy.CreateDefault("http://afb.users.sourceforge.net/zero-install/interfaces/seamonkey2.xml");
            policy.Solve();
            policy.DownloadUncachedImplementations();
            policy.GetLauncher().Execute("");
        }
    }
}
