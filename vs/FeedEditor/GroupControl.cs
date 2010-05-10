using System;
using System.Drawing;
using System.Windows.Forms;
using ZeroInstall.Model;
using System.IO;

namespace ZeroInstall.FeedEditor
{
    public partial class GroupControl : UserControl
    {
        /// <summary>
        /// The <see cref="Group" /> to be displayed and modified by this control.
        /// </summary>
        private Group _group = new Group();

        /// <summary>
        /// The <see cref="Group" /> to be displayed and modified by this control. If <see langword="null"/>, the control resets.
        /// </summary>
        public Group Group
        {
            get { return _group; }
            set {
                this._group = value ?? new Group();
                targetBaseControl.TargetBase = _group;
                UpdateControl();
            }
        }

        /// <summary>
        /// Clear all elements in this control and set their values from <see cref="_group"/>.
        /// </summary>
        private void UpdateControl()
        {
            ClearControl();
            if(_group.Version != null) hintTextBoxVersion.Text = _group.VersionString;
            if(_group.Released != null) dateTimePickerRelease.Value = _group.Released;
            if(!String.IsNullOrEmpty(_group.License)) comboBoxLicense.SelectedText = _group.License;
            if(!String.IsNullOrEmpty(_group.Main)) hintTextBoxMain.Text = _group.Main;
            if(!String.IsNullOrEmpty(_group.SelfTest)) hintTextBoxSelfTest.Text = _group.SelfTest;
            if(!String.IsNullOrEmpty(_group.DocDir)) hintTextBoxDocDir.Text = _group.DocDir;
            comboBoxStability.SelectedItem = _group.Stability;
            targetBaseControl.TargetBase = _group;
        }

        /// <summary>
        /// Clear all elements in this control.
        /// </summary>
        private void ClearControl()
        {
            hintTextBoxVersion.Text = String.Empty;
            dateTimePickerRelease.Value = DateTime.Now;
            comboBoxLicense.SelectedText = "GPL v3 (GNU General Public License)";
            hintTextBoxMain.Text = String.Empty;
            hintTextBoxSelfTest.Text = String.Empty;
            hintTextBoxDocDir.Text = String.Empty;
            comboBoxStability.SelectedText = String.Empty;
            targetBaseControl.TargetBase = null;
        }

        /// <summary>
        /// Creates a new <see cref="GroupControl"/> object.
        /// </summary>
        public GroupControl()
        {
            InitializeComponent();
            InitializeComboBoxStability();
            targetBaseControl.TargetBase = _group;
        }

        /// <summary>
        /// Fills <see cref="comboBoxStability"/> with the items of <see cref="Stability"/>.
        /// </summary>
        private void InitializeComboBoxStability()
        {
            foreach (var stability in Enum.GetValues(typeof(Stability)))
            {
                comboBoxStability.Items.Add(stability);
            }
        }

        /// <summary>
        /// Sets <see cref="_group.Main"/> if <see cref="hintTextBoxMain.Text"/> is an relative path.
        /// Also sets the <see cref="hintTextBoxMain.ForeColor"/> to <see cref="Color.Green"/> or <see cref="Color.Red"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void hintTextBoxMain_TextChanged(object sender, EventArgs e)
        {
            if (!Path.IsPathRooted(hintTextBoxMain.Text))
            {
                _group.Main = hintTextBoxMain.Text;
                hintTextBoxMain.ForeColor = Color.Green;
            }
            else
            {
                _group.Main = null;
                hintTextBoxMain.ForeColor = Color.Red;
            }
        }

        /// <summary>
        /// Sets <see cref="_group.Version"/> if <see cref="hintTextBoxVersion.Text"/> is a valid <see cref="ImplementationVersion"/>.
        /// Also sets <see cref="hintTextBoxVersion.ForeColor"/> to <see cref="Color.Green"/> or <see cref="Color.Red"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void hintTextBoxVersion_TextChanged(object sender, EventArgs e)
        {
            ImplementationVersion implementationVersion;
            hintTextBoxVersion.ForeColor = (ImplementationVersion.TryCreate(hintTextBoxVersion.Text, out implementationVersion) ? Color.Green : Color.Red);
            _group.Version = implementationVersion;
        }

        /// <summary>
        /// Sets <see cref="_group.Released"/> if value of <see cref="dateTimePickerRelease.Value"/> changed.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void dateTimePickerRelease_ValueChanged(object sender, EventArgs e)
        {
            _group.Released = dateTimePickerRelease.Value;
        }

        /// <summary>
        /// Sets <see cref="_group.License"/> if <see cref="comboBoxLicense.SelectedText"/> is not empty.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void comboBoxLicense_SelectedIndexChanged(object sender, EventArgs e)
        {
            _group.License = (String.IsNullOrEmpty(comboBoxLicense.SelectedText) ? null : comboBoxLicense.SelectedText);
        }

        /// <summary>
        /// Sets the <see cref="_group.Stability"/> if the index of <see cref="comboBoxLicense"/> changes.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void comboBoxStability_SelectedIndexChanged(object sender, EventArgs e)
        {
            _group.Stability = (Stability)comboBoxLicense.SelectedValue;
        }

        /// <summary>
        /// Sets <see cref="_group.DocDir"/> if <see cref="hintTextBoxDocDir.Text"/> is a relative path.
        /// Also sets the <see cref="hintTextBoxDocDir.ForeColor"/> to <see cref="Color.Green"/> or <see cref="Color.Red"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void hintTextBoxDocDir_TextChanged(object sender, EventArgs e)
        {
            if (!Path.IsPathRooted(hintTextBoxDocDir.Text))
            {
                _group.DocDir = hintTextBoxDocDir.Text;
                hintTextBoxDocDir.ForeColor = Color.Green;
            }
            else
            {
                _group.DocDir = null;
                hintTextBoxDocDir.ForeColor = Color.Red;
            }
        }
    }
}
