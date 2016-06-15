using System.Xml.Serialization;

namespace ZeroInstall.Store
{
    /// <summary>
    /// Available operation modes for the Zero Install Bootstrapper.
    /// </summary>
    public enum BootstrapMode
    {
        /// <summary>
        /// Perform no application bootstrapping.
        /// </summary>
        [XmlEnum("none")]
        None,

        /// <summary>
        /// Run the target application.
        /// </summary>
        [XmlEnum("run")]
        Run,

        /// <summary>
        /// Perform desktop integration for the target application.
        /// </summary>
        [XmlEnum("integrate")]
        Integrate
    }
}