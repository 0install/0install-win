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
using NUnit.Framework;

namespace ZeroInstall.Store.Trust
{
    /// <summary>
    /// Contains test methods for <see cref="OpenPgpUtils"/>.
    /// </summary>
    [TestFixture]
    public class OpenPgpUtilsTest
    {
        public const string TestKeyIDString = "00000000000000FF";
        public static readonly long TestKeyID = 255;
        public static readonly byte[] TestFingerprint = {170, 170, 0, 0, 0, 0, 0, 0, 0, 255};
        public const string TestFingerprintString = "AAAA00000000000000FF";
        public static readonly ValidSignature TestSignature = new ValidSignature(TestKeyID, TestFingerprint, new DateTime(2000, 1, 1));

        [Test]
        public void TestParseKeyID()
        {
            Assert.AreEqual(
                expected: TestKeyID,
                actual: OpenPgpUtils.ParseKeyID(TestKeyIDString));
        }

        [Test]
        public void TestParseFingerrpint()
        {
            CollectionAssert.AreEqual(
                expected: TestFingerprint,
                actual: OpenPgpUtils.ParseFingerpint(TestFingerprintString));
        }

        [Test]
        public void TestFormatKeyID()
        {
            Assert.AreEqual(
                expected: TestKeyIDString,
                actual: new ErrorSignature(TestKeyID).FormatKeyID());
        }

        [Test]
        public void TestFormatFingerprint()
        {
            Assert.AreEqual(
                expected: TestFingerprintString,
                actual: new OpenPgpSecretKey(TestKeyID, TestFingerprint, "a@b.com").FormatFingerprint());
        }

        [Test]
        public void TestFingerprintToKeyID()
        {
            Assert.AreEqual(
                expected: OpenPgpUtils.ParseKeyID("DEED44B49BE24661"),
                actual: OpenPgpUtils.FingerprintToKeyID(OpenPgpUtils.ParseFingerpint("E91FE1CBFCCF315543F6CB13DEED44B49BE24661")));
        }
    }
}
