/*
 * Copyright 2006-2013 Bastian Eicher
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

namespace Common.Info
{
    /// <summary>
    /// Wraps information about the current application in a serializer-friendly format.
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
        /// The copyright information for the entry assembly.
        /// </summary>
        [XmlIgnore]
        public string Copyright { get; set; }

        /// <summary>
        /// The command-line arguments the application was started with.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Used for XML serialization.")]
        [XmlElement("arg")]
        public string[] Arguments { get; set; }

        #region Static
        /// <summary>
        /// Information about the current operating system.
        /// </summary>
        public static AppInfo Current { get; private set;  }

        [SuppressMessage("Microsoft.Usage", "CA2207:InitializeValueTypeStaticFieldsInline")]
        static AppInfo()
        {
            var assembly = Assembly.GetEntryAssembly();
            if (assembly == null) return;
            var assemblyInfo = assembly.GetName();

            // Try to determine assembly title, fall back to assembly name on failure
            var assemblyTitleAttributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            string name = (assemblyTitleAttributes.Length > 0 ? ((AssemblyTitleAttribute)assemblyTitleAttributes[0]).Title : assemblyInfo.Name);
            
            // Try to determine copyright information
            string copyright = null;
            var assemblyCopyrightAttributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            if (assemblyCopyrightAttributes.Length > 0) copyright = ((AssemblyCopyrightAttribute)assemblyCopyrightAttributes[0]).Copyright;

            Current = new AppInfo
            {
                Name = name,
                Version = new Version(assemblyInfo.Version.Major, assemblyInfo.Version.Minor, assemblyInfo.Version.Build),
                Copyright = copyright,
                Arguments = Environment.GetCommandLineArgs()
            };
        }
        #endregion
    }
}