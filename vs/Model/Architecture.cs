/*
 * Copyright 2010 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.ComponentModel;
using System.Xml.Serialization;
using ZeroInstall.Model.Design;
using ZeroInstall.Model.Properties;

namespace ZeroInstall.Model
{
    #region Enumerations
    /// <summary>
    /// Describes an operating system family.
    /// </summary>
    public enum OS
    {
        /// <summary>Supports all operating systems (e.g. developed with cross-platform language like Java).</summary>
        [XmlEnum("*")] All,

        /// <summary>Supports only Linux operating systems.</summary>
        [XmlEnum("Linux")] Linux,

        /// <summary>Supports only Solaris / OpenSolaris.</summary>
        [XmlEnum("Solaris")] Solaris,

        /// <summary>Supports only MacOS X.</summary>
        [XmlEnum("MacOSX")] MacOsX,

        /// <summary>Supports only Windows NT 5.0+ (Windows 2000, XP, 2003, Vista, 2008, 7, 2008 R2, ...).</summary>
        [XmlEnum("Windows")] Windows,

        /// <summary>The supported operating system has not been set yet.</summary>
        [XmlEnum("unknown")] Unknown = 99,
    }

    /// <summary>
    /// Describes a CPU architecture.
    /// </summary>
    public enum Cpu
    {
        /// <summary>Supports all CPU architectures (e.g. developed with cross-platform language like Java).</summary>
        [XmlEnum("*")] All,

        /// <summary>Supports CPUs with the i386 architecture or newer (up to i686).</summary>
        [XmlEnum("i386")] I386,

        /// <summary>Supports CPUs with the i486 architecture or newer (up to i686).</summary>
        [XmlEnum("i486")] I486,

        /// <summary>Supports CPUs with the i586 architecture or newer (up to i686).</summary>
        [XmlEnum("i586")] I586,

        /// <summary>Supports CPUs with the i686.</summary>
        [XmlEnum("i686")] I686,

        /// <summary>Requires a x86-64 capable CPU.</summary>
        [XmlEnum("x86_64")] X64,

        /// <summary>Supports CPUs with the PowerPC-architecture (used in older Macs).</summary>
        [XmlEnum("ppc")] Ppc,

        /// <summary>Requires a 64-bit capable PowerPC CPU.</summary>
        [XmlEnum("ppc64")] Ppc64,

        /// <summary>This is a source release and therefor architecture-independent.</summary>
        [XmlEnum("src")] Source,

        /// <summary>The supported CPU architecture has not been set yet.</summary>
        [XmlEnum("unknown")]
        Unknown = 99
    }
    #endregion

    /// <summary>
    /// Describes a combination of an operating system and a CPU architecture.
    /// </summary>
    [Serializable]
    [TypeConverter(typeof(ArchitectureConverter))]
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

        #region Parsers
        /// <summary>
        /// Parses a string as an operating system identifier.
        /// </summary>
        /// <param name="value">The case-sensitive string to parse.</param>
        /// <returns>The identified operating system or <see cref="Model.OS.Unknown"/>.</returns>
        public static OS ParseOS(string value)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException("value");
            #endregion

            OS os;
            switch (value)
            {
                case "*": os = OS.All; break;
                case "Linux": os = OS.Linux; break;
                case "Solaris": os = OS.Solaris; break;
                case "MacOSX": os = OS.MacOsX; break;
                case "Windows": os = OS.Windows; break;
                default: os = OS.Unknown; break;
            }
            return os;
        }

        /// <summary>
        /// Parses a string as a CPU identifier.
        /// </summary>
        /// <param name="value">The case-sensitive string to parse.</param>
        /// <returns>The identified CPU or <see cref="Model.Cpu.Unknown"/>.</returns>
        public static Cpu ParseCpu(string value)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException("value");
            #endregion

            Cpu cpu;
            switch (value)
            {
                case "*": cpu = Cpu.All; break;
                case "i386": cpu = Cpu.I386; break;
                case "i486": cpu = Cpu.I486; break;
                case "i586": cpu = Cpu.I586; break;
                case "i686": cpu = Cpu.I686; break;
                case "x86_64": cpu = Cpu.X64; break;
                case "ppc": cpu = Cpu.Ppc; break;
                case "ppc64": cpu = Cpu.Ppc64; break;
                case "src": cpu = Cpu.Source; break;
                default: cpu = Cpu.Unknown; break;
            }
            return cpu;
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new architecture structure from a string in the form "os-cpu".
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
            try
            {
                OS = ParseOS(os);
                Cpu = ParseCpu(cpu);
            }
            catch (ArgumentNullException ex)
            {
                throw new ArgumentException(Resources.ArchitectureStringFormat, architecture, ex);
            }
        }

        /// <summary>
        /// Creates a new architecture structure with pre-set values.
        /// </summary>
        /// <param name="os">Determines which operating systems are supported.</param>
        /// <param name="cpu">Determines which CPU-architectures are supported.</param>
        public Architecture(OS os, Cpu cpu) : this()
        {
            OS = os;
            Cpu = cpu;
        }
        #endregion

        //--------------------//

        #region Support
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

            // Handle x86-series backwards-compatibility
            if ((Cpu >= Cpu.I386 && Cpu <= Cpu.X64) && (cpu >= Cpu.I386 && cpu <= Cpu.X64) && Cpu <= cpu) return true;

            // Fail if nothing fits
            return false;
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the architecture in the form "os-cpu". Safe for parsing.
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

        #region Equality
        /// <inheritdoc/>
        public bool Equals(Architecture other)
        {
            return other.OS == OS && other.Cpu == Cpu;
        }

        /// <inheritdoc/>
        public static bool operator ==(Architecture left, Architecture right)
        {
            return left.Equals(right);
        }

        /// <inheritdoc/>
        public static bool operator !=(Architecture left, Architecture right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj.GetType() == typeof(Architecture) && Equals((Architecture)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (OS.GetHashCode() * 397) ^ Cpu.GetHashCode();
            }
        }
        #endregion
    }
}
