﻿/*
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

using System.IO;
using System.Linq;
using NUnit.Framework;

namespace ZeroInstall.Publish.EntryPoints
{
    /// <summary>
    /// Contains test methods for <see cref="Detection"/>.
    /// </summary>
    [TestFixture]
    public class DetectionTest : CandidateTest
    {
        [Test]
        public void TestListCandidates()
        {
            Deploy(DotNetExeTest.Reference);
            Deploy(WindowsExeTest.Reference32);
            Deploy(PythonScriptTest.Reference, executable: true);
            Deploy(PosixScriptTest.Reference, executable: true);
            Deploy(PosixBinaryTest.Reference32, executable: true);

            var candidates = Detection.ListCandidates(Directory).ToList();
            CollectionAssert.AreEquivalent(
                new Candidate[]
                {
                    DotNetExeTest.Reference,
                    WindowsExeTest.Reference32,
                    PythonScriptTest.Reference,
                    PosixScriptTest.Reference,
                    PosixBinaryTest.Reference32
                },
                candidates);
        }

        [Test(Description = "Should not fail on empty files")]
        public void TestEmpty()
        {
            File.WriteAllText(Path.Combine(Directory.FullName, "empty"), "");
            Assert.IsEmpty(Detection.ListCandidates(Directory).ToList());
        }
    }
}
