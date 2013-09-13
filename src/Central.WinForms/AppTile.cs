/*
 * Copyright 2010-2013 Bastian Eicher
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
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Windows.Forms;
using Common;
using Common.Utils;
using ZeroInstall.Backend;
using ZeroInstall.Central.WinForms.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Model;
using ZeroInstall.Store;
using ZeroInstall.Store.Icons;
using SharedResources = ZeroInstall.Central.Properties.Resources;
using Icon = ZeroInstall.Model.Icon;

namespace ZeroInstall.Central.WinForms
{
    /// <summary>
    /// Represents an application as a tile with buttons for launching, managing, etc..
    /// </summary>
    public partial class AppTile : UserControl, IAppTile
    {
        #region Variables
        // Static resource preload
        private static readonly string _runButtonText = SharedResources.RunButtonText;
        private static readonly Bitmap _addButton = Resources.AddButton, _removeButton = Resources.RemoveButton, _setupButton = Resources.SetupButton, _modifyButton = Resources.ModifyButton;
        private static readonly string _addButtonTooltip = SharedResources.AddButtonTooltip, _removeButtonTooltip = SharedResources.RemoveButtonTooltip, _setupButtonTooltip = SharedResources.SetupButtonTooltip, _modifyButtonTooltip = SharedResources.ModifyButtonTooltip;
        private static readonly string _selectCommandButton = SharedResources.SelectCommandButton, _selectVersionButton = SharedResources.SelectVersionButton, _updateButtonText = SharedResources.UpdateButtonText;

        /// <summary>Apply operations machine-wide instead of just for the current user.</summary>
        private readonly bool _machineWide;

        private static readonly IHandler _handler = new SilentHandler();

        /// <summary>The icon cache used to retrieve icons specified in <see cref="Feed"/>; may be <see langword="null"/>.</summary>
        private readonly IIconCache _iconCache;
        #endregion

        #region Properties
        private Feed _feed;

        /// <inheritdoc/>
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
                    // Load application icon in background
                    var icon = value.GetIcon(Icon.MimeTypePng);
                    if (icon != null) iconDownloadWorker.RunWorkerAsync(icon.Href);
                    else pictureBoxIcon.Image = Resources.App; // Fall back to default icon
                }
                else pictureBoxIcon.Image = Resources.App; // Fall back to default icon
            }
        }

        /// <inheritdoc/>
        public string InterfaceID { get; private set; }

        /// <inheritdoc/>
        public string AppName { get { return labelName.Text; } }

        private AppStatus _status;

        /// <inheritdoc/>
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
        /// <param name="machineWide">Apply operations machine-wide instead of just for the current user.</param>
        /// <param name="interfaceID">The interface ID of the application this tile represents.</param>
        /// <param name="appName">The name of the application this tile represents.</param>
        /// <param name="status">Describes whether the application is listed in the <see cref="AppList"/> and if so whether it is integrated.</param>
        /// <param name="iconCache">The icon cache used to retrieve icons specified in <see cref="Feed"/>; may be <see langword="null"/>.</param>
        public AppTile(bool machineWide, string interfaceID, string appName, AppStatus status, IIconCache iconCache = null)
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
            buttonUpdate.Text = _updateButtonText;

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
                Log.Warn("Unable to store icon");
                Log.Warn(ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Warn("Unable to store icon");
                Log.Warn(ex);
            }
            catch (WebException ex)
            {
                Log.Warn("Unable to store icon");
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
                Log.Error("Unable to load icon");
                Log.Error(e.Error);
            }
        }
        #endregion

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

        private void linkLabelDetails_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (InterfaceID.StartsWith("fake:")) return;
            Program.OpenInBrowser(this, InterfaceID);
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            if (InterfaceID.StartsWith("fake:")) return;
            Program.RunCommand(Commands.Run.Name, "--no-wait", InterfaceID);
        }

        private void buttonSelectVersion_Click(object sender, EventArgs e)
        {
            if (InterfaceID.StartsWith("fake:")) return;
            Program.RunCommand(Commands.Run.Name, "--no-wait", "--gui", InterfaceID);
        }

        private void buttonSelectCommmand_Click(object sender, EventArgs e)
        {
            if (InterfaceID.StartsWith("fake:")) return;

            string args;
            string command = SelectCommandDialog.ShowDialog(this, _feed, out args);
            if (command != null)
            {
                try
                {
                    // Cannot use in-process method here because the "args" string needs to be parsed by the operating system
                    ProcessUtils.LaunchAssembly(Commands.WinForms.Program.ExeName, "run --no-wait --command=" + command.EscapeArgument() + " " + InterfaceID.EscapeArgument() + " " + args);
                }
                    #region Error handling
                catch (FileNotFoundException ex)
                {
                    Msg.Inform(this, ex.Message, MsgSeverity.Error);
                }
                catch (Win32Exception ex)
                {
                    Msg.Inform(this,  ex.Message, MsgSeverity.Error);
                }
                #endregion
            }
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            if (InterfaceID.StartsWith("fake:")) return;
            Program.RunCommand(Commands.Update.Name, InterfaceID);
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (InterfaceID.StartsWith("fake:")) return;

            // Disable button while operation is running
            buttonAdd.Enabled = false;

            Program.RunCommand(UpdateButtons, _machineWide, Commands.AddApp.Name, InterfaceID);
        }

        private void buttonIntegrate_Click(object sender, EventArgs e)
        {
            if (InterfaceID.StartsWith("fake:")) return;

            // Disable buttons while operation is running
            buttonRemove.Enabled = buttonIntegrate.Enabled = false;

            Program.RunCommand(UpdateButtons, _machineWide, Commands.IntegrateApp.Name, InterfaceID);
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            if (InterfaceID.StartsWith("fake:")) return;

            if (!Msg.YesNo(this, string.Format(SharedResources.AppRemoveConfirm, AppName), MsgSeverity.Warn)) return;

            // Disable buttons while operation is running
            buttonRemove.Enabled = buttonIntegrate.Enabled = false;

            Program.RunCommand(UpdateButtons, _machineWide, Commands.RemoveApp.Name, InterfaceID);
        }
        #endregion

        #region Drag and drop handling
        private void MouseDownHandler(object sender, MouseEventArgs e)
        {
            // Copy the interface ID and make sure it goes into another window
            MainForm.DisableDragAndDrop = true;
            DoDragDrop(InterfaceID, DragDropEffects.Copy);
            MainForm.DisableDragAndDrop = true;
        }
        #endregion
    }
}
