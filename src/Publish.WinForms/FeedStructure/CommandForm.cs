using Common.Controls;
using ZeroInstall.Model;

namespace ZeroInstall.Publish.WinForms.FeedStructure
{
    public partial class CommandForm : OKCancelDialog
    {
        #region Properties
        private Command _command = new Command();

        public Command Command
        {
            get { return _command; }
            set
            {
                _command = value ?? new Command();
                UpdateControlForms();
            }
        }
        #endregion

        #region Initialization
        public CommandForm()
        {
            InitializeComponent();
        }
        #endregion

        #region Control management
        private void ClearFormControls()
        {
            textBoxName.Text = string.Empty;
            textBoxPath.Text = string.Empty;
            argumentsControl1.Arguments = null;
        }

        private void UpdateControlForms()
        {
            ClearFormControls();

            textBoxName.Text = _command.Name;
            textBoxPath.Text = _command.Path;
            argumentsControl1.Arguments = _command.Arguments;
        }
        #endregion

        #region Dialog Buttons

        #endregion
    }
}
