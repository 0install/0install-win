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

using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using ZeroInstall.Model;

namespace ZeroInstall.Publish.WinForms.Controls
{
    /// <summary>
    /// Control to manage a <see cref="TargetBase" />. You pass an subclass of <see cref="TargetBase" /> and the values will be written to it.
    /// </summary>
    public partial class TargetBaseControl : UserControl
    {
        #region Properties
        private bool _enableComboBoxCpuSelectedIndexChanged = true;
        private bool _enableComboBoxOsSelectedIndexChanged = true;
        private TargetBase _targetBase;

        /// <summary>
        /// The <see cref="TargetBase" /> which fills the controls. If <see langword="null" /> the control resets.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TargetBase TargetBase
        {
            get { return _targetBase; }
            set
            {
                _targetBase = value;
                UpdateControls();
            }
        }
        #endregion

        # region Initialization
        public TargetBaseControl()
        {
            InitializeComponent();
            InitializeComboBoxLanguage();
            InitializeOS();
            InitializeCpu();
        }

        /// <summary>
        /// Load the possible languages in "comboBoxLanguage".
        /// </summary>
        private void InitializeComboBoxLanguage()
        {
            // load the possible languages in listBoxLanguages
            foreach (CultureInfo language in CultureInfo.GetCultures(CultureTypes.FrameworkCultures))
                comboBoxLanguage.Items.Add(language);
            comboBoxLanguage.SelectedItem = CultureInfo.CurrentCulture;
        }

        /// <summary>
        /// Lista all the possible <see cref="OS"/>s in <see cref="comboBoxOS"/>.
        /// </summary>
        private void InitializeOS()
        {
            foreach (object os in Enum.GetValues(typeof(OS)))
                comboBoxOS.Items.Add(os);
            comboBoxOS.SelectedIndex = (int)OS.All;
        }

        /// <summary>
        /// List all possible <see cref="Cpu"/>s in <see cref="comboBoxLanguage"/>.
        /// </summary>
        private void InitializeCpu()
        {
            foreach (object cpu in Enum.GetValues(typeof(Cpu)))
                comboBoxCpu.Items.Add(cpu);
            comboBoxCpu.SelectedIndex = (int)Cpu.All;
        }
        #endregion

        #region Control management methodes
        /// <summary>
        /// Set the controls values to the standard values if _targetBase is null.
        /// </summary>
        private void ClearControls()
        {
            listBoxLanguages.Items.Clear();

            _enableComboBoxOsSelectedIndexChanged = false;
            comboBoxOS.SelectedItem = OS.All;
            _enableComboBoxOsSelectedIndexChanged = true;

            _enableComboBoxCpuSelectedIndexChanged = false;
            comboBoxCpu.SelectedItem = Cpu.All;
            _enableComboBoxCpuSelectedIndexChanged = true;
        }

        private void UpdateControls()
        {
            ClearControls();
            if (_targetBase == null) return;

            // add languages to the list box
            UpdateLanguages();

            // select right values for OS and CPU in the combo boxes
            _enableComboBoxOsSelectedIndexChanged = false;
            comboBoxOS.SelectedItem = _targetBase.Architecture.OS;
            _enableComboBoxOsSelectedIndexChanged = true;

            _enableComboBoxCpuSelectedIndexChanged = false;
            comboBoxCpu.SelectedItem = _targetBase.Architecture.Cpu;
            _enableComboBoxCpuSelectedIndexChanged = true;
        }
        #endregion

        #region Language methodes
        /// <summary>
        /// Clear <see cref="listBoxLanguages"/> and write the languages from <see cref="_targetBase" /> into it.
        /// </summary>
        private void UpdateLanguages()
        {
            listBoxLanguages.Items.Clear();
            if (_targetBase == null) return;
            listBoxLanguages.BeginUpdate();
            foreach (CultureInfo language in _targetBase.Languages)
                listBoxLanguages.Items.Add(language);
            listBoxLanguages.EndUpdate();
        }

        /// <summary>
        /// Refresh languages in <see cref="listBoxLanguages"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void ListBoxLanguagesEnter(object sender, EventArgs e)
        {
            UpdateLanguages();
        }

        /// <summary>
        /// Add the selected language from <see cref="comboBoxLanguage"/> to <see cref="listBoxLanguages"/>.
        /// </summary>
        /// <remarks>Updates <see cref="_targetBase"/> .</remarks>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void BtnLanguageAddClick(object sender, EventArgs e)
        {
            if (_targetBase == null) return;
            var selectedCulture = (CultureInfo)comboBoxLanguage.SelectedItem;
            _targetBase.Languages.Add(selectedCulture);
            UpdateLanguages();
        }

        /// <summary>
        /// Remove the selected language in <see cref="listBoxLanguages"/>.
        /// </summary>
        /// <remarks>Updates <see cref="_targetBase"/>.</remarks>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void BtnLanguageRemoveClick(object sender, EventArgs e)
        {
            if (_targetBase == null) return;
            var selectedCulture = (CultureInfo)listBoxLanguages.SelectedItem;
            if (selectedCulture == null) return;
            _targetBase.Languages.Remove(selectedCulture);
            UpdateLanguages();
        }

        /// <summary>
        /// Clear all items in <see cref="listBoxLanguages"/>.
        /// </summary>
        /// <remarks>Updates <see cref="_targetBase"/>.</remarks>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void BtnLanguageClearClick(object sender, EventArgs e)
        {
            if (_targetBase == null) return;
            _targetBase.Languages.Clear();
            UpdateLanguages();
        }
        #endregion

        #region OS and Cpu methodes
        /// <summary>
        /// Updates "_targetBase.Architecture.OS".
        /// </summary>
        private void ComboBoxOsSelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_enableComboBoxOsSelectedIndexChanged || _targetBase == null || comboBoxOS.SelectedItem == null)
                return;
            _targetBase.Architecture = new Architecture((OS)comboBoxOS.SelectedItem, _targetBase.Architecture.Cpu);
        }

        /// <summary>
        /// Updates "_targetBase.Architecture.Cpu".
        /// </summary>
        private void ComboBoxCpuSelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_enableComboBoxCpuSelectedIndexChanged || _targetBase == null || comboBoxCpu.SelectedItem == null)
                return;
            _targetBase.Architecture = new Architecture(_targetBase.Architecture.OS, (Cpu)comboBoxCpu.SelectedItem);
        }
        #endregion
    }
}
