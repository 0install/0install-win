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
using FluentAssertions;
using Xunit;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Publish.Capture
{
    /// <summary>
    /// Contains test methods for <see cref="CommandMapper"/>.
    /// </summary>
    public class CommandMapperTest
    {
        /// <summary>
        /// Ensures <see cref="CommandMapper.GetCommand"/> correctly finds the best possible <see cref="Command"/> matches for command-lines;
        /// </summary>
        [Fact]
        public void TestGetCommand()
        {
            var commandNoArgs = new Command {Name = "no-args", Path = "entry.exe"};
            var commandArgs1 = new Command {Name = "args1", Path = "entry.exe", Arguments = {"--arg1", "long argument"}};
            var commandArgs2 = new Command {Name = "args2", Path = "entry.exe", Arguments = {"--arg2", "long argument"}};
            var provider = new CommandMapper("installation directory", new[] {commandNoArgs, commandArgs1, commandArgs2});


            provider.GetCommand("installation directory" + Path.DirectorySeparatorChar + "entry.exe", out string additionalArgs)
                .Should().BeSameAs(commandNoArgs);
            additionalArgs.Should().Be("");

            provider.GetCommand("\"installation directory" + Path.DirectorySeparatorChar + "entry.exe\" --arg1", out additionalArgs)
                .Should().BeSameAs(commandNoArgs);
            additionalArgs.Should().Be("--arg1");

            provider.GetCommand("\"installation directory" + Path.DirectorySeparatorChar + "entry.exe\" --arg1 \"long argument\" bla", out additionalArgs)
                .Should().BeSameAs(commandArgs1);
            additionalArgs.Should().Be("bla");

            provider.GetCommand("\"installation directory" + Path.DirectorySeparatorChar + "entry.exe\" --arg2 \"long argument\" bla", out additionalArgs)
                .Should().BeSameAs(commandArgs2);
            additionalArgs.Should().Be("bla");

            provider.GetCommand("Something" + Path.DirectorySeparatorChar + "else.exe", out additionalArgs)
                .Should().BeNull();
        }
    }
}
