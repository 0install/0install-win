using System.Drawing;
using System.Windows.Forms;
using Common.Collections;
using System.Globalization;

namespace ZeroInstall.Publish.WinForms.Controls
{
    public partial class LocalizableTextControl : UserControl
    {
        #region Properties

        private LocalizableStringCollection _values = new LocalizableStringCollection();

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
            comboBoxLanguages.SelectedIndex = 0;
            var selectedLanguage = GetSelectedLanguage();
            hintTextBoxSummary.Text = _values.ContainsExactLanguage(selectedLanguage)
                                          ? _values.GetExactLanguage(selectedLanguage)
                                          : string.Empty;
        }

        private void UpdateComboBoxLanguages()
        {
            var selectedLanguage = GetSelectedLanguage();
            comboBoxLanguages.BeginUpdate();
            foreach (var language in CultureInfo.GetCultures(CultureTypes.SpecificCultures | CultureTypes.NeutralCultures))
            {
                comboBoxLanguages.Items.Add(_values.ContainsExactLanguage(language)
                                                ? UsingLanguageMarker + language.IetfLanguageTag
                                                : language.IetfLanguageTag);
            }

            comboBoxLanguages.SelectedItem = _values.ContainsExactLanguage(selectedLanguage)
                                                 ? UsingLanguageMarker + selectedLanguage.IetfLanguageTag
                                                 : selectedLanguage.IetfLanguageTag;
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

        private void HintTextBoxSummaryTextChanged(object sender, System.EventArgs e)
        {
            string changedSummary = hintTextBoxSummary.Text;
            var selectedLanguage = GetSelectedLanguage();
            if (string.IsNullOrEmpty(changedSummary) && _values.ContainsExactLanguage(selectedLanguage))
                _values.RemoveAll(selectedLanguage);
            if(!string.IsNullOrEmpty(changedSummary))
                _values.Set(changedSummary, selectedLanguage);

            UpdateComboBoxLanguages();
        }

        private void ComboBoxLanguagesSelectionChangeCommitted(object sender, System.EventArgs e)
        {
            var selectedLanguage = GetSelectedLanguage();
            hintTextBoxSummary.Text = _values.ContainsExactLanguage(selectedLanguage)
                                          ? _values.GetExactLanguage(selectedLanguage)
                                          : string.Empty;
        }
    }
}