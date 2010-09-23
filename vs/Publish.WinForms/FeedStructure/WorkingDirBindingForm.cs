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
        }

        /// <summary>
        /// Clear all controls in this form and set their values from <see cref="_overlayBinding"/>.
        /// </summary>
        private void UpdateControl()
        {
            ClearControl();
        }

        #endregion
    }
}
