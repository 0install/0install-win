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

using System.Collections.Generic;
using NUnit.Framework;

namespace ZeroInstall.Store.Model.Selection
{
    /// <summary>
    /// Contains test methods for <see cref="Selections"/>.
    /// </summary>
    [TestFixture]
    public class SelectionsTest
    {
        #region Helpers
        /// <summary>
        /// Creates a <see cref="Selections"/> with two implementations, one using the other as a runner plus a number of bindings.
        /// </summary>
        public static Selections CreateTestSelections()
        {
            return new Selections
            {
                InterfaceUri = FeedTest.Test1Uri,
                Command = Command.NameRun,
                Implementations =
                {
                    ImplementationSelectionTest.CreateTestImplementation1(),
                    ImplementationSelectionTest.CreateTestImplementation2()
                }
            };
        }
        #endregion

        /// <summary>
        /// Ensures that <see cref="Selections.GetImplementation"/> and <see cref="Selections.this"/> correctly retrieve implementatinos.
        /// </summary>
        [Test]
        public void TestGetImplementation()
        {
            var implementation = CreateTestSelections();

            Assert.AreEqual(implementation.Implementations[0], implementation.GetImplementation(FeedTest.Test1Uri));
            Assert.AreEqual(implementation.Implementations[0], implementation[FeedTest.Test1Uri]);

            // ReSharper disable UnusedVariable
            Assert.IsNull(implementation.GetImplementation(new FeedUri("http://invalid/")));
            Assert.Throws<KeyNotFoundException>(() => { var dummy = implementation[new FeedUri("http://invalid/")]; });
            // ReSharper restore UnusedVariable
        }
    }
}
