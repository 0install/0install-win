/*
 * Copyright 2010 Simon E. Silva Lauinger
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Windows.Forms;

namespace Common.Controls
{
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
