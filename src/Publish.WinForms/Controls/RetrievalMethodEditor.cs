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

using System.Windows.Forms;
using Common;
using Common.Controls;
using Common.Storage;
using Common.Tasks;
using Common.Undo;
using ZeroInstall.Model;

namespace ZeroInstall.Publish.WinForms.Controls
{
    /// <summary>
    /// A common base for <see cref="RetrievalMethod"/> editors.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="RetrievalMethod"/> to edit.</typeparam>
    public abstract class RetrievalMethodEditor<T> : UserControl, IEditorControlContainerRef<T, Implementation>
        where T : RetrievalMethod
    {
        #region Properties
        private T _target;

        /// <inheritdoc/>
        public T Target
        {
            get { return _target; }
            set
            {
                _target = value;
                Refresh();
            }
        }

        /// <inheritdoc/>
        public Implementation ContainerRef { get; set; }

        /// <inheritdoc/>
        public Common.Undo.ICommandExecutor CommandExecutor { get; set; }
        #endregion

        protected void CheckDigest()
        {
            if (ContainerRef == null) return;

            var handler = new GuiTaskHandler();
            using (var tempDir = Download(handler))
            {
                var digest = ImplementationUtils.GenerateDigest(tempDir, false, handler);

                if (ContainerRef.ManifestDigest == default(ManifestDigest)) UpdateDigest(digest);
                else if (digest != ContainerRef.ManifestDigest)
                {
                    if (Msg.YesNo(this, "Digest mismatch. Replace?", MsgSeverity.Warn))
                        UpdateDigest(digest);
                }
            }
        }

        protected abstract TemporaryDirectory Download(ITaskHandler handler);

        private void UpdateDigest(ManifestDigest digest)
        {
            if (CommandExecutor == null) ContainerRef.ManifestDigest = digest;
            else CommandExecutor.Execute(new SetValueCommand<ManifestDigest>(() => ContainerRef.ManifestDigest, value => ContainerRef.ManifestDigest = value, digest));
        }
    }
}
