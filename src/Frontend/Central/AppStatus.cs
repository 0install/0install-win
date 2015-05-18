/*
 * Copyright 2010-2015 Bastian Eicher
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

using ZeroInstall.DesktopIntegration;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Central
{
    /// <summary>
    /// Describes the status of an application represented by an <see cref="IAppTile"/>.
    /// </summary>
    /// <seealso cref="IAppTile.Status"/>
    public enum AppStatus
    {
        /// <summary>The state has not been set yet.</summary>
        Unset,

        /// <summary>The application is listed in a <see cref="Catalog"/> but not in the <see cref="AppList"/>.</summary>
        Candidate,

        /// <summary>The application is listed in the <see cref="AppList"/> but <see cref="AppEntry.AccessPoints"/> is <see langword="null"/>.</summary>
        Added,

        /// <summary>The application is listed in the <see cref="AppList"/> and <see cref="AppEntry.AccessPoints"/> is set.</summary>
        Integrated
    }
}
