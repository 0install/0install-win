using System.ComponentModel;
using System.Xml.Serialization;

namespace ZeroInstall.Model
{
    /// <summary>
    /// An implementation of an <see cref="Interface"/> provided by a distribution-specific package manager.
    /// </summary>
    /// <remarks>
    /// Unlike a normal implementation, a distribution package does not resolve to a directory.
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
        /// The default stability rating for all <see cref="PackageImplementation"/>s is always "packaged".
        /// </summary>
        [XmlIgnore, Browsable(false)]
        public override Stability Stability
        {
            get { return Stability.Packaged; }
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

        /// <summary>
        /// A space-separated list of distribution names where <see cref="Package"/> applies.
        /// </summary>
        [Category("Identity"), Description("A space-separated list of distribution names where the package name applies.")]
        [XmlAttribute("distributions")]
        public string Distributions { get; set; }
        #endregion

        //--------------------//

        #region Checks
        /// <summary>
        /// Checks whether a specific <paramref name="distribution"/> is covered by the <see cref="Distributions"/> list.
        /// </summary>
        /// <remarks>Comparison is case-sensitive!</remarks>
        public bool ContainsDistribution(string distribution)
        {
            var distributionArray = Distributions.Split(new[] { ' ' });
            for (int i = 0; i < distributionArray.Length; i++)
            {
                if (distributionArray[i] == distribution) return true;
            }

            return false;
        }
        #endregion

        // ToDo: Implement Equals and ToString
    }
}
