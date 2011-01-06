using System;
using Common.Controls;
using ZeroInstall.Model;

namespace ZeroInstall.Publish.WinForms.FeedStructure
{
    public partial class WorkingDirForm : OKCancelDialog
    {
        #region Properties

        /// <summary>
        /// The <see cref="WorkingDir"/> to shown and edited by this form.
        /// </summary>
        private WorkingDir _workingDir = new WorkingDir();

        /// <summary>
        /// The <see cref="WorkingDir"/> to show and edit by this form.
        /// </summary>
        public WorkingDir WorkingDir
        {
            get
            {
                return _workingDir;
            }
            set
            {
                _workingDir =  value ?? new WorkingDir();
                UpdateControl();
            }
        }
        #endregion

        #region Initialization
        public WorkingDirForm()
        {
            InitializeComponent();
        }
        #endregion

        #region Control management methodes
        /// <summary>
        /// Clear all controls on this form.
        /// </summary>
        private void ClearControl()
        {
            hintTextBoxSource.Text = string.Empty;
        }

        /// <summary>
        /// Clear all controls in this form and set their values from <see cref="WorkingDir"/>.
        /// </summary>
        private void UpdateControl()
        {
            ClearControl();

            hintTextBoxSource.Text = WorkingDir.Source;
        }

        #endregion

        #region Dialog buttons
        /// <summary>
        /// Saves the values from the filled controls to <see cref="WorkingDir"/> and closes the window.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void ButtonOkClick(object sender, EventArgs e)
        {
            string workingDir = hintTextBoxSource.Text;
            WorkingDir.Source = string.IsNullOrEmpty(workingDir) ? "." : workingDir;
        }
        #endregion
    }
}
