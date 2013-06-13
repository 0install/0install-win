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
using ZeroInstall.Store;

namespace ZeroInstall.Central.WinForms.SyncWizards
{
    /// <summary>
    /// A wizard guiding the user through resetting a lost crypto key or damaged sync data.
    /// </summary>
    /// <remarks>Modifies the default <see cref="Config"/>.</remarks>
    public partial class TroubleshootWizard : Wizard
    {
        /// <exception cref="IOException">Thrown if a problem occurs while reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if a problem occurs while deserializing the config data.</exception>
        public TroubleshootWizard(bool machineWide)
        {
            InitializeComponent();

            // Wizard pages
            var welcomePage = new ResetWelcomePage();
            var config = Config.Load();
            var existingCryptoKeyPage = new ExistingCryptoKeyPage {Server = SyncUtils.ToSyncServer(config)};
            var changeCryptoKeyPage = new ChangeCryptoKeyPage(machineWide);
            var resetCryptoKeyPage = new ResetCryptoKeyPage(machineWide) {Server = SyncUtils.ToSyncServer(config)};
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
            Action<string> newKeySet = delegate(string newKey)
            {
                try
                {
                    config = Config.Load();
                    config.SyncCryptoKey = newKey;
                    config.Save();

                    PushPage(cryptoKeyChangedPaged);
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
            changeCryptoKeyPage.Continue += newKeySet;
            resetCryptoKeyPage.Continue += newKeySet;
            cryptoKeyChangedPaged.OK += Close;
            resetServerPage.Continue += () => PushPage(resetServerFinishedPage);
            resetServerFinishedPage.Done += Close;
            resetClientPage.Continue += () => PushPage(resetClientFinishedPage);
            resetClientFinishedPage.Done += Close;

            // Load first page
            PushPage(welcomePage);
        }
    }
}
