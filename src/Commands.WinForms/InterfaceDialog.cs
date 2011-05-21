/*
 * Copyright 2010-2011 Bastian Eicher
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
using System.IO;
using System.Windows.Forms;
using C5;
using Common;
using Common.Controls;
using ZeroInstall.Injector.Feeds;
using ZeroInstall.Model;

namespace ZeroInstall.Commands.WinForms
{
    /// <summary>
    /// Dialog for modifying <see cref="InterfacePreferences"/> and <see cref="FeedPreferences"/>.
    /// </summary>
    public partial class InterfaceDialog : OKCancelDialog
    {
        #region Variables
        /// <summary>The interface to modify the preferences for.</summary>
        private readonly string _interfaceID;

        /// <summary>The interface preferences being modified.</summary>
        private readonly InterfacePreferences _interfacePreferences;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates anew interface dialog.
        /// </summary>
        /// <param name="interfaceID">The interface to modify the preferences for.</param>
        private InterfaceDialog(string interfaceID)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            #endregion

            _interfaceID = interfaceID;

            InitializeComponent();
            comboBoxStability.Items.AddRange(new object[] { "", Stability.Stable, Stability.Testing, Stability.Developer });

            _interfacePreferences = InterfacePreferences.LoadFor(interfaceID);
            comboBoxStability.SelectedItem = _interfacePreferences.StabilityPolicy;
            listBoxFeeds.Items.Add(interfaceID);
            foreach (var feedReference in _interfacePreferences.Feeds)
            {
                listBoxFeeds.Items.Add(feedReference);
            }
        }
        #endregion

        #region Static access
        /// <summary>
        /// Displays a dialog allowing the user to modify <see cref="InterfacePreferences"/> and <see cref="FeedPreferences"/> for an interface.
        /// </summary>
        /// <param name="owner">The parent window the displayed window is modal to; may be <see langword="null"/>.</param>
        /// <param name="interfaceID">The interface to modify the preferences for.</param>
        /// <returns><see langword="true"/> if the preferences where modified; <see langword="false"/> if everything remained unchanged.</returns>
        public static bool Show(IWin32Window owner, string interfaceID)
        {
            #region Sanity checks
            if (owner == null) throw new ArgumentNullException("owner");
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            #endregion

            using (var dialog = new InterfaceDialog(interfaceID))
            {
                return (dialog.ShowDialog() == DialogResult.OK);
            }
        }
        #endregion

        //--------------------//

        #region Helpers
        /// <summary>
        /// Adds a feed to the list of registered additional feeds.
        /// </summary>
        private void AddFeed(string feedID)
        {
            var feed = new FeedReference { Source = feedID };
            _interfacePreferences.Feeds.Add(feed);
            listBoxFeeds.Items.Add(feed);
        }

        /// <summary>
        /// Removes a feed from the list of registered additional feeds.
        /// </summary>
        private void RemoveFeed(FeedReference feed)
        {
            _interfacePreferences.Feeds.Remove(feed);
            listBoxFeeds.Items.Remove(feed);
        }
        #endregion

        #region Buttons
        private void buttonOK_Click(object sender, EventArgs e)
        {
            _interfacePreferences.StabilityPolicy = (comboBoxStability.SelectedItem is Stability) ? (Stability)comboBoxStability.SelectedItem : Stability.Unset;
            try { _interfacePreferences.SaveFor(_interfaceID); }
            #region Error handling
            catch (IOException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                // ToDo: Cancel closing dialog
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                // ToDo: Cancel closing dialog
            }
            #endregion
        }

        private void listBoxFeeds_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Enable remove button if there is at least one removable object selected
            buttonRemoveFeed.Enabled = false;
            foreach (var item in listBoxFeeds.SelectedItems)
            {
                if (item is FeedReference)
                {
                    buttonRemoveFeed.Enabled = true;
                    return;
                }
            }
        }

        private void buttonAddFeed_Click(object sender, EventArgs e)
        {
            string feedID = InputBox.Show(this, "Feed", "Feed ID:");
            if (string.IsNullOrEmpty(feedID)) return;

            AddFeed(feedID);
        }

        private void buttonRemoveFeed_Click(object sender, EventArgs e)
        {
            var toRemove = new LinkedList<FeedReference>();
            foreach (var item in listBoxFeeds.SelectedItems)
            {
                var feed = item as FeedReference;
                if (feed != null) toRemove.Add(feed);
            }
            foreach (var feed in toRemove) RemoveFeed(feed);
        }
        #endregion

        #region Drag and drop handling
        private void listBoxFeeds_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = e.Data.GetData(DataFormats.FileDrop) as string[];
                foreach (var file in files) AddFeed(file);
            }
            else if (e.Data.GetDataPresent(DataFormats.Text))
            {
                AddFeed((string)e.Data.GetData(DataFormats.Text));
            }
        }

        private void listBoxFeeds_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = (e.Data.GetDataPresent(DataFormats.Text) || e.Data.GetDataPresent(DataFormats.FileDrop))
                ? DragDropEffects.Copy
                : DragDropEffects.None;
        }
        #endregion
    }
}
