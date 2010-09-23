using System;
using Common.Controls;
using ZeroInstall.Model;

namespace ZeroInstall.Publish.WinForms.FeedStructure
{
    public partial class WorkingDirBindingForm : OKCancelDialog
    {
        #region Properties

        /// <summary>
        /// The <see cref="WorkingDirBinding"/> to shown and edited by this form.
        /// </summary>
        private WorkingDirBinding _workingDirBinding = new WorkingDirBinding();

        /// <summary>
        /// The <see cref="WorkingDirBinding"/> to show and edit by this form.
        /// </summary>
        public WorkingDirBinding WorkingDirBinding
        {
            get
            {
                return _workingDirBinding;
            }
            set
            {
                _workingDirBinding =  value ?? new WorkingDirBinding();
                UpdateControl();
            }
        }
        #endregion

        #region Initialization
        public WorkingDirBindingForm()
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
        /// Clear all controls in this form and set their values from <see cref="WorkingDirBinding"/>.
        /// </summary>
        private void UpdateControl()
        {
            ClearControl();

            hintTextBoxSource.Text = WorkingDirBinding.Source;
        }

        #endregion

        #region Dialog buttons
        /// <summary>
        /// Saves the values from the filled controls to <see cref="WorkingDirBinding"/> and closes the window.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void ButtonOkClick(object sender, EventArgs e)
        {
            string workingDirBinding = hintTextBoxSource.Text;
            WorkingDirBinding.Source = string.IsNullOrEmpty(workingDirBinding) ? "." : workingDirBinding;
        }
        #endregion
    }
}
