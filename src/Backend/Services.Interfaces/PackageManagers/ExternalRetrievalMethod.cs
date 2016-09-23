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
using JetBrains.Annotations;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Services.PackageManagers
{
    /// <summary>
    /// Retrieves an implementation by installing it via an external package manager rather than Zero Install itself.
    /// </summary>
    /// <seealso cref="IPackageManager"/>
    public sealed class ExternalRetrievalMethod : RetrievalMethod, IEquatable<ExternalRetrievalMethod>
    {
        /// <summary>
        /// The name of the distribution this package came from.
        /// </summary>
        [CanBeNull]
        public string Distro { get; set; }

        /// <summary>
        /// The package name, in a form recognised by the external package manager.
        /// </summary>
        [CanBeNull]
        public string PackageID { get; set; }

        /// <summary>
        /// The download size in bytes.
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// A question the user shall be asked for confirmation before calling <see cref="Install"/>. <c>null</c> if no confirmation is required.
        /// </summary>
        [CanBeNull]
        public string ConfirmationQuestion { get; set; }

        /// <summary>
        /// A function to call to install this package.
        /// </summary>
        public Action Install { get; set; }

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="ExternalRetrievalMethod"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="ExternalRetrievalMethod"/>.</returns>
        private ExternalRetrievalMethod CloneNativeRetrievalMethod() => new ExternalRetrievalMethod {Distro = Distro, PackageID = PackageID, Size = Size, ConfirmationQuestion = ConfirmationQuestion, Install = Install};

        /// <summary>
        /// Creates a deep copy of this <see cref="ExternalRetrievalMethod"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="ExternalRetrievalMethod"/>.</returns>
        public override RetrievalMethod Clone() => CloneNativeRetrievalMethod();
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(ExternalRetrievalMethod other)
        {
            if (other == null) return false;
            return base.Equals(other) && Distro == other.Distro && PackageID == other.PackageID && Size == other.Size && ConfirmationQuestion == other.ConfirmationQuestion && Equals(Install, other.Install);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj == this) return true;
            return obj is ExternalRetrievalMethod && Equals((ExternalRetrievalMethod)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result * 397) ^ Distro?.GetHashCode() ?? 0;
                result = (result * 397) ^ PackageID?.GetHashCode() ?? 0;
                result = (result * 397) ^ Size.GetHashCode();
                result = (result * 397) ^ ConfirmationQuestion?.GetHashCode() ?? 0;
                return result;
            }
        }
        #endregion
    }
}
