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
    /// Represents an application from a <see cref="Catalog"/> or <see cref="AppList"/> as a tile with control buttons.
    /// </summary>
    public partial class AppTile : UserControl
    {
        #region Variables
        private static readonly IHandler _handler = new SilentHandler();

        /// <summary>The icon cache used to retreive icons specified in <see cref="Feed"/>; may be <see langword="null"/>.</summary>
        private readonly IIconCache _iconCache;
        #endregion

        #region Properties
        /// <summary>
        /// The interface ID of the application this tile represents.
        /// </summary>
        public string InterfaceID { get; private set; }

        /// <summary>
        /// The name of the application this tile represents.
        /// </summary>
        public string AppName { get { return labelName.Text; } }

        private bool _inAppList;

        /// <summary>
        /// <see langword="true"/> if the application is listed in the <see cref="AppList"/>; <see langword="false"/> otherwise.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the value is set from a thread other than the UI thread.</exception>
        /// <remarks>This method must not be called from a background thread.</remarks>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool InAppList
        {
            get { return _inAppList; }
            set
            {
                #region Sanity checks
                if (InvokeRequired) throw new InvalidOperationException("Method called from a non UI thread.");
                #endregion

                _inAppList = value;

                // Toggle button visibility
                buttonAdd.Enabled = buttonAdd.Visible = !value;
                buttonConf.Enabled = buttonConf.Visible = value;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new application tile.
        /// </summary>
        /// <param name="interfaceID">The interface ID of the application this tile represents.</param>
        /// <param name="appName">The name of the application this tile represents.</param>
        /// <param name="iconCache">The icon cache used to retreive icons specified in <see cref="Feed"/>; may be <see langword="null"/>.</param>
        public AppTile(string interfaceID, string appName, IIconCache iconCache)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            if (appName == null) throw new ArgumentNullException("appName");
            #endregion

            InitializeComponent();

            InterfaceID = interfaceID;
            labelName.Text = appName;
            labelSummary.Text = "";
            _iconCache = iconCache;
        }
        #endregion

        //--------------------//

        #region Feed metadata
        /// <summary>
        /// Extracts relevant application metadata such as summaries and icons from a <see cref="Feed"/>.
        /// </summary>
        /// <param name="feed">A <see cref="Feed"/> to retreive additional metadata for the application; may be <see langword="null"/>.</param>
        /// <exception cref="InvalidOperationException">Thrown if the value is set from a thread other than the UI thread.</exception>
        /// <remarks>This method must not be called from a background thread.</remarks>
        public void SetFeed(Feed feed)
        {
            #region Sanity checks
            if (feed == null) throw new ArgumentNullException("feed");
            if (InvokeRequired) throw new InvalidOperationException("Method called from a non UI thread.");
            #endregion

            // Get application summary from feed
            labelSummary.Text = feed.Summaries.GetBestLanguage(CultureInfo.CurrentUICulture);

            if (_iconCache != null)
            { // Load application icon in background
                try
                {
                    var icon = feed.GetIcon(Icon.MimeTypePng, null);
                    iconDownloadWorker.RunWorkerAsync(icon.Location);
                }
                catch (KeyNotFoundException)
                {}
            }
        }

        private void iconDownloadWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Download and load icon in background
            try
            {
                string path = _iconCache.GetIcon((Uri)e.Argument, _handler);
                e.Result = Image.FromFile(path);
            }
                #region Error handling
            catch (UserCancelException)
            {}
            catch (WebException ex)
            {
                Log.Warn("Unable to download application icon:\n" + ex.Message);
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
                Log.Error("Error while loading application icon:\n" + e.Error.Message);
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
        private void linkLabelDetails_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenInBrowser(InterfaceID);
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            LaunchHelperAssembly(Program.CommandsExe, "run --no-wait " + StringUtils.EscapeArgument(InterfaceID));
        }

        private void buttonSelectVersion_Click(object sender, EventArgs e)
        {
            LaunchHelperAssembly(Program.CommandsExe, "run --no-wait --gui " + StringUtils.EscapeArgument(InterfaceID));
        }

        private void buttonSelectComponent_Click(object sender, EventArgs e)
        {
            // ToDo: List entry points
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            LaunchHelperAssembly(Program.CommandsExe, "update " + StringUtils.EscapeArgument(InterfaceID));
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            LaunchHelperAssembly(Program.CommandsExe, "add-app " + StringUtils.EscapeArgument(InterfaceID));
        }

        private void buttonIntegrate_Click(object sender, EventArgs e)
        {
            LaunchHelperAssembly(Program.CommandsExe, "integrate-app " + StringUtils.EscapeArgument(InterfaceID));
        }

        private void buttonConf_Click(object sender, EventArgs e)
        {
            LaunchHelperAssembly(Program.CommandsExe, "integrate-app " + StringUtils.EscapeArgument(InterfaceID));
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            LaunchHelperAssembly(Program.CommandsExe, "remove-app " + StringUtils.EscapeArgument(InterfaceID));
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Attempts to launch a .NET helper assembly in the application's base directory. Displays friendly error messages if something goes wrong.
        /// </summary>
        /// <param name="assembly">The name of the assembly to launch (without the file extension).</param>
        /// <param name="arguments">The command-line arguments to pass to the assembly.</param>
        private void LaunchHelperAssembly(string assembly, string arguments)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(assembly)) throw new ArgumentNullException("assembly");
            #endregion

            try
            {
                ProcessUtils.LaunchHelperAssembly(assembly, arguments);
            }
                #region Error handling
            catch (FileNotFoundException ex)
            {
                Msg.Inform(this, string.Format(Resources.FailedToRun + "\n" + ex.Message, assembly), MsgSeverity.Error);
            }
            catch (Win32Exception ex)
            {
                Msg.Inform(this, string.Format(Resources.FailedToRun + "\n" + ex.Message, assembly), MsgSeverity.Error);
            }
            #endregion
        }

        /// <summary>
        /// Opens a URL in the system's default browser.
        /// </summary>
        /// <param name="url">The URL to open.</param>
        private void OpenInBrowser(string url)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException("url");
            #endregion

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
