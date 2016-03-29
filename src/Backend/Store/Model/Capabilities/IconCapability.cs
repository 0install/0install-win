/*
 * Copyright 2010-2016 Bastian Eicher
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
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Collections;

namespace ZeroInstall.Store.Model.Capabilities
{
    /// <summary>
    /// Abstract base class for capabilities that can have multiple <see cref="Icon"/>s and descriptions.
    /// </summary>
    [Serializable, XmlType("icon-capability", Namespace = CapabilityList.XmlNamespace)]
    public abstract class IconCapability : DefaultCapability, IIconContainer, IDescriptionContainer
    {
        private readonly LocalizableStringCollection _descriptions = new LocalizableStringCollection();

        /// <inheritdoc/>
        [Browsable(false)]
        [XmlElement("description")]
        public LocalizableStringCollection Descriptions { get { return _descriptions; } }

        private readonly List<Icon> _icons = new List<Icon>();

        /// <summary>
        /// Zero or more icons to represent the capability. Used for things like file icons.
        /// </summary>
        [Browsable(false)]
        [XmlElement("icon", Namespace = Feed.XmlNamespace), NotNull]
        public List<Icon> Icons { get { return _icons; } }

        /// <summary>
        /// Returns the first icon with a specific MIME type.
        /// </summary>
        /// <param name="mimeType">The <see cref="Icon.MimeType"/> to try to find. Will only return exact matches.</param>
        /// <returns>The best matching icon that was found or <c>null</c> if no matching icon was found.</returns>
        [CanBeNull]
        public Icon GetIcon([NotNull] string mimeType)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(mimeType)) throw new ArgumentNullException("mimeType");
            #endregion

            return Icons.FirstOrDefault(icon => StringUtils.EqualsIgnoreCase(icon.MimeType, mimeType) && icon.Href != null);
        }

        #region Equality
        /// <inheritdoc/>
        protected bool Equals(IconCapability other)
        {
            if (other == null) return false;
            return base.Equals(other) && Descriptions.UnsequencedEquals(other.Descriptions) && Icons.UnsequencedEquals(other.Icons);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result * 397) ^ Descriptions.GetUnsequencedHashCode();
                result = (result * 397) ^ Icons.GetUnsequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}
