/*
 * Copyright 2010-2017 Bastian Eicher
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

using FluentAssertions;
using NanoByte.Common.Native;
using Xunit;

namespace ZeroInstall.Store.Implementations
{
    /// <summary>
    /// Contains test methods for <see cref="StoreUtils"/>.
    /// </summary>
    public class StoreUtilsTest
    {
        [Fact]
        public void TestDetectImplementationPath()
        {
            StoreUtils.DetectImplementationPath(WindowsUtils.IsWindows ? @"C:\some\dir" : "/some/dir")
                .Should().BeNull();
            StoreUtils.DetectImplementationPath(WindowsUtils.IsWindows ? @"C:\some\dir\sha1new=123" : "/some/dir/sha1new=123")
                .Should().Be(WindowsUtils.IsWindows ? @"C:\some\dir\sha1new=123" : "/some/dir/sha1new=123");
            StoreUtils.DetectImplementationPath(WindowsUtils.IsWindows ? @"C:\some\dir\sha1new=123\subdir" : "/some/dir/sha1new=123/subdir")
                .Should().Be(WindowsUtils.IsWindows ? @"C:\some\dir\sha1new=123" : "/some/dir/sha1new=123");
        }
    }
}
