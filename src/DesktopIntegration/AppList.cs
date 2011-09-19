/*
 * Copyright 2010-2011 Bastian Eicher
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml.Serialization;
using Common.Storage;
using ZeroInstall.DesktopIntegration.AccessPoints;

namespace ZeroInstall.DesktopIntegration
{
    /// <summary>
    /// Stores a list of applications and the kind of desktop integration the user chose for them.
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
        public const string XmlNamespace = "http://0install.de/schema/desktop-integration/app-list";
        #endregion

        #region Properties
        // Preserve order
        private readonly C5.LinkedList<AppEntry> _entries = new C5.LinkedList<AppEntry>();
        /// <summary>
        /// A list of <see cref="AppEntry"/>s.
        /// </summary>
        [Description("A list of application entries.")]
        [XmlElement("app")]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.LinkedList<AppEntry> Entries { get { return _entries; } }
        #endregion

        //--------------------//

        #region Conflict IDs
        /// <summary>
        /// Returns a list of all conflict IDs and the <see cref="AccessPoint"/>s belong to.
        /// </summary>
        /// <seealso cref="AccessPoint.GetConflictIDs"/>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Performs some potentially slow computations.")]
        public IDictionary<string, ConflictData> GetConflictIDs()
        {
            var conflictIDs = new Dictionary<string, ConflictData>();
            foreach (var appEntry in Entries)
            {
                if (appEntry.AccessPoints == null) continue;
                foreach (var accessPoint in appEntry.AccessPoints.Entries)
                {
                    foreach (string conflictID in accessPoint.GetConflictIDs(appEntry))
                    {
                        if (!conflictIDs.ContainsKey(conflictID))
                            conflictIDs.Add(conflictID, new ConflictData(appEntry, accessPoint));
                    }
                }
            }
            return conflictIDs;
        }
        #endregion

        #region Storage
        /// <summary>
        /// Loads a <see cref="AppList"/> from an XML file.
        /// </summary>
        /// <param name="path">The file to load from.</param>
        /// <returns>The loaded <see cref="AppList"/>.</returns>
        /// <exception cref="IOException">Thrown if a problem occurs while reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if a problem occurs while deserializing the XML data.</exception>
        public static AppList Load(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            return XmlStorage.Load<AppList>(path);
        }

        /// <summary>
        /// Loads a <see cref="AppList"/> from a stream containing an XML file.
        /// </summary>
        /// <param name="stream">The stream to load from.</param>
        /// <returns>The loaded <see cref="AppList"/>.</returns>
        public static AppList Load(Stream stream)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

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
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            XmlStorage.Save(path, this);
        }

        /// <summary>
        /// Saves this <see cref="AppList"/> to a stream as an XML file.
        /// </summary>
        /// <param name="stream">The stream to save in.</param>
        public void Save(Stream stream)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

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

        /// <summary>
        /// Creates a deep copy of this <see cref="AppList"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="AppList"/>.</returns>
        public object Clone()
        {
            return CloneAppList();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(AppList other)
        {
            if (other == null) return false;

            if (!Entries.UnsequencedEquals(other.Entries)) return false;
            return true;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(AppList) && Equals((AppList)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Entries.GetUnsequencedHashCode();
        }
        #endregion
    }
}