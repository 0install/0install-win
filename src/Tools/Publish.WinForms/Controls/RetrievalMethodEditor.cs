/*
 * Copyright 2010-2014 Bastian Eicher
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
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows.Forms;
using NanoByte.Common;
using NanoByte.Common.Controls;
using NanoByte.Common.Tasks;
using NanoByte.Common.Undo;
using ZeroInstall.Publish.Properties;
using ZeroInstall.Store.Model;
using ICommandExecutor = NanoByte.Common.Undo.ICommandExecutor;

namespace ZeroInstall.Publish.WinForms.Controls
{
    /// <summary>
    /// A common base for <see cref="RetrievalMethod"/> editors.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="RetrievalMethod"/> to edit.</typeparam>
    public abstract class RetrievalMethodEditor<T> : EditorControlBase<T>, IEditorControlContainerRef<T, Implementation>
        where T : RetrievalMethod
    {
        #region Properties
        private Implementation _containerRef;

        /// <inheritdoc/>
        public virtual Implementation ContainerRef
        {
            get { return _containerRef; }
            set
            {
                _containerRef = value;
                _buttonAddMissing.Visible = (value != null);

                UpdateHint();
            }
        }
        #endregion

        #region Constructor
        private readonly Label _labelUpdateHint;
        private readonly Button _buttonAddMissing;

        protected RetrievalMethodEditor()
        {
            Controls.Add(_labelUpdateHint = new Label
            {
                Location = new Point(0, 150),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                TabIndex = 1000,
                ForeColor = Color.Red,
                Visible = false
            });
            Controls.Add(_buttonAddMissing = new Button
            {
                Top = _labelUpdateHint.Bottom + 6,
                Size = new Size(123, 23),
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                TabIndex = 1001,
                Text = Resources.AddMissing,
                UseVisualStyleBackColor = true,
                Visible = false
            });
            _buttonAddMissing.Click += buttonAddMissing_Click;
        }
        #endregion

        //--------------------//

        #region Add missing
        /// <summary>
        /// Displays hints explaining why calling "Update" may be required.
        /// </summary>
        protected virtual void UpdateHint()
        {
            if (ContainerRef != null && ContainerRef.ManifestDigest == default(ManifestDigest))
                ShowUpdateHint(Resources.ManifestDigestMissing);
            else _labelUpdateHint.Visible = false;
        }

        /// <summary>
        /// Displays a specific update hint if <see cref="_buttonAddMissing"/> is active.
        /// </summary>
        /// <param name="hint">The hint to display.</param>
        protected void ShowUpdateHint(string hint)
        {
            if (!_buttonAddMissing.Visible) return;

            _labelUpdateHint.Text = hint + @" " + Resources.PleaseClick;
            _labelUpdateHint.Visible = true;
        }

        private void buttonAddMissing_Click(object sender, EventArgs e)
        {
            var commandCollector = new CommandCollector {Path = CommandExecutor.Path}; // Represent all changes in a single undo step
            try
            {
                CheckDigest(new GuiTaskHandler(this), commandCollector);
            }
                #region Error handling
            catch (OperationCanceledException)
            {
                return;
            }
            catch (ArgumentException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Warn);
                return;
            }
            catch (IOException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Warn);
                return;
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Warn);
                return;
            }
            catch (WebException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Warn);
                return;
            }
            catch (NotSupportedException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Warn);
                return;
            }
                #endregion

            finally
            {
                var command = commandCollector.BuildComposite();
                if (CommandExecutor == null) command.Execute();
                else CommandExecutor.Execute(command);
            }

            _labelUpdateHint.Visible = false;
        }
        #endregion

        #region Manifest digest
        /// <summary>
        /// Checks whether the <see cref="ManifestDigest"/> in <see cref="ContainerRef"/> matches the value generated by <see cref="GenerateDigest"/>.
        /// </summary>
        /// <param name="handler">A callback object used when the the user is to be informed about progress.</param>
        /// <param name="executor">Used to apply properties in an undoable fashion.</param>
        private void CheckDigest(ITaskHandler handler, ICommandExecutor executor)
        {
            var digest = GenerateDigest(handler, executor);
            if (ContainerRef.ManifestDigest == default(ManifestDigest)) SetDigest(digest, executor);
            else if (digest != ContainerRef.ManifestDigest)
            {
                bool warnOtherImplementations = (ContainerRef.RetrievalMethods.Count > 1);
                if (Msg.YesNo(this,
                    warnOtherImplementations ? Resources.DigestMismatch + "\n" + Resources.DigestOtherImplementations : Resources.DigestMismatch,
                    warnOtherImplementations ? MsgSeverity.Warn : MsgSeverity.Info,
                    Resources.DigestReplace, Resources.DigestKeep))
                    SetDigest(digest, executor);
            }
        }

        /// <summary>
        /// Generates the <see cref="ManifestDigest"/> for the files specified in the <see cref="RetrievalMethod"/>.
        /// </summary>
        /// <param name="handler">A callback object used when the the user is to be informed about progress.</param>
        /// <param name="executor">Used to apply properties in an undoable fashion.</param>
        /// <returns>The newly generated digest.</returns>
        private ManifestDigest GenerateDigest(ITaskHandler handler, ICommandExecutor executor)
        {
            ManifestDigest digest;
            using (var tempDir = Target.DownloadAndApply(handler, executor))
                digest = ImplementationUtils.GenerateDigest(tempDir, handler);

            if (digest.PartialEquals(ManifestDigest.Empty))
                Msg.Inform(this, Resources.EmptyImplementation, MsgSeverity.Warn);
            return digest;
        }

        /// <summary>
        /// Sets the <see cref="ManifestDigest"/> in the <see cref="ContainerRef"/>.
        /// </summary>
        /// <param name="digest">The digest to set.</param>
        /// <param name="executor">Used to apply properties in an undoable fashion.</param>
        private void SetDigest(ManifestDigest digest, ICommandExecutor executor)
        {
            executor.Execute(new SetValueCommand<ManifestDigest>(() => ContainerRef.ManifestDigest, value => ContainerRef.ManifestDigest = value, digest));

            if (string.IsNullOrEmpty(ContainerRef.ID) || ContainerRef.ID.Contains(@"="))
                executor.Execute(new SetValueCommand<string>(() => ContainerRef.ID, value => ContainerRef.ID = value, @"sha1new=" + digest.Sha1New));
        }
        #endregion
    }
}
