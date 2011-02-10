using System;
using System.Windows.Forms;
using C5;

namespace ZeroInstall.Publish.WinForms.Controls
{
    public partial class ArgumentsControl : UserControl
    {
        #region Properties

        private ArrayList<String> _arguments;

        public ArrayList<String> Arguments
        {
            get
            {
                _arguments.Clear();
                foreach (string argument in listBoxArguments.Items)
                {
                    _arguments.Add(argument);
                }
                return _arguments;
            }
            set
            {
                _arguments = value ?? new ArrayList<string>();
                UpdateControls();
            }
        }

        #endregion

        #region Initialization
        public ArgumentsControl()
        {
            InitializeComponent();
            InitializeListButtons();
            InitializeListBoxArguments();
        }

        private void InitializeListButtons()
        {
            buttonAddArgument.Click += (sender, eventArgs) =>
                                           {
                                               if (!string.IsNullOrEmpty(textBoxArgument.Text))
                                                   listBoxArguments.Items.Add(textBoxArgument.Text);
                                           };
            buttonRemoveArgument.Click += (sender, eventArgs) =>
                                              {
                                                  if (listBoxArguments.SelectedItem != null)
                                                  {
                                                      _arguments.Remove((String) listBoxArguments.SelectedItem);
                                                      UpdateControls();
                                                  }
                                              };
            buttonClearArguments.Click += (sender, eventArgs) => listBoxArguments.Items.Clear();
        }

        private void InitializeListBoxArguments()
        {
            listBoxArguments.Click += (sender, eventArgs) =>
                                          {
                                              if(listBoxArguments.SelectedItem != null)
                                                textBoxArgument.Text = (String) listBoxArguments.SelectedItem;
                                          };
        }
        #endregion

        #region Control Management
        private void UpdateControls()
        {
            ClearControls();
            listBoxArguments.Items.AddRange(_arguments.ToArray());
        }

        private void ClearControls()
        {
            listBoxArguments.Items.Clear();
        }
        #endregion
    }
}
