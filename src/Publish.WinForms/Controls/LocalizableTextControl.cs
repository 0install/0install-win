/*
 * Copyright 2011 Simon E. Silva Lauinger
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
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using C5;
using Common.Collections;
using ZeroInstall.Publish.WinForms.Properties;

namespace ZeroInstall.Publish.WinForms.Controls
{
    /// <summary>
    /// A control that a allows the user to localize a description or a summary.
    /// </summary>
    public partial class LocalizableTextControl : UserControl
    {
        #region Properties
        private bool _multiline;

        /// <summary>
        /// The <see cref="LocalizableStringCollection"/> managed by this control.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public LocalizableStringCollection Values { private set; get; }

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
                    hintTextBoxSummary.HintText = Resources.HintTextMultiline;
                }
                else
                {
                    Size = new Size(Size.Width, 23);
                    hintTextBoxSummary.Size = new Size(hintTextBoxSummary.Size.Width, 20);
                    hintTextBoxSummary.HintText = Resources.HintTextSinlgeLine;
                }
            }
        }

        /// <summary>
        /// The selected language from <see cref="comboBoxLanguages"/> as a <see cref="CultureInfo"/>.
        /// </summary>
        /// <returns>The selected language or <see langword="null" /> if no language is selected.</returns>
        public CultureInfo SelectedLanguage
        {
            get
            {
                string languageWithMarker = comboBoxLanguages.Text;
                return CultureInfo.GetCultureInfo(languageWithMarker.StartsWith(UsingLanguageMarker)
                    ? languageWithMarker.Substring(UsingLanguageMarker.Length)
                    : languageWithMarker);
            }

            set { comboBoxLanguages.SelectedItem = (comboBoxLanguages.Items.Contains(value.IetfLanguageTag)) ? value.IetfLanguageTag : UsingLanguageMarker + value.IetfLanguageTag; }
        }
        #endregion

        #region Constants
        /// <summary>
        /// The <see cref="string"/> which marks the already translated languages.
        /// </summary>
        private const string UsingLanguageMarker = "* ";
        #endregion

        public LocalizableTextControl()
        {
            InitializeComponent();
            InitializeValuesCollection();
            InitializeComboBoxLanguages();
            InitializeHintTextBoxSummary();
        }

        /// <summary>
        /// Sets collection changed event handler for <see cref="Values"/>.
        /// </summary>
        private void InitializeValuesCollection()
        {
            Values = new LocalizableStringCollection();

            Values.CollectionChanged += delegate
            {
                var setLanguages = new SortedArray<CultureInfo>(new CultureComparer());
                var notSetLanguages = new SortedArray<CultureInfo>(new CultureComparer());

                foreach (CultureInfo language in CultureInfo.GetCultures(CultureTypes.FrameworkCultures))
                {
                    // Exclude the invariant culture since "no language" defaults to "English generic"
                    if (language.Equals(CultureInfo.InvariantCulture)) continue;

                    // seperate set languages and not set languages
                    if (Values.ContainsExactLanguage(language)) setLanguages.Add(language);
                    else notSetLanguages.Add(language);
                }

                // store selected language
                CultureInfo selectedLanguage = SelectedLanguage;

                comboBoxLanguages.Items.Clear();
                // add languages to comboBox
                comboBoxLanguages.BeginUpdate();
                foreach (CultureInfo settedLanguage in setLanguages) comboBoxLanguages.Items.Add(UsingLanguageMarker + settedLanguage.IetfLanguageTag);
                foreach (CultureInfo notSettedLanguage in notSetLanguages) comboBoxLanguages.Items.Add(notSettedLanguage.IetfLanguageTag);
                comboBoxLanguages.EndUpdate();

                // restore selected language
                SelectedLanguage = selectedLanguage;
            };
        }

        /// <summary>
        /// Adds FrameworkCultures and delegate for <see cref="ComboBox.SelectedIndexChanged"/> to <see cref="comboBoxLanguages"/>.
        /// </summary>
        private void InitializeComboBoxLanguages()
        {
            // Add FrameworkCultures to comboBox
            var sortedCultures = new SortedArray<CultureInfo>(new CultureComparer());
            sortedCultures.AddAll(CultureInfo.GetCultures(CultureTypes.FrameworkCultures));
            sortedCultures.Remove(CultureInfo.InvariantCulture);
            foreach (CultureInfo cultureInfo in sortedCultures)
                comboBoxLanguages.Items.Add(cultureInfo.IetfLanguageTag);
            comboBoxLanguages.SelectedIndex = 0;

            // Sets right translation to hintTextBoxSummary
            comboBoxLanguages.SelectedIndexChanged += delegate
            {
                hintTextBoxSummary.TextChanged -= HintTextBoxSummaryTextChanged;
                hintTextBoxSummary.Text = Values.ContainsExactLanguage(SelectedLanguage)
                    ? Values.GetExactLanguage(SelectedLanguage)
                    : string.Empty;
                hintTextBoxSummary.TextChanged += HintTextBoxSummaryTextChanged;
            };
        }

        /// <summary>
        /// Sets <see cref="Control.TextChanged"/> event handler for <see cref="hintTextBoxSummary"/>.
        /// </summary>
        private void InitializeHintTextBoxSummary()
        {
            hintTextBoxSummary.TextChanged += HintTextBoxSummaryTextChanged;
        }

        /// <summary>
        /// Removes <see cref="SelectedLanguage"/> from <see cref="Values"/> if text of <see cref="hintTextBoxSummary"/> is empty, else adds <see cref="SelectedLanguage"/> to <see cref="Values"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void HintTextBoxSummaryTextChanged(object sender, EventArgs e)
        {
            string changedSummary = hintTextBoxSummary.Text;
            // Remove language from Values if translation has been removed.
            if (string.IsNullOrEmpty(changedSummary) && Values.ContainsExactLanguage(SelectedLanguage))
                Values.RemoveAll(SelectedLanguage);
            // Add language to Values if translation was set.
            if (!string.IsNullOrEmpty(changedSummary))
                Values.Set(changedSummary, SelectedLanguage);
        }
    }
}
