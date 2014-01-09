﻿/*
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

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using Common.Collections;

namespace ZeroInstall.Model.Capabilities
{
    /// <summary>
    /// Abstract base class for capabilities that can have multiple <see cref="Verb"/>s.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 types only need to be disposed when using snapshots")]
    [XmlType("verb-capability", Namespace = CapabilityList.XmlNamespace)]
    public abstract class VerbCapability : IconCapability
    {
        #region Properties
        private readonly List<Verb> _verbs = new List<Verb>();

        /// <summary>
        /// A list of all available operations for the element.
        /// </summary>
        [Browsable(false)]
        [XmlElement("verb")]
        public List<Verb> Verbs { get { return _verbs; } }
        #endregion

        //--------------------//

        #region Equality
        /// <inheritdoc/>
        protected bool Equals(VerbCapability other)
        {
            if (other == null) return false;
            return base.Equals(other) && Verbs.SequencedEquals(other.Verbs);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ Verbs.GetSequencedHashCode();
            }
        }
        #endregion
    }
}
