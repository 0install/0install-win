/*
 * Copyright 2010-2012 Bastian Eicher
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

using Common;
using ZeroInstall.Injector.Feeds;

namespace ZeroInstall.Central.WinForms
{
    /// <summary>
    /// Represents a <see cref="Key"/>-<see cref="Domain"/> pair in a <see cref="TrustDB"/> for display in a GUI.
    /// </summary>
    internal sealed class TrustNode : INamed
    {
        public string Name { get { throw new System.NotImplementedException(); } set { throw new System.NotImplementedException(); } }

        public int CompareTo(object obj)
        {
            throw new System.NotImplementedException();
        }
    }
}
