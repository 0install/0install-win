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
using System.Xml.Serialization;
using JetBrains.Annotations;
using NanoByte.Common.Storage;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Services.Solvers
{
    /// <summary>
    /// A set of <see cref="TestCase"/>s.
    /// </summary>
    [Serializable, XmlRoot("test-cases", Namespace = Feed.XmlNamespace), XmlType("test-cases", Namespace = Feed.XmlNamespace)]
    [XmlNamespace("xsi", XmlStorage.XsiNamespace)]
    public class TestCaseSet : XmlUnknown
    {
        private readonly List<TestCase> _testCases = new List<TestCase>();

        /// <summary>
        /// A list of input <see cref="Feed"/>s for the solver.
        /// </summary>
        [XmlElement("test", typeof(TestCase), Namespace = Feed.XmlNamespace), NotNull]
        public List<TestCase> TestCases => _testCases;
    }
}