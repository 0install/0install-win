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
using System.IO;
using Common;
using Common.Controls;
using ZeroInstall.Commands;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Store;

namespace ZeroInstall.Central.WinForms.Wizards
{
    /// <summary>
    /// A wizard guiding the user through setting up a Sync account and crypto key.
    /// </summary>
    /// <remarks>Modifies the default <see cref="Config"/>.</remarks>
    public partial class SyncSetupWizard : Wizard
    {
        public SyncSetupWizard(bool machineWide)
        {
            InitializeComponent();

            #region State
            bool usedBefore = false;
            var server = new SyncServer();
            string cryptoKey = null;
            #endregion

            #region Pages
            var welcomePage = new SetupWelcomePage();
            var serverPage = new ServerPage();
            var registerPage = new RegisterPage();
            var credentialsPage = new CredentialsPage();
            var existingCryptoKeyPage = new ExistingCryptoKeyPage();
            var resetCryptoKeyPage = new ResetCryptoKeyPage(machineWide);
            var cryptoKeyChangedPage = new CryptoKeyChangedPage();
            var newCryptoKeyPage = new NewCryptoKeyPage();
            var finishedPage = new SetupFinishedPage();
            #endregion

            #region Flows
            welcomePage.UsedBeforeSelected += delegate(bool value)
            {
                usedBefore = value;
                PushPage(serverPage);
            };
            serverPage.OfficialServer += delegate
            {
                server.Uri = credentialsPage.ServerUri = new Uri(Config.DefaultSyncServer);
                resetCryptoKeyPage.Server = existingCryptoKeyPage.Server = server;
                if (usedBefore) PushPage(credentialsPage);
                else PushPage(registerPage);
            };
            serverPage.CustomServer += delegate
            {
                server.Uri = credentialsPage.ServerUri = new Uri(Config.DefaultSyncServer);
                resetCryptoKeyPage.Server = existingCryptoKeyPage.Server = server;
                PushPage(credentialsPage);
            };
            registerPage.Next += () => PushPage(credentialsPage);
            credentialsPage.CredentialsSet += delegate(SyncServer newServer)
            {
                resetCryptoKeyPage.Server = existingCryptoKeyPage.Server = server = newServer;
                if (usedBefore) PushPage(existingCryptoKeyPage);
                else PushPage(newCryptoKeyPage);
            };
            existingCryptoKeyPage.OldKeySet += delegate(string key)
            {
                cryptoKey = key;
                PushPage(finishedPage);
            };
            existingCryptoKeyPage.ResetKey += () => PushPage(resetCryptoKeyPage);
            resetCryptoKeyPage.NewKeySet += delegate(string key)
            {
                cryptoKey = key;
                PushPage(cryptoKeyChangedPage);
            };
            cryptoKeyChangedPage.Finish += () => PushPage(finishedPage);
            newCryptoKeyPage.NewKeySet += delegate(string key)
            {
                cryptoKey = key;
                PushPage(finishedPage);
            };
            finishedPage.Finish += delegate
            {
                try
                {
                    var config = Config.Load();
                    config.FromSyncServer(server);
                    config.SyncCryptoKey = cryptoKey;
                    config.Save();
                    Close();

                    Program.RunCommand(SyncApps.Name);
                }
                    #region Error handling
                catch (IOException ex)
                {
                    Msg.Inform(this, ex.Message, MsgSeverity.Error);
                    Close();
                }
                catch (UnauthorizedAccessException ex)
                {
                    Msg.Inform(this, ex.Message, MsgSeverity.Error);
                    Close();
                }
                catch (InvalidDataException ex)
                {
                    Msg.Inform(this, ex.Message + (ex.InnerException == null ? "" : "\n" + ex.InnerException.Message), MsgSeverity.Error);
                    Close();
                }
                #endregion
            };
            #endregion

            PushPage(welcomePage);
        }
    }
}
