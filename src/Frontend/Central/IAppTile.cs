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

using System;
using System.ComponentModel;
using JetBrains.Annotations;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Store;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Central
{
    /// <summary>
    /// A graphical widget that represents an application as a tile with buttons for launching, managing, etc..
    /// </summary>
    public interface IAppTile
    {
        /// <summary>
        /// The interface URI of the application this tile represents.
        /// </summary>
        [NotNull]
        FeedUri InterfaceUri { get; }

        /// <summary>
        /// The name of the application this tile represents.
        /// </summary>
        [NotNull]
        string AppName { get; }

        /// <summary>
        /// Describes whether the application is listed in the <see cref="AppList"/> and if so whether it is integrated.
        /// </summary>
        /// <exception cref="InvalidOperationException">The value is set from a thread other than the UI thread.</exception>
        /// <remarks>This method must not be called from a background thread.</remarks>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        AppStatus Status { get; set; }

        /// <summary>
        /// A <see cref="Feed"/> from which the tile extracts relevant application metadata such as summaries and icons.
        /// </summary>
        /// <exception cref="InvalidOperationException">The value is set from a thread other than the UI thread.</exception>
        /// <remarks>This method must not be called from a background thread.</remarks>
        [CanBeNull]
        Feed Feed { get; set; }
    }
}
