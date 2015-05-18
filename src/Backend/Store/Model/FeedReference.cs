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
using System.ComponentModel;
using System.Xml.Serialization;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// A linked feed that contains more implementations of this feed's interface. Is treated by the solver as if it were part of the main feed.
    /// </summary>
    /// <seealso cref="Feed.Feeds"/>
    [Description("A linked feed that contains more implementations of this feed's interface. Is treated by the solver as if it were part of the main feed.")]
    [Serializable]
    [XmlRoot("feed-reference", Namespace = Feed.XmlNamespace), XmlType("feed-reference", Namespace = Feed.XmlNamespace)]
    public sealed class FeedReference : TargetBase, ICloneable, IEquatable<FeedReference>
    {
        /// <summary>
        /// The URL or local path used to locate the feed.
        /// </summary>
        [Description("The URL or local path used to locate the feed.")]
        [XmlIgnore]
        public FeedUri Source { get; set; }

        #region XML serialization
        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Source"/>
        [XmlAttribute("src"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public string SourceString { get { return (Source == null) ? null : Source.ToStringRfc(); } set { Source = (value == null) ? null : new FeedUri(value); } }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the feed reference in the form "Source (Architecture, Languages)". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return Languages.Count == 0
                ? string.Format("{0} ({1})", Source, Architecture)
                : string.Format("{0} ({1}, {2})", Source, Architecture, Languages);
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="FeedReference"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="FeedReference"/>.</returns>
        public FeedReference Clone()
        {
            var feedRereference = new FeedReference {Source = Source};
            CloneFromTo(this, feedRereference);
            return feedRereference;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(FeedReference other)
        {
            if (other == null) return false;
            return base.Equals(other) && other.Source == Source;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is FeedReference && Equals((FeedReference)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                if (Source != null) result = (result * 397) ^ Source.GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
