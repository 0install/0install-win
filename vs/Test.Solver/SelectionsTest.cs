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

using System;
using System.IO;
using NUnit.Framework;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;
using IO = System.IO;

namespace ZeroInstall.Solver
{
    /// <summary>
    /// Contains test methods for <see cref="Selections"/>.
    /// </summary>
    [TestFixture]
    public class SelectionsTest
    {
        /// <summary>
        /// Ensures that the class is correctly serialized and deserialized.
        /// </summary>
        [Test]
        public void TestSaveLoad()
        {
            Selections sel1, sel2;
            string tempFile = null;
            try
            {
                tempFile = Path.GetTempFileName();

                // Write and read file
                sel1 = new Selections { Implementations = { new ImplementationSelection
                {
                    Version = new ImplementationVersion("1.0"),
                    Architecture = new Architecture(OS.Windows, Cpu.I586),
                    Interface = new Uri("http://0install.nanobyte.de/feeds/test.xml")
                }} };
                sel1.Save(tempFile);
                sel2 = Selections.Load(tempFile);
            }
            finally
            { // Clean up
                if (tempFile != null) IO.File.Delete(tempFile);
            }

            // Ensure data stayed the same
            Assert.AreEqual(sel1.Interface, sel2.Interface);
            Assert.AreEqual(sel1.Implementations[0].Architecture, sel2.Implementations[0].Architecture);
        }

        ///// <summary>
        ///// Ensures that <see cref="Selections.GetUncachedImplementations"/> correctly finds <see cref="Implementation"/>s not cached in a <see cref="IStore"/>.
        ///// </summary>
        //[Test]
        //public void TestGetUncachedImplementations()
        //{
        //}
    }
}
