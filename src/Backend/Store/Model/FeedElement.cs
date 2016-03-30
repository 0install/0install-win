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

using System.ComponentModel;
using System.Xml.Serialization;
using JetBrains.Annotations;
using NanoByte.Common.Info;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// Abstract base class for XML serializable classes that are part of the Zero Install feed model.
    /// </summary>
    /// <remarks>Does not include <see cref="Store.Model.Capabilities"/>.</remarks>
    public abstract class FeedElement : XmlUnknown
    {
        /// <summary>
        /// Only process this element if the current Zero Install version matches the range.
        /// </summary>
        [Browsable(false)]
        [XmlIgnore, CanBeNull]
        public VersionRange IfZeroInstallVersion { get; set; }

        #region XML serialization
        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="IfZeroInstallVersion"/>
        [XmlAttribute("if-0install-version"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public string IfZeroInstallVersionString { get { return (IfZeroInstallVersion == null) ? null : IfZeroInstallVersion.ToString(); } set { IfZeroInstallVersion = string.IsNullOrEmpty(value) ? null : new VersionRange(value); } }
        #endregion

        #region Filter
        /// <summary>
        /// The version number of the Zero Install model.
        /// </summary>
        public static readonly ImplementationVersion ZeroInstallVersion = new ImplementationVersion(AppInfo.CurrentLibrary.Version);

        /// <summary>
        /// Checks whether an element passes the specified <see cref="IfZeroInstallVersion"/> restriction, if any.
        /// </summary>
        protected internal static bool FilterMismatch<T>(T element)
            where T : FeedElement
        {
            if (element == null) return false;

            return element.IfZeroInstallVersion != null && !element.IfZeroInstallVersion.Match(ZeroInstallVersion);
        }

        /// <summary>
        /// Checks whether an element passes the specified <see cref="IfZeroInstallVersion"/> restriction, if any.
        /// </summary>
        protected static bool FilterMismatch(IRecipeStep step)
        {
            return FilterMismatch(step as FeedElement);
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        protected bool Equals(FeedElement other)
        {
            if (other == null) return false;
            return base.Equals(other) && IfZeroInstallVersion == other.IfZeroInstallVersion;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                if (IfZeroInstallVersion != null) result = (result * 397) ^ IfZeroInstallVersion.GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
