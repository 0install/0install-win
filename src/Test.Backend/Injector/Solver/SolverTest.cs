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

using Common.Storage;
using NUnit.Framework;
using ZeroInstall.Model;

namespace ZeroInstall.Injector.Solver
{
    /// <summary>
    /// Contains common code for testing specific <see cref="ISolver"/> implementations.
    /// </summary>
    public abstract class SolverTest
    {
        private readonly ISolver _solver;

        protected SolverTest(ISolver solver)
        {
            _solver = solver;
        }

        private static Feed CreateTestFeed()
        {
            return new Feed { Name = "Test", Summaries = { "Test" }, Elements = { new Implementation { ID = "test", Version = new ImplementationVersion("1.0"), LocalPath = ".", Main = "test" } } };
        }

        [Test(Description = "Ensures ISolver.Solve() correctly solves the dependencies for a specific feed ID.")]
        public void TestSolve()
        {
            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                CreateTestFeed().Save(tempFile.Path);

                bool staleFeeds;
                Selections selections = _solver.Solve(new Requirements {InterfaceID = tempFile.Path}, Policy.CreateDefault(new SilentHandler()), out staleFeeds);
                Assert.IsFalse(staleFeeds, "Local feed files should never be considered stale");

                Assert.AreEqual(tempFile.Path, selections.InterfaceID);
            }
        }
    }
}
