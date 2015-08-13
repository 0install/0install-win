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

namespace ZeroInstall.Store.Model.Capabilities
{
    /// <summary>
    /// A specific <see cref="AutoPlay"/> event such as "Audio CD inserted".
    /// </summary>
    [Description("A specific AutoPlay event such as \"Audio CD inserted\".")]
    [Serializable, XmlRoot("event", Namespace = CapabilityList.XmlNamespace), XmlType("event", Namespace = CapabilityList.XmlNamespace)]
    public class AutoPlayEvent : XmlUnknown, ICloneable, IEquatable<AutoPlayEvent>
    {
        #region Constants
        /// <summary>
        /// Canonical <see cref="Name"/>.
        /// </summary>
        public const string NamePlayCDAudio = "PlayCDAudioOnArrival", NamePlayDvdAudioO = "PlayDVDAudioOnArrival", NamePlayMusicFiles = "PlayMusicFilesOnArrival",
            NamePlayVideoCDMovie = "PlayVideoCDMovieOnArrival", NamePlaySuperVideoCDMovie = "PlaySuperVideoCDMovieOnArrival", NamePlayDvdMovie = "PlayDVDMovieOnArrival", NamePlayBluRay = "PlayBluRayOnArrival", NamePlayVideoFiles = "PlayVideoFilesOnArrival",
            NameBurnCD = "HandleCDBurningOnArrival", NameBurnDvd = "HandleDVDBurningOnArrival", NameBurnBluRay = "HandleBDBurningOnArrival";
        #endregion

        #region Properties
        /// <summary>
        /// The name of the event.
        /// </summary>
        [Description("The name of the event.")]
        [XmlAttribute("name")]
        public string Name { get; set; }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the event in the form "Name". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return Name;
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="AutoPlayEvent"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="AutoPlayEvent"/>.</returns>
        public AutoPlayEvent Clone()
        {
            return new AutoPlayEvent {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements, Name = Name};
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(AutoPlayEvent other)
        {
            if (other == null) return false;
            return base.Equals(other) && other.Name == Name;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is AutoPlayEvent && Equals((AutoPlayEvent)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result * 397) ^ (Name ?? "").GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
