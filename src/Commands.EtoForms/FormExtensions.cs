// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

namespace ZeroInstall.Commands.EtoForms;

/// <summary>
/// Provides extension methods for <see cref="Window"/>s.
/// </summary>
internal static class FormExtensions
{
    /// <summary>
    /// Prevents the window from being pinned to the taskbar if the current Zero Install instance is not integrated into the desktop environment.
    /// </summary>
    /// <remarks>Has no effect on platforms other than Windows.</remarks>
    public static void PreventPinningIfNotIntegrated(this Window window)
    {
        if (!ZeroInstallInstance.IsIntegrated) window.PreventPinning();
    }

    /// <summary>
    /// Sets the size of the dialog's client area and applies it again once the dialog has loaded.
    /// </summary>
    /// <remarks>The Windows Forms backend of Eto.Forms ignores a <see cref="Dialog"/>'s client size set before it is loaded and makes the dialog larger to fit its content.</remarks>
    public static void SetClientSize(this Dialog dialog, Size size)
    {
        dialog.ClientSize = size;
        dialog.LoadComplete += delegate { dialog.ClientSize = size; };
    }
}
