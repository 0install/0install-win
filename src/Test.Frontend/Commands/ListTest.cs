/*
 * Copyright 2010-2013 Bastian Eicher
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

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Contains integration tests for <see cref="List"/>.
    /// </summary>
    [TestFixture]
    public class ListTest : FrontendCommandTest
    {
        /// <inheritdoc/>
        protected override FrontendCommand GetCommand()
        {
            return new List(HandlerMock.Object);
        }

        [Test(Description = "Ensures calling with no arguments returns all feeds in the cache.")]
        public void TestNoArgs()
        {
            CacheMock.Setup(x => x.ListAll()).Returns(new[] {"http://0install.de/feeds/test/test1.xml", "http://0install.de/feeds/test/test2.xml"});
            RunAndAssert("http://0install.de/feeds/test/test1.xml" + Environment.NewLine + "http://0install.de/feeds/test/test2.xml", 0);
        }

        [Test(Description = "Ensures calling with a single argument returns a filtered list of feeds in the cache.")]
        public void TestPattern()
        {
            CacheMock.Setup(x => x.ListAll()).Returns(new[] {"http://0install.de/feeds/test/test1.xml", "http://0install.de/feeds/test/test2.xml"});
            RunAndAssert("http://0install.de/feeds/test/test2.xml", 0, "test2");
        }

        [Test(Description = "Ensures calling with too many arguments raises an exception.")]
        public void TestTooManyArgs()
        {
            Command.Parse(new[] {"test1", "test2"});
            Assert.Throws<OptionException>(() => Command.Execute(), "Should reject more than one argument");
        }
    }
}
