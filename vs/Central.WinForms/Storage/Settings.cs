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
using System.Xml.Serialization;
using Common;
using Common.Storage;

namespace ZeroInstall.Central.WinForms.Storage
{
    /// <summary>
    /// Stores settings for the application.
    /// </summary>
    [XmlRoot("settings", Namespace = "http://0install.de/central/settings")]
    public sealed class Settings
    {
        #region Variables
        private static readonly string
            _portablePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"central-settings.xml"),
            _profilePath = Path.Combine(Locations.GetUserSettingsDir(Path.Combine("0install.net", "central")), @"settings.xml");
        #endregion

        #region Properties
        /// <summary>
        /// The currently active set of settings
        /// </summary>
        public static Settings Current { get; private set; }

        /// <summary>
        /// Automatically save any changed settings?
        /// </summary>
        public static bool AutoSave { get; set; }
        #endregion

        #region Constructor
        // Dummy constructor to prevent external instancing of this class
        private Settings()
        {}
        #endregion

        //--------------------//

        #region Storage
        // Were the settings loaded from the application's directory?
        private static bool _loadedFromAppDir;

        static Settings()
        {
            AutoSave = true;
        }

        #region Load
        /// <summary>
        /// Loads the current settings from an XML file.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Any problems when loading the settings should be ignored")]
        public static void LoadCurrent()
        {
            if (File.Exists(_portablePath))
            { // Try to load settings file from the application's directory
                _loadedFromAppDir = true;

                try
                {
                    Current = XmlStorage.Load<Settings>(_portablePath);
                    Log.Info("Loaded settings from installation directory");
                }
                catch (Exception ex)
                {
                    Log.Warn("Failed to load settings from installation directory: " + ex.Message + "\nReverting to defaults");
                }
            }
            else
            { // Then fall back to the user profile
                try
                {
                    Current = XmlStorage.Load<Settings>(_profilePath);
                    Log.Info("Loaded settings from user profile");
                }
                catch (Exception ex)
                {
                    Log.Warn("Failed to load settings from user profile: " + ex.Message + "\nReverting to defaults");
                }
            }

            // Fall back to default values if both fail
            if (Current == null) Current = new Settings();
        }
        #endregion

        #region Save
        /// <summary>
        /// Saves the current settings to an XML file.
        /// </summary>
        public static void SaveCurrent()
        {
            try
            {
                if (_loadedFromAppDir)
                {
                    XmlStorage.Save(_portablePath, Current);
                    Log.Info("Saved settings to working directory");
                }
                else
                {
                    XmlStorage.Save(_profilePath, Current);
                    Log.Info("Saved settings to user profile");
                }
            }
            catch (IOException ex)
            {
                Log.Warn("Failed to save settings: " + ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Warn("Insufficient rights to save settings: " + ex.Message);
            }
        }
        #endregion

        #endregion

        #region Values
        // ToDo: Add settings
        #endregion
    }
}
