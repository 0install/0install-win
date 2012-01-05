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
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Common;
using Common.Tasks;
using ZeroInstall.Commands.WinForms.Properties;
using ZeroInstall.Injector.Feeds;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Model;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall.Commands.WinForms
{
    /// <summary>
    /// Visualizes <see cref="Selections"/> and allows modifications to <see cref="FeedPreferences"/> and <see cref="InterfacePreferences"/>.
    /// </summary>
    public partial class SelectionsControl : UserControl
    {
        #region Variables
        /// <summary>The current selections state as set by <see cref="SetSelections"/> or updated during <see cref="BeginAudit"/>.</summary>
        private Selections _selections;

        /// <summary>The feed cache used to retreive feeds for additional information about imlementations.</summary>
        private IFeedCache _feedCache;

        /// <summary>>A wait handle to be signaled once the user is done with <see cref="BeginAudit"/>.</summary>
        private EventWaitHandle _waitHandle;

        /// <summary>A list of all <see cref="TrackingControl"/>s used by <see cref="TrackTask"/>. Adressable by associated <see cref="Implementation"/> via <see cref="ManifestDigest"/>.</summary>
        private readonly Dictionary<ManifestDigest, TrackingControl> _trackingControls = new Dictionary<ManifestDigest, TrackingControl>();

        /// <summary>A list of all controls visible only while the user is busy with <see cref="BeginAudit"/>.</summary>
        private readonly LinkedList<Control> _auditLinks = new LinkedList<Control>();
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new <see cref="Selections"/> control.
        /// </summary>
        public SelectionsControl()
        {
            InitializeComponent();
        }
        #endregion

        //--------------------//

        #region Selections
        /// <summary>
        /// Shows the user the <see cref="Selections"/> made by the <see cref="ISolver"/>.
        /// </summary>
        /// <param name="selections">The <see cref="Selections"/> as provided by the <see cref="ISolver"/>.</param>
        /// <param name="feedCache">The feed cache used to retreive feeds for additional information about imlementations.</param>
        /// <remarks>
        ///   <para>This method must not be called from a background thread.</para>
        ///   <para>This method must not be called before <see cref="Control.Handle"/> has been created.</para>
        /// </remarks>
        public void SetSelections(Selections selections, IFeedCache feedCache)
        {
            #region Sanity checks
            if (selections == null) throw new ArgumentNullException("selections");
            if (InvokeRequired) throw new InvalidOperationException("Method called from a non UI thread.");
            if (!IsHandleCreated) throw new InvalidOperationException("Method called before control handle was created.");
            #endregion

            _selections = selections;
            _feedCache = feedCache;

            // Build TableLayout rows
            tableLayout.Controls.Clear();
            tableLayout.RowStyles.Clear();
            tableLayout.RowCount = selections.Implementations.Count;
            for (int i = 0; i < _selections.Implementations.Count; i++)
            {
                // Lines have a fixed height but a variable width
                tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));

                // Get feed for each selected implementation
                var implementation = _selections.Implementations[i];
                Feed feed = (!string.IsNullOrEmpty(implementation.FromFeed) && _feedCache.Contains(implementation.FromFeed))
                    ? _feedCache.GetFeed(implementation.FromFeed)
                    : _feedCache.GetFeed(implementation.InterfaceID);

                // Display application name and implementation version
                tableLayout.Controls.Add(new Label {Text = feed.Name, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft}, 0, i);
                tableLayout.Controls.Add(new Label {Text = implementation.Version.ToString(), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft}, 1, i);
            }
        }

        /// <summary>
        /// Allows the user to modify the <see cref="InterfacePreferences"/> and rerun the <see cref="ISolver"/> if desired.
        /// </summary>
        /// <param name="solveCallback">Called after <see cref="InterfacePreferences"/> have been changed and the <see cref="ISolver"/> needs to be rerun.</param>
        /// <param name="waitHandle">A wait handle to be signaled once the user is satisfied with the <see cref="Selections"/>.</param>
        /// <remarks>
        ///   <para>This method must not be called from a background thread.</para>
        ///   <para>This method must not be called before <see cref="Control.Handle"/> has been created.</para>
        /// </remarks>
        public void BeginAudit(SimpleResult<Selections> solveCallback, EventWaitHandle waitHandle)
        {
            #region Sanity checks
            if (solveCallback == null) throw new ArgumentNullException("solveCallback");
            if (waitHandle == null) throw new ArgumentNullException("waitHandle");
            if (InvokeRequired) throw new InvalidOperationException("Method called from a non UI thread.");
            if (!IsHandleCreated) throw new InvalidOperationException("Method called before control handle was created.");
            #endregion

            _waitHandle = waitHandle;

            for (int i = 0; i < _selections.Implementations.Count; i++)
            {
                string interfaceID = _selections.Implementations[i].InterfaceID;

                // Setup link label for modifying interface preferences
                var linkLabel = new LinkLabel {Text = Resources.Change, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft};
                linkLabel.LinkClicked += delegate { if (InterfaceDialog.Show(this, interfaceID, _feedCache)) ReSolve(solveCallback, waitHandle); };
                _auditLinks.AddLast(linkLabel);
                tableLayout.Controls.Add(linkLabel, 2, i);
            }

            buttonDone.Visible = true;
            buttonDone.Focus();
        }

        /// <summary>
        /// Reruns the solver to reflect changes made to <see cref="InterfacePreferences"/> and <see cref="FeedPreferences"/>.
        /// </summary>
        /// <param name="solveCallback">Called to invoke the solver.</param>
        /// <param name="waitHandle">A wait handle to be signaled once the user is satisfied with the <see cref="Selections"/>.</param>
        private void ReSolve(SimpleResult<Selections> solveCallback, EventWaitHandle waitHandle)
        {
            // Prevent user interaction while solving
            Visible = false;

            var solveWorker = new BackgroundWorker();
            solveWorker.DoWork += delegate { _selections = solveCallback(); };
            solveWorker.RunWorkerCompleted += delegate
            {
                // Update the UI
                SetSelections(_selections, _feedCache);
                _auditLinks.Clear();
                BeginAudit(solveCallback, waitHandle);

                // Restore user interaction
                Visible = true;
            };
            solveWorker.RunWorkerAsync();
        }
        #endregion

        #region Task tracking
        /// <summary>
        /// Registers an <see cref="ITask"/> for a specific implementation for tracking.
        /// </summary>
        /// <param name="task">The task to be tracked. May or may not alreay be running.</param>
        /// <param name="tag">A digest used to associate the <paramref name="task"/> with a specific implementation.</param>
        /// <remarks>
        ///   <para>This method must not be called from a background thread.</para>
        ///   <para>This method must not be called before <see cref="Control.Handle"/> has been created.</para>
        /// </remarks>
        public void TrackTask(ITask task, ManifestDigest tag)
        {
            #region Sanity checks
            if (task == null) throw new ArgumentNullException("task");
            if (InvokeRequired) throw new InvalidOperationException("Method called from a non UI thread.");
            if (!IsHandleCreated) throw new InvalidOperationException("Method called before control handle was created.");
            #endregion

            for (int i = 0; i < _selections.Implementations.Count; i++)
            {
                // Locate the row for the implementation the task is associated to
                var implementation = _selections.Implementations[i];
                if (implementation.ManifestDigest.PartialEquals(tag))
                {
                    // Try to find an existing tracking control
                    TrackingControl trackingControl;
                    if (!_trackingControls.TryGetValue(implementation.ManifestDigest, out trackingControl))
                    {
                        // Create a new tracking control if none exists
                        trackingControl = new TrackingControl {Dock = DockStyle.Fill};
                        trackingControl.CreateGraphics(); // Ensure control initialization even in tray icon mode
                        _trackingControls.Add(implementation.ManifestDigest, trackingControl);
                        tableLayout.Controls.Add(trackingControl, 2, i);
                    }

                    // Start tracking the task
                    trackingControl.Task = task;
                }
            }
        }

        /// <summary>
        /// Stops tracking <see cref="ITask"/>s.
        /// </summary>
        public void StopTracking()
        {
            foreach (var trackingControl in _trackingControls.Values)
                trackingControl.Task = null;
        }
        #endregion

        #region Buttons
        private void buttonDone_Click(object sender, EventArgs e)
        {
            // Remove all auditing-related controls
            foreach (var control in _auditLinks)
                tableLayout.Controls.Remove(control);
            buttonDone.Visible = false;

            // Signal the waiting thread auditing is complete
            if (_waitHandle != null) _waitHandle.Set();
        }
        #endregion
    }
}
