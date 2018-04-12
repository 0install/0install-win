/*
 * Copyright 2010-2016 Bastian Eicher
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
using NanoByte.Common;
using NanoByte.Common.Tasks;

namespace ZeroInstall
{
    /// <summary>
    /// Uses <see cref="MainForm"/> to show <see cref="ITask"/> progress.
    /// </summary>
    /// <remarks>This class manages a GUI thread with an independent message queue. Invoking methods on the right thread is handled automatically.</remarks>
    public class GuiTaskHandler : GuiTaskHandlerBase
    {
        private readonly AsyncFormWrapper<MainForm> _wrapper;

        public GuiTaskHandler()
        {
            _wrapper = new AsyncFormWrapper<MainForm>(delegate
            {
                var form = new MainForm(CancellationTokenSource);
                form.Show();
                return form;
            });
        }

        public override void Dispose()
        {
            try
            {
                _wrapper.Dispose();
            }
            finally
            {
                base.Dispose();
            }
        }

        /// <inheritdoc/>
        public override void RunTask(ITask task)
        {
            #region Sanity checks
            if (task == null) throw new ArgumentNullException(nameof(task));
            #endregion

            Log.Debug("Task: " + task.Name);
            var progress = _wrapper.Post(form => form.GetProgressControl(task.Name));
            task.Run(CancellationToken, CredentialProvider, progress);
        }
    }
}
