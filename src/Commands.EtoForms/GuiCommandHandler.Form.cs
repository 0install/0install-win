// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using NanoByte.Common.Threading;

namespace ZeroInstall.Commands.EtoForms;

partial class GuiCommandHandler
{
    private readonly Lazy<FeedBranding> _branding;

    private FeedBranding Branding => _branding.Value;

    private readonly object _brandingLock = new();

    private bool _brandingDisposed;

    /// <summary>
    /// Returns <see cref="Branding"/> only if it is safe to access; <c>null</c> otherwise.
    /// </summary>
    private FeedBranding? TryGetBranding()
    {
        lock (_brandingLock)
        {
            // Skipped while the branding is still being loaded, because loading it writes to the Log itself and re-entering the Lazy<> from a Log.Handler would dead-lock
            if (!_branding.IsValueCreated) return null;

            // Skipped after disposal, because a Log.Handler may still be running on other threads
            if (_brandingDisposed) return null;

            return Branding;
        }
    }

    private readonly EtoFormDispatcher<ProgressForm> _form;

    private void ShowForm(Action<ProgressForm> action) => _form.Post(form =>
    {
        ShowOnce(form);
        action(form);
    });

    private T ShowForm<T>(Func<ProgressForm, T> action) => _form.Post(form =>
    {
        ShowOnce(form);
        return action(form);
    });

    private bool _shown;

    private void ShowOnce(ProgressForm form)
    {
        if (_shown) return;
        _shown = true;

        if (Background) form.ShowTrayIcon();
        else form.Show();
    }

    private void SwitchToDialog(Func<Dialog> buildDialog) => _form.Post(form =>
    {
        using var dialog = PrepareDialog(form, buildDialog());
        dialog.ShowModal();
    });

    private bool SwitchToDialog(Func<Dialog<bool>> buildDialog) => _form.Post(form =>
    {
        using var dialog = PrepareDialog(form, buildDialog());
        return dialog.ShowModal();
    });

    private static T PrepareDialog<T>(ProgressForm form, T dialog) where T : Dialog
    {
        dialog.LoadComplete += delegate { dialog.CenterOnScreen(); }; // No owner to center on, final size is known
        dialog.Shown += delegate { form.Visible = false; };
        return dialog;
    }

    public GuiCommandHandler()
    {
        _branding = new(() => new(FeedUri));
        _form = new(() => new ProgressForm(Branding, CancellationTokenSource));
    }

    public override void Dispose()
    {
        try
        {
            _form.Dispose();

            lock (_brandingLock)
            {
                _brandingDisposed = true;
                if (_branding.IsValueCreated) Branding.Dispose();
            }
        }
        finally
        {
            base.Dispose();
        }
    }
}
