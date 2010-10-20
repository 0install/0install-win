using System.Windows.Forms;
using Common.Collections;
using System.Globalization;

namespace ZeroInstall.Publish.WinForms.Controls
{
    public partial class SummariesControl : UserControl
    {
        #region Properties

        private LocalizableStringCollection _summaries = new LocalizableStringCollection();

        public LocalizableStringCollection Summaries
        {
            set
            {
                _summaries = value;
                UpdateControl();
            }
            get { return _summaries; }
        }

        #endregion

        #region Constants

        private const string UsingLanguageMarker = "* ";

        #endregion

        public SummariesControl()
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
            var selectedLanguage = GetLanguageFromComboBox();
            hintTextBoxSummary.Text = _summaries.ContainsExactLanguage(selectedLanguage)
                                          ? _summaries.GetExactLanguage(selectedLanguage)
                                          : string.Empty;
        }

        private void UpdateComboBoxLanguages()
        {
            var selectedLanguage = GetLanguageFromComboBox();
            comboBoxLanguages.BeginUpdate();
            foreach (var language in CultureInfo.GetCultures(CultureTypes.SpecificCultures | CultureTypes.NeutralCultures))
            {
                comboBoxLanguages.Items.Add(_summaries.ContainsExactLanguage(language)
                                                ? UsingLanguageMarker + language.IetfLanguageTag
                                                : language.IetfLanguageTag);
            }

            comboBoxLanguages.SelectedItem = _summaries.ContainsExactLanguage(selectedLanguage)
                                                 ? UsingLanguageMarker + selectedLanguage.IetfLanguageTag
                                                 : selectedLanguage.IetfLanguageTag;
            comboBoxLanguages.EndUpdate();
        }

        /// <summary>
        /// Returns the selected language from <see cref="comboBoxLanguages"/> as a <see cref="CultureInfo"/>.
        /// </summary>
        /// <returns>The selected language or <see langword="null" /> if no language is selected.</returns>
        private CultureInfo GetLanguageFromComboBox()
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
            var selectedLanguage = GetLanguageFromComboBox();
            if (string.IsNullOrEmpty(changedSummary) && _summaries.ContainsExactLanguage(selectedLanguage))
                _summaries.RemoveAll(selectedLanguage);
            if(!string.IsNullOrEmpty(changedSummary))
                _summaries.Set(changedSummary, selectedLanguage);

            UpdateComboBoxLanguages();
        }

        private void ComboBoxLanguagesSelectionChangeCommitted(object sender, System.EventArgs e)
        {
            var selectedLanguage = GetLanguageFromComboBox();
            hintTextBoxSummary.Text = _summaries.ContainsExactLanguage(selectedLanguage)
                                          ? _summaries.GetExactLanguage(selectedLanguage)
                                          : string.Empty;
        }
    }
}