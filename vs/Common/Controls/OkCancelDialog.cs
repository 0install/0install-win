using System.Windows.Forms;

namespace Common.Controls
{
    /// <summary>
    /// A base-class for creating fixed-size dialog boxes with an OK and a Cancel button.
    /// </summary>
    public partial class OKCancelDialog : Form
    {
        public OKCancelDialog()
        {
            InitializeComponent();
        }

        private void buttonOK_Click(object sender, System.EventArgs e)
        {
            OnOKClicked();
        }

        private void buttonCancel_Click(object sender, System.EventArgs e)
        {
            OnCancelClicked();
        }

        /// <summary>
        /// This hook is called when the user clicks the OK button.
        /// </summary>
        protected virtual void OnOKClicked()
        {}

        /// <summary>
        /// This hook is called when the user clicks the Cancel button.
        /// </summary>
        protected virtual void OnCancelClicked()
        {}
    }
}
