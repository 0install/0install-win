using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using Common;
using Common.Storage;

namespace ZeroInstall.Central.Storage
{
    /// <summary>
    /// Stores settings for the application
    /// </summary>
    [XmlRoot("settings", Namespace = "http://zero-install.sourceforge.net/2010/central/settings")]
    public sealed class Settings
    {
        #region Variables
        private static readonly string
            _portablePath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), @"central-settings.xml"),
            _profilePath = Path.Combine(Locations.GetUserSettingsDir(Path.Combine("0install.net", "central")), @"settings.xml");
        #endregion

        #region Properties
        /// <summary>
        /// The currently active set of settings
        /// </summary>
        public static Settings Current { get; private set; }

        private static bool _autoSave = true;
        /// <summary>
        /// Automatically save any changed settings?
        /// </summary>
        public static bool AutoSave { get { return _autoSave; } set { _autoSave = value; } }

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
                    Log.Write("Loaded settings from installation directory");
                }
                catch (Exception ex)
                {
                    Log.Write("Failed to load settings from installation directory: " + ex.Message + "\nReverting to defaults");
                }
            }
            else
            { // Then fall back to the user profile
                try
                {
                    Current = XmlStorage.Load<Settings>(_profilePath);
                    Log.Write("Loaded settings from user profile");
                }
                catch (Exception ex)
                {
                    Log.Write("Failed to load settings from user profile: " + ex.Message + "\nReverting to defaults");
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
                    Log.Write("Saved settings to working directory");
                }
                else
                {
                    XmlStorage.Save(_profilePath, Current);
                    Log.Write("Saved settings to user profile");
                }
            }
            catch (IOException)
            {}
        }
        #endregion

        #endregion

        #region Values
        // ToDo: Add settings
        #endregion
    }
}