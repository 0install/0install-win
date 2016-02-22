/*
 * Copyright 2010-2015 Bastian Eicher
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
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Controls;
using NanoByte.Common.Tasks;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Publish.WinForms
{
    /// <summary>
    /// A dialog for signing multiple <see cref="Feed"/>s with a single <see cref="OpenPgpSecretKey"/>.
    /// </summary>
    public partial class MassSignForm : OKCancelDialog
    {
        #region Variables
        private readonly IEnumerable<FileInfo> _files;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new mass signing dialog.
        /// </summary>
        /// <param name="files">The <see cref="Feed"/> files to be signed.</param>
        private MassSignForm(IEnumerable<FileInfo> files)
        {
            InitializeComponent();

            _files = files;
            foreach (var path in _files)
                listBoxFiles.Items.Add(path);
        }

        private void MassSignDialog_Load(object sender, EventArgs e)
        {
            comboBoxSecretKey.Items.Add("");
            foreach (var secretKey in OpenPgpFactory.CreateDefault().ListSecretKeys())
                comboBoxSecretKey.Items.Add(secretKey);
        }
        #endregion

        #region Static access
        /// <summary>
        /// Displays a dialog allowing the user to sign a set of <see cref="Feed"/>s.
        /// </summary>
        /// <param name="files">The <see cref="Feed"/> files to be signed.</param>
        public static void Show([NotNull, ItemNotNull] IEnumerable<FileInfo> files)
        {
            #region Sanity checks
            if (files == null) throw new ArgumentNullException("files");
            #endregion

            using (var dialog = new MassSignForm(files))
                dialog.ShowDialog();
        }
        #endregion

        //--------------------//

        #region Event handlers
        private void comboBoxSecretKey_SelectedValueChanged(object sender, EventArgs e)
        {
            buttonOK.Enabled = (comboBoxSecretKey.SelectedItem is OpenPgpSecretKey);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            try
            {
                SignFiles(comboBoxSecretKey.SelectedItem as OpenPgpSecretKey, textPassword.Text);
            }
                #region Sanity checks
            catch (OperationCanceledException)
            {}
            catch (ArgumentException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Warn);
            }
            catch (IOException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }
            #endregion
        }

        /// <summary>
        /// Signs a number of <see cref="Feed"/>s with a single <see cref="OpenPgpSecretKey"/>.
        /// </summary>
        /// <param name="secretKey">The private key to use for signing the files.</param>
        /// <param name="passphrase">The passphrase to use to unlock the key.</param>
        /// <exception cref="IOException">The feed file could not be read or written.</exception>
        /// <exception cref="UnauthorizedAccessException">Read or write access to the feed file is not permitted.</exception>
        private void SignFiles(OpenPgpSecretKey secretKey, string passphrase)
        {
            var task = ForEachTask.Create("Signing feeds", _files, file =>
            {
                SignedFeed signedFeed;
                try
                {
                    signedFeed = SignedFeed.Load(file.FullName);
                }
                    #region Error handling
                catch (UnauthorizedAccessException ex)
                {
                    // Wrap exception since only certain exception types are allowed
                    throw new IOException(ex.Message, ex);
                }
                catch (InvalidDataException ex)
                {
                    // Wrap exception since only certain exception types are allowed
                    throw new IOException(ex.Message + (ex.InnerException == null ? "" : Environment.NewLine + ex.InnerException.Message), ex);
                }
                #endregion

                signedFeed.SecretKey = secretKey;
                try
                {
                    signedFeed.Save(file.FullName, passphrase);
                }
                    #region Error handling
                catch (UnauthorizedAccessException ex)
                {
                    // Wrap exception since only certain exception types are allowed
                    throw new IOException(ex.Message, ex);
                }
                catch (KeyNotFoundException ex)
                {
                    // Wrap exception since only certain exception types are allowed
                    throw new IOException(ex.Message, ex);
                }
                catch (WrongPassphraseException ex)
                {
                    // Wrap exception since only certain exception types are allowed
                    throw new IOException(ex.Message, ex);
                }
                #endregion
            });
            using (var handler = new GuiTaskHandler(this)) handler.RunTask(task);
            Msg.Inform(this, "Successfully signed files.", MsgSeverity.Info);
        }
        #endregion
    }
}
