using System.ComponentModel;
using Common.Helpers;

namespace ZeroInstall
{
    /// <summary>
    /// Stores settings for the game itself
    /// </summary>
    /// <seealso cref="Settings.General"/>
    public sealed class GeneralSettings
    {
        #region Constants
        /// <summary>
        /// The complete name of the application
        /// </summary>
        public const string AppName = "Zero Install";

        /// <summary>
        /// The name of the application as used by NanoGrid (without whitespaces)
        /// </summary>
        public const string AppNameGrid = "ZeroInstall";

        /// <summary>
        /// The short version of the application name (as used by the EXE name)
        /// </summary>
        public const string AppNameShort = "ZeroInstall";

        /// <summary>
        /// The name of the application as used in the user profile
        /// </summary>
        public const string AppNameProfile = "0install.net";
        #endregion

        #region Events
        /// <summary>
        /// Occurs when a setting in this group is changed
        /// </summary>
        [Description("Occurs when a setting in this group is changed")]
        public event SimpleEventHandler Changed;

        private void OnChanged()
        {
            if (Changed != null) Changed();
            if (Settings.AutoSave && Settings.Current != null && Settings.Current.General == this) Settings.SaveCurrent();
        }
        #endregion

        // ToDo: Add properties
    }
}