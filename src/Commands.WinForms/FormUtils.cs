// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using NanoByte.Common.Native;

namespace ZeroInstall.Commands.WinForms;

/// <summary>
/// Provides extension methods for <see cref="Form"/>s.
/// </summary>
internal static class FormExtensions
{
    /// <summary>
    /// Prevents the window from being pinned to the taskbar if the current Zero Install instance is not integrated into the desktop environment.
    /// </summary>
    public static void PreventPinningIfNotIntegrated(this Form form)
    {
        if (!ZeroInstallInstance.IsIntegrated)
            form.HandleCreated += delegate { WindowsTaskbar.PreventPinning(form.Handle); };
    }
}
