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
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace ZeroInstall.Model.Capabilities
{
    /// <summary>
    /// Represents an application's entry in a file manager's context menu.
    /// </summary>
    [XmlType("context-menu", Namespace = XmlNamespace)]
    public sealed class ContextMenu : DefaultCapability, IEquatable<ContextMenu>
    {
        #region Properties
        /// <inheritdoc/>
        [XmlIgnore]
        public override bool WindowsSystemWideOnly { get { return false; } }

        /// <summary>
        /// Set to <see langword="true"/> if this context menu entry is applicable to all filesystem entries and not just files.
        /// </summary>
        [Description("Set to true if this context menu entry is applicable to all filesystem entries and not just files.")]
        [XmlAttribute("all-objects"), DefaultValue(false)]
        public bool AllObjects { get; set; }

        /// <summary>
        /// The command to execute when the context menu entry is clicked.
        /// </summary>
        [Description("The command to execute when the context menu entry is clicked.")]
        [XmlElement("verb")]
        public Verb Verb { get; set; }

        /// <inheritdoc/>
        [XmlIgnore]
        public override IEnumerable<string> ConflictIDs
        {
            // Note: Context menu entries are only created via AccessPoints, the capability itself does nothing
            get { return new string[0]; }
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the capability in the form "ContextMenu: ID". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("ContextMenu: {0}", ID);
        }
        #endregion

        #region Clone
        /// <inheritdoc/>
        public override Capability Clone()
        {
            return new ContextMenu {ID = ID, AllObjects = AllObjects, Verb = Verb.Clone()};
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(ContextMenu other)
        {
            if (other == null) return false;

            return base.Equals(other) &&
                other.AllObjects == AllObjects && Equals(other.Verb, Verb);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(ContextMenu) && Equals((ContextMenu)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result * 397) ^ AllObjects.GetHashCode();
                result = (result * 397) ^ Verb.GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
