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
    /// <param name="sender">The <see cref="AddRemoveTabControl"/> instance that raised the event.</param>
    /// <param name="createdTabPage">The new <see cref="TabPage"/> that was created.</param>
    /// <seealso cref="AddRemoveTabControl.NewTabPageCreated"/>
    public delegate void NewTabPageEventHandler(TabControl sender, TabPage createdTabPage);

    /// <param name="sender">The <see cref="AddRemoveTabControl"/> instance that raised the event.</param>
    /// <param name="removeTabPage">The <see cref="TabPage"/> that was removed.</param>
    /// <seealso cref="AddRemoveTabControl.TabPageRemoved"/>
    public delegate void RemovedTabPageEventHandler(TabControl sender, TabPage removeTabPage);
    #endregion

    /// <summary>
    /// A <see cref="TabControl"/> that can add and remove <see cref="TabPage"/>s.
    /// Adds a new tab to the right of the selected by pressing CTRL + W.
    /// Adds a new tab to the left of the last tab by clicking the adding tab.
    /// Removes the selected tab by pressing CTRL + T.
    /// Removes the double clicked <see cref="TabPage"/>.
    /// </summary>
    public class AddRemoveTabControl : TabControl
    {
        #region Events
        /// <summary>
        /// Raised when a new <see cref="TabPage"/> was created by the user.
        /// </summary>
        public event NewTabPageEventHandler NewTabPageCreated;

        /// <summary>
        /// Raised when a new <see cref="TabPage"/> was removed by the user.
        /// </summary>
        public event RemovedTabPageEventHandler TabPageRemoved;

        private void OnNewTabCreated(TabPage createdTabPage)
        {
            if (NewTabPageCreated != null) NewTabPageCreated(this, createdTabPage);
        }

        private void OnTabPageRemoved(TabPage removedTabPage)
        {
            if (TabPageRemoved != null) TabPageRemoved(this, removedTabPage);
        }
        #endregion

        #region Variables
        /// <summary>
        /// Adds or removes the <see cref="TabPage"/> that allows
        /// the user to add a new <see cref="TabPage"/> by clicking
        /// on it.
        /// </summary>
        private bool _useAddTabPage;

        /// <summary>
        /// <see cref="TabPage"/> that allows the user to
        /// add new <see cref="TabPage"/>s by clicking on it.
        /// </summary>
        private readonly TabPage _addNewTabPage = new TabPage("    +   ");
        #endregion

        #region Properties
        /// <summary>
        /// The last inserted <see cref="TabPage"/>. <see langword="null"/> if no <see cref="TabPage"/> was inserted.
        /// </summary>
        public TabPage LastInsertedTabPage { get; private set; }

        /// <summary>
        /// The last removed <see cref="TabPage"/>. <see langword="null"/> if no <see cref="TabPage"/> was removed.
        /// </summary>
        public TabPage LastRemovedTabPage { get; private set; }

        /// <summary>
        /// Adds or removes the <see cref="TabPage"/> that allows
        /// the user to add a new <see cref="TabPage"/> by clicking
        /// on it.
        /// </summary>
        public Boolean UseAddTabPage
        {
            get { return _useAddTabPage; }
            set
            {
                _useAddTabPage = value;
                if (_useAddTabPage) InsertAddTabPage();
                else RemoveAddTabPage();
            }
        }
        #endregion

        //--------------------//

        #region Overriden events
        /// <summary>
        /// Adds a new <see cref="TabPage"/> right beside the selected tab by pressing CTRL + T.
        /// Removes the selected <see cref="TabPage"/> by pressing CTRL + W .
        /// If there is only one <see cref="TabPage"/> left, it will not be removed.
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            #region Sanity checks
            if (e == null) throw new ArgumentNullException("e");
            #endregion

            base.OnKeyDown(e);

            switch (e.KeyData)
            {
                case (Keys.Control | Keys.T):
                    if (TabCount < 0) InsertTabPage(0);
                    else InsertTabPage(SelectedIndex + 1);
                    break;
                case (Keys.Control | Keys.W):
                    if (TabCount > 1) RemoveTabPage(SelectedIndex);
                    break;
            }
        }

        /// <summary>
        /// Checks if the selected <see cref="TabPage"/> is <see cref="_addNewTabPage"/>.
        /// If yes, the selection will be canceled and a new <see cref="TabPage"/>
        /// will be created left beside the <see cref="_addNewTabPage"/>.
        /// </summary>
        protected override void OnSelecting(TabControlCancelEventArgs e)
        {
            #region Sanity checks
            if (e == null) throw new ArgumentNullException("e");
            #endregion

            if (e.Action == TabControlAction.Selecting && e.TabPage == _addNewTabPage)
            {
                InsertTabPage(TabCount - 1);
                e.Cancel = true;
            }

            base.OnSelecting(e);
        }
        #endregion

        #region Insert/Remove new tab page
        /// <summary>
        /// Inserts a new <see cref="TabPage"/> to <paramref name="tabPageIndexToInsert"/>,
        /// sets <see cref="LastInsertedTabPage"/> and fires <see cref="OnNewTabCreated"/>.
        /// </summary>
        /// <param name="tabPageIndexToInsert">Index to insert <see cref="TabPage"/>.</param>
        private void InsertTabPage(int tabPageIndexToInsert)
        {
            var tabPageToInsert = new TabPage();
            TabPages.Insert(tabPageIndexToInsert, tabPageToInsert);
            LastInsertedTabPage = tabPageToInsert;
            OnNewTabCreated(tabPageToInsert);
        }

        /// <summary>
        /// Removes the <see cref="TabPage"/> with index <paramref name="tabPageIndexToRemove"/>,
        /// sets <see cref="LastRemovedTabPage"/> and fires <see cref="OnTabPageRemoved"/>.
        /// </summary>
        /// <param name="tabPageIndexToRemove">Index of the <see cref="TabPage"/> to remove.</param>
        private void RemoveTabPage(int tabPageIndexToRemove)
        {
            var tabToRemove = TabPages[tabPageIndexToRemove];
            TabPages.RemoveAt(tabPageIndexToRemove);
            LastRemovedTabPage = tabToRemove;
            SelectedIndex = tabPageIndexToRemove == TabCount ? tabPageIndexToRemove - 1 : tabPageIndexToRemove;
            OnTabPageRemoved(tabToRemove);
        }
        #endregion

        #region Add/Remove adding tab page
        /// <summary>
        /// Removes <see cref="_addNewTabPage"/>.
        /// </summary>
        private void RemoveAddTabPage()
        {
            TabPages.Remove(_addNewTabPage);
        }

        /// <summary>
        /// Adds <see cref="_addNewTabPage"/> as last <see cref="TabPage"/>.
        /// </summary>
        private void InsertAddTabPage()
        {
            TabPages.Add(_addNewTabPage);
        }
        #endregion
    }
}
