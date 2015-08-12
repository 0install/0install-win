/*
 * Copyright 2010-2016 Bastian Eicher
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
using FluentAssertions;
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

            _homeDir?.Dispose();
        }

        private readonly OpenPgpSecretKey _secretKey = new OpenPgpSecretKey(
            keyID: OpenPgpUtils.ParseKeyID("DEED44B49BE24661"),
            fingerprint: OpenPgpUtils.ParseFingerpint("E91FE1CBFCCF315543F6CB13DEED44B49BE24661"),
            userID: "Test User <test@0install.de>");

        private readonly byte[] _referenceData = {0, 1, 2, 3, 4, 5, 6, 7, 8, 9};

        private readonly byte[] _signatureData = typeof(OpenPgpTest<>).GetEmbeddedBytes("signature.dat");

        [Test]
        public void TestVerifyValidSignature()
        {
            TestImportKey();

            Target.Verify(_referenceData, _signatureData).Should().Equal(
                new ValidSignature(_secretKey.KeyID, _secretKey.GetFingerprint(), new DateTime(2015, 7, 16, 17, 20, 7, DateTimeKind.Utc)));
        }

        [Test]
        public void TestVerifyBadSignature()
        {
            TestImportKey();

            Target.Verify(new byte[] {1, 2, 3}, _signatureData).Should().Equal(
                new BadSignature(_secretKey.KeyID));
        }

        [Test]
        public void TestVerifyMissingKeySignature()
        {
            Target.Verify(_referenceData, _signatureData).Should().Equal(
                new MissingKeySignature(_secretKey.KeyID));
        }

        [Test]
        public void TestVerifyInvalidData()
        {
            Target.Invoking(x => x.Verify(new byte[] {1, 2, 3}, new byte[] {1, 2, 3}))
                .ShouldThrow<InvalidDataException>();
        }

        [Test]
        public void TestSign()
        {
            DeployKeyRings();

            var signatureData = Target.Sign(_referenceData, _secretKey, "passphrase");
            Assert.That(signatureData.Length, Is.GreaterThan(10));

            TestImportKey();
            var signature = (ValidSignature)Target.Verify(_referenceData, signatureData).Single();
            signature.GetFingerprint().Should().Equal(_secretKey.GetFingerprint());
        }

        [Test]
        public void TestSignMissingKey()
        {
            Target.Invoking(x => x.Sign(_referenceData, _secretKey)).ShouldThrow<KeyNotFoundException>();
        }

        [Test]
        public void TestSignWrongPassphrase()
        {
            DeployKeyRings();

            Target.Invoking(x => x.Sign(_referenceData, _secretKey, "wrong-passphrase")).ShouldThrow<WrongPassphraseException>();
        }

        [Test]
        public void TestImportKey()
        {
            Target.ImportKey(typeof(OpenPgpTest<>).GetEmbeddedBytes("pubkey.gpg"));
        }

        [Test]
        public void TestImportKeyInvalidData()
        {
            Target.Invoking(x => x.ImportKey(new byte[] {1, 2, 3})).ShouldThrow<InvalidDataException>();
        }

        [Test]
        public void TestExportKey()
        {
            DeployKeyRings();

            string exportedKey = Target.ExportKey(_secretKey);
            string referenceKeyData = typeof(OpenPgpTest<>).GetEmbeddedString("pubkey.gpg")
                .GetRightPartAtFirstOccurrence("\n\n").GetLeftPartAtLastOccurrence("+");

            Assert.That(exportedKey, Is.StringStarting("-----BEGIN PGP PUBLIC KEY BLOCK-----\n"));
            Assert.That(exportedKey, Is.StringContaining(referenceKeyData));
            Assert.That(exportedKey, Is.StringEnding("-----END PGP PUBLIC KEY BLOCK-----\n"));
        }

        [Test]
        public void TestExportKeyMissingKey()
        {
            Target.Invoking(x => x.ExportKey(_secretKey)).ShouldThrow<KeyNotFoundException>();
        }

        [Test]
        public void TestListSecretKeys()
        {
            DeployKeyRings();

            Target.ListSecretKeys().Should().Equal(_secretKey);
        }

        [Test]
        public void TestGetSecretKey()
        {
            DeployKeyRings();

            Target.GetSecretKey(_secretKey).Should().Be(_secretKey, because: "Should get secret key using parsed id source");

            Target.GetSecretKey(_secretKey.UserID).Should().Be(_secretKey, because: "Should get secret key using user id");
            Target.GetSecretKey(_secretKey.FormatKeyID()).Should().Be(_secretKey, because: "Should get secret key using key id string");
            Target.GetSecretKey(_secretKey.FormatFingerprint()).Should().Be(_secretKey, because: "Should get secret key using fingerprint string");

            Target.GetSecretKey().Should().Be(_secretKey, because: "Should get default secret key");

            Target.Invoking(x => x.GetSecretKey("unknown@user.com")).ShouldThrow<KeyNotFoundException>();
        }

        private void DeployKeyRings()
        {
            typeof(OpenPgpTest<>).CopyEmbeddedToFile("pubring.gpg", Path.Combine(Target.HomeDir, "pubring.gpg"));
            typeof(OpenPgpTest<>).CopyEmbeddedToFile("secring.gpg", Path.Combine(Target.HomeDir, "secring.gpg"));
        }
    }
}
