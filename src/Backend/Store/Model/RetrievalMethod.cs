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
using System.Xml.Serialization;
using JetBrains.Annotations;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// A retrieval method is a way of getting a copy of an <see cref="Store.Model.Implementation"/>.
    /// </summary>
    [XmlType("retrieval-method", Namespace = Feed.XmlNamespace)]
    public abstract class RetrievalMethod : FeedElement, ICloneable
    {
        /// <summary>
        /// Sets missing default values and handles legacy elements.
        /// </summary>
        /// <param name="feedUri">The feed the data was originally loaded from.</param>
        /// <exception cref="UriFormatException"><see cref="DownloadRetrievalMethod.Href"/> is relative and <paramref name="feedUri"/> is a remote URI.</exception>
        /// <remarks>This method should be called to prepare a <see cref="Feed"/> for solver processing. Do not call it if you plan on serializing the feed again since it may loose some of its structure.</remarks>
        public virtual void Normalize([NotNull] FeedUri feedUri)
        {}

        /// <summary>
        /// Creates a deep copy of this <see cref="RetrievalMethod"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="RetrievalMethod"/>.</returns>
        public abstract RetrievalMethod Clone();

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
