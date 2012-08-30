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
using System.Xml.Serialization;

namespace ZeroInstall.Model
{
    /// <summary>
    /// Switches the working directory of a process on startup to a location within an <see cref="Model.Implementation"/>.
    /// </summary>
    /// <remarks>This is to support legacy programs which can't properly locate their installation directory.</remarks>
    [Serializable]
    [XmlType("working-dir", Namespace = Feed.XmlNamespace)]
    public sealed class WorkingDir : XmlUnknown, ICloneable, IEquatable<WorkingDir>
    {
        #region Properties
        /// <summary>
        /// The relative path of the directory in the implementation to publish. The default is to publish everything.
        /// </summary>
        [Description("The relative path of the directory in the implementation to publish. The default is to publish everything.")]
        [XmlAttribute("src"), DefaultValue("")]
        public string Source { get; set; }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the binding in the form "WorkingDir: Source". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("WorkingDir: {0}", Source);
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="WorkingDir"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="WorkingDir"/>.</returns>
        public WorkingDir Clone()
        {
            return new WorkingDir {UnknownElements = UnknownElements, UnknownAttributes = UnknownAttributes, Source = Source};
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
