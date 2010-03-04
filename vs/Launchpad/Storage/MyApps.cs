using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using Common;
using Common.Storage;

namespace ZeroInstall.Launchpad.Storage
{
    /// <summary>
    /// Stores a list of applications for the user in form of feed URLs.
    /// </summary>
    [XmlRoot("my-apps", Namespace = "http://zero-install.sourceforge.net/2010/launchpad/my-apps")]
    // ToDo: Supress xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema"
    public sealed class MyApps
    {
        #region Variables
        private static readonly string
            AppDirPath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "my-apps.xml"),
            ProfilePath = Path.Combine(UserDataDir, "my-apps.xml");
        #endregion

        #region Properties
        /// <summary>
        /// The directory where user data for the application is stored
        /// </summary>
        public static string UserDataDir
        {
            get
            {
                string userDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Path.Combine("0install.net", "launchpad"));
                if (!Directory.Exists(userDataDir)) Directory.CreateDirectory(userDataDir);
                return userDataDir;
            }
        }

        /// <summary>
        /// The directory where local (non-roaming) user data for the application is stored
        /// </summary>
        public static string UserLocalDataDir
        {
            get
            {
                string userLocalDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Path.Combine("0install.net", "launchpad"));
                if (!Directory.Exists(userLocalDataDir)) Directory.CreateDirectory(userLocalDataDir);
                return userLocalDataDir;
            }
        }

        /// <summary>
        /// The currently active set of applications
        /// </summary>
        public static MyApps Current { get; private set; }

        private static bool _autoSave = true;
        /// <summary>
        /// Automatically save any changed applications?
        /// </summary>
        public static bool AutoSave { get { return _autoSave; } set { _autoSave = value; } }

        #endregion

        #region Constructor
        // Dummy constructor to prevent external instancing of this class
        private MyApps()
        {}
        #endregion

        //--------------------//

        #region Storage
        // Were the applications loaded from the application's directory?
        private static bool _loadedFromAppDir;

        #region Load
        /// <summary>
        /// Loads the current applications from an automatically located XML file
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Any problems when loading the applications should be ignored")]
        public static void LoadCurrent()
        {
            if (File.Exists(AppDirPath))
            { // Try to load applications file from the application's directory
                _loadedFromAppDir = true;

                try
                {
                    Current = XmlStorage.Load<MyApps>(AppDirPath);
                    Log.Write("Loaded applications from working directory");
                }
                catch (Exception ex)
                {
                    Log.Write("Failed to load applications from working directory: " + ex.Message + "\nReverting to defaults");
                }
            }
            else
            { // Then fall back to the user profile
                try
                {
                    Current = XmlStorage.Load<MyApps>(ProfilePath);
                    Log.Write("Loaded applications from user profile");
                }
                catch (Exception ex)
                {
                    Log.Write("Failed to load applications from user profile: " + ex.Message + "\nReverting to defaults");
                }
            }

            // Fall back to default values if both fail
            if (Current == null) Current = new MyApps();
        }
        #endregion

        #region Save
        /// <summary>
        /// Saves the current applications to an automatically located XML file
        /// </summary>
        public static void SaveCurrent()
        {
            try
            {
                if (_loadedFromAppDir)
                {
                    XmlStorage.Save(AppDirPath, Current);
                    Log.Write("Saved applications to working directory");
                }
                else
                {
                    XmlStorage.Save(ProfilePath, Current);
                    Log.Write("Saved applications to user profile");
                }
            }
            catch (IOException)
            {}
        }
        #endregion

        #endregion

        #region Values
        private Collection<AppEntry> _applications = new Collection<AppEntry>();
        /// <summary>
        /// The list of application entries.
        /// </summary>
        [XmlElement("app")]
        public Collection<AppEntry> Applications { get { return _applications; } }
        #endregion
    }
}