/*
 * Copyright 2010-2015 Bastian Eicher
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
using JetBrains.Annotations;
using NanoByte.Common.Native;
using NanoByte.Common.Values;
using NanoByte.Common.Values.Design;
using ZeroInstall.Store.Model.Design;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Model
{

    #region Enumerations
    /// <summary>
    /// Describes an operating system family.
    /// </summary>
    [TypeConverter(typeof(EnumXmlConverter<OS>))]
    public enum OS
    {
        /// <summary>Supports all operating systems (e.g. developed with cross-platform language like Java).</summary>
        [XmlEnum("*")]
        All,

        /// <summary>Supports only Linux operating systems.</summary>
        [XmlEnum("Linux")]
        Linux,

        /// <summary>Supports only Solaris.</summary>
        [XmlEnum("Solaris")]
        Solaris,

        /// <summary>Supports only FreeBSD.</summary>
        [XmlEnum("FreeBSD")]
        FreeBsd,

        /// <summary>Supports only MacOS X.</summary>
        [XmlEnum("MacOSX")]
        MacOSX,

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
        Unknown = 99
    }

    /// <summary>
    /// Describes a CPU architecture.
    /// </summary>
    [TypeConverter(typeof(EnumXmlConverter<Cpu>))]
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
    [Description("Describes a combination of an operating system and a CPU architecture.")]
    [TypeConverter(typeof(ArchitectureConverter))]
    [Serializable]
    public struct Architecture : IEquatable<Architecture>
    {
        #region Constants
        /// <summary>A list of all known <see cref="OS"/> values.</summary>
        public static readonly OS[] KnownOS = {OS.All, OS.Linux, OS.Solaris, OS.FreeBsd, OS.MacOSX, OS.Darwin, OS.Windows, OS.Cygwin};

        /// <summary>A list of all known <see cref="Cpu"/> values, except for <see cref="Store.Model.Cpu.Source"/>.</summary>
        public static readonly Cpu[] KnownCpu = {Cpu.All, Cpu.I386, Cpu.I486, Cpu.I586, Cpu.I686, Cpu.X64, Cpu.Ppc, Cpu.Ppc64};
        #endregion

        /// <summary>
        /// Determines which operating systems are supported.
        /// </summary>
        [Description("Determines which operating systems are supported.")]
        [UsedImplicitly]
        public OS OS { get; set; }

        /// <summary>
        /// Determines which CPU-architectures are supported.
        /// </summary>
        [Description("Determines which CPU-architectures are supported.")]
        [UsedImplicitly]
        public Cpu Cpu { get; set; }

        /// <summary>
        /// An architecture representing the currently running system.
        /// </summary>
        public static readonly Architecture CurrentSystem = GetCurrentSystem();

        private static Architecture GetCurrentSystem()
        {
            if (WindowsUtils.IsWindows) return new Architecture(OS.Windows, WindowsUtils.Is64BitOperatingSystem ? Cpu.X64 : Cpu.I586);
            else if (UnixUtils.IsUnix) return new Architecture(UnixUtils.IsMacOSX ? OS.MacOSX : ParseOSString(UnixUtils.OSName), ParseCpuString(UnixUtils.CpuType));
            else return new Architecture(OS.Unknown, Cpu.Unknown);
        }

        /// <summary>
        /// Creates a new architecture structure from a string in the form "os-cpu".
        /// </summary>
        /// <exception cref="FormatException"><paramref name="architecture"/> is not in the form "os-cpu"</exception>
        public Architecture([NotNull] string architecture) : this()
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(architecture)) throw new ArgumentNullException("architecture");
            #endregion

            var architectureArray = architecture.Split('-');
            if (architectureArray.Length != 2) throw new FormatException(Resources.ArchitectureStringFormat);

            OS = ParseOSString(architectureArray[0]);
            Cpu = ParseCpuString(architectureArray[1]);
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

        #region Parse string
        private static OS ParseOSString(string os)
        {
            //try { return os.ConvertFromString<OS>(); }
            //catch (ArgumentException) { return OS.Unknown; }

            // NOTE: Use hard-coded switch instead of reflection-based code for better performance
            switch (os)
            {
                case "*":
                    return OS.All;
                case "Linux":
                    return OS.Linux;
                case "Solaris":
                    return OS.Solaris;
                case "FreeBSD":
                    return OS.FreeBsd;
                case "MacOSX":
                    return OS.MacOSX;
                case "Darwin":
                    return OS.Darwin;
                case "Cygwin":
                    return OS.Cygwin;
                case "POSIX":
                    return OS.Posix;
                case "Windows":
                    return OS.Windows;
                default:
                    return OS.Unknown;
            }
        }

        private static Cpu ParseCpuString(string cpu)
        {
            //try { return cpu.ConvertFromString<Cpu>(); }
            //catch (ArgumentException) { return Cpu.Unknown; }

            // NOTE: Use hard-coded switch instead of reflection-based code for better performance
            switch (cpu)
            {
                case "*":
                    return Cpu.All;
                case "i386":
                    return Cpu.I386;
                case "i486":
                    return Cpu.I486;
                case "i586":
                    return Cpu.I586;
                case "i686":
                    return Cpu.I686;
                case "x86_64":
                    return Cpu.X64;
                case "ppc":
                    return Cpu.Ppc;
                case "ppc64":
                    return Cpu.Ppc64;
                case "src":
                    return Cpu.Source;
                default:
                    return Cpu.Unknown;
            }
        }
        #endregion

        #region Conversion
        /// <summary>
        /// Returns the architecture in the form "os-cpu". Safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return OS.ConvertToString() + "-" + Cpu.ConvertToString();
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
                return ((int)OS * 397) ^ (int)Cpu;
            }
        }
        #endregion
    }
}
