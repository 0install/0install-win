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

namespace ZeroInstall.Publish.WinForms
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
            foreach (var language in CultureInfo.GetCultures(CultureTypes.SpecificCultures | CultureTypes.NeutralCultures))
                comboBoxLanguage.Items.Add(language);
            comboBoxLanguage.SelectedItem = CultureInfo.CurrentCulture;
        }

        /// <summary>
        /// Load the possible <see cref="OS" />s in "comboBoxOS".
        /// </summary>
        private void InitializeOS()
        {
            foreach (var os in Enum.GetValues(typeof(OS)))
                comboBoxOS.Items.Add(os);
            comboBoxOS.SelectedIndex = (int)OS.All;
        }
        /// <summary>
        /// Load the possible <see cref="Cpu" />s in "comboBoxLanguage".
        /// </summary>
        private void InitializeCpu()
        {
            foreach (var cpu in Enum.GetValues(typeof(Cpu)))
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
            comboBoxOS.SelectedItem = OS.All;
            comboBoxCpu.SelectedItem = Cpu.All;
        }

        private void UpdateControls()
        {
            ClearControls();
            if (_targetBase == null) return;

            // add languages to the list box
            UpdateLanguages();
            // select right values for OS and CPU in the combo boxes
            comboBoxOS.SelectedItem = _targetBase.Architecture.OS;
            comboBoxCpu.SelectedItem = _targetBase.Architecture.Cpu;
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
            foreach (var language in _targetBase.Languages)
                listBoxLanguages.Items.Add(language);
            listBoxLanguages.EndUpdate();
        }

        /// <summary>
        /// Refresh languages in <see cref="listBoxLanguages"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void listBoxLanguages_Enter(object sender, EventArgs e)
        {
            UpdateLanguages();
        }

        /// <summary>
        /// Add the selected language from <see cref="comboBoxLanguage"/> to <see cref="listBoxLanguages"/>.
        /// </summary>
        /// <remarks>Updates <see cref="_targetBase"/> .</remarks>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void btnLanguageAdd_Click(object sender, EventArgs e)
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
        private void btnLanguageRemove_Click(object sender, EventArgs e)
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
        private void btnLanguageClear_Click(object sender, EventArgs e)
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
        private void comboBoxOS_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_targetBase == null) return;
            _targetBase.Architecture = new Architecture((OS)comboBoxOS.SelectedItem, _targetBase.Architecture.Cpu);
        }

        /// <summary>
        /// Updates <see cref="comboBoxOS"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void comboBoxOS_Enter(object sender, EventArgs e)
        {
            if (_targetBase == null) return;
            comboBoxOS.SelectedItem = _targetBase.Architecture.OS;
        }

        /// <summary>
        /// Updates "_targetBase.Architecture.Cpu".
        /// </summary>
        private void comboBoxCpu_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_targetBase == null) return;
            _targetBase.Architecture = new Architecture(_targetBase.Architecture.OS, (Cpu)comboBoxCpu.SelectedItem);
        }

        /// <summary>
        /// Updates <see cref="comboBoxCpu"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void comboBoxCpu_Enter(object sender, EventArgs e)
        {
            if (_targetBase == null) return;
            comboBoxCpu.SelectedItem = _targetBase.Architecture.Cpu;
        }
        #endregion
    }
}
