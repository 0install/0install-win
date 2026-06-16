using Eto.Forms;
using Eto.Drawing;
using EtoPlatform = Eto.Platform;

namespace ZeroInstall.Central.Eto.Demo;

/// <summary>
/// Demo application showing Eto.Forms versions of Zero Install dialogs.
/// </summary>
public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        new Application(EtoPlatform.Detect).Run(new MainForm());
    }
}

/// <summary>
/// Main form for the demo application.
/// </summary>
public class MainForm : Form
{
    public MainForm()
    {
        Title = "Zero Install Eto.Forms Demo";
        ClientSize = new Size(400, 300);
        Padding = 10;

        var label = new Label
        {
            Text = "This demo application shows Eto.Forms versions of Zero Install dialogs.\n\n" +
                   "Click the buttons below to see the converted dialogs.",
            Wrap = WrapMode.Word
        };

        var buttonPortableCreator = new Button
        {
            Text = "Show Portable Creator Dialog"
        };
        buttonPortableCreator.Click += (sender, e) =>
        {
            var dialog = new PortableCreatorDialog();
            var result = dialog.ShowModal(this);
            MessageBox.Show(this, $"Dialog result: {result}");
        };

        var buttonSelectCommand = new Button
        {
            Text = "Show Select Command Dialog"
        };
        buttonSelectCommand.Click += (sender, e) =>
        {
            var dialog = new SelectCommandDialog();
            var result = dialog.ShowModal(this);
            MessageBox.Show(this, $"Dialog result: {result}");
        };

        var buttonExit = new Button
        {
            Text = "Exit"
        };
        buttonExit.Click += (sender, e) => Application.Instance.Quit();

        Content = new TableLayout
        {
            Padding = 10,
            Spacing = new Size(5, 10),
            Rows =
            {
                new TableRow(label),
                new TableRow(buttonPortableCreator),
                new TableRow(buttonSelectCommand),
                new TableRow { ScaleHeight = true },
                new TableRow(buttonExit)
            }
        };
    }
}
