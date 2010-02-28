using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using Common.Collections;
using Common.Helpers;

namespace ZeroInstall
{
    /// <summary>
    /// Stores settings for the editor
    /// </summary>
    /// <seealso cref="Settings.Editor"/>
    public sealed class EditorSettings
    {
        #region Events
        /// <summary>
        /// Occurs when a setting in this group is changed
        /// </summary>
        [Description("Occurs when a setting in this group is changed")]
        public event SimpleEventHandler Changed;

        private void OnChanged()
        {
            if (Changed != null) Changed();
            if (Settings.AutoSave && Settings.Current != null && Settings.Current.Editor == this) Settings.SaveCurrent();
        }
        #endregion

        // ToDo: Add properties
    }
}