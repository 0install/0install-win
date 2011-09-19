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
using System.ComponentModel;
using System.Xml.Serialization;

namespace ZeroInstall.Model.Capabilities
{
    /// <summary>
    /// Names a specific <see cref="AutoPlay"/> event.
    /// </summary>
    [XmlType("event", Namespace = Capability.XmlNamespace)]
    public struct AutoPlayEvent : IEquatable<AutoPlayEvent>
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

        #region Equality
        /// <inheritdoc/>
        public bool Equals(AutoPlayEvent other)
        {
            return other.Name == Name;
        }

        /// <inheritdoc/>
        public static bool operator ==(AutoPlayEvent left, AutoPlayEvent right)
        {
            return left.Equals(right);
        }

        /// <inheritdoc/>
        public static bool operator !=(AutoPlayEvent left, AutoPlayEvent right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj.GetType() == typeof(AutoPlayEvent) && Equals((AutoPlayEvent)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return (Name ?? "").GetHashCode();
        }
        #endregion
    }
}
