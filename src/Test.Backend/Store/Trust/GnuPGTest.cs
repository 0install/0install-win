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

using Common.Storage;
using Common.Utils;
using NUnit.Framework;

namespace ZeroInstall.Store.Trust
{
    /// <summary>
    /// Contains test methods for <see cref="GnuPG"/>.
    /// </summary>
    [TestFixture]
    public class GnuPGTest
    {
        #region Shared
        private LocationsRedirect _redirect;
        private GnuPG _gnuPG;

        [SetUp]
        public void SetUp()
        {
            // Don't use real GnuPG profile
            _redirect = new LocationsRedirect("0install-unit-tests");

            _gnuPG = new GnuPG();
        }

        [TearDown]
        public void TearDown()
        {
            _redirect.Dispose();
        }
        #endregion

        [Test]
        public void TestImportExport()
        {
            _gnuPG.ImportKey(TestData.GetResource("5B5CB97421BAA5DC.gpg").ReadToArray());
            Assert.IsTrue(_gnuPG.GetPublicKey("5B5CB97421BAA5DC").StartsWith("-----BEGIN PGP PUBLIC KEY BLOCK-----"));
        }
    }
}
