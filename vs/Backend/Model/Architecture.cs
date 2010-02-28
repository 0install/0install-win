using System;
using System.Xml.Serialization;
using ZeroInstall.Backend.Properties;

namespace ZeroInstall.Backend.Model
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
        [XmlEnum("MacOS")] MacOS,
        [XmlEnum("Windows")] Windows,
        [XmlEnum("none")] None
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
        [XmlEnum("none")] None
    }
    #endregion

    /// <summary>
    /// Describes a combination of an operating system and a CPU-architecture.
    /// </summary>
    public struct Architecture
    {
        #region Properties
        /// <summary>
        /// Determines which operating systems are supported.
        /// </summary>
        public OS OS { get; set; }

        /// <summary>
        /// Determines which CPU-architectures are supported.
        /// </summary>
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
                case "MacOS": OS = OS.MacOS; break;
                case "Windows": OS = OS.Windows; break;
                default: OS = OS.None; break;
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
                default: Cpu = Cpu.None; break;
            }
        }
        #endregion

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
                case OS.MacOS: os = "MacOS"; break;
                case OS.Windows: os = "Windows"; break;
                default: os = "none"; break;
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
                default: cpu = "none"; break;
            }

            return os + "-" + cpu;
        }
        #endregion
    }
}
