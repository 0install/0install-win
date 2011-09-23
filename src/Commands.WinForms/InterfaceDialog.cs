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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using Common;
using Common.Controls;
using ZeroInstall.Commands.WinForms.Properties;
using ZeroInstall.Injector.Feeds;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Model;
using ZeroInstall.Store.Feeds;

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

        /// <summary>The feed cache used to retreive <see cref="Feed"/>s for additional information about imlementations.</summary>
        private readonly IFeedCache _feedCache;

        /// <summary>The interface preferences being modified.</summary>
        private readonly InterfacePreferences _interfacePreferences;

        /// <summary>A list of all feed IDs contributing to the selection process associated with their respective preferences.</summary>
        private readonly IDictionary<string, FeedPreferences> _feeds = new Dictionary<string, FeedPreferences>();
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new interface dialog.
        /// </summary>
        /// <param name="interfaceID">The interface to modify the preferences for.</param>
        /// <param name="feedCache">The feed cache used to retreive feeds for additional information about imlementations.</param>
        private InterfaceDialog(string interfaceID, IFeedCache feedCache)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            if (feedCache == null) throw new ArgumentNullException("feedCache");
            #endregion

            InitializeComponent();
            comboBoxStability.Items.AddRange(new object[] {"Use default setting", Stability.Stable, Stability.Testing, Stability.Developer});
            dataColumnUserStability.Items.AddRange(new object[] {Stability.Unset, Stability.Preferred, Stability.Packaged, Stability.Stable, Stability.Testing, Stability.Developer});

            _interfaceID = interfaceID;
            _feedCache = feedCache;

            _interfacePreferences = InterfacePreferences.LoadForSafe(interfaceID);
        }

        private void InterfaceDialog_Load(object sender, EventArgs e)
        {
            Text = string.Format(Resources.PropertiesFor, _feedCache.GetFeed(_interfaceID).Name);

            if (_interfacePreferences.StabilityPolicy == Stability.Unset) comboBoxStability.SelectedItem = "Use default setting";
            else comboBoxStability.SelectedItem = _interfacePreferences.StabilityPolicy;

            LoadFeeds();
        }
        #endregion

        #region Static access
        /// <summary>
        /// Displays a dialog allowing the user to modify <see cref="InterfacePreferences"/> and <see cref="FeedPreferences"/> for an interface.
        /// </summary>
        /// <param name="owner">The parent window the displayed window is modal to; may be <see langword="null"/>.</param>
        /// <param name="interfaceID">The interface to modify the preferences for.</param>
        /// <param name="feedCache">The feed cache used to retreive feeds for additional information about imlementations.</param>
        /// <returns><see langword="true"/> if the preferences where modified; <see langword="false"/> if everything remained unchanged.</returns>
        public static bool Show(IWin32Window owner, string interfaceID, IFeedCache feedCache)
        {
            #region Sanity checks
            if (owner == null) throw new ArgumentNullException("owner");
            if (feedCache == null) throw new ArgumentNullException("feedCache");
            #endregion

            using (var dialog = new InterfaceDialog(interfaceID, feedCache))
                return (dialog.ShowDialog() == DialogResult.OK);
        }
        #endregion

        //--------------------//

        #region Feed helpers
        /// <summary>
        /// Builds the initial list of feeds.
        /// </summary>
        private void LoadFeeds()
        {
            // Add main feed
            _feeds.Add(_interfaceID, FeedPreferences.LoadForSafe(_interfaceID));
            listBoxFeeds.Items.Add(_interfaceID); // Add string => not removable in GUI

            // Add feeds references from main feed
            foreach (var reference in _feedCache.GetFeed(_interfaceID).Feeds)
            {
                _feeds.Add(reference.Source, FeedPreferences.LoadForSafe(reference.Source));
                listBoxFeeds.Items.Add(reference.Source); // Add string => not removable in GUI
            }

            // Add manually registered feeds
            foreach (var reference in _interfacePreferences.Feeds)
            {
                _feeds.Add(reference.Source, FeedPreferences.LoadForSafe(reference.Source));
                listBoxFeeds.Items.Add(reference); // Add complex object => removable in GUI
            }

            UpdateDataGridVersions();
        }

        /// <summary>
        /// Adds a feed to the list of registered additional feeds.
        /// </summary>
        private void AddFeed(string feedID)
        {
            try
            {
                ModelUtils.ValidateInterfaceID(feedID);
            }
                #region Error handling
            catch (InvalidInterfaceIDException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                return;
            }
            #endregion

            // ToDo: Load feed into cache if missing
            // ToDo: Ensure <feed-for> is set

            _feeds.Add(feedID, FeedPreferences.LoadForSafe(feedID));

            var reference = new FeedReference {Source = feedID};
            _interfacePreferences.Feeds.Add(reference);
            listBoxFeeds.Items.Add(reference);

            UpdateDataGridVersions();
        }

        /// <summary>
        /// Removes a feed from the list of registered additional feeds.
        /// </summary>
        private void RemoveFeed(FeedReference feed)
        {
            listBoxFeeds.Items.Remove(feed);
            _interfacePreferences.Feeds.Remove(feed);

            _feeds.Remove(feed.Source);

            UpdateDataGridVersions();
        }

        /// <summary>
        /// Generates a list of <see cref="SelectionCandidate"/>s to populate <see cref="dataGridVersions"/>.
        /// </summary>
        private void UpdateDataGridVersions()
        {
            var candidates = new BindingList<SelectionCandidate> {AllowEdit = true, AllowNew = false};
            foreach (var feedEntry in _feeds)
            {
                string feedID = feedEntry.Key;
                Feed feed;
                try
                {
                    feed = _feedCache.GetFeed(feedID);
                }
                    #region Error handling
                catch (InvalidInterfaceIDException ex)
                {
                    Log.Error("Unable to load feed '" + feedID + "'; skipping.\n" + ex.Message);
                    continue;
                }
                catch (KeyNotFoundException ex)
                {
                    Log.Error("Unable to load feed '" + feedID + "'; skipping.\n" + ex.Message);
                    continue;
                }
                catch (IOException ex)
                {
                    Log.Error("Unable to load feed '" + feedID + "'; skipping.\n" + ex.Message);
                    continue;
                }
                catch (UnauthorizedAccessException ex)
                {
                    Log.Error("Unable to load feed '" + feedID + "'; skipping.\n" + ex.Message);
                    continue;
                }
                catch (InvalidDataException ex)
                {
                    Log.Error("Unable to load feed '" + feedID + "'; skipping.\n" + ex.Message + (ex.InnerException == null ? "" : "\n" + ex.InnerException.Message));
                    continue;
                }
                #endregion

                var feedPreferences = feedEntry.Value;

                foreach (var element in feed.Elements)
                {
                    var implementation = element as Implementation;
                    if (implementation == null) continue;

                    // ToDo: Respect architecture overrides in requirements
                    var candidate = new SelectionCandidate(feedID, implementation, feedPreferences.GetImplementationPreferences(implementation.ID));
                    if (checkBoxShowAllVersions.Checked || candidate.IsCompatible)
                        candidates.Add(candidate);
                }
            }
            dataGridVersions.DataSource = candidates;
        }
        #endregion

        #region Buttons
        private void checkBoxShowAllVersions_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDataGridVersions();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            // Read interface stability policy from ComboBox
            if (comboBoxStability.SelectedItem is Stability) _interfacePreferences.StabilityPolicy = (Stability)comboBoxStability.SelectedItem;
            else _interfacePreferences.StabilityPolicy = Stability.Unset;

            try
            {
                // Save interface preferences
                _interfacePreferences.SaveFor(_interfaceID);

                // Save all feed preferences
                foreach (var feedEntry in _feeds)
                {
                    var preferences = feedEntry.Value;
                    preferences.Simplify();
                    preferences.SaveFor(feedEntry.Key);
                }
            }
                #region Error handling
            catch (IOException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                // ToDo: Cancel closing the dialog
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                // ToDo: Cancel closing the dialog
            }
            #endregion
        }

        private void listBoxFeeds_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Enable remove button if there is at least one removable object selected
            buttonRemoveFeed.Enabled = false;
            foreach (var item in listBoxFeeds.SelectedItems)
            {
                if (item is FeedReference /*&& _interfacePreferences.Feeds.Contains((FeedReference)item)*/)
                {
                    buttonRemoveFeed.Enabled = true;
                    return;
                }
            }
        }

        private void buttonAddFeed_Click(object sender, EventArgs e)
        {
            string feedID = InputBox.Show(this, "Feed", Resources.EnterFeedUrl);
            if (string.IsNullOrEmpty(feedID)) return;

            AddFeed(feedID);
        }

        private void buttonRemoveFeed_Click(object sender, EventArgs e)
        {
            var toRemove = new LinkedList<FeedReference>();
            foreach (var item in listBoxFeeds.SelectedItems)
            {
                var feed = item as FeedReference;
                if (feed != null) toRemove.AddLast(feed);
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
                AddFeed((string)e.Data.GetData(DataFormats.Text));
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
