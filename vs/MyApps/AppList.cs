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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml.Serialization;
using Common.Storage;

namespace ZeroInstall.MyApps
{
    /// <summary>
    /// Stores a list of applications the user prefers to use.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    [XmlRoot("app-list", Namespace = XmlNamespace)]
    [XmlType("app-list", Namespace = XmlNamespace)]
    public sealed class AppList : ICloneable, IEquatable<AppList>
    {
        #region Constants
        /// <summary>
        /// The XML namespace used for storing application list data.
        /// </summary>
        public const string XmlNamespace = "http://0install.de/schema/my-apps/app-list";
        #endregion

        #region Properties
        // Preserve order
        private readonly C5.ArrayList<AppEntry> _entries = new C5.ArrayList<AppEntry>();
        /// <summary>
        /// A list of <see cref="AppEntry"/>s.
        /// </summary>
        [Description("A list of application entries.")]
        [XmlElement("entry")]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.ArrayList<AppEntry> Entries { get { return _entries; } }
        #endregion

        //--------------------//

        #region Storage
        /// <summary>
        /// Loads an <see cref="AppList"/> from an XML file.
        /// </summary>
        /// <param name="path">The file to load from.</param>
        /// <returns>The loaded <see cref="AppList"/>.</returns>
        /// <exception cref="IOException">Thrown if a problem occurs while reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        public static AppList Load(string path)
        {
            return XmlStorage.Load<AppList>(path);
        }

        /// <summary>
        /// Loads an <see cref="AppList"/> from a stream containing an XML file.
        /// </summary>
        /// <param name="stream">The stream to load from.</param>
        /// <returns>The loaded <see cref="AppList"/>.</returns>
        public static AppList Load(Stream stream)
        {
            return XmlStorage.Load<AppList>(stream);
        }

        /// <summary>
        /// Saves this <see cref="AppList"/> to an XML file.
        /// </summary>
        /// <param name="path">The file to save in.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        public void Save(string path)
        {
            XmlStorage.Save(path, this);
        }

        /// <summary>
        /// Saves this <see cref="AppList"/> to a stream as an XML file.
        /// </summary>
        /// <param name="stream">The stream to save in.</param>
        public void Save(Stream stream)
        {
            XmlStorage.Save(stream, this);
        }
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="AppList"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="AppList"/>.</returns>
        public AppList CloneAppList()
        {
            var appList = new AppList();
            foreach (var entry in Entries) appList.Entries.Add(entry.CloneEntry());

            return appList;
        }

        public object Clone()
        {
            return CloneAppList();
        }
        #endregion

        #region Equality
        public bool Equals(AppList other)
        {
            if (ReferenceEquals(null, other)) return false;

            if (!Entries.UnsequencedEquals(other.Entries)) return false;
            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(AppList) && Equals((AppList)obj);
        }

        public override int GetHashCode()
        {
            return Entries.GetUnsequencedHashCode();
        }
        #endregion
    }
}