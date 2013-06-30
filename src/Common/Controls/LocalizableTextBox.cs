/*
 * Copyright 2011-2012 Simon E. Silva Lauinger
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Common.Collections;
using Common.Undo;

namespace Common.Controls
{
    /// <summary>
    /// A control for editing a <see cref="LocalizableStringCollection"/>.
    /// </summary>
    public partial class LocalizableTextBox : UserControl, IEditorControl<LocalizableStringCollection>
    {
        #region Variables
        private CultureInfo _selectedLanguage;
        private bool _textBoxDirty;
        #endregion

        #region Properties
        private LocalizableStringCollection _target;

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Target collection is injected from outside.")]
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public LocalizableStringCollection Target
        {
            get { return _target; }
            set
            {
                _target = value;
                FillComboBox();
                FillTextBox();
            }
        }

        /// <inheritdoc/>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Undo.ICommandExecutor CommandExecutor { get; set; }

        /// <summary>
        /// Controls whether the text can span more than one line.
        /// </summary>
        [DefaultValue(true)]
        [Category("Behavior"), Description("Controls whether the text can span more than one line.")]
        public bool Multiline { get { return textBox.Multiline; } set { textBox.Multiline = value; } }

        /// <summary>
        /// A text to be displayed in <see cref="SystemColors.GrayText"/> when the text box is empty.
        /// </summary>
        [Description("A text to be displayed in gray when Text is empty."), Category("Appearance"), Localizable(true)]
        public string HintText { get { return textBox.HintText; } set { textBox.HintText = value; } }
        #endregion

        #region Constructor
        public LocalizableTextBox()
        {
            InitializeComponent();
        }
        #endregion

        //--------------------//

        #region Fill
        private void FillComboBox()
        {
            var setLanguages = new List<CultureInfo>();
            var unsetLanguages = new List<CultureInfo>();
            foreach (var language in LanguageSet.KnownLanguages)
            {
                if (Target.ContainsExactLanguage(language)) setLanguages.Add(language);
                else unsetLanguages.Add(language);
            }

            if (_selectedLanguage == null)
                _selectedLanguage = setLanguages.Count == 0 ? LocalizableString.DefaultLanguage : setLanguages[0];

            comboBoxLanguage.BeginUpdate();
            comboBoxLanguage.Items.Clear();
            foreach (var language in setLanguages) comboBoxLanguage.Items.Add(language);
            foreach (var language in unsetLanguages) comboBoxLanguage.Items.Add(language);
            comboBoxLanguage.SelectedItem = _selectedLanguage;
            comboBoxLanguage.EndUpdate();
        }

        private void FillTextBox()
        {
            try
            {
                textBox.Text = Target.GetExactLanguage(_selectedLanguage);
            }
            catch (KeyNotFoundException)
            {
                textBox.Text = "";
            }
            _textBoxDirty = false;
        }
        #endregion

        #region Apply
        private void ApplyValue()
        {
            string newValue = string.IsNullOrEmpty(textBox.Text) ? null : textBox.Text;

            if (Target.GetExactLanguage(_selectedLanguage) == newValue) return;

            if (CommandExecutor == null) Target.Set(_selectedLanguage, newValue);
            else CommandExecutor.Execute(new SetLocalizableString(Target, new LocalizableString {Language = _selectedLanguage, Value = newValue}));
        }
        #endregion

        #region Event handlers
        private void comboBoxLanguage_SelectionChangeCommitted(object sender, EventArgs e)
        {
            _selectedLanguage = (CultureInfo)comboBoxLanguage.SelectedItem;
            FillTextBox();
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            _textBoxDirty = true;
        }

        private void textBox_Validating(object sender, CancelEventArgs e)
        {
            if (_selectedLanguage == null) return;

            if (_textBoxDirty)
            {
                ApplyValue();
                _textBoxDirty = false;
            }
        }

        public override void Refresh()
        {
            FillComboBox();
            FillTextBox();

            base.Refresh();
        }
        #endregion
    }
}
