/*
 * Copyright 2010-2012 Bastian Eicher
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

// Indicates that the solver provides no c and that they must therefore be generated here

#define NO_SOLVER_CANDIDATES

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using Common;
using Common.Controls;
using ZeroInstall.Commands.WinForms.Properties;
using ZeroInstall.Injector;
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

        /// <summary>Called after <see cref="InterfacePreferences"/> have been changed and the <see cref="ISolver"/> needs to be rerun.</summary>
        private readonly Func<Selections> _solveCallback;

        /// <summary>The feed cache used to retrieve <see cref="Feed"/>s for additional information about imlementations.</summary>
        private readonly IFeedCache _feedCache;

        /// <summary>The interface preferences being modified.</summary>
        private readonly InterfacePreferences _interfacePreferences;

#if NO_SOLVER_CANDIDATES
        /// <summary>A list of all feed IDs contributing to the selection process associated with their respective preferences.</summary>
        private readonly IDictionary<string, FeedPreferences> _feeds = new Dictionary<string, FeedPreferences>();
#else
    /// <summary>The last implementation selected for this interface.</summary>
        private ImplementationSelection _selection;
#endif
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new interface dialog.
        /// </summary>
        /// <param name="interfaceID">The interface to modify the preferences for.</param>
        /// <param name="solveCallback">Called after <see cref="InterfacePreferences"/> have been changed and the <see cref="ISolver"/> needs to be rerun.</param>
        /// <param name="feedCache">The feed cache used to retrieve feeds for additional information about imlementations.</param>
        private InterfaceDialog(string interfaceID, Func<Selections> solveCallback, IFeedCache feedCache)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            if (feedCache == null) throw new ArgumentNullException("feedCache");
            #endregion

            InitializeComponent();
            comboBoxStability.Items.AddRange(new object[] {"Use default setting", Stability.Stable, Stability.Testing, Stability.Developer});
            dataColumnUserStability.Items.AddRange(new object[] {Stability.Unset, Stability.Preferred, Stability.Packaged, Stability.Stable, Stability.Testing, Stability.Developer});

            _interfaceID = interfaceID;
            _solveCallback = solveCallback;
            _feedCache = feedCache;

            _interfacePreferences = InterfacePreferences.LoadForSafe(interfaceID);
        }

        private void InterfaceDialog_Load(object sender, EventArgs e)
        {
            Text = string.Format(Resources.PropertiesFor, _feedCache.GetFeed(_interfaceID).Name);

            if (_interfacePreferences.StabilityPolicy == Stability.Unset) comboBoxStability.SelectedItem = "Use default setting";
            else comboBoxStability.SelectedItem = _interfacePreferences.StabilityPolicy;

            UpdateDataGridVersions();
            LoadFeeds();

            if (_interfacePreferences.StabilityPolicy == Stability.Unset) comboBoxStability.SelectedItem = "Use default setting";
            else comboBoxStability.SelectedItem = _interfacePreferences.StabilityPolicy;
        }
        #endregion

        #region Static access
        /// <summary>
        /// Displays a dialog allowing the user to modify <see cref="InterfacePreferences"/> and <see cref="FeedPreferences"/> for an interface.
        /// </summary>
        /// <param name="owner">The parent window the displayed window is modal to; may be <see langword="null"/>.</param>
        /// <param name="interfaceID">The interface to modify the preferences for.</param>
        /// <param name="solveCallback">Called after <see cref="InterfacePreferences"/> have been changed and the <see cref="ISolver"/> needs to be rerun.</param>
        /// <param name="feedCache">The feed cache used to retrieve feeds for additional information about imlementations.</param>
        /// <returns><see langword="true"/> if the preferences where modified; <see langword="false"/> if everything remained unchanged.</returns>
        public static bool Show(IWin32Window owner, string interfaceID, Func<Selections> solveCallback, IFeedCache feedCache)
        {
            #region Sanity checks
            if (owner == null) throw new ArgumentNullException("owner");
            if (feedCache == null) throw new ArgumentNullException("feedCache");
            #endregion

            using (var dialog = new InterfaceDialog(interfaceID, solveCallback, feedCache))
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
#if NO_SOLVER_CANDIDATES
            _feeds.Add(_interfaceID, FeedPreferences.LoadForSafe(_interfaceID));
#endif
            listBoxFeeds.Items.Add(_interfaceID); // Add string => not removable in GUI

            // Add feeds references from main feed
            foreach (var reference in _feedCache.GetFeed(_interfaceID).Feeds)
            {
#if NO_SOLVER_CANDIDATES
                _feeds.Add(reference.Source, FeedPreferences.LoadForSafe(reference.Source));
#endif
                listBoxFeeds.Items.Add(reference.Source); // Add string => not removable in GUI
            }

            // Add manually registered feeds
            foreach (var reference in _interfacePreferences.Feeds)
            {
#if NO_SOLVER_CANDIDATES
                _feeds.Add(reference.Source, FeedPreferences.LoadForSafe(reference.Source));
#endif
                listBoxFeeds.Items.Add(reference); // Add complex object => removable in GUI
            }

#if NO_SOLVER_CANDIDATES
            UpdateDataGridVersions();
#endif
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

            // Make sure the feed is in the cache
            var policy = Policy.CreateDefault(new MinimalHandler(this));
            Feed feed;
            try
            {
                feed = policy.FeedManager.GetFeed(feedID, policy);
            }
                #region Error handling
            catch (OperationCanceledException)
            {
                return;
            }
            catch (InvalidInterfaceIDException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                return;
            }
            catch (IOException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                return;
            }
            catch (WebException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                return;
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                return;
            }
            catch (SignatureException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                return;
            }
            #endregion

            // Ensure a matching <feed-for> is present for online feeds
            Uri interfaceUri;
            if (ModelUtils.TryParseUri(_interfaceID, out interfaceUri) && !feed.FeedFor.Exists(entry => entry.Target == interfaceUri))
                if (!Msg.YesNo(this, Resources.IgnoreMissingFeedFor, MsgSeverity.Warn)) return;

#if NO_SOLVER_CANDIDATES
            if (_feeds.ContainsKey(feedID)) return; // Prevent duplicates
            _feeds.Add(feedID, FeedPreferences.LoadForSafe(feedID));
#endif

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

#if NO_SOLVER_CANDIDATES
            _feeds.Remove(feed.Source);
            UpdateDataGridVersions();
#endif
        }

        /// <summary>
        /// Gets a list of <see cref="SelectionCandidate"/>s from the <see cref="ISolver"/> to populate <see cref="dataGridVersions"/>.
        /// </summary>
        private void UpdateDataGridVersions()
        {
#if NO_SOLVER_CANDIDATES
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
                    Log.Warn("Unable to load feed '" + feedID + "'; skipping.");
                    Log.Error(ex);
                    continue;
                }
                catch (KeyNotFoundException ex)
                {
                    Log.Warn("Unable to load feed '" + feedID + "'; skipping.");
                    Log.Error(ex);
                    continue;
                }
                catch (IOException ex)
                {
                    Log.Warn("Unable to load feed '" + feedID + "'; skipping.");
                    Log.Error(ex);
                    continue;
                }
                catch (UnauthorizedAccessException ex)
                {
                    Log.Warn("Unable to load feed '" + feedID + "'; skipping.");
                    Log.Error(ex);
                    continue;
                }
                catch (InvalidDataException ex)
                {
                    Log.Warn("Unable to load feed '" + feedID + "'; skipping.");
                    Log.Error(ex);
                    continue;
                }
                #endregion

                var feedPreferences = feedEntry.Value;

                // ToDo: Get selection candidates from Solver
                foreach (var candidate in feed.Elements.OfType<Implementation>().
                    Select(implementation => GetSelectionCandidate(feedID, implementation, feedPreferences)).
                    Where(candidate => checkBoxShowAllVersions.Checked || candidate.IsSuitable))
                    candidates.Add(candidate);
            }
            dataGridVersions.DataSource = candidates;
#else
    // ToDo: Display progress
            _selection = _solveCallback()[_interfaceID];

            var list = new BindingList<SelectionCandidate> {AllowEdit = true, AllowNew = false};
            foreach (var candidate in _selection.Candidates)
                if (checkBoxShowAllVersions.Checked || candidate.IsUsable) list.Add(candidate);
            dataGridVersions.DataSource = list;
#endif
        }

        private static SelectionCandidate GetSelectionCandidate(string feedID, Implementation implementation, FeedPreferences feedPreferences)
        {
            return new SelectionCandidate(feedID, implementation, feedPreferences[implementation.ID], new Requirements {Architecture = Architecture.CurrentSystem});
        }
        #endregion

        #region Feed buttons
        private void listBoxFeeds_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Enable remove button if there is at least one removable object selected
            buttonRemoveFeed.Enabled = listBoxFeeds.SelectedItems.OfType<FeedReference>().Any();
        }

        private void buttonAddFeed_Click(object sender, EventArgs e)
        {
            string feedID = InputBox.Show(this, "Feed", Resources.EnterFeedUrl);
            if (string.IsNullOrEmpty(feedID)) return;

            AddFeed(feedID);
        }

        private void buttonRemoveFeed_Click(object sender, EventArgs e)
        {
            var toRemove = listBoxFeeds.SelectedItems.OfType<FeedReference>().ToList();

            if (!Msg.YesNo(this, string.Format(Resources.RemoveSelectedEntries, toRemove.Count), MsgSeverity.Warn)) return;
            foreach (var feed in toRemove) RemoveFeed(feed);
        }
        #endregion

        #region Global buttons
        private void checkBoxShowAllVersions_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDataGridVersions();
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            try
            {
                ApplySettings();
                UpdateDataGridVersions();
            }
                #region Error handling
            catch (IOException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }
            #endregion
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
#if NO_SOLVER_CANDIDATES
            // Read interface stability policy from ComboBox
            if (comboBoxStability.SelectedItem is Stability) _interfacePreferences.StabilityPolicy = (Stability)comboBoxStability.SelectedItem;
            else _interfacePreferences.StabilityPolicy = Stability.Unset;
#endif

            try
            {
                ApplySettings();
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

            // Window is automatically closed
        }

        private void ApplySettings()
        {
            // Read interface stability policy from ComboBox
            if (comboBoxStability.SelectedItem is Stability) _interfacePreferences.StabilityPolicy = (Stability)comboBoxStability.SelectedItem;
            else _interfacePreferences.StabilityPolicy = Stability.Unset;

            // Save interface preferences
            _interfacePreferences.SaveFor(_interfaceID);

            // Save all feed preferences
#if NO_SOLVER_CANDIDATES
            foreach (var feedEntry in _feeds)
#else
            foreach (var feedEntry in _selection.Feeds)
#endif
                feedEntry.Value.SaveFor(feedEntry.Key);
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
