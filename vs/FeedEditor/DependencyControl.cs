using System;
using System.Drawing;
using System.Windows.Forms;
using ZeroInstall.Model;
using Constraint = ZeroInstall.Model.Constraint;

namespace ZeroInstall.FeedEditor
{
    public partial class DependencyControl : UserControl
    {
        private Dependency _dependency = new Dependency();

        public Dependency Dependency
        {
            get { return _dependency; }
            set
            {
                _dependency = value ?? new Dependency();
                UpdateControl();
            }
        }

        /// <summary>
        /// Clear all elements in this control and set their values from <see cref="_dependency"/>.
        /// </summary>
        private void UpdateControl()
        {
            ClearControl();
            if(_dependency.Uri != null) hintTextBoxInterface.Text = _dependency.UriString;
            if (!String.IsNullOrEmpty(_dependency.Use)) hintTextBoxUse.Text = _dependency.Use;
            foreach (var constraint in _dependency.Constraints)
            {
                listBoxConstraints.Items.Add(constraint);
            }
        }

        /// <summary>
        /// Clear all elements in this control.
        /// </summary>
        private void ClearControl()
        {
            hintTextBoxInterface.Text = String.Empty;
            hintTextBoxUse.Text = String.Empty;
            hintTextBoxNotBefore.Text = String.Empty;
            hintTextBoxBefore.Text = String.Empty;
            listBoxConstraints.Items.Clear();
        }

        public DependencyControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Adds a <see cref="Constraint"/> from hintTextBoxNotBefore.Text and/or hintTextBoxBefore.Text to listBoxConstraints.Items.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if hintTextBoxNotBefore.Text and/or hintTextBoxNotBefore.Text is <see cref="null"/> or empty.</exception>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void buttonConstraintAdd_Click(object sender, EventArgs e)
        {
            Constraint constraint;
            // add NotBefore and Before
            if (!String.IsNullOrEmpty(hintTextBoxNotBefore.Text) && !String.IsNullOrEmpty(hintTextBoxBefore.Text))
            {
                constraint = new Constraint(new ImplementationVersion(hintTextBoxNotBefore.Text), new ImplementationVersion(hintTextBoxBefore.Text));
            }
                //add only NotBefore
            else if (!String.IsNullOrEmpty(hintTextBoxNotBefore.Text))
            {
                constraint = new Constraint(new ImplementationVersion(hintTextBoxNotBefore.Text), null);
            }
                //add only Before
            else
            {
                constraint = new Constraint(null, new ImplementationVersion(hintTextBoxBefore.Text));
            }
            // add to the list if it is not allready in the list
            if (!listBoxConstraints.Items.Contains(constraint))
                listBoxConstraints.Items.Add(constraint);
        }

        /// <summary>
        /// Checks if hintTextBoxNotBefore.Text for a valid <see cref="ImplementationVersion"/> and sets the hintTextBoxNotBefore.ForeColor to <see cref="Color.Green"/> or to <see cref="Color.Red"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void hintTextBoxNotBefore_TextChanged(object sender, EventArgs e)
        {
            ImplementationVersion implementationVersion;
            hintTextBoxNotBefore.ForeColor = ImplementationVersion.TryCreate(hintTextBoxNotBefore.Text, out implementationVersion) ? Color.Green : Color.Red;
            checkConstraints();
        }

        /// <summary>
        /// Checks if hintTextBoxBefore.Text for a valid <see cref="ImplementationVersion"/> and sets the hintTextBoxBefore.ForeColor to <see cref="Color.Green"/> or to <see cref="Color.Red"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void hintTextBoxBefore_TextChanged(object sender, EventArgs e)
        {
            ImplementationVersion implementationVersion;
            hintTextBoxBefore.ForeColor = ImplementationVersion.TryCreate(hintTextBoxBefore.Text, out implementationVersion) ? Color.Green : Color.Red;
            checkConstraints();
        }

        /// <summary>
        /// Checks if hintTextBoxNotBefore.Text or hintTextBoxBefore.Text have a valid <see cref="ImplementationVersion"/> and enables or disables buttonConstraintAdd.
        /// </summary>
        private void checkConstraints()
        {
            ImplementationVersion implementationVersion;
            buttonConstraintAdd.Enabled = ImplementationVersion.TryCreate(hintTextBoxNotBefore.Text, out implementationVersion) || ImplementationVersion.TryCreate(hintTextBoxBefore.Text, out implementationVersion);
        }

        /// <summary>
        /// Removes the selected item from listBoxConstraints.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonConstraintRemove_Click(object sender, EventArgs e)
        {
            var selectedVersion = (Constraint)listBoxConstraints.SelectedItem;
            listBoxConstraints.Items.Remove(selectedVersion);
        }

        /// <summary>
        /// Check if hintTextBoxInterface.Text is a valid interface url and if yes, set hintTextBoxInterface.ForeColor to <see cref="Color.Green"/> or to <see cref="Color.Red"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void hintTextBoxInterface_TextChanged(object sender, EventArgs e)
        {
            Uri uri;
            if(IsValidFeedURL(hintTextBoxInterface.Text, out uri))
            {
                hintTextBoxInterface.ForeColor = Color.Green;
                _dependency.Uri = uri;
            } else
            {
                hintTextBoxInterface.ForeColor = Color.Red;
                _dependency.Uri = null;
            }
        }

        /// <summary>
        /// Check if <para>url</para> is a valid <see cref="Uri"/> (begins with <see cref="UriSchemeHttp"/> or <see cref="UriSchemeHttps"/> and has the right format).
        /// </summary>
        /// <param name="url">Not used.</param>
        /// <param name="uri">Not used.</param>
        /// <returns></returns>
        private bool IsValidFeedURL(string url, out Uri uri)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out uri))
                return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
            return false;
        }

        /// <summary>
        /// Set hintTextBoxBefore.Text and hintTextBoxBefore.Text with the values from a <see cref="ImplementationVersion"/> from listBoxConstraints.SelectedItem
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void listBoxConstraints_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedItem = (Constraint)listBoxConstraints.SelectedItem;
            hintTextBoxBefore.Text = selectedItem.BeforeVersionString ?? String.Empty;
            hintTextBoxNotBefore.Text = selectedItem.NotBeforeVersionString ?? String.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void hintTextBoxUse_TextChanged(object sender, EventArgs e)
        {
            _dependency.Use = hintTextBoxUse.Text;
        }

        private void DependencyControl_Enter(object sender, EventArgs e)
        {
            UpdateControl();
        }
    }
}
