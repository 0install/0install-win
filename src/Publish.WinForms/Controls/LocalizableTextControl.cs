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
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using C5;
using Common;
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
            get {
                return ((ToStringWrapper<CultureInfo>)comboBoxLanguages.SelectedItem).Element;
            }

            set
            {
                #region Sanity checks
                if (value == null) throw new ArgumentNullException("value");
                #endregion

                comboBoxLanguages.SelectedItem = CreateToStringWrapper(value, Values.ContainsExactLanguage(value));                
            }
        }
        #endregion

        #region Constants
        /// <summary>
        /// The <see cref="string"/> which marks the already translated languages.
        /// </summary>
        private const string TranslatedLanguageMarker = "* ";
        #endregion

        #region
        private BindingList<ToStringWrapper<CultureInfo>> _comboBoxEntries;
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
                UpdateComboBoxLanguages();
            };
        }

        /// <summary>
        /// Adds FrameworkCultures and delegate for <see cref="ComboBox.SelectedIndexChanged"/> to <see cref="comboBoxLanguages"/>.
        /// </summary>
        private void InitializeComboBoxLanguages()
        {
            FillComboBoxLanguages();
            
            comboBoxLanguages.SelectedIndexChanged += ComboBoxLanguage_SelectedIndexChanged;
        }

// ReSharper disable InconsistentNaming
        private void ComboBoxLanguage_SelectedIndexChanged(object sender, EventArgs args)
// ReSharper restore InconsistentNaming
        {
            hintTextBoxSummary.TextChanged -= HintTextBoxSummary_TextChanged;
            hintTextBoxSummary.Text = Values.ContainsExactLanguage(SelectedLanguage)
                ? Values.GetExactLanguage(SelectedLanguage)
                : string.Empty;
            hintTextBoxSummary.TextChanged += HintTextBoxSummary_TextChanged;
        }

        private void FillComboBoxLanguages()
        {
            var frameworkCultures = CultureInfo.GetCultures(CultureTypes.FrameworkCultures);
            var sortedToStringWrappers = new ArrayList<ToStringWrapper<CultureInfo>>(frameworkCultures.Length);
            foreach (var cultureInfo in frameworkCultures)
            {
                if (cultureInfo.Equals(CultureInfo.InvariantCulture)) continue;
                sortedToStringWrappers.Add(CreateToStringWrapper(cultureInfo, Values.ContainsExactLanguage(cultureInfo)));
            }
            sortedToStringWrappers.Sort(new ToStringWrapperCultureComparer());

            comboBoxLanguages.BeginUpdate();
            _comboBoxEntries=new BindingList<ToStringWrapper<CultureInfo>>(sortedToStringWrappers);
            comboBoxLanguages.DataSource = _comboBoxEntries;
            comboBoxLanguages.EndUpdate();
        }

        private void UpdateComboBoxLanguages()
        {
            CultureInfo selectedLanguage = SelectedLanguage;
            _comboBoxEntries[comboBoxLanguages.SelectedIndex] = CreateToStringWrapper(selectedLanguage, Values.ContainsExactLanguage(selectedLanguage));
            SelectedLanguage = selectedLanguage;
        }

        /// <summary>
        /// Sets <see cref="Control.TextChanged"/> event handler for <see cref="hintTextBoxSummary"/>.
        /// </summary>
        private void InitializeHintTextBoxSummary()
        {
            hintTextBoxSummary.TextChanged += HintTextBoxSummary_TextChanged;
        }

        /// <summary>
        /// Removes <see cref="SelectedLanguage"/> from <see cref="Values"/> if text of <see cref="hintTextBoxSummary"/> is empty, else adds <see cref="SelectedLanguage"/> to <see cref="Values"/>.
        /// </summary>
        /// <param name="sender">Not Used.</param>
        /// <param name="e">Not Used.</param>
// ReSharper disable InconsistentNaming
        private void HintTextBoxSummary_TextChanged(object sender, EventArgs e)
// ReSharper restore InconsistentNaming
        {
            string changedSummary = hintTextBoxSummary.Text;
            // Remove language from Values if translation has been removed.
            if (string.IsNullOrEmpty(changedSummary) && Values.ContainsExactLanguage(SelectedLanguage))
                Values.RemoveAll(SelectedLanguage);
            // Add language to Values if translation was set.
            if (!string.IsNullOrEmpty(changedSummary))
            {
                comboBoxLanguages.SelectedIndexChanged -= ComboBoxLanguage_SelectedIndexChanged;
                Values.Set(changedSummary, SelectedLanguage);
                comboBoxLanguages.SelectedIndexChanged += ComboBoxLanguage_SelectedIndexChanged;
            }
        }

        private ToStringWrapper<CultureInfo> CreateToStringWrapper(CultureInfo forCulture, bool cultureUsed)
        {
            return new ToStringWrapper<CultureInfo>(forCulture, () => cultureUsed ? TranslatedLanguageMarker + forCulture.IetfLanguageTag : forCulture.IetfLanguageTag);
        }

        private class ToStringWrapperCultureComparer : CultureComparer, IComparer<ToStringWrapper<CultureInfo>>
        {
            public int Compare(ToStringWrapper<CultureInfo> x, ToStringWrapper<CultureInfo> y)
            {
                bool isXUsed = x.ToString().StartsWith(TranslatedLanguageMarker);
                bool isYUsed = y.ToString().StartsWith(TranslatedLanguageMarker);

                int compareNumber;

                if (isXUsed == isYUsed) compareNumber = Compare(x.Element, y.Element);
                else if (isXUsed) compareNumber = -1;
                else compareNumber = 1;

                return compareNumber;
            }
        }
    }
}
