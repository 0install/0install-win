// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System.Resources;
using ZeroInstall.Commands.Desktop;
using ZeroInstall.Central.Eto.Properties;

namespace ZeroInstall.Central.Eto;

/// <summary>
/// Eto.Forms version of the portable creator dialog.
/// </summary>
public sealed class PortableCreatorDialog : Dialog<bool>
{
    private readonly TextBox _textBoxTarget;
    private readonly Button _buttonBrowse;
    private readonly Button _buttonDeploy;
    private readonly Button _buttonCancel;
    private readonly TextBox _textBoxCommandLine;
    private readonly ResourceManager _resources;

    public PortableCreatorDialog()
    {
        // Load resources for localization
        _resources = new ResourceManager(
            "ZeroInstall.Central.Eto.PortableCreatorDialog",
            typeof(PortableCreatorDialog).Assembly);

        Title = GetString("$this.Text");
        Resizable = false;
        Padding = 10;
        MinimumSize = new Size(500, 300);

        // Create controls
        var labelInfo = new Label
        {
            Text = GetString("labelInfo.Text"),
            Wrap = WrapMode.Word
        };

        _textBoxTarget = new TextBox
        {
            PlaceholderText = GetString("textBoxTarget.HintText")
        };
        _textBoxTarget.TextChanged += TextBoxTarget_TextChanged;

        _buttonBrowse = new Button
        {
            Text = GetString("buttonBrowse.Text"),
            Width = 30
        };
        _buttonBrowse.Click += ButtonBrowse_Click;

        var labelInfo2 = new Label
        {
            Text = GetString("labelInfo2.Text"),
            Wrap = WrapMode.Word
        };

        _textBoxCommandLine = new TextBox
        {
            ReadOnly = true
        };

        var groupBoxCommandLine = new GroupBox
        {
            Text = GetString("groupBoxCommandLine.Text"),
            Padding = 10,
            Content = _textBoxCommandLine
        };

        _buttonDeploy = new Button
        {
            Text = GetString("buttonDeploy.Text").Replace("&", ""),
            Enabled = false
        };
        _buttonDeploy.Click += ButtonDeploy_Click;

        _buttonCancel = new Button
        {
            Text = GetString("buttonCancel.Text")
        };
        _buttonCancel.Click += (_, _) => Close(false);

        // Layout
        var targetLayout = new TableLayout
        {
            Spacing = new Size(5, 5),
            Rows =
            {
                new TableRow(
                    new TableCell(_textBoxTarget, true),
                    new TableCell(_buttonBrowse, false)
                )
            }
        };

        Content = new TableLayout
        {
            Padding = 10,
            Spacing = new Size(5, 10),
            Rows =
            {
                new TableRow(labelInfo),
                new TableRow(targetLayout),
                new TableRow(labelInfo2),
                new TableRow(groupBoxCommandLine),
                new TableRow { ScaleHeight = true }
            }
        };

        // Buttons
        PositiveButtons.Add(_buttonDeploy);
        NegativeButtons.Add(_buttonCancel);
        DefaultButton = _buttonDeploy;
        AbortButton = _buttonCancel;
    }

    private string GetString(string name)
    {
        try
        {
            return _resources.GetString(name) ?? name;
        }
        catch
        {
            return name;
        }
    }

    private void TextBoxTarget_TextChanged(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_textBoxTarget.Text))
        {
            _buttonDeploy.Enabled = false;
            _textBoxCommandLine.Text = "";
        }
        else
        {
            _buttonDeploy.Enabled = true;
            _textBoxCommandLine.Text = new[] { "0install", Self.Name, Self.Deploy.Name, "--portable", _textBoxTarget.Text }.JoinEscapeArguments();
        }
    }

    private void ButtonBrowse_Click(object? sender, EventArgs e)
    {
        using var folderDialog = new SelectFolderDialog
        {
            Directory = _textBoxTarget.Text
        };

        if (folderDialog.ShowDialog(this) == DialogResult.Ok)
        {
            _textBoxTarget.Text = folderDialog.Directory;
            _textBoxTarget.Focus();
        }
    }

    private async void ButtonDeploy_Click(object? sender, EventArgs e)
    {
        if (Directory.Exists(_textBoxTarget.Text) && Directory.GetFileSystemEntries(_textBoxTarget.Text).Length != 0)
        {
            var result = MessageBox.Show(
                this,
                string.Format(Resources.PortableDirNotEmptyAsk, _textBoxTarget.Text),
                MessageBoxButtons.YesNo,
                MessageBoxType.Warning);

            if (result != DialogResult.Yes)
                return;
        }

        Enabled = false;
        
        // Note: CommandUtils.RunAsync is a WinForms-specific utility
        // For this proof of concept, we're showing the structure
        await Task.Run(() =>
        {
            // In a real implementation, this would call the deployment logic
            // For now, we'll just simulate it
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "0install",
                Arguments = $"{Self.Name} {Self.Deploy.Name} --portable \"{_textBoxTarget.Text}\" --restart-central",
                UseShellExecute = true
            });
        });
        
        Close(true);
    }

    /// <summary>
    /// Shows the dialog.
    /// </summary>
    /// <param name="parent">The parent window.</param>
    /// <returns>True if the user completed the deployment; false if cancelled.</returns>
    public bool ShowDialog(Window parent)
    {
        return base.ShowModal(parent);
    }
}
