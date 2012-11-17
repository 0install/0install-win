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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Windows.Forms;
using Common;
using Common.Tasks;
using Common.Utils;
using ZeroInstall.Central.WinForms.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Injector;
using ZeroInstall.Model;
using ZeroInstall.Store.Feeds;
using Icon = ZeroInstall.Model.Icon;

namespace ZeroInstall.Central.WinForms
{
    /// <summary>
    /// Describes the status of an application represented by an <see cref="AppTile"/>.
    /// </summary>
    /// <seealso cref="AppTile.Status"/>
    public enum AppStatus
    {
        /// <summary>The state has not been set yet.</summary>
        Unset,

        /// <summary>The application is listed in a <see cref="Catalog"/> but not in the <see cref="AppList"/>.</summary>
        Candidate,

        /// <summary>The application is listed in the <see cref="AppList"/> but <see cref="AppEntry.AccessPoints"/> is <see langword="null"/>.</summary>
        Added,

        /// <summary>The application is listed in the <see cref="AppList"/> and <see cref="AppEntry.AccessPoints"/> is set.</summary>
        Integrated
    }

    /// <summary>
    /// Represents an application from a <see cref="Catalog"/> or <see cref="AppList"/> as a tile with control buttons.
    /// </summary>
    public partial class AppTile : UserControl
    {
        #region Variables
        // Static resource preload
        private static readonly string _runButtonText = Resources.Run;
        private static readonly Bitmap _addButton = Resources.AddButton, _removeButton = Resources.RemoveButton, _setupButton = Resources.SetupButton, _modifyButton = Resources.ModifyButton;
        private static readonly string _addButtonTooltip = Resources.AddButtonTooltip, _removeButtonTooltip = Resources.RemoveButtonTooltip, _setupButtonTooltip = Resources.SetupButtonTooltip, _modifyButtonTooltip = Resources.ModifyButtonTooltip;
        private static readonly string _selectCommandButton = Resources.SelectCommandButton, _selectVersionButton = Resources.SelectVersionButton, _updateButton = Resources.UpdateButton;

        /// <summary>Apply operations sachine-wide instead of just for the current user.</summary>
        private readonly bool _machineWide;

        private static readonly IHandler _handler = new SilentHandler();

        /// <summary>The icon cache used to retrieve icons specified in <see cref="Feed"/>; may be <see langword="null"/>.</summary>
        private readonly IIconCache _iconCache;
        #endregion

        #region Properties
        private Feed _feed;

        /// <summary>
        /// A <see cref="Feed"/> from which the tile extracts relevant application metadata such as summaries and icons.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the value is set from a thread other than the UI thread.</exception>
        /// <remarks>This method must not be called from a background thread.</remarks>
        public Feed Feed
        {
            get { return _feed; }
            set
            {
                #region Sanity checks
                if (InvokeRequired) throw new InvalidOperationException("Method called from a non UI thread.");
                #endregion

                _feed = value;
                if (value == null) return;

                // Get application summary from feed
                labelSummary.Text = value.Summaries.GetBestLanguage(CultureInfo.CurrentUICulture);

                if (_iconCache != null)
                {
                    try
                    {
                        // Load application icon in background
                        var icon = value.GetIcon(Icon.MimeTypePng, null);
                        iconDownloadWorker.RunWorkerAsync(icon.Location);
                    }
                    catch (KeyNotFoundException)
                    {
                        // Fall back to default icon
                        pictureBoxIcon.Image = Resources.App;
                    }
                }
            }
        }

        /// <summary>
        /// The interface ID of the application this tile represents.
        /// </summary>
        public string InterfaceID { get; private set; }

        /// <summary>
        /// The name of the application this tile represents.
        /// </summary>
        public string AppName { get { return labelName.Text; } }

        private AppStatus _status;

        /// <summary>
        /// Describes whether the application is listed in the <see cref="AppList"/> and if so whether it is integrated.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the value is set from a thread other than the UI thread.</exception>
        /// <remarks>This method must not be called from a background thread.</remarks>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public AppStatus Status
        {
            get { return _status; }
            set
            {
                #region Sanity checks
                if (value < AppStatus.Candidate || value > AppStatus.Integrated) throw new InvalidEnumArgumentException("value", (int)value, typeof(AppStatus));
                if (InvokeRequired) throw new InvalidOperationException("Property set from a non UI thread.");
                #endregion

                _status = value;

                UpdateButtons();
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new application tile.
        /// </summary>
        /// <param name="machineWide">Apply operations sachine-wide instead of just for the current user.</param>
        /// <param name="interfaceID">The interface ID of the application this tile represents.</param>
        /// <param name="appName">The name of the application this tile represents.</param>
        /// <param name="status">Describes whether the application is listed in the <see cref="AppList"/> and if so whether it is integrated.</param>
        /// <param name="iconCache">The icon cache used to retrieve icons specified in <see cref="Feed"/>; may be <see langword="null"/>.</param>
        public AppTile(bool machineWide, string interfaceID, string appName, AppStatus status, IIconCache iconCache)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            if (appName == null) throw new ArgumentNullException("appName");
            #endregion

            _machineWide = machineWide;

            InitializeComponent();
            buttonRun.Text = _runButtonText;
            buttonAdd.Image = _addButton;
            buttonRemove.Image = _removeButton;
            buttonIntegrate.Image = _setupButton;
            toolTip.SetToolTip(buttonAdd, _addButtonTooltip);
            toolTip.SetToolTip(buttonRemove, _removeButtonTooltip);
            buttonSelectCommand.Text = _selectCommandButton;
            buttonSelectVersion.Text = _selectVersionButton;
            buttonUpdate.Text = _updateButton;

            InterfaceID = interfaceID;
            labelName.Text = appName;
            labelSummary.Text = "";
            Status = status;

            _iconCache = iconCache;
        }
        #endregion

        //--------------------//

        #region Feed processing
        private void iconDownloadWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Download and load icon in background
            try
            {
                string path = _iconCache.GetIcon((Uri)e.Argument, _handler);
                using (var stream = File.OpenRead(path))
                    e.Result = Image.FromStream(stream);
            }
                #region Error handling
            catch (OperationCanceledException)
            {}
            catch (IOException ex)
            {
                Log.Warn(Resources.UnableToStoreIcon);
                Log.Warn(ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Warn(Resources.UnableToStoreIcon);
                Log.Warn(ex);
            }
            catch (WebException ex)
            {
                Log.Warn(Resources.UnableToStoreIcon);
                Log.Warn(ex);
            }
            #endregion
        }

        private void iconDownloadWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            { // Display icon in UI thread
                var image = e.Result as Image;
                if (image != null) pictureBoxIcon.Image = image;
            }
            else
            {
                Log.Error(Resources.UnableToLoadIcon);
                Log.Error(e.Error);
            }
        }
        #endregion

        #region Task tracking
        /// <summary>
        /// Registers a generic <see cref="ITask"/> for tracking. Should only be one running at a time.
        /// </summary>
        /// <param name="task">The task to be tracked. May or may not alreay be running.</param>
        /// <exception cref="InvalidOperationException">Thrown if the value is set from a thread other than the UI thread.</exception>
        /// <remarks>This method must not be called from a background thread.</remarks>
        public void TrackTask(ITask task)
        {
            #region Sanity checks
            if (task == null) throw new ArgumentNullException("task");
            if (InvokeRequired) throw new InvalidOperationException("Method called from a non UI thread.");
            #endregion

            linkLabelDetails.Visible = labelSummary.Visible = false;
            trackingProgressBar.Visible = true;
        }
        #endregion

        //--------------------//

        #region Buttons
        /// <summary>
        /// Updates the visibility and icons of buttons based on the <see cref="Status"/>.
        /// </summary>
        private void UpdateButtons()
        {
            buttonAdd.Enabled = buttonAdd.Visible = (Status == AppStatus.Candidate);
            buttonRemove.Enabled = buttonRemove.Visible = (Status >= AppStatus.Added);

            toolTip.SetToolTip(buttonIntegrate, (Status == AppStatus.Integrated) ? _modifyButtonTooltip : _setupButtonTooltip);
            buttonIntegrate.Image = (Status == AppStatus.Integrated) ? _modifyButton : _setupButton;
            buttonIntegrate.Enabled = (Status >= AppStatus.Added);
        }

        /// <summary>
        /// Calls <see cref="UpdateButtons"/> on the UI thread.
        /// </summary>
        private void InvokeUpdateButtons()
        {
            try
            {
                Invoke(new Action(UpdateButtons));
            }
            catch (InvalidOperationException)
            {
                // Don't worry if the control was disposed in the meantime
            }
        }

        private void linkLabelDetails_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenInBrowser(InterfaceID);
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            ProcessUtils.RunAsync(() => Commands.WinForms.Program.Main(new[] {"run", "--no-wait", InterfaceID}));
        }

        private void buttonSelectVersion_Click(object sender, EventArgs e)
        {
            ProcessUtils.RunAsync(() => Commands.WinForms.Program.Main(new[] {"run", "--no-wait", "--gui", InterfaceID}));
        }

        private void buttonSelectCommmand_Click(object sender, EventArgs e)
        {
            string args;
            string command = SelectCommandDialog.ShowDialog(this, _feed, out args);
            if (command != null)
            {
                try
                {
                    // Cannot use in-process method here because the "args" string needs to be parsed as multiple arguments instead of one
                    ProcessUtils.LaunchHelperAssembly(Commands.WinForms.Program.ExeName, "run --no-wait --command=" + command.EscapeArgument() + " " + InterfaceID.EscapeArgument() + " " + args);
                }
                    #region Error handling
                catch (FileNotFoundException ex)
                {
                    Msg.Inform(this, string.Format(Resources.FailedToRun + "\n" + ex.Message, Commands.WinForms.Program.ExeName), MsgSeverity.Error);
                }
                catch (Win32Exception ex)
                {
                    Msg.Inform(this, string.Format(Resources.FailedToRun + "\n" + ex.Message, Commands.WinForms.Program.ExeName), MsgSeverity.Error);
                }
                #endregion
            }
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            ProcessUtils.RunAsync(() => Commands.WinForms.Program.Main(new[] {"update", InterfaceID}));
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            // Disable button while operation is running
            buttonAdd.Enabled = false;

            ProcessUtils.RunAsync(delegate
            {
                Commands.WinForms.Program.Main(_machineWide
                    ? new[] {"add-app", "--machine", InterfaceID}
                    : new[] {"add-app", InterfaceID});
                InvokeUpdateButtons(); // Restore buttons
            });
        }

        private void buttonIntegrate_Click(object sender, EventArgs e)
        {
            // Disable buttons while operation is running
            buttonRemove.Enabled = buttonIntegrate.Enabled = false;

            ProcessUtils.RunAsync(delegate
            {
                Commands.WinForms.Program.Main(_machineWide
                    ? new[] {"integrate-app", "--machine", InterfaceID}
                    : new[] {"integrate-app", InterfaceID});
                InvokeUpdateButtons(); // Restore buttons
            });
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            if (!Msg.YesNo(this, string.Format(Resources.AppRemoveConfirm, AppName), MsgSeverity.Warn, Resources.YesRemoveApp, Resources.NoKeepApp)) return;

            // Disable buttons while operation is running
            buttonRemove.Enabled = buttonIntegrate.Enabled = false;

            ProcessUtils.RunAsync(delegate
            {
                Commands.WinForms.Program.Main(_machineWide
                    ? new[] {"remove-app", "--machine", InterfaceID}
                    : new[] {"remove-app", InterfaceID});
                InvokeUpdateButtons(); // Restore buttons
            });
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Opens a URL in the system's default browser.
        /// </summary>
        /// <param name="url">The URL to open.</param>
        private void OpenInBrowser(string url)
        {
            try
            {
                Process.Start(url);
            }
                #region Error handling
            catch (FileNotFoundException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }
            catch (Win32Exception ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }
            #endregion
        }
        #endregion
    }
}
