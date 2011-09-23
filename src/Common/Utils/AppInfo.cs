/*
 * Copyright 2006-2011 Bastian Eicher
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
using System.Reflection;

namespace Common.Utils
{
    /// <summary>
    /// Provides information about the main application that is using this library (the entry assembly).
    /// </summary>
    public static class AppInfo
    {
        /// <summary>
        /// The name of the entry assembly.
        /// </summary>
        public static string Name { get; private set; }

        /// <summary>
        /// The version of the entry assembly.
        /// </summary>
        public static Version Version { get; private set; }

        /// <summary>
        /// The copyright information for the entry assembly.
        /// </summary>
        public static string Copyright { get; private set; }

        static AppInfo()
        {
            var assembly = Assembly.GetEntryAssembly();
            if (assembly == null) return;
            var assemblyInfo = assembly.GetName();

            // Try to determine assembly title, fall back to assembly name on failure
            var assemblyTitleAttributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            Name = (assemblyTitleAttributes.Length > 0 ? ((AssemblyTitleAttribute)assemblyTitleAttributes[0]).Title : assemblyInfo.Name);

            // Get version information
            Version = assemblyInfo.Version;

            // Try to determine copyright information
            var assemblyCopyrightAttributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            if (assemblyCopyrightAttributes.Length > 0) Copyright = ((AssemblyCopyrightAttribute)assemblyCopyrightAttributes[0]).Copyright;
        }
    }
}
