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
using NUnit.Framework;
using ZeroInstall.Model;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// Contains test methods for <see cref="Store"/>.
    /// </summary>
    public class StoreTest
    {
        /// <summary>
        /// Ensures <see cref="Store.Contains"/> correctly differentiates between available and not available <see cref="Implementation"/>s.
        /// </summary>
        [Test]
        public void TestContains()
        {
            Assert.IsFalse(new Store().Contains(new ManifestDigest { Sha256 = "test" }));
        }

        /// <summary>
        /// Ensures <see cref="Store.GetPath"/> correctly determines the local path of a cached <see cref="Implementation"/>.
        /// </summary>
        [Test]
        public void TestGetPath()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Ensures <see cref="Store.Add"/> correctly adds new <see cref="Implementation"/>s to the store.
        /// </summary>
        [Test]
        public void TestAdd()
        {
            throw new NotImplementedException();
        }
    }
}
