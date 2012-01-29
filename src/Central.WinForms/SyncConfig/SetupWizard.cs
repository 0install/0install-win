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
using Common.Controls;
using ZeroInstall.Injector;

namespace ZeroInstall.Central.WinForms.SyncConfig
{
    /// <summary>
    /// A wizard guiding the user through setting up a Sync account and crypto key.
    /// </summary>
    /// <remarks>Modifies the default <see cref="Config"/>.</remarks>
    public partial class SetupWizard : Wizard
    {
        public SetupWizard()
        {
            InitializeComponent();

            // State variables
            bool usedBefore = false;
            Uri syncServer = null;
            var syncCredentials = new SyncCredentials();
            string cryptoKey = null;

            // Wizard pages
            var welcomePage = new SetupWelcomePage();
            var serverPage = new ServerPage();
            var registerPage = new RegisterPage();
            var credentialsPage = new CredentialsPage();
            var existingCryptoKeyPage = new ExistingCryptoKeyPage();
            var newCryptoKeyPage = new NewCryptoKeyPage();
            var finishedPage = new SetupFinishedPage();

            // Page flows
            welcomePage.UsedBeforeSelected += delegate(bool value)
            {
                usedBefore = value;
                PushPage(serverPage);
            };
            serverPage.OfficialServer += delegate
            {
                credentialsPage.SyncServer = existingCryptoKeyPage.SyncServer = syncServer = new Uri(Config.DefaultSyncServer);
                if (usedBefore) PushPage(credentialsPage);
                else PushPage(registerPage);
            };
            serverPage.CustomServer += delegate(Uri server)
            {
                credentialsPage.SyncServer = syncServer = server;
                PushPage(credentialsPage);
            };
            registerPage.Continue += () => PushPage(credentialsPage);
            credentialsPage.Continue += delegate(SyncCredentials credentials)
            {
                existingCryptoKeyPage.SyncCredentials = syncCredentials = credentials;
                if (usedBefore) PushPage(existingCryptoKeyPage);
                else PushPage(newCryptoKeyPage);
            };
            Action<string> cryptoKeyHandler = delegate(string key)
            {
                cryptoKey = key;
                PushPage(finishedPage);
            };
            existingCryptoKeyPage.Continue += cryptoKeyHandler;
            newCryptoKeyPage.Continue += cryptoKeyHandler;
            finishedPage.Done += delegate
            {
                var config = Config.Load();
                config.SyncServer = syncServer;
                config.SyncServerUsername = syncCredentials.Username;
                config.SyncServerPassword = syncCredentials.Password;
                config.SyncCryptoKey = cryptoKey;
                config.Save();
                Close();
            };

            // Load first page
            PushPage(welcomePage);
        }
    }
}
