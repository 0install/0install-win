using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using Common;
using Common.Storage;

namespace ZeroInstall.Launchpad.Storage
{
    /// <summary>
    /// Stores settings for the application
    /// </summary>
    [XmlRoot("settings", Namespace = "http://zero-install.sourceforge.net/2010/launchpad/settings")]
    // ToDo: Supress xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema"
    public sealed class Settings
    {
        #region Variables
        private static readonly string
            PortablePath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "launchpad-settings.xml"),
            ProfilePath = Path.Combine(Locations.GetUserLocalSettingsDir(Path.Combine("0install.net", "launchpad")), "settings.xml");
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
        /// Loads the current settings from an automatically located XML file
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Any problems when loading the settings should be ignored")]
        public static void LoadCurrent()
        {
            if (File.Exists(PortablePath))
            { // Try to load settings file from the application's directory
                _loadedFromAppDir = true;

                try
                {
                    Current = XmlStorage.Load<Settings>(PortablePath);
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
                    Current = XmlStorage.Load<Settings>(ProfilePath);
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
        /// Saves the current settings to an automatically located XML file
        /// </summary>
        public static void SaveCurrent()
        {
            try
            {
                if (_loadedFromAppDir)
                {
                    XmlStorage.Save(PortablePath, Current);
                    Log.Write("Saved settings to working directory");
                }
                else
                {
                    XmlStorage.Save(ProfilePath, Current);
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