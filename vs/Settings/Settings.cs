using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows.Forms;
using Common;
using Common.Storage;
using LuaInterface;

namespace ZeroInstall
{
    /// <summary>
    /// Stores settings for the application
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "The contained configuration form will dispose itself automatically when closed")]
    public sealed class Settings
    {
        #region Variables
        private static readonly string
            AppDirPath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "Settings.xml"),
            ProfilePath = Path.Combine(UserDataDir, "Settings.xml");
        #endregion

        #region Properties
        /// <summary>
        /// The directory where user data for the game is stored
        /// </summary>
        public static string UserDataDir
        {
            get
            {
                string userDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), GeneralSettings.AppNameProfile);
                if (!Directory.Exists(userDataDir)) Directory.CreateDirectory(userDataDir);
                return userDataDir;
            }
        }

        /// <summary>
        /// The directory where local (non-roaming) user data for the game is stored
        /// </summary>
        public static string UserLocalDataDir
        {
            get
            {
                string userLocalDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), GeneralSettings.AppNameProfile);
                if (!Directory.Exists(userLocalDataDir)) Directory.CreateDirectory(userLocalDataDir);
                return userLocalDataDir;
            }
        }

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
        [LuaGlobal(Name = "LoadSettings", Description = "Loads the current settings from an automatically located XML file")]
        public static void LoadCurrent()
        {
            if (File.Exists(AppDirPath))
            { // Try to load settings file from the application's directory
                _loadedFromAppDir = true;

                try
                {
                    Current = XmlStorage.Load<Settings>(AppDirPath);
                    Log.Write("Loaded settings from working directory");
                }
                catch (Exception ex)
                {
                    Log.Write("Failed to load settings from working directory: " + ex.Message + "\nReverting to defaults");
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
        [LuaGlobal(Name = "SaveSettings", Description = "Saves the current settings to an automatically located XML file")]
        public static void SaveCurrent()
        {
            try
            {
                if (_loadedFromAppDir)
                {
                    XmlStorage.Save(AppDirPath, Current);
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
// ReSharper disable FieldCanBeMadeReadOnly.Global
        /// <summary>Stores settings for the game itself</summary>
        public GeneralSettings General = new GeneralSettings();
        /// <summary>Stores settings for the editor</summary>
        public EditorSettings Editor = new EditorSettings();
// ReSharper restore FieldCanBeMadeReadOnly.Global
        #endregion

        #region Config
        /// <summary>Contains a reference to the <see cref="ConfigForm"/> while it is open</summary>
        private ConfigForm _configForm;

        /// <summary>
        /// Displays a configuration interface for the settings, allowing easy manipulation of values
        /// </summary>
        public void Config()
        {
            // Only create a new form if there isn't already one open
            if (_configForm == null)
            {
                _configForm = new ConfigForm(this);

                // Remove the reference as soon the form is closed
                _configForm.Closed += delegate { _configForm = null; };
            }

            _configForm.Show();
        }
        #endregion
    }
}