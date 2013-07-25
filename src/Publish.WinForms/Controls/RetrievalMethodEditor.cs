/*
 * Copyright 2010-2013 Bastian Eicher
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
using Common;
using Common.Controls;
using Common.Storage;
using Common.Tasks;
using Common.Undo;
using ZeroInstall.Model;
using ICommandExecutor = Common.Undo.ICommandExecutor;

namespace ZeroInstall.Publish.WinForms.Controls
{
    /// <summary>
    /// A common base for <see cref="RetrievalMethod"/> editors.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="RetrievalMethod"/> to edit.</typeparam>
    public abstract class RetrievalMethodEditor<T> : EditorControlBase<T>, IEditorControlContainerRef<T, Implementation>
        where T : RetrievalMethod
    {
        #region Variables
        private readonly Button _buttonUpdateImplementation;
        #endregion

        #region Properties
        private Implementation _containerRef;
        /// <inheritdoc/>
        public virtual Implementation ContainerRef
        {
            get { return _containerRef; }
            set
            {
                _containerRef = value;
                _buttonUpdateImplementation.Visible = (value != null);
            }
        }
        #endregion

        #region Constructor
        protected RetrievalMethodEditor()
        {
            Controls.Add(_buttonUpdateImplementation = new Button
            {
                Size = new Size(123, 23),
                Location = new Point(14, 150),
                Anchor = AnchorStyles.Top,
                TabIndex = 1000,
                Text = "Update implementation",
                UseVisualStyleBackColor = true,
                Visible = false
            });
            _buttonUpdateImplementation.Click += buttonUpdate_Click;
        }
        #endregion

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            var commandCollector = new CommandCollector();
            try
            {
                CheckDigest(new GuiTaskHandler(), commandCollector);
            }
                #region Error handling
            catch (OperationCanceledException)
            {}
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
            #endregion

            var command = commandCollector.BuildComposite();

            if (CommandExecutor == null) command.Execute();
            else CommandExecutor.Execute(command);
        }

        private void CheckDigest(ITaskHandler handler, ICommandExecutor executor)
        {
            var digest = GenerateDigest(handler, executor);
            if (ContainerRef.ManifestDigest == default(ManifestDigest)) UpdateDigest(digest, executor);
            else if (digest != ContainerRef.ManifestDigest)
            {
                if (Msg.YesNo(this, "Digest mismatch. Replace?", MsgSeverity.Warn))
                    UpdateDigest(digest, executor);
            }
        }

        private ManifestDigest GenerateDigest(ITaskHandler handler, ICommandExecutor executor)
        {
            using (var tempDir = Download(handler, executor))
                return ImplementationUtils.GenerateDigest(tempDir, false, handler);
        }

        protected abstract TemporaryDirectory Download(ITaskHandler handler, ICommandExecutor executor);

        private void UpdateDigest(ManifestDigest digest, ICommandExecutor executor)
        {
            var setDigestCommand = new SetValueCommand<ManifestDigest>(() => ContainerRef.ManifestDigest, value => ContainerRef.ManifestDigest = value, digest);
            executor.Execute(setDigestCommand);
        }
    }
}
