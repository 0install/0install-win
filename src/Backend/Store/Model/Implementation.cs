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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Collections;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// An implementation is a specific version of an application that can be downloaded and executed (e.g. Firefox 3.6 for Windows).
    /// </summary>
    /// <seealso cref="Feed.Elements"/>
    [Description("An implementation is a specific version of an application that can be downloaded and executed (e.g. Firefox 3.6 for Windows).")]
    [Serializable, XmlRoot("implementation", Namespace = Feed.XmlNamespace), XmlType("implementation", Namespace = Feed.XmlNamespace)]
    public class Implementation : ImplementationBase, IEquatable<Implementation>
    {
        private readonly List<RetrievalMethod> _retrievalMethods = new List<RetrievalMethod>();

        /// <summary>
        /// A list of <see cref="Archive"/>s as <see cref="RetrievalMethod"/>s.
        /// </summary>
        [Browsable(false)]
        [XmlElement(typeof(Archive)), XmlElement(typeof(SingleFile)), XmlElement(typeof(Recipe))]
        [NotNull]
        public List<RetrievalMethod> RetrievalMethods { get { return _retrievalMethods; } }

        #region Normalize
        /// <summary>
        /// Sets missing default values and handles legacy elements.
        /// </summary>
        /// <param name="feedUri">The feed the data was originally loaded from.</param>
        /// <remarks>This method should be called to prepare a <see cref="Feed"/> for solver processing. Do not call it if you plan on serializing the feed again since it may loose some of its structure.</remarks>
        public override void Normalize(FeedUri feedUri)
        {
            #region Sanity checks
            if (feedUri == null) throw new ArgumentNullException("feedUri");
            #endregion

            base.Normalize(feedUri);

            // Apply if-0install-version filter
            RetrievalMethods.RemoveAll(FilterMismatch);

            var toRemove = new List<RetrievalMethod>();
            foreach (var retrievalMethod in RetrievalMethods)
            {
                try
                {
                    retrievalMethod.Normalize(feedUri);
                }
                    #region Error handling
                catch (UriFormatException ex)
                {
                    Log.Error(ex);
                    toRemove.Add(retrievalMethod);
                }
                #endregion
            }
            RetrievalMethods.RemoveRange(toRemove);
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Implementation"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Implementation"/>.</returns>
        public Implementation CloneImplementation()
        {
            var implementation = new Implementation();
            CloneFromTo(this, implementation);
            implementation.RetrievalMethods.AddRange(RetrievalMethods.CloneElements());
            return implementation;
        }

        /// <summary>
        /// Creates a deep copy of this <see cref="Implementation"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Implementation"/>.</returns>
        public override Element Clone()
        {
            return CloneImplementation();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(Implementation other)
        {
            if (other == null) return false;
            return base.Equals(other) && RetrievalMethods.SequencedEquals(other.RetrievalMethods);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(Implementation)) return false;
            return Equals((Implementation)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ RetrievalMethods.GetSequencedHashCode();
            }
        }
        #endregion
    }
}
