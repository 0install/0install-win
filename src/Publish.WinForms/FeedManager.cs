using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using Common;
using System.Windows.Forms;
using System.IO;
using ZeroInstall.Publish.WinForms.Properties;

namespace ZeroInstall.Publish.WinForms
{
    /// <summary>
    /// 
    /// </summary>
    public partial class FeedManager : Component
    {
        #region Variables
        /// <summary>
        /// The current <see cref="FeedEditing" /> to edit.
        /// </summary>
        private FeedEditing _feedEditing = new FeedEditing();
        /// <summary>
        /// The parent <see cref="IWin32Window"/> of this component.
        /// </summary>
        private IWin32Window parent;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new <see cref="FeedManager"/> instance.
        /// </summary>
        /// <param name="parent">The parent <see cref="IWin32Window"/>. Must not be <see langword="null"/>.</param>
        public FeedManager(IWin32Window parent)
        {
            #region Sanity checks
            if (parent == null) throw new ArgumentNullException("parent");
            #endregion

            this.parent = parent;
            InitializeComponent();
        }
        #endregion

        //--------------------//

        #region New/Open/SaveChanges
        /// <summary>
        /// Creates a new <see cref="FeedEditing"/> after asking the user for saving changes on <see cref="_feedEditing"/>..
        /// </summary>
        /// <returns>The current <see cref="FeedEditing"/>. A new one if the user allowed it, else the old one.</returns>
        public FeedEditing New()
        {
            return SaveChanges(delegate { _feedEditing = new FeedEditing(); });
        }

        /// <summary>
        /// Opens a <see cref="FeedEditing"/> after asking the user for saving changes on <see cref="_feedEditing"/>..
        /// </summary>
        /// <returns>The current <see cref="FeedEditing"/>. The opened one if the user allowed it, else the old one.</returns>
        public FeedEditing Open()
        {
            return SaveChanges(OpenFeed);
        }

        /// <summary>
        /// Asks the user for saving changes on <see cref="_feedEditing"/>.
        /// </summary>
        public void SaveChanges()
        {
            SaveChanges(delegate { });
        }

        /// <summary>
        /// Asks the user for saving changes and resets the feed.
        /// </summary>
        /// <param name="AfterSave">Procedure that will be performed after resetting <see cref="_feedEditing"/>.</param>
        /// <returns>The current <see cref="_feedEditing"/></returns>
        private FeedEditing SaveChanges(SimpleEventHandler AfterSave)
        {
            #region Sanity checks
            if (AfterSave == null) throw new ArgumentNullException("AfterSave");
            #endregion

            if (_feedEditing.Changed && (AskSave() == DialogResult.OK) && Save()) AfterSave();

            return _feedEditing;
        }

        /// <summary>
        /// Asks the user to save the edited <see cref="Feed"/>.
        /// </summary>
        /// <returns></returns>
        private DialogResult AskSave()
        {
            return Msg.Choose(parent, Resources.SaveQuestion, MsgSeverity.Info, true,
                           Resources.SaveChanges, Resources.DiscardChanges);
        }

        /// <summary>
        /// Shows an <see cref="openFileDialog"/> and opens the chosen <see cref="Feed"/>.
        /// </summary>
        private void OpenFeed()
        {
            openFileDialog.FileName = _feedEditing.Path;
            if (openFileDialog.ShowDialog(parent) != DialogResult.OK) return;

            try
            {
                _feedEditing = FeedEditing.Load(openFileDialog.FileName);
            }
            #region Error handling
            catch (InvalidOperationException)
            {
                Msg.Inform(parent, Resources.FeedNotValid, MsgSeverity.Error);
            }
            catch (UnauthorizedAccessException exception)
            {
                Msg.Inform(parent, exception.Message, MsgSeverity.Error);
            }
            catch (IOException exception)
            {
                Msg.Inform(parent, exception.Message, MsgSeverity.Error);
            }
            #endregion
        }
        #endregion

        #region Save
        /// <summary>
        /// Saves the <see cref="Feed"/> under the current path or asks for path if necessary.
        /// </summary>
        /// <returns><see langword="true"/>, if the saving was successful, else <see langword="false"/>.</returns>
        public bool Save()
        {
            return string.IsNullOrEmpty(_feedEditing.Path) ? SaveAs() : SaveAs(_feedEditing.Path);
        }

        /// <summary>
        /// Asks the user for a path under which the <see cref="Feed"/> will be saved.
        /// </summary>
        /// <returns><see langword="true"/>, if the saving was successful, else <see langword="false"/>.</returns>
        public bool SaveAs()
        {
            saveFileDialog.FileName = _feedEditing.Path;
            if (saveFileDialog.ShowDialog(parent) != DialogResult.OK) return false;

            return SaveAs(saveFileDialog.FileName);
        }

        /// <summary>
        /// Saves the <see cref="Feed"/>.
        /// </summary>
        /// <param name="path">Where to save the <see cref="Feed"/>.</param>
        /// <returns><see langword="true"/>, if the saving was successful, else <see langword="false"/>.</returns>
        private bool SaveAs(string path)
        {
            try
            {
                _feedEditing.Save(path);
            }
            #region Error handling
            catch (IOException exception)
            {
                Msg.Inform(parent, exception.Message, MsgSeverity.Error);
                return false;
            }
            catch (UnauthorizedAccessException exception)
            {

                Msg.Inform(parent, exception.Message, MsgSeverity.Error);
                return false;
            }
            #endregion

            return true;
        }
        #endregion
    }
}
