using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Common.Properties;

namespace Common.Controls
{
    /// <summary>
    /// A base class for creating wizard interfaces. Manages pages as a stack of <see cref="UserControl"/>s.
    /// </summary>
    public partial class Wizard : Form
    {
        /// <summary>
        /// The wizard page history with the currently visible page on top.
        /// </summary>
        protected readonly Stack<UserControl> PageStack = new Stack<UserControl>();

        /// <summary>
        /// Creates a new wizard.
        /// </summary>
        public Wizard()
        {
            InitializeComponent();

            buttonBack.Text = @"<- " + Resources.Back;
            buttonCancel.Text = Resources.Cancel;
        }

        /// <summary>
        /// Displays a new wizard page and adds it to the <see cref="PageStack"/>.
        /// </summary>
        /// <param name="page">The page to display and add.</param>
        protected void PushPage(UserControl page)
        {
            if (PageStack.Count != 0) panelPage.Controls.Remove(PageStack.Peek());
            panelPage.Controls.Add(page);
            PageStack.Push(page);

            buttonBack.Enabled = (PageStack.Count > 1);
            page.Focus();
        }

        /// <summary>
        /// Removes the current wizard page form the <see cref="PageStack"/> and displays the previous one.
        /// </summary>
        protected void PopPage()
        {
            panelPage.Controls.Remove(PageStack.Pop());
            panelPage.Controls.Add(PageStack.Peek());

            buttonBack.Enabled = (PageStack.Count > 1);
            PageStack.Peek().Focus();
        }

        private void buttonBack_Click(object sender, EventArgs e)
        {
            PopPage();
        }
    }
}
