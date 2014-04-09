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
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Xml.Serialization;
using NanoByte.Common.Utils;

namespace NanoByte.Common.Info
{
    /// <summary>
    /// Wraps information about an application in a serializer-friendly format.
    /// </summary>
    [XmlType("application")]
    public struct AppInfo
    {
        /// <summary>
        /// The name of the application.
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// The version number of the application.
        /// </summary>
        [XmlIgnore]
        public Version Version;

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Version"/>
        [XmlAttribute("version"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string VersionString { get { return (Version == null ? null : Version.ToString()); } set { Version = string.IsNullOrEmpty(value) ? null : new Version(value); } }

        /// <summary>
        /// The copyright information for the application.
        /// </summary>
        [XmlIgnore]
        public string Copyright { get; set; }

        /// <summary>
        /// A description of the application.
        /// </summary>
        [XmlIgnore]
        public string Description { get; set; }

        /// <summary>
        /// The command-line arguments the application was started with.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Used for XML serialization.")]
        [XmlElement("arg")]
        public string[] Arguments { get; set; }

        #region Load
        private static readonly AppInfo _current = Load();

        /// <summary>
        /// Information about the currently running application.
        /// </summary>
        public static AppInfo Current { get { return _current; } }

        /// <summary>
        /// Loads application information for the currently running application.
        /// </summary>
        /// <returns></returns>
        private static AppInfo Load()
        {
            var appInfo = Load(Assembly.GetEntryAssembly());
            appInfo.Arguments = Environment.GetCommandLineArgs();
            return appInfo;
        }

        /// <summary>
        /// Loads application information for a specific <see cref="Assembly"/>.
        /// </summary>
        public static AppInfo Load(Assembly assembly)
        {
            if (assembly == null) return new AppInfo();

            var assemblyInfo = assembly.GetName();
            return new AppInfo
            {
                Name = assembly.GetAttributeValue((AssemblyTitleAttribute x) => x.Title) ?? assemblyInfo.Name,
                Version = new Version(assemblyInfo.Version.Major, assemblyInfo.Version.Minor, assemblyInfo.Version.Build),
                Description = assembly.GetAttributeValue((AssemblyDescriptionAttribute x) => x.Description),
                Copyright = assembly.GetAttributeValue((AssemblyCopyrightAttribute x) => x.Copyright)
            };
        }
        #endregion
    }
}
