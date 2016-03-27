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

using System.IO;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NUnit.Framework;

namespace ZeroInstall.Store.Trust
{
    /// <summary>
    /// Runs test methods for <see cref="GnuPG"/>.
    /// </summary>
    [TestFixture, Ignore("No longer used and potentially buggy on some Mono 4.x releases")]
    public class GnuPGTest : OpenPgpTest<GnuPG>
    {
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            if (WindowsUtils.IsWindows && !File.Exists(Path.Combine(Locations.InstallBase, "..", "..", "..", "bundled", "GnuPG", "gpg.exe")))
                Assert.Ignore("GnuPG Windows executable not bundled");
        }
    }
}
