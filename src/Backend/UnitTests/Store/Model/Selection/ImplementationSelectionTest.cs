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

using NUnit.Framework;

namespace ZeroInstall.Store.Model.Selection
{
    /// <summary>
    /// Contains test methods for <see cref="ImplementationSelection"/>.
    /// </summary>
    [TestFixture]
    public class ImplementationSelectionTest
    {
        #region Helpers
        /// <summary>
        /// Creates a fictive test <see cref="ImplementationSelection"/>.
        /// </summary>
        internal static ImplementationSelection CreateTestImplementation1()
        {
            return new ImplementationSelection
            {
                InterfaceID = "http://0install.de/feeds/test/test1.xml",
                FromFeed = "http://0install.de/feeds/test/sub1.xml",
                ID = "id1", ManifestDigest = new ManifestDigest(sha256: "123"), Version = new ImplementationVersion("1.0"),
                Architecture = new Architecture(OS.Windows, Cpu.I586), Languages = {"en-US"},
                DocDir = "doc", Stability = Stability.Developer,
                Bindings =
                {
                    new EnvironmentBinding {Name = "TEST1_PATH_SELF", Default = "default", Mode = EnvironmentMode.Append},
                    new EnvironmentBinding {Name = "TEST1_VALUE", Value = "test1", Mode = EnvironmentMode.Replace},
                    new EnvironmentBinding {Name = "TEST1_EMPTY", Value = "", Mode = EnvironmentMode.Append}
                },
                Dependencies =
                {
                    new Dependency
                    {
                        InterfaceID = "http://0install.de/feeds/test/test2.xml",
                        Bindings = {new EnvironmentBinding {Name = "TEST2_PATH_SUB_DEP", Insert = "sub", Default = "default", Mode = EnvironmentMode.Append}}
                    }
                },
                Commands = {CommandTest.CreateTestCommand1(), CommandTest.CreateTestCommand1Test()}
            };
        }

        /// <summary>
        /// Creates a fictive test <see cref="ImplementationSelection"/>.
        /// </summary>
        internal static ImplementationSelection CreateTestImplementation2()
        {
            return new ImplementationSelection
            {
                InterfaceID = "http://0install.de/feeds/test/test2.xml",
                FromFeed = "http://0install.de/feeds/test/sub2.xml",
                ID = "id2", ManifestDigest = new ManifestDigest(sha256: "abc"), Version = new ImplementationVersion("1.0"),
                Architecture = new Architecture(OS.Windows, Cpu.I586), Languages = {"en-US"},
                DocDir = "doc", Stability = Stability.Developer,
                Bindings =
                {
                    new EnvironmentBinding {Name = "TEST2_PATH_SELF", Default = "default", Mode = EnvironmentMode.Prepend},
                    new EnvironmentBinding {Name = "TEST2_VALUE", Value = "test2", Mode = EnvironmentMode.Replace}
                },
                Dependencies =
                {
                    new Dependency
                    {
                        InterfaceID = "http://0install.de/feeds/test/test1.xml",
                        Bindings =
                        {
                            new ExecutableInVar {Name = "exec-in-var", Command = Command.NameTest},
                            new ExecutableInPath {Name = "exec-in-path", Command = Command.NameTest}
                        }
                    }
                },
                Commands = {CommandTest.CreateTestCommand2()}
            };
        }
        #endregion

        /// <summary>
        /// Ensures that the class can be correctly cloned.
        /// </summary>
        [Test]
        public void TestClone()
        {
            var implementation1 = CreateTestImplementation1();
            var implementation2 = implementation1.CloneImplementation();

            // Ensure data stayed the same
            Assert.AreEqual(implementation1, implementation2, "Cloned objects should be equal.");
            Assert.AreEqual(implementation1.GetHashCode(), implementation2.GetHashCode(), "Cloned objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(implementation1, implementation2), "Cloning should not return the same reference.");
        }
    }
}
