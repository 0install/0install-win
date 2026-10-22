// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

namespace ZeroInstall.Commands.EtoForms;

partial class GuiCommandHandler
{
    /// <inheritdoc/>
    protected override void LogHandler(LogSeverity severity, string? message, Exception? exception)
    {
        // Do not bother user with connectivity errors for background update checks
        if (Background && exception is WebException) return;

        base.LogHandler(severity, message, exception);
    }

    /// <summary>
    /// Displays <see cref="Log"/> entries as notification messages detached from the main GUI.
    /// </summary>
    protected override void DisplayLogEntry(LogSeverity severity, string message)
    {
        if (TryGetBranding() is {} branding)
            ShowNotification(branding, title: branding.Name ?? "Zero Install", message);
    }

    /// <summary>Tray icon used for showing notifications detached from the main GUI. Only access on the UI thread.</summary>
    private TrayIndicator? _notificationTrayIcon;

    /// <summary>
    /// Displays a notification message detached from the main GUI.
    /// </summary>
    /// <param name="branding">Branding to apply to the tray icon.</param>
    /// <param name="title">The title of the message.</param>
    /// <param name="message">The message text.</param>
    /// <param name="wait"><c>true</c> to block until the notification has been shown; <c>false</c> to return immediately (required when called from a <see cref="Log.Handler"/>, which may run while the UI thread waits for the caller).</param>
    private void ShowNotification(FeedBranding branding, string title, string message, bool wait = false)
    {
        void Show()
        {
            try
            {
                _notificationTrayIcon ??= new() {Title = branding.Name ?? "Zero Install", Image = branding.Icon};
                _notificationTrayIcon.Show();
                new Notification {Title = title, Message = message}.Show(_notificationTrayIcon);
            }
            catch
            {
                // Notifications are only shown with best effort
            }
        }

        if (wait) Application.Instance.Invoke(Show);
        else Application.Instance.AsyncInvoke(Show);
    }
}
