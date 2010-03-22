using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using ZeroInstall.Model;

namespace ZeroInstall.FeedEditor
{
    /// <summary>
    /// Control to manage a <see cref="TargetBase" />. You pass an subclass of <see cref="TargetBase" /> and the values will be written to it.
    /// </summary>
    public partial class TargetBaseControl : UserControl
    {
        #region Properties
        private TargetBase _targetBase;
        /// <summary>
        /// The <see cref="TargetBase" /> which fills the controls. If <see langword="null" /> the control resets.
        /// </summary>
        [Browsable(false)]
        public TargetBase TargetBase
        {
            get { return _targetBase; }
            set
            {
                _targetBase = value;
                if (_targetBase == null)
                {
                    ResetControls();
                    return;
                }
                // add languages to the list box
                UpdateLanguages();
                // select right values for OS and CPU in the combo boxes
                comboBoxOS.SelectedItem = _targetBase.Architecture.OS;
                comboBoxCpu.SelectedItem = _targetBase.Architecture.Cpu;
            }
        }
        #endregion

        public TargetBaseControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Load the possible languages from <see cref="Model.TargetBase.Languages" />, OSs from <see cref="Architecture.OS" />
        /// and CPUs from <see cref="Architecture.Cpu" /> in the controls.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TargetBaseControl_Load(object sender, EventArgs e)
        {
            // load the possible languages in listBoxLanguages
            foreach (var lang in CultureInfo.GetCultures(CultureTypes.SpecificCultures | CultureTypes.NeutralCultures))
                comboBoxLanguage.Items.Add(lang);
            comboBoxLanguage.SelectedItem = CultureInfo.CurrentCulture;

            // load the possible OSs in comboBoxOS
            foreach (var os in Enum.GetValues(typeof(OS)))
                comboBoxOS.Items.Add(os);
            comboBoxOS.SelectedIndex = (int)OS.All;

            // load the possible CPUs in comboBoxCPU
            foreach (var cpu in Enum.GetValues(typeof(Cpu)))
                comboBoxCpu.Items.Add(cpu);
            comboBoxCpu.SelectedIndex = (int)Cpu.All;
        }

        /// <summary>
        /// Add the selected language from comboBoxLanguage to listBoxLanguages.
        /// </summary>
        /// <remarks>Updates _targetBase .</remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLanguageAdd_Click(object sender, EventArgs e)
        {
            if (_targetBase == null) return;
            var selectedCulture = (CultureInfo)comboBoxLanguage.SelectedItem;
            _targetBase.Languages.Add(selectedCulture);
            UpdateLanguages();
        }

        /// <summary>
        /// Remove the selected language in listBoxLanguages.
        /// </summary>
        /// <remarks>Updates _targetBase .</remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLanguageRemove_Click(object sender, EventArgs e)
        {
            if (_targetBase == null) return;
            var selectedCulture = (CultureInfo)listBoxLanguages.SelectedItem;
            if (selectedCulture == null) return;
            _targetBase.Languages.Remove(selectedCulture);
            UpdateLanguages();
        }

        /// <summary>
        /// Clear all items in listBoxLanguages.
        /// </summary>
        /// <remarks>Updates _targetBase .</remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLanguageClear_Click(object sender, EventArgs e)
        {
            if (_targetBase == null) return;
            _targetBase.Languages.Clear();
            UpdateLanguages();
        }

        /// <summary>
        /// Updates _targetBase.Architecture.OS .
        /// </summary>
        private void comboBoxOS_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_targetBase == null) return;
            _targetBase.Architecture = new Architecture((OS)comboBoxOS.SelectedItem, _targetBase.Architecture.Cpu);
        }

        /// <summary>
        /// Updates _targetBase.Architecture.Cpu .
        /// </summary>
        private void comboBoxCPU_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_targetBase == null) return;
            _targetBase.Architecture = new Architecture(_targetBase.Architecture.OS, (Cpu)comboBoxCpu.SelectedItem);
        }

        /// <summary>
        /// Set the controls values to the standard values if _targetBase is null.
        /// </summary>
        private void ResetControls()
        {
            UpdateLanguages();
            comboBoxOS.SelectedItem = OS.All;
            comboBoxCpu.SelectedItem = Cpu.All;
        }

        /// <summary>
        /// Updates comboBoxOS.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBoxOS_Enter(object sender, EventArgs e)
        {
            if (_targetBase == null) return;
            comboBoxOS.SelectedItem = _targetBase.Architecture.OS;
        }

        /// <summary>
        /// Updates comboBoxCPU.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBoxCPU_Enter(object sender, EventArgs e)
        {
            if (_targetBase == null) return;
            comboBoxCpu.SelectedItem = _targetBase.Architecture.Cpu;
        }

        /// <summary>
        /// Clear language list box and write the languages from <see cref="_targetBase" /> into it.
        /// </summary>
        private void UpdateLanguages()
        {
            listBoxLanguages.Items.Clear();
            if (_targetBase == null) return;
            listBoxLanguages.BeginUpdate();
            foreach (var language in _targetBase.Languages)
                listBoxLanguages.Items.Add(language);
            listBoxLanguages.EndUpdate();
        }

        /// <summary>
        /// Clear language list box and write the languages from <see cref="_targetBase" /> into it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBoxLanguages_Enter(object sender, EventArgs e)
        {
            UpdateLanguages();
        }
    }
}