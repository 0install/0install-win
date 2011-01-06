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
using ZeroInstall.Model;

namespace ZeroInstall.Launcher.Solver
{
    /// <summary>
    /// Contains test methods for <see cref="Selections"/>.
    /// </summary>
    [TestFixture]
    public class SelectionsTest
    {
        #region Helpers
        /// <summary>
        /// Creates a <see cref="Selections"/> with two implementations, one using the other as a runner.
        /// </summary>
        public static Selections CreateTestSelections()
        {
            return new Selections
            {
                InterfaceID = "http://0install.de/feeds/test/test1.xml",
                Implementations = {ImplementationSelectionTest.CreateTestImplementation1(), ImplementationSelectionTest.CreateTestImplementation2()},
                Commands = {CommandTest.CreateTestCommand1(), CommandTest.CreateTestCommand2()}
            };
        }
        #endregion

        /// <summary>
        /// Ensures that the class is correctly serialized and deserialized.
        /// </summary>
        [Test]
        public void TestSaveLoad()
        {
            var selections1 = CreateTestSelections();

            // Serialize and deserialize data
            string data = selections1.WriteToString();
            var selections2 = Selections.LoadFromString(data);

            // Ensure data stayed the same
            Assert.AreEqual(selections1, selections2, "Serialized objects should be equal.");
            Assert.AreEqual(selections1.GetHashCode(), selections2.GetHashCode(), "Serialized objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(selections1, selections2), "Serialized objects should not return the same reference.");
        }

        /// <summary>
        /// Ensures that the class can be correctly cloned.
        /// </summary>
        [Test]
        public void TestClone()
        {
            var selections1 = CreateTestSelections();
            var selections2 = selections1.CloneSelections();

            // Ensure data stayed the same
            Assert.AreEqual(selections1, selections2, "Cloned objects should be equal.");
            Assert.AreEqual(selections1.GetHashCode(), selections2.GetHashCode(), "Cloned objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(selections1, selections2), "Cloning should not return the same reference.");
        }
    }
}
