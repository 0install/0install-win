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
using System.Linq;
using System.Xml.Serialization;

namespace ZeroInstall.Store.Model.Capabilities
{
    /// <summary>
    /// An entry in the file manager's context menu for all file types.
    /// </summary>
    [Description("An entry in the file manager's context menu for all file types.")]
    [Serializable]
    [XmlRoot("context-menu", Namespace = CapabilityList.XmlNamespace), XmlType("context-menu", Namespace = CapabilityList.XmlNamespace)]
    public sealed class ContextMenu : DefaultCapability, ISingleVerb, IEquatable<ContextMenu>
    {
        #region Properties
        /// <inheritdoc/>
        [XmlIgnore]
        public override bool WindowsMachineWideOnly { get { return false; } }

        /// <summary>
        /// Set to <see langword="true"/> if this context menu entry is applicable to all filesystem entries and not just files.
        /// </summary>
        [Description("Set to true if this context menu entry is applicable to all filesystem entries and not just files.")]
        [XmlAttribute("all-objects"), DefaultValue(false)]
        public bool AllObjects { get; set; }

        /// <summary>
        /// The command to execute when the context menu entry is clicked.
        /// </summary>
        [Browsable(false)]
        [XmlElement("verb")]
        public Verb Verb { get; set; }

        /// <inheritdoc/>
        [XmlIgnore]
        public override IEnumerable<string> ConflictIDs
        {
            // Note: Context menu entries are only created via AccessPoints, the capability itself does nothing
            get { return Enumerable.Empty<string>(); }
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the capability in the form "ID". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return ID;
        }
        #endregion

        #region Clone
        /// <inheritdoc/>
        public override Capability Clone()
        {
            return new ContextMenu {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements, ID = ID, ExplicitOnly = ExplicitOnly, AllObjects = AllObjects, Verb = Verb.Clone()};
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
            return obj is ContextMenu && Equals((ContextMenu)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result * 397) ^ AllObjects.GetHashCode();
                if (Verb != null) result = (result * 397) ^ Verb.GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
