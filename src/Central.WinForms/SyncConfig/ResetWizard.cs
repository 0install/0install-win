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
using System.IO;
using Common;
using Common.Controls;
using ZeroInstall.Commands;
using ZeroInstall.Injector;

namespace ZeroInstall.Central.WinForms.SyncConfig
{
    /// <summary>
    /// A wizard guiding the user through resetting a lost crypto key or damaged sync data.
    /// </summary>
    /// <remarks>Modifies the default <see cref="Config"/>.</remarks>
    public partial class ResetWizard : Wizard
    {
        /// <exception cref="IOException">Thrown if a problem occurs while reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if a problem occurs while deserializing the config data.</exception>
        public ResetWizard(bool machineWide)
        {
            InitializeComponent();

            // State variables
            string cryptoKey = null;

            // Wizard pages
            var welcomePage = new ResetWelcomePage();
            var config = Config.Load();
            var existingCryptoKeyPage = new ExistingCryptoKeyPage {Server = config.ToSyncServer()};
            var changeCryptoKeyPage = new ChangeCryptoKeyPage(machineWide);
            var resetCryptoKeyPage = new ResetCryptoKeyPage(machineWide) {Server = config.ToSyncServer()};
            var cryptoKeyChangedPaged = new CryptoKeyChangedPage();
            var resetServerPage = new ResetServerPage(machineWide);
            var resetServerFinishedPage = new ResetServerFinishedPage();
            var resetClientPage = new ResetClientPage(machineWide);
            var resetClientFinishedPage = new ResetClientFinishedPage();

            // Page flows
            welcomePage.ChangeCryptoKey += () => PushPage(existingCryptoKeyPage);
            welcomePage.ResetServer += () => PushPage(resetServerPage);
            welcomePage.ResetClient += () => PushPage(resetClientPage);
            existingCryptoKeyPage.Continue += delegate(string oldKey)
            {
                changeCryptoKeyPage.OldKey = oldKey;
                PushPage(changeCryptoKeyPage);
            };
            existingCryptoKeyPage.ResetKey += () => PushPage(resetCryptoKeyPage);
            changeCryptoKeyPage.Continue += delegate(string newKey)
            {
                cryptoKey = newKey;
                PushPage(cryptoKeyChangedPaged);
            };
            resetCryptoKeyPage.Continue += delegate(string newKey)
            {
                cryptoKey = newKey;
                PushPage(cryptoKeyChangedPaged);
            };
            cryptoKeyChangedPaged.OK += delegate
            {
                try
                {
                    config = Config.Load();
                    config.SyncCryptoKey = cryptoKey;
                    config.Save();
                    Close();
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
                    Msg.Inform(this, ex.Message +
                        (ex.InnerException == null ? "" : "\n" + ex.InnerException.Message), MsgSeverity.Error);
                    Close();
                }
                #endregion
            };
            resetServerPage.Continue += () => PushPage(resetServerFinishedPage);
            resetServerFinishedPage.Done += Close;
            resetClientPage.Continue += () => PushPage(resetClientFinishedPage);
            resetClientFinishedPage.Done += Close;

            // Load first page
            PushPage(welcomePage);
        }
    }
}
