using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace ZeroInstall.MyApps
{
    /// <summary>
    /// Stores a list of applications in form of feed URLs.
    /// </summary>
    [XmlRoot("my-apps", Namespace = "http://zero-install.sourceforge.net/2010/my-apps/app-list")]
    // ToDo: Suppress xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema"
    public sealed class AppList
    {
        #region Properties
        private Collection<AppEntry> _applications = new Collection<AppEntry>();
        /// <summary>
        /// The list of application entries.
        /// </summary>
        [XmlElement("app")]
        public Collection<AppEntry> Applications { get { return _applications; } }
        #endregion
    }
}