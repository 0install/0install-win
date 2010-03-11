using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace ZeroInstall.Model
{
    /// <summary>
    /// An additional feed for an <see cref="Interface"/>.
    /// </summary>
    public class InterfaceReference : IEquatable<InterfaceReference>
    {
        #region Properties
        /// <summary>
        /// The URI used to locate the interface.
        /// </summary>
        [Description("The URI used to locate the interface.")]
        [XmlIgnore]
        public Uri Target
        { get; set; }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Target"/>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Used for XML serialization")]
        [XmlAttribute("interface"), Browsable(false)]
        public String TargetString
        {
            get { return (Target == null ? null : Target.ToString()); }
            set { Target = new Uri(value); }
        }
        #endregion

        //--------------------//

        #region Equality
        public bool Equals(InterfaceReference other)
        {
            if (other == null) return false;
            if (ReferenceEquals(other, this)) return true;
            return other.Target == Target;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(obj, this)) return true;
            return obj.GetType() == typeof(InterfaceReference) && Equals((InterfaceReference)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Target != null ? Target.GetHashCode() : 0);
            }
        }
        #endregion

        #region Conversion
        public override string ToString()
        {
            return TargetString;
        }
        #endregion
    }
}
