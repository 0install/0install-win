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

using NUnit.Framework;

namespace ZeroInstall.Store.Trust
{
    /// <summary>
    /// Contains test methods for <see cref="OpenPgpUtils"/>.
    /// </summary>
    [TestFixture]
    public class OpenPgpUtilsTest
    {
        [Test]
        public void TestParseKeyID()
        {
            Assert.AreEqual(
                expected: 255,
                actual: OpenPgpUtils.ParseKeyID("00000000000000FF"));
        }

        [Test]
        public void TestFormatKeyID()
        {
            Assert.AreEqual(
                expected: "00000000000000FF",
                actual: OpenPgpUtils.FormatKeyID(255));
        }

        [Test]
        public void TestParseFingerrpint()
        {
            CollectionAssert.AreEqual(
                expected: new byte[] {0, 255},
                actual: OpenPgpUtils.ParseFingerpint("00FF"));
        }

        [Test]
        public void TestFormatFingerprint()
        {
            Assert.AreEqual(
                expected: "00FF",
                actual: OpenPgpUtils.FormatFingerprint(new byte[] {0, 255}));
        }
    }
}
