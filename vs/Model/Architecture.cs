using System;
using System.ComponentModel;
using System.Xml.Serialization;
using ZeroInstall.Model.Properties;

namespace ZeroInstall.Model
{
    #region Enumerations
    /// <summary>
    /// Describes an operating system.
    /// </summary>
    public enum OS
    {
        [XmlEnum("*")] All,
        [XmlEnum("Linux")] Linux,
        [XmlEnum("Solaris")] Solaris,
        [XmlEnum("MacOSX")] MacOsX,
        [XmlEnum("Windows")] Windows,
        [XmlEnum("unknown")] Unknown
    }

    /// <summary>
    /// Describes a CPU-architecture.
    /// </summary>
    public enum Cpu
    {
        [XmlEnum("*")] All,
        [XmlEnum("i386")] I386,
        [XmlEnum("i486")] I486,
        [XmlEnum("i586")] I586,
        [XmlEnum("i686")] I686,
        [XmlEnum("x86_64")] X64,
        [XmlEnum("ppc")] Ppc,
        [XmlEnum("ppc64")] Ppc64,
        [XmlEnum("src")] Source,
        [XmlEnum("unknown")] Unknown
    }
    #endregion

    /// <summary>
    /// Describes a combination of an operating system and a CPU-architecture.
    /// </summary>
    public struct Architecture : IEquatable<Architecture>
    {
        #region Properties
        /// <summary>
        /// Determines which operating systems are supported.
        /// </summary>
        [Description("Determines which operating systems are supported.")]
        public OS OS { get; set; }

        /// <summary>
        /// Determines which CPU-architectures are supported.
        /// </summary>
        [Description("Determines which CPU-architectures are supported.")]
        public Cpu Cpu { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates an architecture from a string in the form "os-cpu".
        /// </summary>
        public Architecture(string architecture) : this()
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(architecture)) throw new ArgumentNullException("architecture");
            #endregion

            // Split string
            var architectureArray = architecture.Split(new[] {'-'});
            if (architectureArray.Length != 2) throw new ArgumentException(Resources.ArchitectureStringFormat, "architecture");
            string os = architectureArray[0];
            string cpu = architectureArray[1];

            // Parse string as enumeration and gracefully handle unkown entries as unsupported platforms
            switch (os)
            {
                case "*": OS = OS.All; break;
                case "Linux": OS = OS.Linux; break;
                case "Solaris": OS = OS.Solaris; break;
                case "MacOSX": OS = OS.MacOsX; break;
                case "Windows": OS = OS.Windows; break;
                default: OS = OS.Unknown; break;
            }
            switch (cpu)
            {
                case "*": Cpu = Cpu.All; break;
                case "i386": Cpu = Cpu.I386; break;
                case "i486": Cpu = Cpu.I486; break;
                case "i586": Cpu = Cpu.I586; break;
                case "i686": Cpu = Cpu.I686; break;
                case "x86_64": Cpu = Cpu.X64; break;
                case "ppc": Cpu = Cpu.Ppc; break;
                case "ppc64": Cpu = Cpu.Ppc64; break;
                case "src": Cpu = Cpu.Source; break;
                default: Cpu = Cpu.Unknown; break;
            }
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the architecture in the form "os-cpu".
        /// </summary>
        public override string ToString()
        {
            string os, cpu;

            // Convert enumeration to string
            switch (OS)
            {
                case OS.All: os = "*"; break;
                case OS.Linux: os = "Linux"; break;
                case OS.Solaris: os = "Solaris"; break;
                case OS.MacOsX: os = "MacOSX"; break;
                case OS.Windows: os = "Windows"; break;
                default: os = "unknown"; break;
            }
            switch (Cpu)
            {
                case Cpu.All: cpu = "*"; break;
                case Cpu.I386: cpu = "i386"; break;
                case Cpu.I486: cpu = "i486"; break;
                case Cpu.I586: cpu = "i586"; break;
                case Cpu.I686: cpu = "i686"; break;
                case Cpu.X64: cpu = "x86_64"; break;
                case Cpu.Ppc: cpu = "ppc"; break;
                case Cpu.Ppc64: cpu = "ppc64"; break;
                case Cpu.Source: cpu = "src"; break;
                default: cpu = "unknown"; break;
            }

            return os + "-" + cpu;
        }
        #endregion

        #region Compare
        public bool Equals(Architecture other)
        {
            return other.OS == OS && other.Cpu == Cpu;
        }

        public static bool operator ==(Architecture left, Architecture right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Architecture left, Architecture right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj.GetType() == typeof(Architecture) && Equals((Architecture)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (OS.GetHashCode() * 397) ^ Cpu.GetHashCode();
            }
        }
        #endregion

        #region Coverage
        /// <summary>
        /// Checks whether an <see cref="Implementation"/> that specifies this <see cref="Architecture"/> will be able to run on a specific system.
        /// </summary>
        /// <param name="os">The operating system of the system to test. Must be a specific operating system and not a wildcard.</param>
        /// <param name="cpu">The CPU of the system to test. Must be a specific CPU and not a wildcard.</param>
        /// <returns><see langword="true"/> if the system is supported; <see langword="false"/> otherwise.</returns>
        public bool Supports(OS os, Cpu cpu)
        {
            #region Sanity checks
            if (os == OS.All || os == OS.Unknown) throw new ArgumentException(Resources.MustBeSpecificOS, "os");
            if (cpu == Cpu.All || cpu == Cpu.Unknown) throw new ArgumentException(Resources.MustBeSpecificCPU, "cpu");
            #endregion

            // Fail if OS is neither a wildcard nor identical
            if (OS != OS.All && OS != os) return false;

            // Pass if CPU is either a wildcard or identical
            if (Cpu == Cpu.All || Cpu == cpu) return true;

            // Handle feature inheritance in x86 hierachy
            if (cpu >= Cpu.I386 && cpu <= Cpu.X64 && cpu <= Cpu) return true;

            // Fail if nothing fits
            return false;
        }
        #endregion
    }
}
