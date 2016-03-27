/*
 * Copyright 2010-2016 Bastian Eicher
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

using FluentAssertions;
using NUnit.Framework;

namespace ZeroInstall.Store.Model
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
                    InterfaceUri = FeedTest.Test2Uri, Arguments = {"runner argument"}, Bindings = {new EnvironmentBinding {Name = "TEST2_PATH_RUNNER_SELF"}}
                },
                Bindings = {new EnvironmentBinding {Name = "TEST1_PATH_COMMAND"}},
                WorkingDir = new WorkingDir {Source = "bin"}
            };
        }

        /// <summary>
        /// Creates a fictive test <see cref="Command"/> using <see cref="Command.NameTest"/>.
        /// </summary>
        public static Command CreateTestCommand1Test()
        {
            return new Command
            {
                Name = Command.NameTest,
                Path = "dir 1/test1", Arguments = {"--test1"},
                Runner = new Runner
                {
                    InterfaceUri = FeedTest.Test2Uri, Arguments = {"runner argument"}
                }
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
                        InterfaceUri = FeedTest.Test1Uri,
                        Bindings = {new EnvironmentBinding {Name = "TEST1_PATH_COMMAND_DEP"}}
                    }
                },
                Restrictions =
                {
                    new Restriction
                    {
                        InterfaceUri = FeedTest.Test2Uri,
                        Constraints = {new Constraint {Before = new ImplementationVersion("2.0")}}
                    }
                }
            };
        }
        #endregion

        /// <summary>
        /// Ensures that the class can be correctly cloned and compared.
        /// </summary>
        [Test]
        public void TestCloneEquals()
        {
            var command1 = CreateTestCommand1();
            command1.Should().Be(command1, because: "Equals() should be reflexive.");
            command1.GetHashCode().Should().Be(command1.GetHashCode(), because: "GetHashCode() should be reflexive.");

            var command2 = command1.Clone();
            command2.Should().Be(command1, because: "Cloned objects should be equal.");
            command2.GetHashCode().Should().Be(command1.GetHashCode(), because: "Cloned objects' hashes should be equal.");
            command2.Should().NotBeSameAs(command1, because: "Cloning should not return the same reference.");

            command2.Bindings.Add(new EnvironmentBinding());
            command2.Should().NotBe(command1, because: "Modified objects should no longer be equal");
        }
    }
}
