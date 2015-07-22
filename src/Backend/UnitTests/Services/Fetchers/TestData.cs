/*
 * Copyright 2010-2015 Bastian Eicher, Roland Leopold Walkling
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
using System.Security.Cryptography;
using NanoByte.Common;
using NanoByte.Common.Storage;
using NanoByte.Common.Streams;

namespace ZeroInstall.Services.Fetchers
{
    /// <summary>
    /// Utility class that provides access to the test data contained in the assembly as resources.
    /// </summary>
    public static class TestData
    {
        public const string RegularString = "regular\n";
        public static readonly string RegularHash = RegularString.Hash(SHA256.Create());
        public static readonly long RegularTimestamp = new DateTime(2000, 1, 1, 12, 0, 0).ToUnixTime();

        public const string ExecutableString = "executable\n";
        public static readonly string ExecutableHash = ExecutableString.Hash(SHA256.Create());
        public static readonly long ExecutableTimestamp = new DateTime(2000, 1, 1, 12, 0, 0).ToUnixTime();

        public static readonly Stream ZipArchiveStream = typeof(TestData).GetEmbedded("testArchive.zip");
    }
}
