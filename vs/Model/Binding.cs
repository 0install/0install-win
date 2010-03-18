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
    /// Bindings specify how the chosen implementation is made known to the running program.
    /// </summary>
    /// <remarks>
    /// Bindings can appear in <see cref="Dependency"/>s, in which case they tell a component how to find its dependency,
    /// or in <see cref="ImplementationBase"/>, where they tell a component how to find itself.
    /// </remarks>
    public abstract class Binding
    {}
}
