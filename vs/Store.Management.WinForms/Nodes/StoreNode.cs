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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Common;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Store.Management.WinForms.Nodes
{
    /// <summary>
    /// Models information about elements in a cache for display in a GUI.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Justification = "Comparison only used for INamed sorting")]
    public abstract class StoreNode : INamed
    {
        #region Properties
        /// <inheritdoc/>
        [Browsable(false)]
        public abstract string Name { get; set; }

        /// <summary>
        /// A counter that can be used to prevent naming collisions.
        /// </summary>
        /// <remarks>If this value is not zero it is appended to the <see cref="Name"/>.</remarks>
        public int SuffixCounter;
        #endregion

        #region Delete
        /// <summary>
        /// Deletes this element from the cache it is stored in.
        /// </summary>
        /// <param name="handler">A callback object used when the the user is to be informed about progress.</param>
        /// <exception cref="KeyNotFoundException">Thrown if no matching element could be found in the cache.</exception>
        /// <exception cref="IOException">Thrown if the element could not be deleted because it was in use.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the cache is not permitted.</exception>
        public abstract void Delete(IIOHandler handler);
        #endregion

        #region Verify
        /// <summary>
        /// Verify this element is valid.
        /// </summary>
        /// <param name="handler">A callback object used when the the user is to be informed about progress.</param>
        /// <exception cref="UserCancelException">Thrown if the user cancelled the task.</exception>
        /// <exception cref="IOException">Thrown if the entry's directory could not be processed.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the entry's directory is not permitted.</exception>
        /// <exception cref="DigestMismatchException">Thrown if the entry's directory doesn't match the <see cref="ManifestDigest"/>.</exception>
        public abstract void Verify(IIOHandler handler);
        #endregion

        #region Comparison
        int IComparable.CompareTo(object obj)
        {
            string otherName;
            var named = obj as INamed;
            if (named != null) otherName = named.Name;
            else if (obj != null) otherName = obj.ToString();
            else otherName = null;
            return string.Compare(Name, otherName, StringComparison.OrdinalIgnoreCase);
        }
        #endregion
    }
}
