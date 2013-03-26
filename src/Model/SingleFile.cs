﻿/*
 * Copyright 2010-2013 Bastian Eicher
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

namespace ZeroInstall.Model
{
    /// <summary>
    /// Represents a single file to be downloaded.
    /// </summary>
    [Serializable]
    [XmlType("file", Namespace = Feed.XmlNamespace)]
    public sealed class SingleFile : DownloadRetrievalMethod, IEquatable<SingleFile>
    {
        #region Properties
        /// <summary>
        /// The file's target path relative to the implementation root as a Unix-style path.
        /// </summary>
        [Description("The file's target path relative to the implementation root as a Unix-style path.")]
        [XmlAttribute("dest")]
        public string Destination { get; set; }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the file in the form "SingleFile: Location (Size) => Destination". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("SingleFile: {0} ({1}) => {2}", Location, Size, Destination);
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="SingleFile"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="SingleFile"/>.</returns>
        private SingleFile CloneFile()
        {
            return new SingleFile {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements, Location = Location, Size = Size, Destination = Destination};
        }

        /// <summary>
        /// Creates a deep copy of this <see cref="SingleFile"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="SingleFile"/>.</returns>
        public override IRecipeStep CloneRecipeStep()
        {
            return CloneFile();
        }

        /// <summary>
        /// Creates a deep copy of this <see cref="SingleFile"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="SingleFile"/>.</returns>
        public override RetrievalMethod Clone()
        {
            return CloneFile();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(SingleFile other)
        {
            if (other == null) return false;
            return base.Equals(other) && other.Destination == Destination;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is SingleFile && Equals((SingleFile)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                if (Destination != null) result = (result * 397) ^ Destination.GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
