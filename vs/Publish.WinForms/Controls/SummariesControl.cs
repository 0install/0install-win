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
            set {
                _summaries = value;
                UpdateControl();
            }
            get {
                return _summaries;
            }
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
            var selectedLanguage = GetLanguageFromComboBox();
            ClearControl();

            comboBoxLanguages.BeginUpdate();
            foreach (var language in CultureInfo.GetCultures(CultureTypes.SpecificCultures | CultureTypes.NeutralCultures))
            {
                comboBoxLanguages.Items.Add(_summaries.ContainsExactLanguage(language)
                                                ? UsingLanguageMarker + language.IetfLanguageTag
                                                : language.IetfLanguageTag);
            }
            if (selectedLanguage == null)
            {
                comboBoxLanguages.SelectedItem = CultureInfo.CurrentCulture.IetfLanguageTag;
            }
            else
            {
                comboBoxLanguages.SelectedItem = _summaries.ContainsExactLanguage(selectedLanguage)
                                                     ? UsingLanguageMarker + selectedLanguage.IetfLanguageTag
                                                     : selectedLanguage.IetfLanguageTag;
                hintTextBoxSummary.Text = _summaries.ContainsExactLanguage(selectedLanguage) ? _summaries.GetExactLanguage(selectedLanguage) : string.Empty;
            }
            comboBoxLanguages.EndUpdate();
        }

        private void HintTextBoxSummaryTextChanged(object sender, System.EventArgs e)
        {
            string changedSummary = hintTextBoxSummary.Text;
            if (string.IsNullOrEmpty(changedSummary) && _summaries.ContainsExactLanguage(GetLanguageFromComboBox()))
            {
                _summaries.RemoveAt(GetIndexOfLanguage(GetLanguageFromComboBox()));
                UpdateControl();
            }
            else
            {
                if (_summaries.ContainsExactLanguage(GetLanguageFromComboBox()))
                {
                    _summaries[GetIndexOfLanguage(GetLanguageFromComboBox())] = new LocalizableString(changedSummary, GetLanguageFromComboBox());
                }
                else
                {
                    _summaries.Add(new LocalizableString(changedSummary, GetLanguageFromComboBox()));
                    UpdateControl();
                }
            }
        }

        private CultureInfo GetLanguageFromComboBox()
        {
            if (comboBoxLanguages.SelectedItem == null) return null;
            string languageWithMarker = comboBoxLanguages.SelectedItem.ToString();
            return CultureInfo.GetCultureInfo(languageWithMarker.StartsWith(UsingLanguageMarker) ? languageWithMarker.Substring(UsingLanguageMarker.Length) : languageWithMarker);
        }

        private int GetIndexOfLanguage(CultureInfo toGetIndexFor)
        {
            for (int i = 0; i < _summaries.Count; i++)
            {
                var currentLanguage = _summaries[i].Language;
                if (currentLanguage != null)
                {
                    if (_summaries[i].Language.Equals(toGetIndexFor))
                        return i;
                } else if(toGetIndexFor.Equals(CultureInfo.GetCultureInfo(string.Empty)))
                {
                    return i;
                }
            }
            return -1;
        }

        private void ComboBoxLanguagesSelectedValueChanged(object sender, System.EventArgs e)
        {
            hintTextBoxSummary.Text = _summaries.ContainsExactLanguage(GetLanguageFromComboBox()) ? _summaries.GetExactLanguage(GetLanguageFromComboBox()) : string.Empty;
        }
    }
}
