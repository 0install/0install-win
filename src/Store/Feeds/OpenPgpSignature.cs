/*
 * Copyright 2010-2011 Bastian Eicher, Simon E. Silva Lauinger
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

namespace ZeroInstall.Store.Feeds
{
    /// <summary>
    /// Represents a signature validated by an <see cref="IOpenPgp"/> implementation.
    /// </summary>
    public struct OpenPgpSignature
    {
        #region Variables
        /// <summary>
        /// The ID of the key used to create this signature.
        /// </summary>
        public readonly string KeyID;
        #endregion

        // ToDo: Implement
    }
}
