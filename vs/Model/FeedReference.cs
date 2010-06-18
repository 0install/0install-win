/*
 * Copyright 2010 Bastian Eicher
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
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace ZeroInstall.Model
{
    /// <summary>
    /// An additional feed for an <see cref="Feed"/>.
    /// </summary>
    public sealed class FeedReference : TargetBase, ICloneable, IEquatable<FeedReference>
    {
        #region Properties
        /// <summary>
        /// The URL used to locate the feed.
        /// </summary>
        [Description("The URL used to locate the feed.")]
        [XmlIgnore]
        public Uri Source
        { get; set; }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Source"/>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Used for XML serialization")]
        [XmlAttribute("src"), Browsable(false)]
        public String SourceString
        {
            get { return (Source == null ? null : Source.ToString()); }
            set { Source = (value == null ? null : new Uri(value)); }
        }
        #endregion

        //--------------------//

        #region Conversion
        public override string ToString()
        {
            return (string.IsNullOrEmpty(LanguagesString))
                       ? string.Format("{0} ({1})", Source, Architecture)
                       : string.Format("{0} ({1}) ({2})", Source, Architecture, LanguagesString);
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="FeedReference"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="FeedReference"/>.</returns>
        public FeedReference CloneFeedReference()
        {
            var feedRereference = new FeedReference { Source = Source };
            CloneFromTo(this, feedRereference);
            return feedRereference;
        }

        /// <summary>
        /// Creates a deep copy of this <see cref="FeedReference"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="FeedReference"/>.</returns>
        public object Clone()
        {
            return CloneFeedReference();
        }
        #endregion

        #region Equality
        public bool Equals(FeedReference other)
        {
            if (ReferenceEquals(null, other)) return false;

            return base.Equals(other) && other.Source == Source;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(FeedReference) && Equals((FeedReference)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result * 397) ^ (Source != null ? Source.GetHashCode() : 0);
                return result;
            }
        }
        #endregion
    }
}
