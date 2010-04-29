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

namespace Common
{
    /// <summary>
    /// Contains test methods for <see cref="Arguments"/>.
    /// </summary>
    public class ArgumentsTest
    {
        /// <summary>
        /// Ensures the <see cref="Arguments"/> constructor correctly parses argument string arrays.
        /// </summary>
        [Test]
        public void TestParse()
        {
            var args = new Arguments(new[] {"-command1", "option", "file", "/command2"});

            Assert.IsTrue(args.Contains("command1"), "command1");
            Assert.IsTrue(args.Contains("command2"), "command2");
            Assert.IsFalse(args.Contains("option"), "option");
            Assert.IsFalse(args.Contains("file"), "file");

            Assert.AreEqual("option", args["command1"]);
            
            Assert.IsTrue(args.Files.Contains("file"), "file");
        }
    }
}
