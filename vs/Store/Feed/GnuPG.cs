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

using System.Security;
using Common;

namespace ZeroInstall.Store.Feed
{
    public class GnuPG : ConsoleAppControl
    {
        #region Properties
        /// <inheritdoc/>
        protected override string AppName { get { return "GnuPG"; } }

        /// <inheritdoc/>
        protected override string AppBinaryName { get { return "gpg"; } }
        #endregion

        public void DetachSign(string path, SecureString passphrase)
        {
            Execute("--batch --passphrase-fd 0 --detach-sign " + path, null, null);
        }

        //--------------------//
    }
}
