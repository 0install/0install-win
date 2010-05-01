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
