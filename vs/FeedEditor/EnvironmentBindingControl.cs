using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using ZeroInstall.Model;

namespace ZeroInstall.FeedEditor
{
    public partial class EnvironmentBindingControl : UserControl
    {
        private EnvironmentBinding _environmentBinding = new EnvironmentBinding();

        public EnvironmentBinding EnvironmentBinding
        {
            get { return _environmentBinding; }
            set
            {
                EnvironmentBinding = (value ?? new EnvironmentBinding());
                UpdateControl();
            }
        }

        private void UpdateControl()
        {
            ClearControl();
            if (_environmentBinding.Name != null) comboBoxName.SelectedItem = _environmentBinding.Name;
            if (_environmentBinding.Value != null) hintTextBoxInsert.Text = _environmentBinding.Value;
            comboBoxMode.SelectedItem = _environmentBinding.Mode;
            if (_environmentBinding.Default != null) hintTextBoxDefault.Text = _environmentBinding.Default;
        }

        private void ClearControl()
        {
            comboBoxName.SelectedIndex = 0;
            hintTextBoxInsert.Text = String.Empty;
            comboBoxMode.SelectedIndex = 0;
            hintTextBoxDefault.Text = String.Empty;
        }

        public EnvironmentBindingControl()
        {
            InitializeComponent();
            InitializeComboBoxMode();
        }

        private void InitializeComboBoxMode()
        {
            foreach (var mode in Enum.GetValues(typeof(EnvironmentMode)))
            {
                comboBoxMode.Items.Add(mode);
            }
        }

        private void hintTextBoxInsert_TextChanged(object sender, EventArgs e)
        {
            var inputText = hintTextBoxInsert.Text;
            _environmentBinding.Value = String.IsNullOrEmpty(inputText) ? null : inputText;
        }

        private void comboBoxMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            _environmentBinding.Mode = (EnvironmentMode) comboBoxName.SelectedItem;
        }

        private void hintTextBoxDefault_TextChanged(object sender, EventArgs e)
        {
            var inputText = hintTextBoxDefault.Text;
            _environmentBinding.Default = String.IsNullOrEmpty(inputText) ? null : inputText;
        }

        private void comboBoxName_TextChanged(object sender, EventArgs e)
        {
            var selectedText = comboBoxName.SelectedText;
            _environmentBinding.Name = String.IsNullOrEmpty(selectedText) ? null : selectedText;
        }

    }
}
