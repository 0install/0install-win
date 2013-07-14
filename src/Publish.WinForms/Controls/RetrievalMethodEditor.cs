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
        #region Properties
        /// <inheritdoc/>
        public Implementation ContainerRef { get; set; }
        #endregion

        protected void CheckDigest()
        {
            if (ContainerRef == null) return;

            var commandCollector = new CommandCollector();
            CheckDigest(new GuiTaskHandler(), commandCollector);
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
