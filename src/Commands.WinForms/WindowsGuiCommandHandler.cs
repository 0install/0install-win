// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System.Runtime.CompilerServices;
using NanoByte.Common.Native;
using ZeroInstall.Commands;
using Windows.Data.Xml.Dom;
using ToastNotification = Windows.UI.Notifications.ToastNotification;
using ToastNotificationManager = Windows.UI.Notifications.ToastNotificationManager;

namespace ZeroInstall.Commands.WinForms;

/// <summary>
/// Extends <see cref="GuiCommandHandler"/> with Windows-specific functionality.
/// </summary>
internal sealed class WindowsGuiCommandHandler : GuiCommandHandler
{
    /// <summary>
    /// Shows Windows toast notifications, which stick around even after the process ends.
    /// </summary>
    protected override bool TryShowSystemNotification(FeedBranding branding, string title, string message)
    {
        if (!WindowsUtils.IsWindows10 || GetToastAppId(branding) is not {} appId) return false;

        ShowToastNotification(title, message, appId);
        return true;
    }

    /// <summary>
    /// Returns the Application User Model ID to show toast notifications for; <c>null</c> if there is none Windows will accept.
    /// </summary>
    private static string? GetToastAppId(FeedBranding branding)
    {
        if (ZeroInstallInstance.IsIntegrated) return "ZeroInstall";
        if (ZeroInstallInstance.IsLibraryMode && !string.IsNullOrEmpty(branding.AppId)) return branding.AppId;
        return null;
    }

    /// <remarks>Separate method so the WinRT types are only loaded on Windows 10 or newer.</remarks>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ShowToastNotification(string title, string message, string appId)
    {
        var doc = new XmlDocument();

        XmlElement Element(string tagName, IEnumerable<IXmlNode>? children = null, IDictionary<string, string>? attributes = null, string? innerText = null)
        {
            var element = doc.CreateElement(tagName);
            foreach (var child in children ?? [])
                element.AppendChild(child);
            foreach (var pair in attributes ?? new Dictionary<string, string>())
                element.SetAttribute(pair.Key, pair.Value);
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
