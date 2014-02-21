/*
 * Copyright 2010-2014 Bastian Eicher
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
using System.Linq;
using System.Net;
using System.Windows.Forms;
using Common;
using Common.Controls;
using Common.Storage;
using Common.Tasks;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Services.Solvers;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Preferences;
using ZeroInstall.Store.Model.Selection;

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

        /// <summary>The feed loaded from <see cref="_interfaceID"/>.</summary>
        private readonly Feed _mainFeed;

        /// <summary>The feed cache used to retrieve <see cref="Feed"/>s for additional information about implementations.</summary>
        private readonly IFeedCache _feedCache;

        /// <summary>Called after <see cref="InterfacePreferences"/> have been changed and the <see cref="ISolver"/> needs to be rerun.</summary>
        private readonly Func<Selections> _solveCallback;

        /// <summary>The interface preferences being modified.</summary>
        private InterfacePreferences _interfacePreferences;

        /// <summary>The last implementation selected for this interface.</summary>
        private IEnumerable<SelectionCandidate> _candidates;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new interface dialog.
        /// </summary>
        /// <param name="interfaceID">The interface to modify the preferences for.</param>
        /// <param name="solveCallback">Called after <see cref="InterfacePreferences"/> have been changed and the <see cref="ISolver"/> needs to be rerun.</param>
        /// <param name="feedCache">The feed cache used to retrieve feeds for additional information about implementations.</param>
        private InterfaceDialog(string interfaceID, Func<Selections> solveCallback, IFeedCache feedCache)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            if (feedCache == null) throw new ArgumentNullException("feedCache");
            #endregion

            InitializeComponent();
            comboBoxStability.Items.AddRange(new object[] {Resources.UseDefaultSetting, Stability.Stable, Stability.Testing, Stability.Developer});
            dataColumnUserStability.Items.AddRange(new object[] {Stability.Unset, Stability.Preferred, Stability.Packaged, Stability.Stable, Stability.Testing, Stability.Developer});

            _interfaceID = interfaceID;
            _mainFeed = feedCache.GetFeed(_interfaceID);
            _solveCallback = solveCallback;
            _feedCache = feedCache;
        }

        private void InterfaceDialog_Load(object sender, EventArgs e)
        {
            Text = string.Format(Resources.PropertiesFor, _mainFeed.Name);

            _interfacePreferences = InterfacePreferences.LoadForSafe(_interfaceID);
            if (_interfacePreferences.StabilityPolicy == Stability.Unset) comboBoxStability.SelectedItem = Resources.UseDefaultSetting;
            else comboBoxStability.SelectedItem = _interfacePreferences.StabilityPolicy;

            LoadFeeds();
            Solve();
        }
        #endregion

        #region Static access
        /// <summary>
        /// Displays a dialog allowing the user to modify <see cref="InterfacePreferences"/> and <see cref="FeedPreferences"/> for an interface.
        /// </summary>
        /// <param name="owner">The parent window the displayed window is modal to; may be <see langword="null"/>.</param>
        /// <param name="interfaceID">The interface to modify the preferences for.</param>
        /// <param name="solveCallback">Called after <see cref="InterfacePreferences"/> have been changed and the <see cref="ISolver"/> needs to be rerun.</param>
        /// <param name="feedCache">The feed cache used to retrieve feeds for additional information about implementations.</param>
        public static void Show(IWin32Window owner, string interfaceID, Func<Selections> solveCallback, IFeedCache feedCache)
        {
            #region Sanity checks
            if (owner == null) throw new ArgumentNullException("owner");
            if (feedCache == null) throw new ArgumentNullException("feedCache");
            #endregion

            using (var dialog = new InterfaceDialog(interfaceID, solveCallback, feedCache))
                dialog.ShowDialog(owner);
        }
        #endregion

        //--------------------//

        #region Candidates
        /// <summary>
        /// Uses the <see cref="_solveCallback"/> to get the <see cref="SelectionCandidate"/>s.
        /// </summary>
        private void Solve()
        {
            try
            {
                TrackingDialog.Run(this, new SimpleTask(Resources.Working, () => { _candidates = _solveCallback()[_interfaceID].Candidates ?? GenerateDummyCandidates(); }));
            }
                #region Error handling
            catch (OperationCanceledException)
            {
                _candidates = Enumerable.Empty<SelectionCandidate>();
            }
            catch (IOException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                _candidates = Enumerable.Empty<SelectionCandidate>();
            }
            catch (WebException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                _candidates = Enumerable.Empty<SelectionCandidate>();
            }
            #endregion

            UpdateDataGridVersions();
        }

        /// <summary>
        /// Generates <see cref="SelectionCandidate"/>s using minimalistic dummy <see cref="Requirements"/>.
        /// </summary>
        private IEnumerable<SelectionCandidate> GenerateDummyCandidates()
        {
            var feedIDs = Enumerable.Concat(
                listBoxFeeds.Items.OfType<string>(),
                listBoxFeeds.Items.OfType<FeedReference>().Select(x => x.Source));
            return feedIDs.SelectMany(GenerateDummyCandidates).ToList();
        }

        private IEnumerable<SelectionCandidate> GenerateDummyCandidates(string feedID)
        {
            try
            {
                var feed = _feedCache.GetFeed(feedID);
                var feedPreferences = FeedPreferences.LoadForSafe(feedID);
                return feed.Elements.OfType<Implementation>().Select(implementation => GenerateDummyCandidate(feedID, feedPreferences, implementation));
            }
                #region Error handling
            catch (KeyNotFoundException)
            {
                return Enumerable.Empty<SelectionCandidate>();
            }
            #endregion
        }

        private static SelectionCandidate GenerateDummyCandidate(string feedID, FeedPreferences feedPreferences, Implementation implementation)
        {
            return new SelectionCandidate(feedID, feedPreferences, implementation,
                new Requirements
                {
                    Command = Command.NameRun,
                    Architecture = new Architecture(Architecture.CurrentSystem.OS, Cpu.All)
                });
        }

        /// <summary>
        /// Gets a list of <see cref="SelectionCandidate"/>s from the <see cref="ISolver"/> to populate <see cref="dataGridVersions"/>.
        /// </summary>
        private void UpdateDataGridVersions()
        {
            var candidates = checkBoxShowAllVersions.Checked ? _candidates : _candidates.Where(candidate => candidate.IsSuitable);
            var list = new BindingList<SelectionCandidate> {AllowEdit = true, AllowNew = false};
            foreach (var candidate in candidates) list.Add(candidate);
            dataGridVersions.DataSource = list;
        }
        #endregion

        #region Preferences
        private void ApplySettings()
        {
            try
            {
                // Read interface stability policy from ComboBox
                if (comboBoxStability.SelectedItem is Stability) _interfacePreferences.StabilityPolicy = (Stability)comboBoxStability.SelectedItem;
                else _interfacePreferences.StabilityPolicy = Stability.Unset;

                // Save interface preferences
                _interfacePreferences.SaveFor(_interfaceID);

                // Use one representative candidate for each feed to save preferences
                foreach (var candidate in _candidates.ToLookup(x => x.FeedID).Select(x => x.First()))
                {
                    candidate.FeedPreferences.Normalize();
                    candidate.FeedPreferences.SaveFor(candidate.FeedID);
                }
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
        #endregion

        #region Feeds
        /// <summary>
        /// Builds the initial list of feeds.
        /// </summary>
        private void LoadFeeds()
        {
            // Main feed
            listBoxFeeds.Items.Add(_interfaceID); // string, not removable in GUI

            // Feeds references from main feed
            foreach (var reference in _mainFeed.Feeds)
                listBoxFeeds.Items.Add(reference.Source); // string, not removable in GUI

            // Custom feeds
            foreach (var reference in _interfacePreferences.Feeds)
                listBoxFeeds.Items.Add(reference); // complex object, removable in GUI
        }

        /// <summary>
        /// Adds a feed to the list of custom feeds.
        /// </summary>
        private void AddCustomFeed(string feedID)
        {
            try
            {
                ModelUtils.ValidateInterfaceID(feedID);
            }
            catch (InvalidInterfaceIDException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                return;
            }

            Feed feed = null;
            try
            {
                TrackingDialog.Run(this, new SimpleTask(Resources.CheckingFeed, () => feed = XmlStorage.FromXmlString<Feed>(new WebClient().DownloadString(feedID))));
            }
                #region Error handling
            catch (OperationCanceledException)
            {
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
            #endregion

            // Ensure a matching <feed-for> is present for online feeds
            Uri interfaceUri;
            if (ModelUtils.TryParseUri(_interfaceID, out interfaceUri) && feed.FeedFor.All(entry => entry.Target != interfaceUri))
                if (!Msg.YesNo(this, Resources.IgnoreMissingFeedFor, MsgSeverity.Warn)) return;

            var reference = new FeedReference {Source = feedID};
            if (_interfacePreferences.Feeds.Contains(reference)) return;
            _interfacePreferences.Feeds.Add(reference);
            listBoxFeeds.Items.Add(reference);
        }

        /// <summary>
        /// Removes a feed from the list of custom feeds.
        /// </summary>
        private void RemoveCustomFeed(FeedReference feed)
        {
            listBoxFeeds.Items.Remove(feed);
            _interfacePreferences.Feeds.Remove(feed);
        }
        #endregion

        //--------------------//

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

            AddCustomFeed(feedID);
        }

        private void buttonRemoveFeed_Click(object sender, EventArgs e)
        {
            var toRemove = listBoxFeeds.SelectedItems.OfType<FeedReference>().ToList();

            if (!Msg.YesNo(this, string.Format(Resources.RemoveSelectedEntries, toRemove.Count), MsgSeverity.Warn)) return;
            foreach (var feed in toRemove) RemoveCustomFeed(feed);
        }
        #endregion

        #region Global buttons
        private void checkBoxShowAllVersions_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDataGridVersions();
        }

        private void buttonOKApply_Click(object sender, EventArgs e)
        {
            ApplySettings();
            Solve();
        }
        #endregion

        #region Drag and drop handling
        private void listBoxFeeds_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (files != null) foreach (var file in files) AddCustomFeed(file);
            }
            else if (e.Data.GetDataPresent(DataFormats.Text))
                AddCustomFeed((string)e.Data.GetData(DataFormats.Text));
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
