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

namespace ZeroInstall.Launcher.Solver
{
    /// <summary>
    /// Contains test methods for <see cref="PythonSolver"/>.
    /// </summary>
    [TestFixture]
    public class PythonSolverTest
    {
        private readonly PythonSolver _solver = new PythonSolver();

        /// <summary>
        /// Ensures <see cref="PythonSolver.Solve"/> throws the correct exceptions.
        /// </summary>
        [Test]
        public void TestExceptions()
        {
            Assert.Throws<ArgumentException>(() => _solver.Solve("invalid", Policy.CreateDefault(new SilentHandler())), "Relative paths should be rejected");
        }

        /// <summary>
        /// Ensures <see cref="PythonSolver.Solve"/> correctly solves the dependencies for a specific feed URI.
        /// </summary>
        // Test deactivated because it may perform network IO
        //[Test]
        public void TestSolve()
        {
            Selections selections = _solver.Solve("http://afb.users.sourceforge.net/zero-install/interfaces/seamonkey2.xml", Policy.CreateDefault(new SilentHandler()));

            Assert.AreEqual("http://afb.users.sourceforge.net/zero-install/interfaces/seamonkey2.xml", selections.InterfaceID);
        }
    }
}
