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

using System.Collections.Generic;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Model;

namespace ZeroInstall.Injector.WinForms
{
    /// <summary>
    /// Structure for storing user-selected argument for an operation.
    /// </summary>
    public struct ParseResults
    {
        /// <summary>User settings controlling the dependency solving process.</summary>
        public Policy Policy;

        /// <summary>Only download <see cref="Implementation"/>s but don't execute them.</summary>
        public bool DownloadOnly;

        /// <summary>Load <see cref="Selections"/> from this file instead of using an <see cref="ISolver"/>.</summary>
        public string SelectionsFile;

        /// <summary>An alternative executable to to run from the main <see cref="Implementation"/> instead of <see cref="ImplementationBase.Main"/>.</summary>
        public string Main;

        /// <summary>Instead of executing the selected program directly, pass it as an argument to this program. Useful for debuggers.</summary>
        public string Wrapper;

        /// <summary>The interface to launch, feed to download/add, term to search for, etc.</summary>
        public string Feed;

        /// <summary>Arguments to pass to the launched application, additional feeds to add, additional terms to search for, etc.</summary>
        public IList<string> AdditionalArgs;
    }
}
