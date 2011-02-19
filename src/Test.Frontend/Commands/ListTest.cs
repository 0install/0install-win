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

using System;
using NDesk.Options;
using NUnit.Framework;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Contains code for testing <see cref="List"/>.
    /// </summary>
    [TestFixture]
    public class ListTest : CommandBaseTest
    {
        /// <inheritdoc/>
        protected override CommandBase GetCommand()
        {
 	        return new List(Handler, Policy);
        }

        /// <summary>
        /// Ensures calling with no arguments returns all feeds in the <see cref="IFeedCache"/>.
        /// </summary>
        [Test]
        public void TestNoArgs()
        {
            CacheMock.ExpectAndReturn("ListAll", new[] {new Uri("http://0install.de/feeds/test/test1.xml"), new Uri("http://0install.de/feeds/test/test2.xml")});
            AssertParseExecuteResult(new string[0], "http://0install.de/feeds/test/test1.xml" + Environment.NewLine + "http://0install.de/feeds/test/test2.xml", 0);
        }

        /// <summary>
        /// Ensures calling with a single argument returns a filtered list of feeds in the <see cref="IFeedCache"/>.
        /// </summary>
        [Test]
        public void TestPattern()
        {
            CacheMock.ExpectAndReturn("ListAll", new[] {new Uri("http://0install.de/feeds/test/test1.xml"), new Uri("http://0install.de/feeds/test/test2.xml")});
            AssertParseExecuteResult(new[] {"test2"}, "http://0install.de/feeds/test/test2.xml", 0);
        }

        /// <summary>
        /// Ensures calling with too many arguments raises an exception.
        /// </summary>
        [Test]
        public void TestTooManyArgs()
        {
            Command.Parse(new[] {"test1", "test2"});
            Assert.Throws<OptionException>(() => Command.Execute(), "Should reject more than one argument");
        }
    }
}
