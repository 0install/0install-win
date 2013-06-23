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

using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Common.Collections;

namespace Common.Controls
{
    /// <summary>
    /// A control for editing a <see cref="LocalizableStringCollection"/>.
    /// </summary>
    public partial class LocalizableTextBox : UserControl, IEditorControl<LocalizableStringCollection>
    {
        #region Properties
        private LocalizableStringCollection _target;

        /// <inheritdoc/>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public LocalizableStringCollection Target
        {
            get { return _target; }
            set
            {
                _target = value;
                FillComboBoxLanguages();
            }
        }

        /// <inheritdoc/>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Undo.ICommandExecutor CommandExecutor { get; set; }
        #endregion

        public LocalizableTextBox()
        {
            InitializeComponent();
        }

        private static readonly CultureInfo[] _cultures = CultureInfo.GetCultures(CultureTypes.FrameworkCultures);

        private void FillComboBoxLanguages()
        {
            var languages = _cultures.Select(culture => new Language {Culture = culture, IsValueSet = Target.ContainsExactLanguage(culture)});

            comboBoxLanguage.BeginUpdate();
            comboBoxLanguage.DataSource = languages;
            comboBoxLanguage.EndUpdate();
        }

        private class Language
        {
            public CultureInfo Culture;
            public bool IsValueSet;

            public override string ToString()
            {
                return IsValueSet
                    ? "* " + Culture.IetfLanguageTag
                    : Culture.IetfLanguageTag;
            }
        }

        private void textBox_Validating(object sender, CancelEventArgs e)
        {
            var language = comboBoxLanguage.SelectedItem as Language;
            if (language == null) return;

            var command = new Undo.SetLocalizableString(Target, new LocalizableString(textBox.Text, language.Culture));
            CommandExecutor.Execute(command);
        }

        public override void Refresh()
        {
            FillComboBoxLanguages();
            base.Refresh();
        }
    }
}
