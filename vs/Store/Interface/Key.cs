/*
 * Copyright 2010 Bastian Eicher
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
using System.Xml.Serialization;

namespace ZeroInstall.Store.Interface
{
    /// <summary>
    /// An entry in the <see cref="Trust"/> database.
    /// </summary>
    public sealed class Key
    {
        #region Properties
        /// <summary>
        /// ToDo
        /// </summary>
        [XmlAttribute("fingerprint")]
        public string Fingerprint { get; set; }

        // Order is always alphabetical, duplicate entries are not allowed
        private readonly C5.TreeSet<Domain> _domains = new C5.TreeSet<Domain>();
        /// <summary>
        /// ToDo
        /// </summary>
        [XmlElement("domain")]
        public ICollection<Domain> Domains { get { return _domains; } }
        #endregion
    }
}