using System;
using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;
using Common.Collections;

namespace ZeroInstall.Model
{
    /// <summary>
    /// An implementation of an <see cref="Interface"/> provided by a distribution-specific package manager.
    /// </summary>
    /// <remarks>
    /// Unlike a normal <see cref="Implementation"/>, a distribution package does not resolve to a directory.
    /// Any <see cref="Binding"/>s inside <see cref="Dependency"/>s for the <see cref="Interface"/> will be ignored; it is assumed that the requiring component knows how to use the packaged version without further help.
    /// Therefore, adding<see cref="PackageImplementation"/>s to your <see cref="Interface"/> considerably weakens the guarantees you are making about what the requestor may get. 
    /// </remarks>
    public sealed class PackageImplementation : ImplementationBase
    {
        #region Override Properties
        /// <summary>
        /// The version number as provided by the operating system.
        /// </summary>
        [XmlIgnore, Browsable(false)]
        public override string Version
        {
            get
            {
                // ToDo: Get from OS
                return null;
            }
            set {}
        }

        /// <summary>
        /// The version number as provided by the operating system.
        /// </summary>
        [XmlIgnore, Browsable(false)]
        public override DateTime Released
        {
            get
            {
                // ToDo: Get from OS
                return new DateTime();
            }
            set {}
        }

        /// <summary>Not used.</summary>
        [XmlIgnore, Browsable(false)]
        public override string ReleasedString
        {
            set {}
        }

        /// <summary>
        /// The default stability rating for all <see cref="PackageImplementation"/>s is always "packaged".
        /// </summary>
        [XmlIgnore, Browsable(false)]
        public override Stability Stability
        {
            get { return Stability.Unset; }
            set {}
        }
        #endregion

        #region Properties
        /// <summary>
        /// The name of the package in the distribution-specific package manager.
        /// </summary>
        [Category("Identity"), Description("The name of the package in the distribution-specific package manager.")]
        [XmlAttribute("package")]
        public string Package { get; set; }

        private readonly Set<string> _distributions = new Set<string>();
        /// <summary>
        /// A space-separated list of distribution names where <see cref="Package"/> applies.
        /// </summary>
        [Category("Identity"), Description("A space-separated list of distribution names where the package name applies.")]
        [XmlIgnore]
        public Set<string> Distributions { get { return _distributions; } }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Version"/>
        [XmlAttribute("distributions"), Browsable(false)]
        public string DistributionsString
        {
            get
            {
                // Serialize list as string split by spaces
                var output = new StringBuilder();
                foreach (var distribution in _distributions) output.Append(distribution.Replace(' ', '_') + ' ');

                // Return without trailing space
                return output.ToString().TrimEnd();
            }
            set
            {
                _distributions.Clear();
                if (string.IsNullOrEmpty(value)) return;

                // Replace list by parsing input string split by spaces
                foreach (string distribution in value.Split(' ')) _distributions.Add(distribution);
            }
        }
        #endregion

        //--------------------//

        #region Simplify
        /// <summary>
        /// Sets missing default values.
        /// </summary>
        /// <remarks>This should be called to prepare an interface for launch.
        /// It should not be called if you plan on serializing the interface again since it will may some of its structure.</remarks>
        public override void Simplify()
        {
            // Transfer the version modifier to the normal version attribute
            if (!string.IsNullOrEmpty(VersionModifier))
            {
                Version += VersionModifier;
                VersionModifier = null;
            }
        }
        #endregion

        //--------------------//

        #region Conversion
        public override string ToString()
        {
            return string.Format("{0} ({1})", Package, DistributionsString);
        }
        #endregion

        // ToDo: Implement Equals
    }
}
