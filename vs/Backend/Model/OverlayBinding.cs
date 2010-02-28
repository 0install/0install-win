using System.ComponentModel;
using System.Xml.Serialization;

namespace ZeroInstall.Backend.Model
{
    /// <summary>
    /// An overlay binding specifies that the chosen <see cref="Implementation"/> should be made available at the given location in the filesystem.
    /// </summary>
    public sealed class OverlayBinding : Binding
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
    }
}
