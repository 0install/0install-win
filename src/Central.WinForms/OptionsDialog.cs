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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Common;
using Common.Controls;
using ZeroInstall.Central.WinForms.Properties;
using ZeroInstall.Injector;

namespace ZeroInstall.Central.WinForms
{
    public partial class OptionsDialog : OKCancelDialog
    {
        public OptionsDialog()
        {
            InitializeComponent();
        }

        private void OptionsDialog_Load(object sender, EventArgs e)
        {
            LoadConfig();
        }

        #region Sync
        private void linkSyncRegister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                OpenInBrowser(Config.Load().SyncServer + "register");
            }
                #region Error handling
            catch (IOException ex)
            {
                Msg.Inform(this, Resources.ProblemLoadingOptions + "\n" + ex.Message, MsgSeverity.Error);
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(this, Resources.ProblemLoadingOptions + "\n" + ex.Message, MsgSeverity.Error);
            }
            catch (InvalidDataException ex)
            {
                Msg.Inform(this, Resources.ProblemLoadingOptions + "\n" + ex.Message, MsgSeverity.Error);
            }
            #endregion
        }

        private void linkSyncAccount_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                OpenInBrowser(Config.Load().SyncServer + "account");
            }
                #region Error handling
            catch (IOException ex)
            {
                Msg.Inform(this, Resources.ProblemLoadingOptions + "\n" + ex.Message, MsgSeverity.Error);
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(this, Resources.ProblemLoadingOptions + "\n" + ex.Message, MsgSeverity.Error);
            }
            catch (InvalidDataException ex)
            {
                Msg.Inform(this, Resources.ProblemLoadingOptions + "\n" + ex.Message, MsgSeverity.Error);
            }
            #endregion
        }

        private void buttonSyncCryptoKey_Click(object sender, EventArgs e)
        {
            Msg.Inform(this, Resources.SyncCryptoKeyDescription, MsgSeverity.Info);
        }
        #endregion

        private void buttonAdvanced_Click(object sender, EventArgs e)
        {
            new Thread(() => Commands.WinForms.Program.Main(new[] {"config"})).Start();
            Close();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            SaveConfig();
        }

        #region Data access
        private void LoadConfig()
        {
            try
            {
                // Fill fields with data from config
                var config = Config.Load();
                textBoxSyncUsername.Text = config.SyncServerUsername;
                textBoxSyncPassword.Text = config.SyncServerPassword;
                textBoxSyncCryptoKey.Text = config.SyncCryptoKey;
            }
                #region Error handling
            catch (IOException ex)
            {
                Msg.Inform(this, Resources.ProblemLoadingOptions + "\n" + ex.Message, MsgSeverity.Error);
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(this, Resources.ProblemLoadingOptions + "\n" + ex.Message, MsgSeverity.Error);
            }
            catch (InvalidDataException ex)
            {
                Msg.Inform(this, Resources.ProblemLoadingOptions + "\n" + ex.Message, MsgSeverity.Error);
            }
            #endregion
        }

        private void SaveConfig()
        {
            try
            {
                // Write data from fields back to config
                var config = Config.Load();
                config.SyncServerUsername = textBoxSyncUsername.Text;
                config.SyncServerPassword = textBoxSyncPassword.Text;
                config.SyncCryptoKey = textBoxSyncCryptoKey.Text;
                config.Save();
            }
                #region Error handling
            catch (IOException ex)
            {
                Msg.Inform(this, Resources.ProblemSavingOptions + "\n" + ex.Message, MsgSeverity.Error);
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(this, Resources.ProblemSavingOptions + "\n" + ex.Message, MsgSeverity.Error);
            }
            catch (InvalidDataException ex)
            {
                Msg.Inform(this, Resources.ProblemSavingOptions + "\n" + ex.Message, MsgSeverity.Error);
            }
            #endregion
        }
        #endregion

        #region Helpers
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
