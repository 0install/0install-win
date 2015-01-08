/*
 * Copyright 2010-2014 Bastian Eicher
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
using System.Linq;
using NanoByte.Common.Storage;
using NanoByte.Common.Streams;
using NUnit.Framework;

namespace ZeroInstall.Store.Trust
{
    /// <summary>
    /// Contains test methods for <see cref="GnuPG"/>.
    /// </summary>
    [TestFixture]
    public class GnuPGTest : TestWithContainer<GnuPG>
    {
        [Test]
        public void TestImportExport()
        {
            const string testKeyID = "5B5CB97421BAA5DC";
            Target.ImportKey(TestData.GetResource(testKeyID + ".gpg").ReadToArray());
            Assert.IsTrue(Target.GetPublicKey(testKeyID).StartsWith("-----BEGIN PGP PUBLIC KEY BLOCK-----"));
        }

        [Test]
        public void TestDetachSignAndVerify()
        {
            const string testKeyID = "E91FE1CBFCCF315543F6CB13DEED44B49BE24661";
            var data = new byte[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9};
            var stream = new MemoryStream(data);

            TestData.GetResource("pubring.gpg").WriteTo(Locations.GetSaveConfigPath("0install.net", true, "gnupg", "pubring.gpg"));
            TestData.GetResource("secring.gpg").WriteTo(Locations.GetSaveConfigPath("0install.net", true, "gnupg", "secring.gpg"));

            var signatureData = Convert.FromBase64String(Target.DetachSign(stream, "test@0install.de"));
            var signatures = Target.Verify(data, signatureData);

            Assert.AreEqual(testKeyID, ((ValidSignature)signatures.Single()).Fingerprint);
        }
    }
}
