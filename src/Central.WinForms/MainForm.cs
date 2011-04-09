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
using System.Diagnostics;
using System.Windows.Forms;
using Common;
using Common.Collections;
using Common.Controls;
using Common.Utils;
using ZeroInstall.Central.WinForms.Properties;

namespace ZeroInstall.Central.WinForms
{
    partial class MainForm : Form
    {
        #region Constructor
        public MainForm()
        {
            InitializeComponent();

            browserNewApps.CanGoBackChanged += delegate { toolStripButtonBack.Enabled = browserNewApps.CanGoBack; };
        }
        #endregion

        #region Startup
        private void MainForm_Load(object sender, EventArgs e)
        {
            // ToDo: Check if the user has any MyApps entries, before showing the "new apps" page
            tabControlApps.SelectedTab = tabPageNewApps;

            browserNewApps.Navigate(Resources.AppstoreUri);

            labelVersion.Text = "v" + Application.ProductVersion;
        }
        #endregion

        //--------------------//

        #region Launch feed
        /// <summary>
        /// Attempts to launch a feed and closes the main window.
        /// </summary>
        /// <param name="feedUri">The URI of the feed to be launched.</param>
        private void LaunchFeed(string feedUri)
        {
            if (feedUri.Contains(" ")) feedUri = "\"" + feedUri + "\"";
            Program.LaunchHelperAssembly(this, "0install-win", "run --no-wait " + feedUri);
            Close();
        }
        #endregion

        //--------------------//

        #region Toolbar
        private void toolStripButtonBack_Click(object sender, EventArgs e)
        {
            browserNewApps.GoBack();
        }
        #endregion

        #region Browser
        /// <summary>A URL postfix that indicates that the URL points to an installable Zero Install feed.</summary>
        private const string UrlPostfixFeed = "#0install-feed";

        /// <summary>A URL postfix that indicates that the URL should be opened in an external browser.</summary>
        private const string UrlPostfixBrowser = "#external-browser";

        private void browserNewApps_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            switch (e.Url.Fragment)
            {
                case UrlPostfixFeed:
                    e.Cancel = true;

                    string feedUri = e.Url.AbsoluteUri.Replace(UrlPostfixFeed, "");

                    // ToDo: Display more details about the feed
                    if (Msg.Ask(this, "Do you want to launch this application?\n" + feedUri, MsgSeverity.Info, "Yes\nLaunch the application", "No\nGo back to the list"))
                        LaunchFeed(feedUri);
                    break;

                case UrlPostfixBrowser:
                    e.Cancel = true;

                    // Use the system's default web browser to open the URL
                    Process.Start(e.Url.AbsoluteUri.Replace(UrlPostfixBrowser, ""));
                    break;
            }
        }

        private void browserNewApps_NewWindow(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Prevent any popups
            e.Cancel = true;
        }
        #endregion

        #region Tools
        private void buttonLaunchInterface_Click(object sender, EventArgs e)
        {
            string interfaceID = InputBox.Show(null, "Zero Install", "Please enter the URI of a Zero Install interface here:");
            if (string.IsNullOrEmpty(interfaceID)) return;

            Program.LaunchHelperAssembly(this, "0install-win", "run " + StringUtils.Escape(interfaceID));
        }

        private void buttonCacheManagement_Click(object sender, EventArgs e)
        {
            Program.LaunchHelperAssembly(this, "0store-win", null);
        }

        private void buttonConfiguration_Click(object sender, EventArgs e)
        {
            Program.LaunchHelperAssembly(this, "0install-win", "config");
        }

        private void buttonHelp_Click(object sender, EventArgs e)
        {
            // Use the system's default web browser to open the URL
            Process.Start("http://0install.de/help/");
        }
        #endregion

        #region Drag and drop handling
        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = e.Data.GetData(DataFormats.FileDrop) as string[];
                LaunchFeed(EnumerableUtils.GetFirst(files));
            }
            else if (e.Data.GetDataPresent(DataFormats.Text))
            {
                LaunchFeed((string)e.Data.GetData(DataFormats.Text));
            }
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = (e.Data.GetDataPresent(DataFormats.Text) || e.Data.GetDataPresent(DataFormats.FileDrop))
                ? DragDropEffects.Copy
                : DragDropEffects.None;
        }
        #endregion
    }
}
