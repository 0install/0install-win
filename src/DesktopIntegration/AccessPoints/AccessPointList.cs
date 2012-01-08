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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml.Serialization;
using Common.Storage;
using ZeroInstall.Model;

namespace ZeroInstall.DesktopIntegration.AccessPoints
{
    /// <summary>
    /// Contains a set of <see cref="AccessPoint"/>s to be registered in a desktop environment.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    [Serializable]
    [XmlRoot("access-points", Namespace = AppList.XmlNamespace)]
    [XmlType("access-points", Namespace = AppList.XmlNamespace)]
    public sealed class AccessPointList : XmlUnknown, ICloneable, IEquatable<AccessPointList>
    {
        #region Properties
        // Preserve order
        private readonly C5.LinkedList<AccessPoint> _accessPoints = new C5.LinkedList<AccessPoint>();

        /// <summary>
        /// A list of <see cref="AccessPoint"/>s.
        /// </summary>
        [Description("A list of access points.")]
        [XmlElement(typeof(AppAlias)), XmlElement(typeof(AutoPlay)), XmlElement(typeof(CapabilityRegistration)), XmlElement(typeof(ContextMenu)), XmlElement(typeof(DefaultProgram)), XmlElement(typeof(DesktopIcon)), XmlElement(typeof(FileType)), XmlElement(typeof(MenuEntry)), XmlElement(typeof(SendTo)), XmlElement(typeof(UrlProtocol)), XmlElement(typeof(QuickLaunch)), XmlElement(typeof(MockAccessPoint))]
        // Note: Can not use ICollection<T> interface with XML Serialization
            public C5.LinkedList<AccessPoint> Entries
        {
            get { return _accessPoints; }
        }
        #endregion

        //--------------------//

        #region Storage
        /// <summary>
        /// Loads a <see cref="AccessPointList"/> from an XML file.
        /// </summary>
        /// <param name="path">The file to load from.</param>
        /// <returns>The loaded <see cref="AccessPointList"/>.</returns>
        /// <exception cref="IOException">Thrown if a problem occurs while reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if a problem occurs while deserializing the XML data.</exception>
        public static AccessPointList Load(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            return XmlStorage.Load<AccessPointList>(path);
        }

        /// <summary>
        /// Loads a <see cref="AccessPointList"/> from a stream containing an XML file.
        /// </summary>
        /// <param name="stream">The stream to load from.</param>
        /// <returns>The loaded <see cref="AccessPointList"/>.</returns>
        /// <exception cref="InvalidDataException">Thrown if a problem occurs while deserializing the XML data.</exception>
        public static AccessPointList Load(Stream stream)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            return XmlStorage.Load<AccessPointList>(stream);
        }

        /// <summary>
        /// Saves this <see cref="AccessPointList"/> to an XML file.
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
        /// Saves this <see cref="AccessPointList"/> to a stream as an XML file.
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
        /// Creates a deep copy of this <see cref="AccessPointList"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="AccessPointList"/>.</returns>
        public AccessPointList Clone()
        {
            var accessPointList = new AccessPointList();
            foreach (var entry in Entries) accessPointList.Entries.Add(entry.Clone());

            return accessPointList;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion

        #region Conversion
        /// <summary>
        /// Returns the access point list in the form "AccessPoints: Entries". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("AccessPoints: {0}", Entries);
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(AccessPointList other)
        {
            if (other == null) return false;

            if (!Entries.SequencedEquals(other.Entries)) return false;
            return true;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(AccessPointList) && Equals((AccessPointList)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Entries.GetSequencedHashCode();
        }
        #endregion
    }
}
