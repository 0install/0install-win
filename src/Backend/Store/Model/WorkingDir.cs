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
using System.ComponentModel;
using System.Xml.Serialization;
using JetBrains.Annotations;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// Switches the working directory of a process on startup to a location within an implementation.
    /// Useful for supporting legacy Windows applications which do not properly locate their installation directory.
    /// </summary>
    /// <seealso cref="Command.WorkingDir"/>
    [Description("Switches the working directory of a process on startup to a location within an implementation.\r\nUseful for supporting legacy Windows applications which do not properly locate their installation directory.")]
    [Serializable]
    [XmlRoot("working-dir", Namespace = Feed.XmlNamespace), XmlType("working-dir", Namespace = Feed.XmlNamespace)]
    public sealed class WorkingDir : FeedElement, ICloneable, IEquatable<WorkingDir>
    {
        #region Properties
        /// <summary>
        /// The relative path of the directory in the implementation to set as the working directory. Defaults to use the root of the implementation if unset.
        /// </summary>
        [Description("The relative path of the directory in the implementation to set as the working directory. Defaults to use the root of the implementation if unset.")]
        [XmlAttribute("src"), DefaultValue("")]
        [CanBeNull]
        public string Source { get; set; }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the binding in the form "Source". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return Source ?? "(unset)";
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="WorkingDir"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="WorkingDir"/>.</returns>
        public WorkingDir Clone()
        {
            return new WorkingDir {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements, IfZeroInstallVersion = IfZeroInstallVersion, Source = Source};
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(WorkingDir other)
        {
            if (other == null) return false;
            return base.Equals(other) && other.Source == Source;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is WorkingDir && Equals((WorkingDir)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (Source ?? "").GetHashCode();
            }
        }
        #endregion
    }
}
