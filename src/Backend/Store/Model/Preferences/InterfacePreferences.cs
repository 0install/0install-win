/*
 * Copyright 2010-2016 Bastian Eicher
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
using JetBrains.Annotations;
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
        /// <summary>
        /// The URI of the interface to be configured.
        /// </summary>
        [Browsable(false)]
        [XmlIgnore, CanBeNull]
        public FeedUri Uri { get; set; }

        #region XML serialization
        /// <summary>Used for XML serialization and PropertyGrid.</summary>
        /// <seealso cref="Uri"/>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Used for XML serialization")]
        [DisplayName(@"Uri"), Description("The URI of the interface to be configured.")]
        [XmlAttribute("uri"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public String UriString { get { return (Uri == null ? null : Uri.ToStringRfc()); } set { Uri = (string.IsNullOrEmpty(value) ? null : new FeedUri(value)); } }
        #endregion

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
        [XmlElement("feed"), NotNull]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public List<FeedReference> Feeds
        {
            get { return _feeds; }
        }

        #region Storage
        /// <summary>
        /// Loads <see cref="InterfacePreferences"/> for a specific interface.
        /// </summary>
        /// <param name="interfaceUri">The interface to load the preferences for.</param>
        /// <returns>The loaded <see cref="InterfacePreferences"/>.</returns>
        /// <exception cref="IOException">A problem occurs while reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the file is not permitted.</exception>
        /// <exception cref="InvalidDataException">A problem occurs while deserializing the XML data.</exception>
        [NotNull]
        public static InterfacePreferences LoadFor([NotNull] FeedUri interfaceUri)
        {
            #region Sanity checks
            if (interfaceUri == null) throw new ArgumentNullException("interfaceUri");
            #endregion

            var path = Locations.GetLoadConfigPaths("0install.net", true, "injector", "interfaces", interfaceUri.PrettyEscape()).FirstOrDefault();
            if (string.IsNullOrEmpty(path)) return new InterfacePreferences();

            Log.Debug("Loading interface preferences for " + interfaceUri.ToStringRfc() + " from: " + path);
            return XmlStorage.LoadXml<InterfacePreferences>(path);
        }

        /// <summary>
        /// Tries to load <see cref="InterfacePreferences"/> for a specific interface. Automatically falls back to defaults on errors.
        /// </summary>
        /// <param name="interfaceUri">The interface to load the preferences for.</param>
        /// <returns>The loaded <see cref="InterfacePreferences"/> or default value if there was a problem.</returns>
        [NotNull]
        public static InterfacePreferences LoadForSafe([NotNull] FeedUri interfaceUri)
        {
            #region Sanity checks
            if (interfaceUri == null) throw new ArgumentNullException("interfaceUri");
            #endregion

            try
            {
                return LoadFor(interfaceUri);
            }
                #region Error handling
            catch (IOException ex)
            {
                Log.Warn(string.Format(Resources.ErrorLoadingInterfacePrefs, interfaceUri));
                Log.Warn(ex);
                return new InterfacePreferences();
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Warn(string.Format(Resources.ErrorLoadingInterfacePrefs, interfaceUri));
                Log.Warn(ex);
                return new InterfacePreferences();
            }
            catch (InvalidDataException ex)
            {
                Log.Warn(string.Format(Resources.ErrorLoadingInterfacePrefs, interfaceUri));
                Log.Warn(ex);
                return new InterfacePreferences();
            }
            #endregion
        }

        /// <summary>
        /// Saves these <see cref="InterfacePreferences"/> for a specific interface.
        /// </summary>
        /// <param name="interfaceUri">The interface to save the preferences for.</param>
        /// <exception cref="IOException">A problem occurs while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the file is not permitted.</exception>
        public void SaveFor([NotNull] FeedUri interfaceUri)
        {
            #region Sanity checks
            if (interfaceUri == null) throw new ArgumentNullException("interfaceUri");
            #endregion

            var path = Locations.GetSaveConfigPath("0install.net", true, "injector", "interfaces", interfaceUri.PrettyEscape());

            Log.Debug("Saving interface preferences for " + interfaceUri.ToStringRfc() + " to: " + path);
            this.SaveXml(path);
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="InterfacePreferences"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="InterfacePreferences"/>.</returns>
        public InterfacePreferences Clone()
        {
            var feed = new InterfacePreferences {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements, Uri = Uri, StabilityPolicy = StabilityPolicy};
            feed.Feeds.AddRange(Feeds.CloneElements());

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
            if (!Feeds.UnsequencedEquals(other.Feeds)) return false;
            return true;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj == this) return true;
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
                result = (result * 397) ^ Feeds.GetUnsequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}
