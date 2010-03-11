using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace ZeroInstall.Model
{
    /// <summary>
    /// An additional feed for an <see cref="Interface"/>.
    /// </summary>
    public sealed class FeedReference : TargetBase, IEquatable<FeedReference>
    {
        #region Properties
        /// <summary>
        /// The URL used to locate the feed.
        /// </summary>
        [Description("The URL used to locate the feed.")]
        [XmlIgnore]
        public Uri Target
        { get; set; }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Target"/>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Used for XML serialization")]
        [XmlAttribute("src"), Browsable(false)]
        public String TargetString
        {
            get { return (Target == null ? null : Target.ToString()); }
            set { Target = new Uri(value); }
        }
        #endregion

        //--------------------//

        #region Conversion
        public override string ToString()
        {
            return (string.IsNullOrEmpty(LanguagesString))
                       ? string.Format("{0} ({1})", Target, Architecture)
                       : string.Format("{0} ({1}) ({2})", Target, Architecture, LanguagesString);
        }
        #endregion

        #region Equality
        public bool Equals(FeedReference other)
        {
            if (other == null) return false;
            if (ReferenceEquals(other, this)) return true;
            return other.Target == Target && other.Architecture == Architecture && Languages.IsEqualTo(other.Languages);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(obj, this)) return true;
            return obj.GetType() == typeof(FeedReference) && Equals((FeedReference)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Target != null ? Target.GetHashCode() : 0);
                result = (result * 397) ^ (Target != null ? Target.GetHashCode() : 0);
                return result;
            }
        }
        #endregion
    }
}
