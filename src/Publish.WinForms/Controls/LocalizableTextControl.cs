/*
 * Copyright 2010 Simon E. Silva Lauinger
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

using System.Drawing;
using System.Windows.Forms;
using Common.Collections;
using System.Globalization;
using System.ComponentModel;
using System;

namespace ZeroInstall.Publish.WinForms.Controls
{
    public partial class LocalizableTextControl : UserControl
    {
        #region Properties

        private LocalizableStringCollection _values = new LocalizableStringCollection();

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public LocalizableStringCollection Values
        {
            set { _values = value; UpdateControl(); }
            get { return _values; }
        }

        private bool _multiline;

        public bool Multiline
        {
            get { return _multiline; }
            set
            {
                _multiline = value;
                hintTextBoxSummary.Multiline = _multiline;
                if (_multiline)
                {
                    Size = new Size(Size.Width, 83);
                    hintTextBoxSummary.Size = new Size(hintTextBoxSummary.Size.Width, 79);
                    hintTextBoxSummary.HintText = "a full description, which can be several paragraphs long";
                }
                else
                {
                    Size = new Size(Size.Width, 23);
                    hintTextBoxSummary.Size = new Size(hintTextBoxSummary.Size.Width, 20);
                    hintTextBoxSummary.HintText = "a short one-line description";
                }
            }
        }
        #endregion

        #region Constants

        private const string UsingLanguageMarker = "* ";

        #endregion

        public LocalizableTextControl()
        {
            InitializeComponent();
            UpdateControl();
        }

        private void ClearControl()
        {
            comboBoxLanguages.Items.Clear();
            hintTextBoxSummary.Text = string.Empty;
        }

        private void UpdateControl()
        {
            ClearControl();

            UpdateComboBoxLanguages();
            if (comboBoxLanguages.Items.Count != 0) comboBoxLanguages.SelectedIndex = 0;
            var selectedLanguage = GetSelectedLanguage();
            hintTextBoxSummary.Text = _values.ContainsExactLanguage(selectedLanguage)
                ? _values.GetExactLanguage(selectedLanguage)
                : string.Empty;
        }

        private void UpdateComboBoxLanguages()
        {
            var setLanguages = new C5.SortedArray<CultureInfo>(new CultureComparer());
            var notSetLanguages = new C5.SortedArray<CultureInfo>(new CultureComparer());

            foreach (var language in CultureInfo.GetCultures(CultureTypes.FrameworkCultures))
            {
                // Exclude the invariant culture since "no language" defaults to "English generic"
                if (language.Equals(CultureInfo.InvariantCulture)) continue;

                if(_values.ContainsExactLanguage(language)) {
                    setLanguages.Add(language);
                } else {
                    notSetLanguages.Add(language);
                }
            }

            comboBoxLanguages.BeginUpdate();
            foreach(var settedLanguage in setLanguages) {
                comboBoxLanguages.Items.Add(UsingLanguageMarker + settedLanguage.IetfLanguageTag);
            }
            foreach (var notSettedLanguage in notSetLanguages)
            {
                comboBoxLanguages.Items.Add(notSettedLanguage.IetfLanguageTag);
            }
            comboBoxLanguages.EndUpdate();
        }

        /// <summary>
        /// Returns the selected language from <see cref="comboBoxLanguages"/> as a <see cref="CultureInfo"/>.
        /// </summary>
        /// <returns>The selected language or <see langword="null" /> if no language is selected.</returns>
        private CultureInfo GetSelectedLanguage()
        {
            string languageWithMarker = comboBoxLanguages.Text;
            return
                CultureInfo.GetCultureInfo(languageWithMarker.StartsWith(UsingLanguageMarker)
                                               ? languageWithMarker.Substring(UsingLanguageMarker.Length)
                                               : languageWithMarker);
        }

        private void ComboBoxLanguagesSelectionChangeCommitted(object sender, EventArgs e)
        {
            var selectedLanguage = GetSelectedLanguage();
            hintTextBoxSummary.Text = _values.ContainsExactLanguage(selectedLanguage)
                                          ? _values.GetExactLanguage(selectedLanguage)
                                          : string.Empty;
        }

        private void HintTextBoxSummaryValidating(object sender, CancelEventArgs e)
        {
            string changedSummary = hintTextBoxSummary.Text;
            var selectedLanguage = GetSelectedLanguage();
            if (string.IsNullOrEmpty(changedSummary) && _values.ContainsExactLanguage(selectedLanguage))
                _values.RemoveAll(selectedLanguage);
            if (!string.IsNullOrEmpty(changedSummary))
                _values.Set(changedSummary, selectedLanguage);

            UpdateComboBoxLanguages();
        }
    }
}