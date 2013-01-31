/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

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
            #region Sanity checks
            if (page == null) throw new ArgumentNullException("page");
            #endregion

            if (PageStack.Count != 0) panelPage.Controls.Remove(PageStack.Peek());
            panelPage.Controls.Add(page);
            PageStack.Push(page);

            buttonBack.Visible = (PageStack.Count > 1);
            page.Focus();
        }

        /// <summary>
        /// Removes the current wizard page form the <see cref="PageStack"/> and displays the previous one.
        /// </summary>
        protected void PopPage()
        {
            panelPage.Controls.Remove(PageStack.Pop());
            panelPage.Controls.Add(PageStack.Peek());

            buttonBack.Visible = (PageStack.Count > 1);
            PageStack.Peek().Focus();
        }

        private void buttonBack_Click(object sender, EventArgs e)
        {
            PopPage();
        }
    }
}
