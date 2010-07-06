using System.Windows.Forms;

namespace Common.Controls
{
    /// <summary>
    /// A base-class for creating fixed-size dialog boxes with an OK and a Cancel button.
    /// </summary>
    public partial class OKCancelDialog : Form
    {
        #region Constructor
        public OKCancelDialog()
        {
            InitializeComponent();
        }
        #endregion

        //--------------------//

        #region Event handling
        private void buttonOK_Click(object sender, System.EventArgs e)
        {
            OnOKClicked();
        }

        private void buttonCancel_Click(object sender, System.EventArgs e)
        {
            OnCancelClicked();
        }
        #endregion

        #region Hooks
        /// <summary>
        /// This hook is called when the user clicks the OK button.
        /// </summary>
        protected virtual void OnOKClicked()
        {}

        /// <summary>
        /// This hook is called when the user clicks the Cancel button.
        /// </summary>
        protected virtual void OnCancelClicked()
        { }
        #endregion
    }
}
