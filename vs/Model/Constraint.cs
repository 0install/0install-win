using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace ZeroInstall.Model
{
    /// <summary>
    /// Restrict the set of versions from which the injector may choose an <see cref="Implementation"/>. 
    /// </summary>
    public struct Constraint : IEquatable<Constraint>
    {
        #region Properties
        /// <summary>
        /// This is the lowest-numbered version that can be chosen.
        /// </summary>
        [Description("This is the lowest-numbered version that can be chosen.")]
        [XmlAttribute("not-before")]
        public string NotBeforeVersion { get; set; }

        /// <summary>
        /// This version and all later versions are unsuitable.
        /// </summary>
        [Description("This version and all later versions are unsuitable.")]
        [XmlAttribute("before")]
        public string BeforeVersion { get; set; }
        #endregion

        //--------------------//

        #region Conversion
        public override string ToString()
        {
            return string.Format("{0}  =< Ver < {1}", NotBeforeVersion, BeforeVersion);
        }
        #endregion

        #region Compare
        public bool Equals(Constraint other)
        {
            return other.NotBeforeVersion == NotBeforeVersion && other.BeforeVersion == BeforeVersion;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            return obj.GetType() == typeof(Constraint) && Equals((Constraint)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((NotBeforeVersion != null ? NotBeforeVersion.GetHashCode() : 0) * 397) ^ (BeforeVersion != null ? BeforeVersion.GetHashCode() : 0);
            }
        }

        public static bool operator ==(Constraint left, Constraint right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Constraint left, Constraint right)
        {
            return !left.Equals(right);
        }
        #endregion
    }
}
