using System.ComponentModel;
using System.Globalization;
using System.Xml.Serialization;

namespace ZeroInstall.Backend.Model
{
    /// <summary>
    /// A common base class for <see cref="ImplementationBase"/> and <see cref="Feed"/>.
    /// Contains language and architecture parameters.
    /// </summary>
    public abstract class ReleaseBase
    {
        #region Properties
        /// <summary>
        /// The natural language(s) which an <see cref="Implementation"/> supports, as a space-separated list of languages codes (in the same format as used by the $LANG environment variable).
        /// </summary>
        /// <example>For example, the value "en_GB fr" would be used for a package supporting British English and French.</example>
        [Description("The natural language(s) which an implementation supports, as a space-separated list of languages codes (in the same format as used by the $LANG environment variable).")]
        [XmlAttribute("langs")]
        public string Languages { get; set; }

        /// <summary>
        /// For platform-specific binaries, the platform for which an <see cref="Implementation"/> was compiled, in the form os-cpu. Either the os or cpu part may be *, which will make it available on any OS or CPU. 
        /// </summary>
        /// <remarks>The injector knows that certain platforms are backwards-compatible with others, so binaries with arch="Linux-i486"  will still be available on Linux-i686 machines, for example.</remarks>
        [Description("For platform-specific binaries, the platform for which an implementation was compiled, in the form os-cpu. Either the os or cpu part may be *, which will make it available on any OS or CPU. ")]
        [XmlIgnore]
        public Architecture Architecture { get; set; }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Architecture"/>
        [Browsable(false)]
        [XmlAttribute("arch"), DefaultValue("*-*")]
        public string ArchitectureString
        {
            get { return Architecture.ToString(); }
            set { Architecture = new Architecture(value); }
        }
        #endregion

        #region Checks
        /// <summary>
        /// Checks whether a specific <paramref name="language"/> is covered by the <see cref="Languages"/> list.
        /// </summary>
        public bool ContainsLanguage(CultureInfo language)
        {
            // ToDo: Implement
            return false;
        }

        /// <summary>
        /// Checks whether a specific <paramref name="architecture"/> is covered by the <see cref="Architecture"/> (possibly with wildcards).
        /// </summary>
        /// <remarks>Comparison is case-sensitive!</remarks>
        public bool ContainsArchitecture(string architecture)
        {
            // ToDo: Implement
            return false;
        }
        #endregion
    }
}
