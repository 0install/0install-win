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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using Common;
using Common.Storage;
using ZeroInstall.Model;

namespace ZeroInstall.Store.Interface
{
    /// <summary>
    /// A database of trusted GPG signatures for <see cref="Interface"/>s.
    /// </summary>
    [XmlRoot("trusted-keys", Namespace = "http://zero-install.sourceforge.net/2007/injector/trust")]
    public sealed class Trust
    {
        #region Variables
        private static readonly string
            _portablePath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), @"trustdb.xml"),
            _profilePath = Path.Combine(Locations.GetUserSettingsDir(Path.Combine("0install.net", "injector")), @"trustdb.xml");
        #endregion

        #region Properties
        /// <summary>
        /// The currently active set of trust data
        /// </summary>
        public static Trust Current { get; private set; }

        /// <summary>
        /// Automatically save any changed trust data?
        /// </summary>
        public static bool AutoSave { get; set; }
        #endregion

        #region Constructor
        // Dummy constructor to prevent external instancing of this class
        private Trust()
        { }
        #endregion

        //--------------------//

        #region Storage
        // Were the trust data loaded from the application's directory?
        private static bool _loadedFromAppDir;

        static Trust()
        {
            AutoSave = true;
        }

        #region Load
        /// <summary>
        /// Loads the current trust data from an XML file.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Any problems when loading the trust data should be ignored")]
        public static void LoadCurrent()
        {
            if (File.Exists(_portablePath))
            { // Try to load trust data file from the application's directory
                _loadedFromAppDir = true;

                try
                {
                    Current = XmlStorage.Load<Trust>(_portablePath);
                    Log.Write("Loaded trust data from installation directory");
                }
                catch (Exception ex)
                {
                    Log.Write("Failed to load trust data from installation directory: " + ex.Message + "\nReverting to defaults");
                }
            }
            else
            { // Then fall back to the user profile
                try
                {
                    Current = XmlStorage.Load<Trust>(_profilePath);
                    Log.Write("Loaded trust data from user profile");
                }
                catch (Exception ex)
                {
                    Log.Write("Failed to load trust data from user profile: " + ex.Message + "\nReverting to defaults");
                }
            }

            // Fall back to default values if both fail
            if (Current == null) Current = new Trust();
        }
        #endregion

        #region Save
        /// <summary>
        /// Saves the current trust data to an XML file.
        /// </summary>
        public static void SaveCurrent()
        {
            try
            {
                if (_loadedFromAppDir)
                {
                    XmlStorage.Save(_portablePath, Current);
                    Log.Write("Saved trust data to working directory");
                }
                else
                {
                    XmlStorage.Save(_profilePath, Current);
                    Log.Write("Saved trust data to user profile");
                }
            }
            catch (IOException)
            { }
        }
        #endregion

        #endregion

        #region Values
        // ToDo: Add trust data
        #endregion
    }
}