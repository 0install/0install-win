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

namespace ZeroInstall.Model
{
    /// <summary>
    /// A retrieval method is a way of getting an <see cref="Implementation"/>.
    /// </summary>
    public abstract class RetrievalMethod : ISimplifyable
    {
        /// <summary>
        /// Sets missing default values, flattens the inheritance structure, etc.
        /// </summary>
        /// <remarks>This should be called to prepare an interface for launch.
        /// It should not be called if you plan on serializing the <see cref="Feed"/> again since it will may some of its structure.</remarks>
        public abstract void Simplify();
    }
}
