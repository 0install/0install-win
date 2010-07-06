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

using NUnit.Framework;
using ZeroInstall.Store.Feed;

namespace ZeroInstall.Injector.Solver
{
    /// <summary>
    /// Contains test methods for <see cref="PythonSolver"/>.
    /// </summary>
    [TestFixture]
    public class PythonSolverTest
    {
        /// <summary>
        /// Ensures <see cref="PythonSolver.Solve"/> correctly solves the dependencies for a specific feed URI.
        /// </summary>
        // Test deactivated because it may perform network IO
        //[Test]
        public void TestSolve()
        {
            var solver = new PythonSolver();

            Selections selections = solver.Solve("http://afb.users.sourceforge.net/zero-install/interfaces/seamonkey2.xml", Policy.CreateDefault(new SilentHandler()));

            Assert.AreEqual("http://afb.users.sourceforge.net/zero-install/interfaces/seamonkey2.xml", selections.Interface);
        }
    }
}
