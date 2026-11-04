// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

#if WINDOWS
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using Windows.Data.Xml.Dom;
using ToastNotification = Windows.UI.Notifications.ToastNotification;
using ToastNotificationManager = Windows.UI.Notifications.ToastNotificationManager;
#endif

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
    /// <remarks>Uses Windows toast notifications if possible, which stick around even after the process ends.</remarks>
    private void ShowNotification(FeedBranding branding, string title, string message, bool wait = false)
    {
#if WINDOWS
        if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 10240) && GetToastAppId(branding) is {} appId)
        {
            try
            {
                ShowToastNotification(title, message, appId);
                return;
            }
            catch
            {
                // Fall back to tray icon notification (not logged, since this may run inside a Log.Handler)
            }
        }
#endif

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

#if WINDOWS
    /// <summary>
    /// Returns the Application User Model ID to show toast notifications for; <c>null</c> if there is none Windows will accept.
    /// </summary>
    private static string? GetToastAppId(FeedBranding branding)
    {
        if (ZeroInstallInstance.IsIntegrated) return "ZeroInstall";
        if (ZeroInstallInstance.IsLibraryMode && !string.IsNullOrEmpty(branding.AppId)) return branding.AppId;
        return null;
    }

    [SupportedOSPlatform("windows10.0.10240.0")]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ShowToastNotification(string title, string message, string appId)
    {
        var doc = new XmlDocument();

        XmlElement Element(string tagName, IEnumerable<IXmlNode>? children = null, IDictionary<string, string>? attributes = null, string? innerText = null)
        {
            var element = doc.CreateElement(tagName);
            foreach (var child in children ?? [])
                element.AppendChild(child);
            foreach ((string key, string value) in attributes ?? new Dictionary<string, string>())
                element.SetAttribute(key, value);
            if (innerText != null) element.InnerText = innerText;
            return element;
        }

        doc.AppendChild(Element("toast", [
            Element("visual", [
                Element("binding", [
                    Element("text", innerText: title),
                    Element("text", innerText: message)
                ], new Dictionary<string, string> {["template"] = "ToastGeneric"})
            ])
        ]));

        ToastNotificationManager.CreateToastNotifier(appId)
                                .Show(new ToastNotification(doc));
    }
#endif
}
