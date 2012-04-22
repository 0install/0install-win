/*
 * Copyright 2010-2012 Bastian Eicher
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
using System.Reflection;
using System.Xml.Serialization;
using Common.Utils;
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
        [XmlEnum("*")]
        All,

        /// <summary>Supports only Linux operating systems.</summary>
        [XmlEnum("Linux")]
        Linux,

        /// <summary>Supports only Solaris / OpenSolaris.</summary>
        [XmlEnum("Solaris")]
        Solaris,

        /// <summary>Supports only MacOS X.</summary>
        [XmlEnum("MacOSX")]
        MacOsX,

        /// <summary>MacOSX, without the proprietary bits.</summary>
        [XmlEnum("Darwin")]
        Darwin,

        /// <summary>A Unix-compatibility layer for Windows.</summary>
        [XmlEnum("Cygwin")]
        Cygwin,

        /// <summary>Everthing except <see cref="Windows"/>.</summary>
        [XmlEnum("POSIX")]
        Posix,

        /// <summary>Supports only Windows NT 5.0+ (Windows 2000, XP, 2003, Vista, 2008, 7, 2008 R2, ...).</summary>
        [XmlEnum("Windows")]
        Windows,

        /// <summary>The supported operating system has not been set yet.</summary>
        [XmlEnum("unknown")]
        Unknown = 99,
    }

    /// <summary>
    /// Describes a CPU architecture.
    /// </summary>
    public enum Cpu
    {
        /// <summary>Supports all CPU architectures (e.g. developed with cross-platform language like Java).</summary>
        [XmlEnum("*")]
        All,

        /// <summary>Supports CPUs with the i386 architecture or newer (up to i686).</summary>
        [XmlEnum("i386")]
        I386,

        /// <summary>Supports CPUs with the i486 architecture or newer (up to i686).</summary>
        [XmlEnum("i486")]
        I486,

        /// <summary>Supports CPUs with the i586 architecture or newer (up to i686).</summary>
        [XmlEnum("i586")]
        I586,

        /// <summary>Supports CPUs with the i686.</summary>
        [XmlEnum("i686")]
        I686,

        /// <summary>Requires a x86-64 capable CPU.</summary>
        [XmlEnum("x86_64")]
        X64,

        /// <summary>Supports CPUs with the PowerPC-architecture (used in older Macs).</summary>
        [XmlEnum("ppc")]
        Ppc,

        /// <summary>Requires a 64-bit capable PowerPC CPU.</summary>
        [XmlEnum("ppc64")]
        Ppc64,

        /// <summary>This is a source release and therefore architecture-independent.</summary>
        [XmlEnum("src")]
        Source,

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
        #region Constants
        /// <summary>A list of all known string representations of <see cref="OS"/> values.</summary>
        public static readonly string[] KnownOSStrings = {"*", "Linux", "Solaris", "MacOSX", "Darwin", "Windows", "Cygwin"};

        /// <summary>A list of all known string representations of <see cref="Cpu"/> values, except for <see cref="Model.Cpu.Source"/>.</summary>
        public static readonly string[] KnownCpuStrings = {"*", "i386", "i486", "i586", "i686", "x86_64", "ppc", "ppc64"};
        #endregion

        #region Properties
        /// <summary>
        /// Determines which operating systems are supported.
        /// </summary>
        [Description("Determines which operating systems are supported.")]
        public OS OS { get; set; }

        /// <summary>
        /// The canonical string representation of <see cref="OS"/>.
        /// </summary>
        [Browsable(false)]
        public string OSString { get { return EnumToString(OS); } }

        /// <summary>
        /// Determines which CPU-architectures are supported.
        /// </summary>
        [Description("Determines which CPU-architectures are supported.")]
        public Cpu Cpu { get; set; }

        /// <summary>
        /// The canonical string representation of <see cref="Cpu"/>.
        /// </summary>
        [Browsable(false)]
        public string CpuString { get { return EnumToString(Cpu); } }

        private static string EnumToString(Enum value)
        {
            Type type = value.GetType();
            // ReSharper disable SpecifyACultureInStringConversionExplicitly
            FieldInfo fieldInfo = type.GetField(value.ToString());
            // ReSharper restore SpecifyACultureInStringConversionExplicitly

            // Get the XmlEnum attributes
            var attribs = (XmlEnumAttribute[])fieldInfo.GetCustomAttributes(typeof(XmlEnumAttribute), false);

            // Return the first if there was a match
            return (attribs.Length > 0 ? attribs[0].Name : "");
        }

        /// <summary>
        /// Returns an architecture representing the currently running system.
        /// </summary>
        public static Architecture CurrentSystem
        {
            get
            {
                if (WindowsUtils.IsWindows) return new Architecture(OS.Windows, WindowsUtils.Is64BitOperatingSystem ? Cpu.X64 : Cpu.I486);

                // ToDo: Improve detection of other operating systems and CPUs
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Unix:
                        return new Architecture(OS.Linux, WindowsUtils.Is64BitProcess ? Cpu.X64 : Cpu.I586);
                    case PlatformID.MacOSX:
                        return new Architecture(OS.MacOsX, WindowsUtils.Is64BitProcess ? Cpu.X64 : Cpu.I686);
                    default:
                        return new Architecture(OS.Unknown, Cpu.Unknown);
                }
            }
        }
        #endregion

        #region Parsers
        /// <summary>
        /// Parses a string as an operating system identifier.
        /// </summary>
        /// <param name="value">The case-sensitive string to parse.</param>
        /// <returns>The identified operating system or <see cref="Model.OS.Unknown"/>.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not a known operating system.</exception>
        public static OS ParseOS(string value)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException("value");
            #endregion

            OS os;
            switch (value)
            {
                case "*":
                    os = OS.All;
                    break;
                case "Linux":
                    os = OS.Linux;
                    break;
                case "Solaris":
                    os = OS.Solaris;
                    break;
                case "MacOSX":
                    os = OS.MacOsX;
                    break;
                case "Darwin":
                    os = OS.Darwin;
                    break;
                case "Windows":
                    os = OS.Windows;
                    break;
                case "Cygwin":
                    os = OS.Cygwin;
                    break;
                default:
                    throw new ArgumentException(Resources.UnknownOS);
            }
            return os;
        }

        /// <summary>
        /// Parses a string as a CPU identifier.
        /// </summary>
        /// <param name="value">The case-sensitive string to parse.</param>
        /// <returns>The identified CPU or <see cref="Model.Cpu.Unknown"/>.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not a known CPU.</exception>
        public static Cpu ParseCpu(string value)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException("value");
            #endregion

            Cpu cpu;
            switch (value)
            {
                case "*":
                    cpu = Cpu.All;
                    break;
                case "i386":
                    cpu = Cpu.I386;
                    break;
                case "i486":
                    cpu = Cpu.I486;
                    break;
                case "i586":
                    cpu = Cpu.I586;
                    break;
                case "i686":
                    cpu = Cpu.I686;
                    break;
                case "x86_64":
                    cpu = Cpu.X64;
                    break;
                case "ppc":
                    cpu = Cpu.Ppc;
                    break;
                case "ppc64":
                    cpu = Cpu.Ppc64;
                    break;
                case "src":
                    cpu = Cpu.Source;
                    break;
                default:
                    throw new ArgumentException(Resources.UnknownCpu);
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
            var architectureArray = architecture.Split('-');
            if (architectureArray.Length != 2) throw new ArgumentException(Resources.ArchitectureStringFormat, "architecture");
            string os = architectureArray[0];
            string cpu = architectureArray[1];

            try
            {
                OS = ParseOS(os);
                Cpu = ParseCpu(cpu);
            }
                #region Error handling
            catch (ArgumentNullException ex)
            {
                throw new ArgumentException(Resources.ArchitectureStringFormat, architecture, ex);
            }
            catch (ArgumentException)
            {
                // Gracefully handle unknown entries as unsupported platforms
                OS = OS.Unknown;
                Cpu = Cpu.Unknown;
            }
            #endregion
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

        #region Compatibility
        /// <summary>
        /// Determines whether this implementation architecture (the current instance) can run on a <paramref name="system"/> architecture.
        /// </summary>
        /// <seealso cref="CurrentSystem"/>
        public bool IsCompatible(Architecture system)
        {
            return AreCompatible(OS, system.OS) && AreCompatible(Cpu, system.Cpu);
        }

        /// <summary>
        /// Determines whether an <paramref name="implementation"/> OS is compatible with a <paramref name="system"/> OS.
        /// </summary>
        private static bool AreCompatible(OS implementation, OS system)
        {
            if (implementation == OS.Unknown || system == OS.Unknown) return false;

            // Exact OS match or platform-neutral implementation
            if (implementation == system || implementation == OS.All || system == OS.All) return true;

            // Compatible supersets
            if (implementation == OS.Windows && system == OS.Cygwin) return true;
            if (implementation == OS.Darwin && system == OS.MacOsX) return true;
            if (implementation == OS.Posix && system <= OS.Posix) return true;

            // No match
            return false;
        }

        /// <summary>
        /// Determines whether an <paramref name="implementation"/> CPU is compatible with a <paramref name="system"/> CPU.
        /// </summary>
        private static bool AreCompatible(Cpu implementation, Cpu system)
        {
            if (implementation == Cpu.Unknown || system == Cpu.Unknown) return false;

            // Exact CPU match or platform-neutral implementation
            if (implementation == system || implementation == Cpu.All || system == Cpu.All) return true;

            // Compatible supersets
            if (implementation == Cpu.Ppc && system == Cpu.Ppc64) return true;
            if (implementation >= Cpu.I386 && implementation <= Cpu.X64 && system >= implementation && system <= Cpu.X64) return true;

            // No match
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
            return OSString + "-" + CpuString;
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
            return obj is Architecture && Equals((Architecture)obj);
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
