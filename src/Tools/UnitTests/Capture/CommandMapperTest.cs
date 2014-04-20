/*
 * Copyright 2011 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as Captureed by
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

using System.IO;
using NUnit.Framework;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Capture
{
    /// <summary>
    /// Contains test methods for <see cref="CommandMapper"/>.
    /// </summary>
    [TestFixture]
    public class CommandMapperTest
    {
        /// <summary>
        /// Ensures <see cref="CommandMapper.GetCommand"/> correctly finds the best possible <see cref="Command"/> matches for command-lines;
        /// </summary>
        [Test]
        public void TestGetCommand()
        {
            var commandNoArgs = new Command {Name = "no-args", Path = "entry.exe"};
            var commandArgs1 = new Command {Name = "args1", Path = "entry.exe", Arguments = {"--arg1", "long argument"}};
            var commandArgs2 = new Command {Name = "args2", Path = "entry.exe", Arguments = {"--arg2", "long argument"}};
            var provider = new CommandMapper("installation directory", new[] {commandNoArgs, commandArgs1, commandArgs2});

            string additionalArgs;

            Assert.AreSame(commandNoArgs, provider.GetCommand("installation directory" + Path.DirectorySeparatorChar + "entry.exe", out additionalArgs));
            Assert.AreEqual("", additionalArgs);

            Assert.AreSame(commandNoArgs, provider.GetCommand("\"installation directory" + Path.DirectorySeparatorChar + "entry.exe\" --arg1", out additionalArgs));
            Assert.AreEqual("--arg1", additionalArgs);

            Assert.AreSame(commandArgs1, provider.GetCommand("\"installation directory" + Path.DirectorySeparatorChar + "entry.exe\" --arg1 \"long argument\" bla", out additionalArgs));
            Assert.AreEqual("bla", additionalArgs);

            Assert.AreSame(commandArgs2, provider.GetCommand("\"installation directory" + Path.DirectorySeparatorChar + "entry.exe\" --arg2 \"long argument\" bla", out additionalArgs));
            Assert.AreEqual("bla", additionalArgs);

            Assert.IsNull(provider.GetCommand("Something" + Path.DirectorySeparatorChar + "else.exe", out additionalArgs));
        }
    }
}
