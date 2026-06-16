// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System.Resources;
using ZeroInstall.Central.Eto.Properties;

namespace ZeroInstall.Central.Eto;

/// <summary>
/// Eto.Forms version of the select command dialog.
/// A simplified dialog for selecting command options.
/// </summary>
public sealed class SelectCommandDialog : Dialog<bool>
{
    private readonly ResourceManager _resources;
    private readonly ComboBox _comboBoxVersion;
    private readonly ComboBox _comboBoxCommand;
    private readonly Label _labelSummary;
    private readonly CheckBox _checkBoxCustomize;
    private readonly CheckBox _checkBoxPin;
    private readonly CheckBox _checkBoxUnpin;
    private readonly CheckBox _checkBoxRefresh;
    private readonly TextBox _textBoxArgs;
    private readonly TextBox _textBoxCommandLine;
    private readonly Button _buttonReload;
    private readonly Button _buttonOK;
    private readonly Button _buttonCancel;

    public SelectCommandDialog()
    {
        // Load resources for localization
        _resources = new ResourceManager(
            "ZeroInstall.Central.Eto.SelectCommandDialog",
            typeof(SelectCommandDialog).Assembly);

        Title = Resources.Run;
        Resizable = true;
        Padding = 10;
        MinimumSize = new Size(500, 400);

        // Create controls
        var labelVersion = new Label { Text = GetString("labelVersion.Text") };
        _comboBoxVersion = new ComboBox { };
        _comboBoxVersion.TextChanged += UpdateLabels;

        _buttonReload = new Button { Text = GetString("buttonReload.Text") };
        _buttonReload.Click += ButtonReload_Click;

        var labelCommand = new Label { Text = GetString("labelCommand.Text") };
        _comboBoxCommand = new ComboBox { };
        _comboBoxCommand.TextChanged += UpdateLabels;

        _labelSummary = new Label
        {
            Text = "",
            Wrap = WrapMode.Word
        };

        var labelOptions = new Label { Text = GetString("labelOptions.Text") };
        
        _checkBoxCustomize = new CheckBox { Text = GetString("checkBoxCustomize.Text") };
        _checkBoxCustomize.CheckedChanged += UpdateLabels;
        
        _checkBoxPin = new CheckBox { Text = GetString("checkBoxPin.Text") };
        _checkBoxPin.CheckedChanged += CheckBoxPin_CheckedChanged;
        
        _checkBoxUnpin = new CheckBox { Text = GetString("checkBoxUnpin.Text") };
        _checkBoxUnpin.CheckedChanged += CheckBoxUnpin_CheckedChanged;
        
        _checkBoxRefresh = new CheckBox { Text = GetString("checkBoxRefresh.Text") };
        _checkBoxRefresh.CheckedChanged += UpdateLabels;

        var labelArgs = new Label { Text = GetString("labelArgs.Text") };
        _textBoxArgs = new TextBox();
        _textBoxArgs.TextChanged += UpdateLabels;

        var groupBoxCommandLine = new GroupBox
        {
            Text = GetString("groupBoxCommandLine.Text"),
            Padding = 10
        };
        
        _textBoxCommandLine = new TextBox { ReadOnly = true };
        groupBoxCommandLine.Content = _textBoxCommandLine;

        _buttonOK = new Button { Text = Resources.OK };
        _buttonOK.Click += ButtonOK_Click;
        
        _buttonCancel = new Button { Text = Resources.Cancel };
        _buttonCancel.Click += (_, _) => Close(false);

        // Layout
        var versionLayout = new TableLayout
        {
            Spacing = new Size(5, 5),
            Rows =
            {
                new TableRow(
                    new TableCell(labelVersion, false),
                    new TableCell(_comboBoxVersion, true),
                    new TableCell(_buttonReload, false)
                )
            }
        };

        var optionsLayout = new StackLayout
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Items =
            {
                _checkBoxCustomize,
                _checkBoxPin,
                _checkBoxUnpin,
                _checkBoxRefresh
            }
        };

        Content = new TableLayout
        {
            Padding = 10,
            Spacing = new Size(5, 10),
            Rows =
            {
                versionLayout,
                new TableRow(labelCommand),
                new TableRow(_comboBoxCommand),
                new TableRow(_labelSummary),
                new TableRow(labelOptions),
                new TableRow(optionsLayout),
                new TableRow(labelArgs),
                new TableRow(_textBoxArgs),
                new TableRow(groupBoxCommandLine),
                new TableRow { ScaleHeight = true }
            }
        };

        // Buttons
        PositiveButtons.Add(_buttonOK);
        NegativeButtons.Add(_buttonCancel);
        DefaultButton = _buttonOK;
        AbortButton = _buttonCancel;

        UpdateLabels(null, EventArgs.Empty);
    }

    private string GetString(string name)
    {
        try
        {
            var value = _resources.GetString(name);
            if (value == null)
            {
                // Log missing resource key (in production, use proper logging)
                System.Diagnostics.Debug.WriteLine($"Missing resource key: {name}");
                return name;
            }
            return value;
        }
        catch (System.Resources.MissingManifestResourceException ex)
        {
            // Resource file not found
            System.Diagnostics.Debug.WriteLine($"Resource file not found: {ex.Message}");
            return name;
        }
        catch (System.Resources.MissingSatelliteAssemblyException ex)
        {
            // Satellite assembly not found for current culture
            System.Diagnostics.Debug.WriteLine($"Satellite assembly not found: {ex.Message}");
            return name;
        }
    }

    private void UpdateLabels(object? sender, EventArgs e)
    {
        _textBoxCommandLine.Text = "0install run [options]";
    }

    private void CheckBoxPin_CheckedChanged(object? sender, EventArgs e)
    {
        if (_checkBoxPin.Checked == true)
            _checkBoxUnpin.Checked = false;
        UpdateLabels(sender, e);
    }

    private void CheckBoxUnpin_CheckedChanged(object? sender, EventArgs e)
    {
        if (_checkBoxUnpin.Checked == true)
            _checkBoxPin.Checked = false;
        UpdateLabels(sender, e);
    }

    private void ButtonReload_Click(object? sender, EventArgs e)
    {
        // In a real implementation, this would reload the feed
        MessageBox.Show(this, "Reload functionality not implemented in proof of concept", MessageBoxType.Information);
    }

    private void ButtonOK_Click(object? sender, EventArgs e)
    {
        // In a real implementation, this would execute the command
        Close(true);
    }

    /// <summary>
    /// Shows the dialog.
    /// </summary>
    /// <param name="parent">The parent window.</param>
    /// <returns>True if the user confirmed; false if cancelled.</returns>
    public bool ShowDialog(Window parent)
    {
        return base.ShowModal(parent);
    }
}
