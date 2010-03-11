using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace ZeroInstall.Model
{
    /// <summary>
    /// An overlay binding specifies that the chosen <see cref="Implementation"/> should be made available at the given location in the filesystem.
    /// </summary>
    public sealed class OverlayBinding : Binding, IEquatable<OverlayBinding>
    {
        #region Properties
        private string _source = ".";
        /// <summary>
        /// The relative path of the directory in the implementation to publish. The default is '.', to publish everything.
        /// </summary>
        [Description("The name of the environment variable.")]
        [XmlAttribute("src"), DefaultValue(".")]
        public string Source { get { return _source; } set { _source = value; } }

        /// <summary>
        /// The mount point on which src is to appear in the filesystem. If missing, '/' (on POSIX) or '%systemdrive%' (on Windows) is assumed.
        /// </summary>
        [Description("The mount point on which src is to appear in the filesystem. If missing, '/' (on POSIX) or '%systemdrive%' (on Windows) is assumed.")]
        [XmlAttribute("mount-point")]
        public string MountPoint { get; set; }
        #endregion

        //--------------------//

        #region Conversion
        public override string ToString()
        {
            return string.Format("{0} => {1}", Source, MountPoint);
        }
        #endregion

        #region Equality
        public bool Equals(OverlayBinding other)
        {
            if (other == null) return false;
            if (ReferenceEquals(other, this)) return true;
            return other.Source == Source || other.MountPoint == MountPoint;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(obj, this)) return true;
            return obj.GetType() == typeof(OverlayBinding) && Equals((OverlayBinding)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Source != null ? Source.GetHashCode() : 0)*397) ^ (MountPoint != null ? MountPoint.GetHashCode() : 0);
            }
        }
        #endregion
    }
}
