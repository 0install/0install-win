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

namespace ZeroInstall.Injector.Arguments
{
    /// <summary>
    /// List of operational modes that can usually be selected via command-line arguments.
    /// </summary>
    public enum OperationMode
    {
        /// <summary>Launch or download an <see cref="Model.Feed"/>.</summary>
        Normal,
        /// <summary>List known <see cref="Model.Feed"/>s.</summary>
        List,
        /// <summary>Import feed files from local source.</summary>
        Import,
        /// <summary>Add feed aliases.</summary>
        Manage,
        /// <summary>Display version information.</summary>
        Version,
        /// <summary>Help text has already been displayed.</summary>
        Help
    }
}
