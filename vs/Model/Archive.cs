using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace ZeroInstall.Model
{
    public sealed class Archive : RetrievalStep
    {
        #region Properties
        /// <summary>
        /// The URL used to locate the archive.
        /// </summary>
        [Description("The URL used to locate the archive.")]
        [XmlIgnore]
        public Uri Location { get; set; }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Location"/>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Used for XML serialization")]
        [XmlAttribute("href"), Browsable(false)]
        public String LocationString
        {
            get { return (Location == null ? null : Location.ToString()); }
            set { Location = new Uri(value); }
        }

        /// <summary>
        /// The size of the archive in bytes. The archive must have the given size or it will be rejected.
        /// </summary>
        [Description("The size of the archive in bytes. The archive must have the given size or it will be rejected.")]
        [XmlAttribute("size")]
        public long Size { get; set; }

        /// <summary>
        /// The name of the subdirectory in the archive to extract; <see langword="null"/> for entire archive.
        /// </summary>
        [Description("The name of the subdirectory in the archive to extract; null for entire archive.")]
        [XmlAttribute("extract")]
        public string Extract { get; set; }

        /// <summary>
        /// The type of the archive as a MIME type in the type attribute. If missing, the type is guessed from the extension on the <see cref="Location"/> attribute.
        /// </summary>
        [Description("The type of the archive as a MIME type in the type attribute. If missing, the type is guessed from the extension on the location attribute.")]
        [XmlAttribute("type")]
        public string MimeType { get; set; }

        /// <summary>
        /// The number of bytes at the beginning of the file which should be ignored. The value in the size attribute does not include the skipped bytes. 
        /// </summary>
        /// <remarks>This is useful for some self-extracting archives which are made up of a shell script followed by a normal archive in a single file.</remarks>
        [Description("The number of bytes at the beginning of the file which should be ignored. The value in the size attribute does not include the skipped bytes.")]
        [XmlAttribute("start-offset"), DefaultValue(0L)]
        public long StartOffset { get; set; }
        #endregion
    }
}
