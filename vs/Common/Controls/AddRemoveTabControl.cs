/*
 * Copyright 2010 Simon E. Silva Lauinger
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
using System.Windows.Forms;

namespace Common.Controls
{
    #region Delegates
    public delegate void NewTabEventHandler(TabControl sender, TabPage createdTabPage);
    #endregion

    /// <summary>
    /// A tab control that can add and remove tabs.
    /// Adds a new tab to the right of the selected by pressing ctrl + w .
    /// Adds a new tab to the left of the last tab by clicking the adding tab.
    /// Removes the selected tab by pressing ctrl + t .
    /// Removes the double clicked tab.
    /// </summary>
    public partial class AddRemoveTabControl : UserControl
    {
        #region Properties
        /// <summary>
        /// The last inserted <see cref="TabPage"/>. <see langword="null"/> if no <see cref="TabPage"/> was inserted.
        /// </summary>
        public TabPage LastInsertedTabPage { get; private set; }
        #endregion

        #region Events
        public event NewTabEventHandler NewTabCreated;

        private void OnNewTabCreated()
        {
            if (NewTabCreated != null) NewTabCreated(tabControl1, LastInsertedTabPage);
        }
        #endregion

        public AddRemoveTabControl()
        {
            InitializeComponent();
            // makes inserting tabs possible.
            IntPtr dummy = tabControl1.Handle;
        }

        /// <summary>
        /// Adds a new <see cref="TabPage"/> rigth beside the selected tab by pressing ctrl + t .
        /// Removes the selected tab by pressing ctrl + w .
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void TabControl1PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            var selectedTabIndex = tabControl1.SelectedIndex;
            var lastTabIndex = tabControl1.TabCount - 1;

            if(e.KeyData == (Keys.Control | Keys.T))
            {
                // if the selected tab is the last, insert to the left beside the tab, else to the right.
                var insertTabIndex = (selectedTabIndex == lastTabIndex) ? selectedTabIndex : selectedTabIndex + 1;
                var tabPageToInsert = new TabPage();
                tabControl1.TabPages.Insert(insertTabIndex, tabPageToInsert);
                LastInsertedTabPage = tabPageToInsert;
                OnNewTabCreated();
            } else if(e.KeyData == (Keys.Control | Keys.W))
            {
                RemoveTab(selectedTabIndex);
            }
        }

        /// <summary>
        /// Adds a new <see cref="TabPage"/> to the left of the "+" tab if clicked left on it.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Clicked mouse buttons.</param>
        private void TabControl1MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Clicks != 1) return;

            var selectedTabIndex = tabControl1.SelectedIndex;
            var lastTabIndex = tabControl1.TabCount - 1;

            if (e.Button == MouseButtons.Left)
            {
                if (selectedTabIndex != lastTabIndex) return;

                var tabPageToInsert = new TabPage();
                tabControl1.TabPages.Insert(lastTabIndex, tabPageToInsert);
                LastInsertedTabPage = tabPageToInsert;
                OnNewTabCreated();
                // select the new tab
                tabControl1.SelectedIndex = lastTabIndex;
            }
        }

        /// <summary>
        /// Removes double clicked tab.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void TabControl1MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var selectedTabIndex = tabControl1.SelectedIndex;

            RemoveTab(selectedTabIndex);
        }

        /// <summary>
        /// Remove tab with index <paramref name="tabIndex"/>.
        /// </summary>
        /// <param name="tabIndex">Index to the tab to remove.</param>
        private void RemoveTab(int tabIndex)
        {
            var lastTabIndex = tabControl1.TabCount - 1;

            // Don't remove if the selected tab is the tab to add new tabs.
            if (tabControl1.SelectedTab.Name == "tabPageAddNew" && tabIndex == lastTabIndex) return;

            tabControl1.TabPages.RemoveAt(tabIndex);
            // select  the tab right beside the removed.
            tabControl1.SelectedIndex = tabIndex;
        }
    }
}
