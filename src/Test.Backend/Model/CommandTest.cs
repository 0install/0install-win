/*
 * Copyright 2010-2011 Bastian Eicher
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

namespace ZeroInstall.Model
{
    /// <summary>
    /// Contains test methods for <see cref="Command"/>.
    /// </summary>
    [TestFixture]
    public class CommandTest
    {
        #region Helpers
        /// <summary>
        /// Creates a fictive test <see cref="Command"/>.
        /// </summary>
        public static Command CreateTestCommand1()
        {
            return new Command
            {
                Name = Command.NameRun,
                Path = "dir 1/executable1", Arguments = {"--executable1"},
                Runner = new Runner
                {
                    Interface = "http://0install.de/feeds/test/test2.xml", Arguments = {"runner argument"}, Bindings = {new EnvironmentBinding {Name = "TEST2_PATH_RUNNER_SELF"}}
                },
                Bindings = {new EnvironmentBinding {Name = "TEST1_PATH_COMMAND"}},
                WorkingDir = new WorkingDir {Source = "bin"}
            };
        }

        /// <summary>
        /// Creates a fictive test <see cref="Command"/>.
        /// </summary>
        public static Command CreateTestCommand2()
        {
            return new Command
            {
                Name = Command.NameRun,
                Path = "dir 2/executable2", Arguments = {"--executable2"},
                Dependencies =
                    {
                        new Dependency
                        {
                            Interface = "http://0install.de/feeds/test/test1.xml", Bindings = {new EnvironmentBinding {Name = "TEST1_PATH_COMMAND_DEP"}}
                        }
                    }
            };
        }
        #endregion

        /// <summary>
        /// Ensures that the class can be correctly cloned.
        /// </summary>
        [Test]
        public void TestClone()
        {
            var command1 = CreateTestCommand1();
            var command2 = command1.CloneCommand();

            // Ensure data stayed the same
            Assert.AreEqual(command1, command2, "Cloned objects should be equal.");
            Assert.AreEqual(command1.GetHashCode(), command2.GetHashCode(), "Cloned objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(command1, command2), "Cloning should not return the same reference.");
        }

        /// <summary>
        /// Ensures that the class can be correctly compared.
        /// </summary>
        [Test]
        public void TestEquals()
        {
            var command1 = CreateTestCommand1();
            var command2 = command1.CloneCommand();
            command2.Bindings.Add(new EnvironmentBinding());

            // Ensure data stayed the same
            Assert.AreEqual(command1, command1, "Equals() should be reflexive.");
            Assert.AreEqual(command1.GetHashCode(), command1.GetHashCode(), "GetHashCode() should be reflexive.");
            Assert.AreNotEqual(command1, command2);
            Assert.AreNotEqual(command1.GetHashCode(), command2.GetHashCode());
        }
    }
}
