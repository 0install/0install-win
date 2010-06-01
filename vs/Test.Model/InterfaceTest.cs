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

using System.IO;
using NUnit.Framework;

namespace ZeroInstall.Model
{
    /// <summary>
    /// Contains test methods for <see cref="Interface"/>.
    /// </summary>
    [TestFixture]
    public class InterfaceTest
    {
        /// <summary>
        /// Ensures that the class is correctly serialized and deserialized.
        /// </summary>
        [Test]
        public void TestSaveLoad()
        {
            Interface feedp1, feed2;
            string tempFile = null;
            try
            {
                tempFile = Path.GetTempFileName();

                // Write and read file
                feedp1 = new Interface { Name = "MyApp", Description = "Some text.", Categories = {"Category"} };
                feedp1.Save(tempFile);
                feed2 = Interface.Load(tempFile);
            }
            finally
            { // Clean up
                if (tempFile != null) File.Delete(tempFile);
            }
            
            // Ensure data stayed the same
            Assert.AreEqual(feedp1, feed2, "Serialized objects should be equal.");
            Assert.AreEqual(feedp1.GetHashCode(), feed2.GetHashCode(), "Serialized objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(feedp1, feed2), "Serialized should not return the same reference.");
        }

        /// <summary>
        /// Ensures that <see cref="Interface.Simplify"/> correctly collapsed <see cref="Group"/> structures.
        /// </summary>
        [Test]
        public void TestSimplify()
        {
            var feed = new Interface { Groups =
            {
                new Group
                {
                    LanguagesString = "de",
                    Architecture = new Architecture(OS.Solaris, Cpu.I586),
                    License = "GPL",
                    Stability = Stability.Developer,
                    Implementations =
                    {
                        new Implementation { Main = "main1" },
                        new Implementation { Main = "main2" },
                    }
                }
            } };
            feed.Simplify();

            var implementation = feed.Implementations[0];
            Assert.AreEqual(new Architecture(OS.Solaris, Cpu.I586), implementation.Architecture);
            Assert.AreEqual("de", implementation.LanguagesString);
            Assert.AreEqual("GPL", implementation.License);
            Assert.AreEqual(Stability.Developer, implementation.Stability);
            Assert.AreEqual("main1", implementation.Main);

            implementation = feed.Implementations[1];
            Assert.AreEqual(new Architecture(OS.Solaris, Cpu.I586), implementation.Architecture);
            Assert.AreEqual("de", implementation.LanguagesString);
            Assert.AreEqual("GPL", implementation.License);
            Assert.AreEqual(Stability.Developer, implementation.Stability);
            Assert.AreEqual("main2", implementation.Main);
        }
    }
}
