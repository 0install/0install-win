// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.ComponentModel;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Model;

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
        FeedUri InterfaceUri { get; }

        /// <summary>
        /// The name of the application this tile represents.
        /// </summary>
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
        Feed? Feed { get; set; }
    }
}
