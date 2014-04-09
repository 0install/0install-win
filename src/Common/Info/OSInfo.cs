/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.ComponentModel;
using System.Xml.Serialization;
using NanoByte.Common.Utils;

namespace NanoByte.Common.Info
{
    /// <summary>
    /// Wraps information about an operating system in a serializer-friendly format.
    /// </summary>
    [XmlType("os")]
    public struct OSInfo
    {
        /// <summary>
        /// The operating system platform (e.g. Windows NT).
        /// </summary>
        [XmlAttribute("platform")]
        public PlatformID Platform;

        /// <summary>
        /// True if the operating system is a 64-bit version of Windows.
        /// </summary>
        [XmlAttribute("is64bit")]
        public bool Is64Bit;

        /// <summary>
        /// The version of the operating system (e.g. 6.0 for Vista).
        /// </summary>
        [XmlIgnore]
        public Version Version;

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Version"/>
        [XmlAttribute("version"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string VersionString { get { return (Version == null ? null : Version.ToString()); } set { Version = string.IsNullOrEmpty(value) ? null : new Version(value); } }

        /// <summary>
        /// The service pack level (e.g. "Service Pack 1").
        /// </summary>
        [XmlAttribute("service-pack"), DefaultValue("")]
        public string ServicePack;

        /// <summary>
        /// The version of the operating system (e.g. 6.0 for Vista).
        /// </summary>
        [XmlIgnore]
        public Version FrameworkVersion;

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Version"/>
        [XmlAttribute("framework-version"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string FrameworkVersionString { get { return (FrameworkVersion == null ? null : FrameworkVersion.ToString()); } set { FrameworkVersion = string.IsNullOrEmpty(value) ? null : new Version(value); } }

        public override string ToString()
        {
            return Platform + " " + (Is64Bit ? "64-bit " : "") + Version + " " + ServicePack;
        }

        #region Static
        private static readonly OSInfo _current = Load();

        /// <summary>
        /// Information about the current operating system.
        /// </summary>
        public static OSInfo Current { get { return _current; } }

        private static OSInfo Load()
        {
            return new OSInfo
            {
                Platform = Environment.OSVersion.Platform,
                Is64Bit = WindowsUtils.Is64BitOperatingSystem,
                Version = Environment.OSVersion.Version,
                ServicePack = Environment.OSVersion.ServicePack,
                FrameworkVersion = Environment.Version
            };
        }
        #endregion
    }
}
