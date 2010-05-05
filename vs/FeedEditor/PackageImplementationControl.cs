using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using ZeroInstall.Model;
using System.IO;

namespace ZeroInstall.FeedEditor
{
    public partial class PackageImplementationControl : UserControl
    {
        /// <summary>
        /// The <see cref="PackageImplementation" /> to be displayed and modified by this control.
        /// </summary>
        private PackageImplementation _packageImplementation = new PackageImplementation();

        public PackageImplementationControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The <see cref="PackageImplementation" /> to be displayed and modified by this control. If <see langword="null"/>, the control resets.
        /// </summary>
        public PackageImplementation PackageImplementation
        {
            get { return _packageImplementation; }
            set
            {
                _packageImplementation = value ?? new PackageImplementation();
                UpdateControl();
            }
        }

        /// <summary>
        /// Clear all elements in this control and set their values from <see cref="_packageImplementation"/>.
        /// </summary>
        private void UpdateControl()
        {
            ClearControl();
            if (!String.IsNullOrEmpty(_packageImplementation.License)) comboBoxLicense.SelectedText = _packageImplementation.License;
            if (!String.IsNullOrEmpty(_packageImplementation.Main)) hintTextBoxMain.Text = _packageImplementation.Main;
            if (!String.IsNullOrEmpty(_packageImplementation.DocDir)) hintTextBoxDocDir.Text = _packageImplementation.DocDir;
            if (!String.IsNullOrEmpty(hintTextBoxPackage.Text)) hintTextBoxPackage.Text = _packageImplementation.Package;
            if (!String.IsNullOrEmpty(hintTextBoxMain.Text)) hintTextBoxMain.Text = _packageImplementation.Main;
            foreach (var distribution in _packageImplementation.Distributions)
            {
                checkedListBoxDistribution.SelectedItem = distribution;
            }
        }

        /// <summary>
        /// Clear all elements in this control.
        /// </summary>
        private void ClearControl()
        {
            comboBoxLicense.SelectedText = "GPL v3 (GNU General Public License)";
            hintTextBoxMain.Text = String.Empty;
            hintTextBoxDocDir.Text = String.Empty;
            hintTextBoxPackage.Text = String.Empty;
            hintTextBoxMain.Text = String.Empty;
            checkedListBoxDistribution.ClearSelected();
        }

        /// <summary>
        /// Sets <see cref="_packageImplementation.Main"/> if <see cref="hintTextBoxMain.Text"/> is an absolute path.
        /// Also sets the <see cref="hintTextBoxMain.ForeColor"/> to <see cref="Color.Green"/> or <see cref="Color.Red"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void hintTextBoxMain_TextChanged(object sender, EventArgs e)
        {
            if (Path.IsPathRooted(hintTextBoxMain.Text))
            {
                _packageImplementation.Main = hintTextBoxMain.Text;
                hintTextBoxMain.ForeColor = Color.Green;
            }
            else
            {
                _packageImplementation.Main = null;
                hintTextBoxMain.ForeColor = Color.Red;
            }
        }

        /// <summary>
        /// Sets <see cref="_packageImplementation.DocDir"/> if <see cref="hintTextBoxDocDir.Text"/> is a relative path.
        /// Also sets the <see cref="hintTextBoxDocDir.ForeColor"/> to <see cref="Color.Green"/> or <see cref="Color.Red"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void hintTextBoxDocDir_TextChanged(object sender, EventArgs e)
        {
            if (!Path.IsPathRooted(hintTextBoxDocDir.Text))
            {
                _packageImplementation.DocDir = hintTextBoxDocDir.Text;
                hintTextBoxDocDir.ForeColor = Color.Green;
            }
            else
            {
                _packageImplementation.DocDir = null;
                hintTextBoxDocDir.ForeColor = Color.Red;
            }
        }

        /// <summary>
        /// Sets <see cref="_packageImplementation.Distributions"/> if <see cref="checkedListBoxDistribution.SelectedItems"/> changes.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void checkedListBoxDistribution_SelectedIndexChanged(object sender, EventArgs e)
        {
            _packageImplementation.Distributions.Clear();
            foreach (string distribution in checkedListBoxDistribution.SelectedItems)
            {
                _packageImplementation.Distributions.Add(distribution);
            }
        }

        /// <summary>
        /// Sets <see cref="_packageImplementation.Package"/> if <see cref="hintTextBoxPackage.Text"/> is not empty.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void hintTextBoxPackage_TextChanged(object sender, EventArgs e)
        {
            _packageImplementation.Package = (String.IsNullOrEmpty(hintTextBoxPackage.Text) ? null : hintTextBoxPackage.Text);
        }

        /// <summary>
        /// Sets <see cref="_packageImplementation.License"/> if <see cref="comboBoxLicense.SelectedText"/> is not empty.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void comboBoxLicense_SelectedIndexChanged(object sender, EventArgs e)
        {
            _packageImplementation.License = (String.IsNullOrEmpty(comboBoxLicense.SelectedText) ? null : comboBoxLicense.SelectedText);
        }

        /// <summary>
        /// Sets the <see cref="_packageImplementation.Stability"/> if the index of <see cref="comboBoxLicense"/> changes.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void comboBoxStability_SelectedIndexChanged(object sender, EventArgs e)
        {
            _packageImplementation.Stability = (Stability)comboBoxLicense.SelectedValue;
        }
    }
}
