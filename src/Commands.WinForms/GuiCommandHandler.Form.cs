// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using NanoByte.Common.Threading;

namespace ZeroInstall.Commands.WinForms;

partial class GuiCommandHandler
{
    private readonly Lazy<FeedBranding> _branding;

    private FeedBranding Branding => _branding.Value;

    private readonly AsyncFormWrapper<ProgressForm> _form;

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

    private DialogResult SwitchToDialog(Func<Form> buildDialog) => _form.Post(form =>
    {
        using var dialog = buildDialog();
        dialog.Shown += delegate { form.Hide(); };
        return dialog.ShowDialog();
    });

    public GuiCommandHandler()
    {
        _branding = new(() => new(FeedUri));
        _form = new AsyncFormWrapper<ProgressForm>(() => new ProgressForm(Branding, CancellationTokenSource));
    }

    public override void Dispose()
    {
        try
        {
            _form.Dispose();
            if (_branding.IsValueCreated) Branding.Dispose();
        }
        finally
        {
            base.Dispose();
        }
    }
}
