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
using ZeroInstall.Launcher.Solver;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Launcher
{
    /// <summary>
    /// Contains test methods for <see cref="Launcher"/>.
    /// </summary>
    [TestFixture]
    public class LauncherTest
    {
        /// <summary>
        /// Ensures the <see cref="Launcher"/> constructor throws the correct exceptions.
        /// </summary>
        [Test]
        public void TestExceptions()
        {
            Assert.Throws<ArgumentException>(() => new Launcher("invalid", new Selections {Implementations = {new ImplementationSelection()}}, StoreProvider.Default), "Relative paths should be rejected");
            Assert.Throws<ArgumentException>(() => new Launcher("http://nothin", new Selections(), StoreProvider.Default), "Empty selections should be rejected");
        }

        /// <summary>
        /// Ensures <see cref="Controller.GetLauncher"/> correctly provides an application that can be launched.
        /// </summary>
        // Test deactivated because it uses an external process and performs network IO
        //[Test]
        public void TestGetLauncher()
        {
            var controller = new Controller("http://afb.users.sourceforge.net/zero-install/interfaces/seamonkey2.xml", SolverProvider.Default, Policy.CreateDefault(new SilentHandler()));
            controller.Solve();
            controller.DownloadUncachedImplementations();
            var launcher = controller.GetLauncher();
            var startInfo = launcher.GetStartInfo("--help");
            Assert.AreEqual("--help", startInfo.Arguments);
        }
    }
}
