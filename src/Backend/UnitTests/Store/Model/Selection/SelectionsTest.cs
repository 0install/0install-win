/*
 * Copyright 2010-2014 Bastian Eicher
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
                InterfaceID = "http://0install.de/feeds/test/test1.xml",
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

            Assert.AreEqual(implementation.Implementations[0], implementation.GetImplementation("http://0install.de/feeds/test/test1.xml"));
            Assert.AreEqual(implementation.Implementations[0], implementation["http://0install.de/feeds/test/test1.xml"]);

            // ReSharper disable UnusedVariable
            Assert.Throws<ArgumentNullException>(() => { var dummy = implementation.GetImplementation(""); });
            Assert.Throws<ArgumentNullException>(() => { var dummy = implementation[""]; });

            Assert.IsNull(implementation.GetImplementation("invalid"));
            Assert.Throws<KeyNotFoundException>(() => { var dummy = implementation["invalid"]; });
            // ReSharper restore UnusedVariable
        }
    }
}
