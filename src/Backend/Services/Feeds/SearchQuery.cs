/*
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

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Net;
using NanoByte.Common.Storage;
using ZeroInstall.Store;

namespace ZeroInstall.Services.Feeds
{
    /// <summary>
    /// Performs a feed search query and stores the response.
    /// </summary>
    [Serializable]
    [XmlRoot("results"), XmlType("results")]
    public class SearchQuery
    {
        /// <summary>
        /// The keywords the search was performed for.
        /// </summary>
        [XmlIgnore]
        [CanBeNull]
        public string Keywords { get; private set; }

        private readonly List<SearchResult> _results = new List<SearchResult>();

        /// <summary>
        /// A list of results matching the <see cref="Keywords"/>.
        /// </summary>
        [XmlElement("result")]
        [NotNull, ItemNotNull]
        public List<SearchResult> Results { get { return _results; } }

        /// <summary>
        /// Performs a feed search query using the <see cref="Config.FeedMirror"/>.
        /// </summary>
        /// <param name="config">The current configuration determining which mirror server to query.</param>
        /// <param name="keywords">The keywords to search for.</param>
        public static SearchQuery Perform([NotNull] Config config, [CanBeNull] string keywords)
        {
            #region Sanity checks
            if (config == null) throw new ArgumentNullException("config");
            #endregion

            if (string.IsNullOrEmpty(keywords)) return new SearchQuery();

            var url = new Uri(
                config.FeedMirror.EnsureTrailingSlash(),
                new Uri("search/?q=" + Uri.EscapeUriString(keywords), UriKind.Relative));

            Log.Info("Performing search query: " + url.ToStringRfc());
            using (var webClient = new WebClientTimeout())
            {
                var result = XmlStorage.FromXmlString<SearchQuery>(webClient.DownloadString(url));
                result.Keywords = keywords;
                return result;
            }
        }

        /// <summary>
        /// Returns the query in the form "Feed search: Keywords". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return "Feed search: " + Keywords;
        }
    }
}
