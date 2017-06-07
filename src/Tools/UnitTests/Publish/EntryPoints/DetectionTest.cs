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
using System.Linq;
using FluentAssertions;
using NanoByte.Common.Storage;
using Xunit;

namespace ZeroInstall.Publish.EntryPoints
{
    /// <summary>
    /// Contains test methods for <see cref="Detection"/>.
    /// </summary>
    public class DetectionTest : CandidateTest
    {
        [Fact]
        public void TestListCandidates()
        {
            Deploy(DotNetExeTest.Reference, xbit: false);
            Deploy(WindowsExeTest.Reference32, xbit: false);
            Deploy(PythonScriptTest.Reference, xbit: true);
            Deploy(PosixScriptTest.Reference, xbit: true);
            Deploy(PosixBinaryTest.Reference32, xbit: true);

            var candidates = Detection.ListCandidates(Directory).ToList();
            candidates.Should().BeEquivalentTo(
                DotNetExeTest.Reference,
                WindowsExeTest.Reference32,
                PythonScriptTest.Reference,
                PosixScriptTest.Reference,
                PosixBinaryTest.Reference32);
        }

        [Fact] // Should not fail on empty files
        public void TestEmpty()
        {
            FileUtils.Touch(Path.Combine(Directory.FullName, "empty"));
            Detection.ListCandidates(Directory).Should().BeEmpty();
        }
    }
}
