// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System.Runtime.CompilerServices;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using NanoByte.Common.Native;

namespace ZeroInstall.Commands.WinForms;

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
        // Avoid dead-lock
        if (!_branding.IsValueCreated) return;

        ShowNotification(
            title: Branding.Name ?? "Zero Install",
            message,
            icon: severity switch
            {
                LogSeverity.Info => ToolTipIcon.Info,
                LogSeverity.Warn => ToolTipIcon.Warning,
                LogSeverity.Error => ToolTipIcon.Error,
                _ => ToolTipIcon.None
            });
    }

    /// <summary>
    /// Displays a notification message detached from the main GUI. Will stick around even after the process ends.
    /// </summary>
    /// <param name="title">The title of the message.</param>
    /// <param name="message">The message text.</param>
    /// <param name="icon">The icon to display next to the notification.</param>
    private void ShowNotification(string title, string message, ToolTipIcon icon = ToolTipIcon.None)
    {
        void Classic()
        {
            try
            {
                new NotifyIcon {Visible = true, Text = Branding.Name ?? "Zero Install", Icon = Branding.Icon}
                   .ShowBalloonTip(10000, title, message, icon);
            }
            catch (Win32Exception)
            {
                // Notifications are only shown with best effort
            }
        }

        void Modern(string appId)
        {
            try
            {
                ShowNotificationModern(title, message, appId);
            }
            catch
            {
                Classic();
            }
        }

        if (WindowsUtils.IsWindows10)
        {
            if (ZeroInstallInstance.IsIntegrated)
                Modern(appId: "ZeroInstall");
            else if (ZeroInstallInstance.IsLibraryMode && !string.IsNullOrEmpty(Branding.AppId))
                Modern(Branding.AppId);
            else Classic();
        }
        else Classic();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ShowNotificationModern(string title, string message, string appId)
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
}
