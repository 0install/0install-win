/*
 * Copyright 2010 Bastian Eicher
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
                    if (Msg.Ask(this, "Do you want to launch this application?\n" + feedUri, MsgSeverity.Information, "Yes\nLaunch the application", "No\nGo back to the list"))
                    {
                        Program.LaunchHelperApp(this, "0launchw.exe", feedUri);
                        Close();
                    }
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
            Program.LaunchHelperApp(this, "0launchw.exe");
        }

        private void buttonHelp_Click(object sender, EventArgs e)
        {
            // ToDo
        }
        #endregion
    }
}
