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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Serialization;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Collections;
using ZeroInstall.Store;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Services.Feeds
{
    /// <summary>
    /// A single result of a feed search.
    /// </summary>
    [Serializable, XmlType("result")]
    public class SearchResult
    {
        /// <summary>
        /// The URI of the feed.
        /// </summary>
        [XmlIgnore]
        public FeedUri Uri { get; set; }

        #region XML serialization
        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Uri"/>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Used for XML serialization")]
        [Browsable(false)]
        [XmlAttribute("uri"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public string UriString { get => Uri?.ToStringRfc(); set => Uri = (string.IsNullOrEmpty(value) ? null : new FeedUri(value)); }
        #endregion

        /// <summary>
        /// A short name to identify the interface (e.g. "Foo").
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// A value between 0 and 100 indicating how good this result matches the query.
        /// </summary>
        [XmlAttribute("score")]
        public int Score { get; set; }

        /// <summary>
        /// Short one-line description for different languages; the first word should not be upper-case unless it is a proper noun (e.g. "cures all ills").
        /// </summary>
        [XmlElement("summary")]
        public string Summary { get; set; }

        /// <summary>
        /// A list of well-known categories the applications fits into.
        /// </summary>
        [Browsable(false)]
        [XmlElement("category"), NotNull]
        public List<Category> Categories { get; } = new List<Category>();

        /// <summary>Used for DataGrid rendering.</summary>
        /// <seealso cref="Categories"/>
        [XmlIgnore, DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public string CategoriesString { get { return StringUtils.Join(", ", Categories.Select(x => x.Name)); } }

        /// <summary>
        /// Generates a pseudo-<see cref="Feed"/> using the information from this result.
        /// </summary>
        /// <returns>A pseudo-<see cref="Feed"/>; not a complete feed that can be used to launch an implementation.</returns>
        public Feed ToPseudoFeed()
        {
            var feed = new Feed {Uri = Uri, Name = Name, Summaries = {Summary}};
            feed.Categories.AddRange(Categories.CloneElements());
            return feed;
        }

        /// <summary>
        /// Returns the result in the form "Uri NEWLINE Name - Summary [Score]". Not safe for parsing!
        /// </summary>
        public override string ToString() => Uri.ToStringRfc() + Environment.NewLine + $"{Name} - {Summary} [{Score}%]";
    }
}
