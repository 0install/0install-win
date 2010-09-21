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

namespace ZeroInstall.Model
{
    /// <summary>
    /// Make a chosen <see cref="Implementation"/> available by overlaying it onto another part of the file-system.
    /// </summary>
    /// <remarks>This is to support legacy programs which use hard-coded paths.</remarks>
    public sealed class WorkingDirBinding : Binding, IEquatable<WorkingDirBinding>
    {
        #region Conversion
        /// <summary>
        /// Returns "WorkingDirBinding". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return "WorkingDirBinding";
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="WorkingDirBinding"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="WorkingDirBinding"/>.</returns>
        public override Binding CloneBinding()
        {
            return new WorkingDirBinding();
        }
        #endregion

        #region Equality
        public bool Equals(WorkingDirBinding other)
        {
            if (ReferenceEquals(null, other)) return false;

            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(WorkingDirBinding) && Equals((WorkingDirBinding)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return 0;
            }
        }
        #endregion
    }
}
