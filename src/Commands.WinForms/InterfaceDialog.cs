// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using NanoByte.Common.Controls;
using NanoByte.Common.Net;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Model;
using ZeroInstall.Model.Preferences;
using ZeroInstall.Model.Selection;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Services.Solvers;

namespace ZeroInstall.Commands.WinForms
{
    /// <summary>
    /// Dialog for modifying <see cref="InterfacePreferences"/> and <see cref="FeedPreferences"/>.
    /// </summary>
    public partial class InterfaceDialog : OKCancelDialog
    {
        #region Variables
        /// <summary>The interface to modify the preferences for.</summary>
        private readonly FeedUri _interfaceUri;

        /// <summary>The feed loaded from <see cref="_interfaceUri"/>.</summary>
        private readonly Feed _mainFeed;

        /// <summary>The feed cache used to retrieve <see cref="Feed"/>s for additional information about implementations.</summary>
        private readonly IFeedManager _feedManager;

        /// <summary>Called after <see cref="InterfacePreferences"/> have been changed and the <see cref="ISolver"/> needs to be rerun.</summary>
        private readonly Func<Selections> _solveCallback;

        /// <summary>The interface preferences being modified.</summary>
        private readonly InterfacePreferences _interfacePreferences;

        /// <summary>The last implementation selected for this interface.</summary>
        private IEnumerable<SelectionCandidate> _candidates = Enumerable.Empty<SelectionCandidate>();
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new interface dialog.
        /// </summary>
        /// <param name="interfaceUri">The interface to modify the preferences for.</param>
        /// <param name="solveCallback">Called after <see cref="InterfacePreferences"/> have been changed and the <see cref="ISolver"/> needs to be rerun.</param>
        /// <param name="feedManager">The feed manager used to retrieve feeds for additional information about implementations.</param>
        private InterfaceDialog(FeedUri interfaceUri, Func<Selections> solveCallback, IFeedManager feedManager)
        {
            #region Sanity checks
            if (interfaceUri == null) throw new ArgumentNullException(nameof(interfaceUri));
            if (solveCallback == null) throw new ArgumentNullException(nameof(solveCallback));
            if (feedManager == null) throw new ArgumentNullException(nameof(feedManager));
            #endregion

            InitializeComponent();
            comboBoxStability.Items.AddRange(new object[] {Resources.UseDefaultSetting, Stability.Stable, Stability.Testing, Stability.Developer});
            dataColumnUserStability.Items.AddRange(Stability.Unset, Stability.Preferred, Stability.Stable, Stability.Testing, Stability.Developer, Stability.Buggy, Stability.Insecure);

            _interfaceUri = interfaceUri;
            _mainFeed = feedManager[_interfaceUri];
            _solveCallback = solveCallback;
            _feedManager = feedManager;
            _interfacePreferences = InterfacePreferences.LoadForSafe(_interfaceUri);
        }

        private void InterfaceDialog_Load(object sender, EventArgs e)
        {
            Text = string.Format(Resources.PropertiesFor, _mainFeed.Name);

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
        /// <param name="owner">The parent window the displayed window is modal to; can be <c>null</c>.</param>
        /// <param name="interfaceUri">The interface to modify the preferences for.</param>
        /// <param name="solveCallback">Called after <see cref="InterfacePreferences"/> have been changed and the <see cref="ISolver"/> needs to be rerun.</param>
        /// <param name="feedManager">The feed manager used to retrieve feeds for additional information about implementations.</param>
        public static void Show(IWin32Window? owner, FeedUri interfaceUri, Func<Selections> solveCallback, IFeedManager feedManager)
        {
            #region Sanity checks
            if (interfaceUri == null) throw new ArgumentNullException(nameof(interfaceUri));
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            if (solveCallback == null) throw new ArgumentNullException(nameof(solveCallback));
            if (feedManager == null) throw new ArgumentNullException(nameof(feedManager));
            #endregion

            using var dialog = new InterfaceDialog(interfaceUri, solveCallback, feedManager);
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
                _candidates = _solveCallback()[_interfaceUri].Candidates ?? GenerateDummyCandidates();
            }
            #region Error handling
            catch (Exception ex) when (ex is KeyNotFoundException or OperationCanceledException)
            {
                _candidates = Enumerable.Empty<SelectionCandidate>();
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or WebException)
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
            // ReSharper disable once InvokeAsExtensionMethod
            var feedUris = Enumerable.Concat(
                listBoxFeeds.Items.OfType<FeedUri>(),
                listBoxFeeds.Items.OfType<FeedReference>().Select(x => x.Source));
            return feedUris.SelectMany(GenerateDummyCandidates).ToList();
        }

        private IEnumerable<SelectionCandidate> GenerateDummyCandidates(FeedUri feedUri)
        {
            if (feedUri.IsFromDistribution) return Enumerable.Empty<SelectionCandidate>();

            try
            {
                var feed = _feedManager[feedUri];
                var feedPreferences = _feedManager.GetPreferences(feedUri);
                return feed.Implementations.Select(implementation => GenerateDummyCandidate(feedUri, feedPreferences, implementation));
            }
            #region Error handling
            catch (KeyNotFoundException)
            {
                return Enumerable.Empty<SelectionCandidate>();
            }
            #endregion
        }

        private SelectionCandidate GenerateDummyCandidate(FeedUri feedUri, FeedPreferences feedPreferences, Implementation implementation)
            => new(feedUri, feedPreferences, implementation,
                new Requirements(_interfaceUri, "", new Architecture(Architecture.CurrentSystem.OS, Cpu.All)));

        /// <summary>
        /// Gets a list of <see cref="SelectionCandidate"/>s from the <see cref="ISolver"/> to populate <see cref="dataGridVersions"/>.
        /// </summary>
        private void UpdateDataGridVersions()
        {
            var list = new BindingList<SelectionCandidate> {AllowEdit = true, AllowNew = false};
            foreach (var candidate in _candidates)
            {
                if (candidate.IsSuitable || checkBoxShowAllVersions.Checked)
                    list.Add(candidate);
            }
            dataGridVersions.DataSource = list;
        }
        #endregion

        #region Preferences
        private void ApplySettings()
        {
            try
            {
                // Read interface stability policy from ComboBox
                if (comboBoxStability.SelectedItem is Stability stability) _interfacePreferences.StabilityPolicy = stability;
                else _interfacePreferences.StabilityPolicy = Stability.Unset;

                // Save interface preferences
                _interfacePreferences.SaveFor(_interfaceUri);

                // Use one representative candidate for each feed to save preferences
                foreach (var candidate in _candidates.ToLookup(x => x.FeedUri).Select(x => x.First()))
                {
                    candidate.FeedPreferences.Normalize();
                    candidate.FeedPreferences.SaveFor(candidate.FeedUri);
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
            listBoxFeeds.Items.Add(_interfaceUri); // string, not removable in GUI

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
        private void AddCustomFeed(string input)
        {
            FeedUri feedUri;
            try
            {
                feedUri = new(input);
            }
            catch (UriFormatException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                return;
            }

            if (_interfaceUri.IsFile)
            {
                if (!File.Exists(_interfaceUri.LocalPath))
                {
                    Msg.Inform(this, string.Format(Resources.FileOrDirNotFound, _interfaceUri.LocalPath), MsgSeverity.Warn);
                    return;
                }
            }
            else
            {
                Feed feed = null;
                try
                {
                    using var handler = new DialogTaskHandler(this);
                    handler.RunTask(new SimpleTask(Resources.CheckingFeed,
                        delegate
                        {
                            using var webClient = new WebClientTimeout();
                            feed = XmlStorage.FromXmlString<Feed>(webClient.DownloadString(feedUri));
                        }));
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
                catch (InvalidDataException ex)
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
                if (feed.FeedFor.All(entry => entry.Target != _interfaceUri))
                    if (!Msg.YesNo(this, Resources.IgnoreMissingFeedFor, MsgSeverity.Warn))
                        return;
            }

            var reference = new FeedReference {Source = feedUri};
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
            => buttonRemoveFeed.Enabled = listBoxFeeds.SelectedItems.OfType<FeedReference>().Any();

        private void buttonAddFeed_Click(object sender, EventArgs e)
        {
            string input = InputBox.Show(this, "Feed", Resources.EnterFeedUrl);
            if (!string.IsNullOrEmpty(input)) AddCustomFeed(input);
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
            => UpdateDataGridVersions();

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
                if (e.Data.GetData(DataFormats.FileDrop) is string[] files)
                {
                    foreach (string file in files)
                        AddCustomFeed(file);
                }
            }
            else if (e.Data.GetDataPresent(DataFormats.Text))
                AddCustomFeed((string)e.Data.GetData(DataFormats.Text));
        }

        private void listBoxFeeds_DragEnter(object sender, DragEventArgs e)
            => e.Effect = (e.Data.GetDataPresent(DataFormats.Text) || e.Data.GetDataPresent(DataFormats.FileDrop))
                ? DragDropEffects.Copy
                : DragDropEffects.None;
        #endregion
    }
}
