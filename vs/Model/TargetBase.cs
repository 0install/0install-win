using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Xml.Serialization;
using Common.Collections;

namespace ZeroInstall.Model
{
    /// <summary>
    /// A common base class for <see cref="ImplementationBase"/> and <see cref="FeedReference"/>.
    /// Contains language and architecture parameters.
    /// </summary>
    public abstract class TargetBase
    {
        #region Properties
        private readonly Set<CultureInfo> _languages = new Set<CultureInfo>();
        /// <summary>
        /// The natural language(s) which an <see cref="Implementation"/> supports, as a space-separated list of languages codes (in the same format as used by the $LANG environment variable).
        /// </summary>
        /// <example>For example, the value "en_GB fr" would be used for a package supporting British English and French.</example>
        [Category("Release"), Description("The natural language(s) which an implementation supports, as a space-separated list of languages codes (in the same format as used by the $LANG environment variable).")]
        [XmlIgnore]
        public Set<CultureInfo> Languages { get { return _languages; } }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Architecture"/>
        [XmlAttribute("langs"), Browsable(false)]
        public string LanguagesString
        {
            get
            {
                // Serialize list as string split by spaces
                var output = new StringBuilder();
                foreach (var language in _languages)
                {
                    // .NET uses a hypen while Zero Install uses an underscore as a seperator
                    output.Append(language.ToString().Replace('-', '_') + " ");
                }
                // Return without trailing space
                return output.ToString().TrimEnd();
            }
            set
            {
                _languages.Clear();
                if (string.IsNullOrEmpty(value)) return;

                // Replace language list by parsing input string split by spaces
                foreach (string language in value.Split(' '))
                {
                    // .NET uses a hypen while Zero Install uses an underscore as a seperator
                    _languages.Add(new CultureInfo(language.Replace('_', '-')));
                }
            }
        }

        /// <summary>
        /// For platform-specific binaries, the platform for which an <see cref="Implementation"/> was compiled, in the form os-cpu. Either the os or cpu part may be *, which will make it available on any OS or CPU. 
        /// </summary>
        /// <remarks>The injector knows that certain platforms are backwards-compatible with others, so binaries with arch="Linux-i486"  will still be available on Linux-i686 machines, for example.</remarks>
        [Category("Release"), Description("For platform-specific binaries, the platform for which an implementation was compiled, in the form os-cpu. Either the os or cpu part may be *, which will make it available on any OS or CPU. ")]
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks whether a specific <paramref name="architecture"/> is covered by the <see cref="Architecture"/> (possibly with wildcards).
        /// </summary>
        /// <remarks>Comparison is case-sensitive!</remarks>
        public bool ContainsArchitecture(string architecture)
        {
            // ToDo: Implement
            throw new NotImplementedException();
        }
        #endregion
    }
}
