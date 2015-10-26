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

using System.Collections.Generic;
using NanoByte.Common.Storage;
using NanoByte.Common.Streams;
using NUnit.Framework;

namespace ZeroInstall.Services.Solvers
{
    /// <summary>
    /// Provides NUnit <see cref="TestCaseData"/> for <see cref="ISolver"/> test cases.
    /// </summary>
    public static class SolverTestCases
    {
        /// <summary>
        /// Test cases loaded from an embedded XML file.
        /// </summary>
        public static IEnumerable<TestCaseData> Xml
        {
            get
            {
                var testCaseSet = XmlStorage.LoadXml<TestCaseSet>(typeof(SolverTestCases).GetEmbedded("test-cases.xml"));
                foreach (var testCase in testCaseSet.TestCases)
                {
                    var testCaseData = new TestCaseData(testCase.Feeds, testCase.Requirements).Returns(testCase.Selections).SetName(testCase.Name);
                    if (!string.IsNullOrEmpty(testCase.Problem)) testCaseData.Throws(typeof(SolverException));
                    yield return testCaseData;
                }
            }
        }
    }
}
