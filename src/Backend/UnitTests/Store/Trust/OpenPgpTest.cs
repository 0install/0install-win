/*
 * Copyright 2010-2015 Bastian Eicher
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NanoByte.Common;
using NanoByte.Common.Storage;
using NanoByte.Common.Streams;
using NUnit.Framework;

namespace ZeroInstall.Store.Trust
{
    /// <summary>
    /// Contains common code for testing specific <see cref="IOpenPgp"/> implementations.
    /// </summary>
    public abstract class OpenPgpTest<T> : TestWithContainer<T> where T : class, IOpenPgp
    {
        private TemporaryDirectory _homeDir;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            Target.HomeDir = _homeDir = new TemporaryDirectory("0install-unit-test");
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();

            if (_homeDir != null) _homeDir.Dispose();
        }

        private readonly OpenPgpSecretKey _secretKey = new OpenPgpSecretKey(
            keyID: OpenPgpUtils.ParseKeyID("DEED44B49BE24661"),
            fingerprint: OpenPgpUtils.ParseFingerpint("E91FE1CBFCCF315543F6CB13DEED44B49BE24661"),
            userID: "Test User <test@0install.de>");

        private readonly byte[] _referenceData = {0, 1, 2, 3, 4, 5, 6, 7, 8, 9};

        private readonly byte[] _signatureData = typeof(OpenPgpTest<T>).GetEmbedded("signature.dat").ToArray();

        [Test]
        public void TestVerifyValidSignature()
        {
            TestImportKey();

            CollectionAssert.AreEqual(
                expected: new[] {new ValidSignature(_secretKey.KeyID, _secretKey.GetFingerprint(), new DateTime(2015, 7, 16, 17, 20, 7))},
                actual: Target.Verify(_referenceData, _signatureData));
        }

        [Test]
        public void TestVerifyBadSignature()
        {
            TestImportKey();

            CollectionAssert.AreEqual(
                expected: new[] {new BadSignature(_secretKey.KeyID)},
                actual: Target.Verify(new byte[] {1, 2, 3}, _signatureData));
        }

        [Test]
        public void TestVerifyMissingKeySignature()
        {
            CollectionAssert.AreEqual(
                expected: new[] {new MissingKeySignature(_secretKey.KeyID)},
                actual: Target.Verify(_referenceData, _signatureData));
        }

        [Test]
        public void TestVerifyInvalidData()
        {
            Assert.Throws<InvalidDataException>(() => Target.Verify(new byte[] {1, 2, 3}, new byte[] {1, 2, 3}));
        }

        [Test]
        public void TestSign()
        {
            DeployKeyRings();

            var signatureData = Target.Sign(_referenceData, _secretKey, "passphrase");
            Assert.That(signatureData.Length, Is.GreaterThan(10));

            TestImportKey();
            var signatures = Target.Verify(_referenceData, signatureData);
            CollectionAssert.AreEqual(
                expected: _secretKey.GetFingerprint(),
                actual: ((ValidSignature)signatures.Single()).GetFingerprint());
        }

        [Test]
        public void TestSignMissingKey()
        {
            Assert.Throws<KeyNotFoundException>(() => Target.Sign(_referenceData, _secretKey));
        }

        [Test]
        public void TestSignWrongPassphrase()
        {
            DeployKeyRings();

            Assert.Throws<WrongPassphraseException>(() => Target.Sign(_referenceData, _secretKey, "wrong-passphrase"));
        }

        [Test]
        public void TestImportKey()
        {
            Target.ImportKey(this.GetEmbedded("pubkey.gpg").ToArray());
        }

        [Test]
        public void TestImportKeyInvalidData()
        {
            Assert.Throws<InvalidDataException>(() => Target.ImportKey(new byte[] {1, 2, 3}));
        }

        [Test]
        public void TestExportKey()
        {
            DeployKeyRings();

            string exportedKey = Target.ExportKey(_secretKey);
            string referenceKeyData = this.GetEmbedded("pubkey.gpg").ReadToString()
                .GetRightPartAtFirstOccurrence("\n\n").GetLeftPartAtLastOccurrence("+");

            Assert.That(exportedKey, Is.StringStarting("-----BEGIN PGP PUBLIC KEY BLOCK-----\n"));
            Assert.That(exportedKey, Is.StringContaining(referenceKeyData));
            Assert.That(exportedKey, Is.StringEnding("-----END PGP PUBLIC KEY BLOCK-----\n"));
        }

        [Test]
        public void TestExportKeyMissingKey()
        {
            Assert.Throws<KeyNotFoundException>(() => Target.ExportKey(_secretKey));
        }

        [Test]
        public void TestListSecretKeys()
        {
            DeployKeyRings();

            CollectionAssert.AreEqual(
                expected: new[] {_secretKey},
                actual: Target.ListSecretKeys());
        }

        [Test]
        public void TestGetSecretKey()
        {
            DeployKeyRings();

            Assert.AreEqual(_secretKey, Target.GetSecretKey(_secretKey), "Should get secret key using parsed id source");

            Assert.AreEqual(_secretKey, Target.GetSecretKey(_secretKey.UserID), "Should get secret key using user id");
            Assert.AreEqual(_secretKey, Target.GetSecretKey(_secretKey.FormatKeyID()), "Should get secret key using key id string");
            Assert.AreEqual(_secretKey, Target.GetSecretKey(_secretKey.FormatFingerprint()), "Should get secret key using fingerprint string");

            Assert.AreEqual(_secretKey, Target.GetSecretKey(), "Should get default secret key");

            Assert.Throws<KeyNotFoundException>(() => Target.GetSecretKey("unknown@user.com"));
        }

        private void DeployKeyRings()
        {
            this.GetEmbedded("pubring.gpg").CopyToFile(Path.Combine(Target.HomeDir, "pubring.gpg"));
            this.GetEmbedded("secring.gpg").CopyToFile(Path.Combine(Target.HomeDir, "secring.gpg"));
        }
    }
}
