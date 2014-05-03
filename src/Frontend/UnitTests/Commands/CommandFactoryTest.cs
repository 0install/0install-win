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

using NDesk.Options;
using NUnit.Framework;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Contains test methods for <see cref="CommandFactory"/>.
    /// </summary>
    [TestFixture]
    public class CommandFactoryTest
    {
        /// <summary>
        /// Ensures <see cref="CommandFactory.CreateAndParse"/> correctly parses command names in argument lists.
        /// </summary>
        [Test]
        public void TestCreateCommand()
        {
            foreach (string name in CommandFactory.CommandNames)
            {
                var command = CreateCommand(name);
                Assert.AreEqual(name, command.Name);
            }
        }

        private static FrontendCommand CreateCommand(string name)
        {
            var handler = new MockCommandHandler();
            try
            {
                return CommandFactory.CreateAndParse(new[] {name, "--verbose"}, handler);
            }
            catch (OptionException)
            {
                // Pass in an additional dummy argument if required
                return CommandFactory.CreateAndParse(new[] {name, "--verbose", "dummy"}, handler);
            }
        }
    }
}
