/*
 * Copyright 2010-2014 Bastian Eicher
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
using System.Linq;
using System.Xml.Serialization;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Storage;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Model.Preferences
{
    /// <summary>
    /// Stores user-specific preferences for an interface.
    /// </summary>
    [XmlRoot("interface-preferences", Namespace = Feed.XmlNamespace), XmlType("interface-preferences", Namespace = Feed.XmlNamespace)]
    public sealed class InterfacePreferences : XmlUnknown, ICloneable, IEquatable<InterfacePreferences>
    {
        #region Properties
        /// <summary>
        /// The URI of the interface to be configured.
        /// </summary>
        [XmlIgnore, Browsable(false)]
        public Uri Uri { get; set; }

        /// <summary>Used for XML serialization and PropertyGrid.</summary>
        /// <seealso cref="Uri"/>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Used for XML serialization")]
        [DisplayName(@"Uri"), Description("The URI of the interface to be configured.")]
        [XmlAttribute("uri"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public String UriString { get { return (Uri == null ? null : Uri.ToString()); } set { Uri = (string.IsNullOrEmpty(value) ? null : new Uri(value)); } }

        private Stability _stabilityPolicy = Stability.Unset;

        /// <summary>
        /// Implementations at this stability level or higher are preferred. Lower levels are used only if there is no other choice.
        /// </summary>
        [Description("Implementations at this stability level or higher are preferred. Lower levels are used only if there is no other choice.")]
        [XmlAttribute("stability-policy"), DefaultValue(typeof(Stability), "Unset")]
        public Stability StabilityPolicy { get { return _stabilityPolicy; } set { _stabilityPolicy = value; } }

        private readonly List<FeedReference> _feeds = new List<FeedReference>();

        /// <summary>
        /// Zero ore more additional feeds containing implementations of this interface.
        /// </summary>
        [Description("Zero ore more additional feeds containing implementations of this interface.")]
        [XmlElement("feed")]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public List<FeedReference> Feeds
        {
            get { return _feeds; }
        }
        #endregion

        //--------------------//

        #region Storage
        /// <summary>
        /// Loads <see cref="InterfacePreferences"/> for a specific interface.
        /// </summary>
        /// <param name="interfaceID">The interface to load the preferences for.</param>
        /// <returns>The loaded <see cref="InterfacePreferences"/>.</returns>
        /// <exception cref="IOException">Thrown if a problem occurs while reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if a problem occurs while deserializing the XML data.</exception>
        public static InterfacePreferences LoadFor(string interfaceID)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            #endregion

            var path = Locations.GetLoadConfigPaths("0install.net", true, "injector", "interfaces", ModelUtils.PrettyEscape(interfaceID)).FirstOrDefault();
            if (string.IsNullOrEmpty(path)) return new InterfacePreferences();

            return XmlStorage.LoadXml<InterfacePreferences>(path);
        }

        /// <summary>
        /// Tries to load <see cref="InterfacePreferences"/> for a specific interface. Automatically falls back to defaults on errors.
        /// </summary>
        /// <param name="interfaceID">The interface to load the preferences for.</param>
        /// <returns>The loaded <see cref="InterfacePreferences"/> or default value if there was a problem.</returns>
        public static InterfacePreferences LoadForSafe(string interfaceID)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            #endregion

            try
            {
                return LoadFor(interfaceID);
            }
                #region Error handling
            catch (FileNotFoundException)
            {
                return new InterfacePreferences();
            }
            catch (IOException ex)
            {
                Log.Warn(string.Format(Resources.ErrorLoadingInterfacePrefs, interfaceID));
                Log.Warn(ex);
                return new InterfacePreferences();
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Warn(string.Format(Resources.ErrorLoadingInterfacePrefs, interfaceID));
                Log.Warn(ex);
                return new InterfacePreferences();
            }
            catch (InvalidDataException ex)
            {
                Log.Warn(string.Format(Resources.ErrorLoadingInterfacePrefs, interfaceID));
                Log.Warn(ex);
                return new InterfacePreferences();
            }
            #endregion
        }

        /// <summary>
        /// Saves these <see cref="InterfacePreferences"/> for a specific interface.
        /// </summary>
        /// <param name="interfaceID">The interface to save the preferences for.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        public void SaveFor(string interfaceID)
        {
            var path = Locations.GetSaveConfigPath("0install.net", true, "injector", "interfaces", ModelUtils.PrettyEscape(interfaceID));
            this.SaveXml(path);
        }
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="InterfacePreferences"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="InterfacePreferences"/>.</returns>
        public InterfacePreferences Clone()
        {
            var feed = new InterfacePreferences {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements, Uri = Uri, StabilityPolicy = StabilityPolicy};
            foreach (var feedReference in Feeds) feed.Feeds.Add(feedReference.Clone());

            return feed;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion

        #region Conversion
        /// <summary>
        /// Returns the preferences in the form "InterfacePreferences: Uri". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("InterfacePreferences: {0}", Uri);
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(InterfacePreferences other)
        {
            if (other == null) return false;
            if (!base.Equals(other)) return false;
            if (Uri != other.Uri) return false;
            if (StabilityPolicy != other.StabilityPolicy) return false;
            if (!Feeds.SequencedEquals(other.Feeds)) return false;
            return true;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is InterfacePreferences && Equals((InterfacePreferences)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                if (Uri != null) result = (result * 397) ^ Uri.GetHashCode();
                result = (result * 397) ^ StabilityPolicy.GetHashCode();
                result = (result * 397) ^ Feeds.GetSequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}
