using System.Windows.Forms;
using Common.Collections;
using System.Globalization;
using System.Collections.Generic;

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

        private void UpdateControl()
        {
            comboBoxLanguages.BeginUpdate();
            comboBoxLanguages.Items.Clear();
            foreach (var language in CultureInfo.GetCultures(CultureTypes.SpecificCultures | CultureTypes.NeutralCultures))
            {
                comboBoxLanguages.Items.Add(_summaries.ContainsLanguage(language) ? UsingLanguageMarker + language.DisplayName : language.DisplayName);   
            }
            comboBoxLanguages.EndUpdate();
        }

        private void hintTextBoxSummary_TextChanged(object sender, System.EventArgs e)
        {

        }

        private CultureInfo GetLanguageFromComboBox()
        {
            string languageWithMarker = comboBoxLanguages.SelectedValue.ToString();
            if(languageWithMarker.StartsWith(UsingLanguageMarker)
                return CultureInfo.CreateSpecificCulture(languageWithMarker.Substring(UsingLanguageMarker.Length));
            else
                return CultureInfo.CreateSpecificCulture(languageWithMarker);
        }
    }
}
