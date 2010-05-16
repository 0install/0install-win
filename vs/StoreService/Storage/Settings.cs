using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml.Serialization;
using Common;
using Common.Storage;

namespace ZeroInstall.StoreService.Storage
{
    /// <summary>
    /// Stores settings for the application.
    /// </summary>
    [XmlRoot("settings", Namespace = "http://zero-install.sourceforge.net/2010/store-service/settings")]
    public sealed class Settings
    {
        #region Variables
        private static readonly string
            _configPath = Path.Combine(Locations.GetSystemSettingsDir(Path.Combine("0install.net", "store-service")), @"settings.xml");
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
        { }

        static Settings()
        {
            AutoSave = true;
        }
        #endregion

        //--------------------//

        #region Storage

        #region Load
        /// <summary>
        /// Loads the current settings from an XML file.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Any problems when loading the settings should be ignored")]
        public static void LoadCurrent()
        {
            try
            {
                Current = XmlStorage.Load<Settings>(_configPath);
                Log.Write("Loaded settings from user profile");
            }
            catch (Exception ex)
            {
                Log.Write("Failed to load settings from user profile: " + ex.Message + "\nReverting to defaults");
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
                XmlStorage.Save(_configPath, Current);
                Log.Write("Saved settings to user profile");
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