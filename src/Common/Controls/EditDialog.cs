using System;
using System.Windows.Forms;
using Common.Properties;

namespace Common.Controls
{
    /// <summary>
    /// Edits arbritary types of elements using a <see cref="PropertyGrid"/>.
    /// </summary>
    /// <typeparam name="T">The type of element to edit.</typeparam>
    public abstract partial class EditDialog<T> : OKCancelDialog where T : class
    {
        protected EditDialog()
        {
            InitializeComponent();
            buttonResetValue.Text = Resources.ResetValue;

            Load += delegate { Text = typeof(T).Name; };
        }

        /// <summary>
        /// Displays a modal dialog for editing an element.
        /// </summary>
        /// <param name="owner">The parent window used to make the dialog modal.</param>
        /// <param name="element">The element to be edited.</param>
        /// <returns>Save changes if <see cref="DialogResult.OK"/>; discard changes if  <see cref="DialogResult.Cancel"/>.</returns>
        public DialogResult ShowDialog(IWin32Window owner, T element)
        {
            #region Sanity checks
            if (owner == null) throw new ArgumentNullException("owner");
            if (element == null) throw new ArgumentNullException("element");
            #endregion

            propertyGrid.SelectedObject = element;

            return ShowDialog(owner);
        }

        /// <summary>
        /// Displays a modal dialog without a parent (listed in the taskbar) for editing an element.
        /// </summary>
        /// <param name="element">The element to be edited.</param>
        /// <returns>Save changes if <see cref="DialogResult.OK"/>; discard changes if <see cref="DialogResult.Cancel"/>.</returns>
        public DialogResult ShowDialog(T element)
        {
            #region Sanity checks
            if (element == null) throw new ArgumentNullException("element");
            #endregion

            propertyGrid.SelectedObject = element;

            ShowInTaskbar = true;
            return ShowDialog();
        }

        private void propertyGrid_SelectedGridItemChanged(object sender, SelectedGridItemChangedEventArgs e)
        {
            buttonResetValue.Enabled = propertyGrid.CanResetSelectedProperty;
        }

        private void buttonResetValue_Click(object sender, EventArgs e)
        {
            propertyGrid.ResetSelectedProperty();
        }
    }
}
