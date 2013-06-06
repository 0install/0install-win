/*
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
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace ZeroInstall.Model
{
    /// <summary>
    /// Represents a retrieval method that downloads data from the net.
    /// </summary>
    [Serializable]
    public abstract class DownloadRetrievalMethod : RetrievalMethod, IRecipeStep, IEquatable<DownloadRetrievalMethod>
    {
        #region Properties
        /// <summary>
        /// The URL used to locate the file.
        /// </summary>
        [Description("The URL used to locate the file.")]
        [XmlIgnore]
        public Uri Location { get; set; }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Location"/>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Used for XML serialization")]
        [XmlAttribute("href"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public string LocationString { get { return (Location == null ? null : Location.ToString()); } set { Location = (value == null ? null : new Uri(value)); } }

        /// <summary>
        /// The size of the file in bytes. The file must have the given size or it will be rejected.
        /// </summary>
        [Description("The size of the file in bytes. The file must have the given size or it will be rejected.")]
        [XmlAttribute("size"), DefaultValue(0L)]
        public long Size { get; set; }

        /// <summary>
        /// The effective size of the file on the server.
        /// </summary>
        [XmlIgnore, Browsable(false)]
        public virtual long DownloadSize { get { return Size; } }
        #endregion

        //--------------------//

        #region Clone
        /// <inheritdoc/>
        public abstract IRecipeStep CloneRecipeStep();
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(DownloadRetrievalMethod other)
        {
            if (other == null) return false;
            return base.Equals(other) && other.Location == Location && other.Size == Size;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                if (Location != null) result = (result * 397) ^ Location.GetHashCode();
                result = (result * 397) ^ Size.GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
