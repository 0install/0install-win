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
using System.ComponentModel;
using System.Xml.Serialization;
using JetBrains.Annotations;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Services.Solvers
{
    /// <summary>
    /// A single <see cref="ISolver"/> test case.
    /// </summary>
    [Serializable, XmlType("test", Namespace = Feed.XmlNamespace)]
    public class TestCase : XmlUnknown
    {
        private readonly List<Feed> _feeds = new List<Feed>();

        [XmlAttribute("name")]
        public string Name { get; set; }

        [DefaultValue(false), XmlAttribute("add-downloads")]
        public bool AddDownloads { get; set; }

        /// <summary>
        /// A list of input <see cref="Feed"/>s for the solver.
        /// </summary>
        [XmlElement("interface", typeof(Feed), Namespace = Feed.XmlNamespace), NotNull]
        public List<Feed> Feeds { get { return _feeds; } }

        /// <summary>
        /// The input requirements for the solver.
        /// </summary>
        [XmlElement("requirements")]
        public Requirements Requirements { get; set; }

        /// <summary>
        /// The expected output of the solver.
        /// </summary>
        [XmlElement("selections")]
        public Selections Selections { get; set; }

        /// <summary>
        /// A string describing the expected solver error message or <c>null</c> if no failure is expected.
        /// </summary>
        [XmlElement("problem"), CanBeNull]
        public string Problem { get; set; }
    }
}