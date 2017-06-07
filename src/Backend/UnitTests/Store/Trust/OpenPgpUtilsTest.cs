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
using System.IO;
using FluentAssertions;
using NanoByte.Common.Storage;
using Xunit;

namespace ZeroInstall.Store.Trust
{
    /// <summary>
    /// Contains test methods for <see cref="OpenPgpUtils"/>.
    /// </summary>
    public class OpenPgpUtilsTest : TestWithMocks
    {
        public const string TestKeyIDString = "00000000000000FF";
        public static readonly long TestKeyID = 255;
        public static readonly byte[] TestFingerprint = {170, 170, 0, 0, 0, 0, 0, 0, 0, 255};
        public const string TestFingerprintString = "AAAA00000000000000FF";
        public static readonly ValidSignature TestSignature = new ValidSignature(TestKeyID, TestFingerprint, new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        [Fact]
        public void TestFormatKeyID()
        {
            new ErrorSignature(TestKeyID).FormatKeyID()
                .Should().Be(TestKeyIDString);
        }

        [Fact]
        public void TestFormatFingerprint()
        {
            new OpenPgpSecretKey(TestKeyID, TestFingerprint, "a@b.com").FormatFingerprint()
                .Should().Be(TestFingerprintString);
        }

        [Fact]
        public void TestParseKeyID()
        {
            OpenPgpUtils.ParseKeyID(TestKeyIDString)
                .Should().Be(TestKeyID);
        }

        [Fact]
        public void TestParseFingerrpint()
        {
            OpenPgpUtils.ParseFingerpint(TestFingerprintString)
                .Should().Equal(TestFingerprint);
        }

        [Fact]
        public void TestFingerprintToKeyID()
        {
            OpenPgpUtils.FingerprintToKeyID(OpenPgpUtils.ParseFingerpint("E91FE1CBFCCF315543F6CB13DEED44B49BE24661"))
                .Should().Be(OpenPgpUtils.ParseKeyID("DEED44B49BE24661"));
        }

        [Fact]
        public void TestDeployPublicKey()
        {
            using (var tempDir = new TemporaryDirectory("0install-unit-tests"))
            {
                const string publicKey = "public";
                var secretKey = new OpenPgpSecretKey(keyID: 123, fingerprint: new byte[] { 1, 2, 3 }, userID: "user");

                var openPgpMock = CreateMock<IOpenPgp>();
                openPgpMock.Setup(x => x.ExportKey(secretKey)).Returns(publicKey);
                openPgpMock.Object.DeployPublicKey(secretKey, tempDir.Path);

                File.ReadAllText(tempDir + Path.DirectorySeparatorChar + secretKey.FormatKeyID() + ".gpg")
                    .Should().Be(publicKey, because: "Public key should be written to parallel file in directory");
            }
        }
    }
}
