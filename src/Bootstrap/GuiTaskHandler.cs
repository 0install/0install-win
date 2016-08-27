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

        protected override void Dispose(bool disposing)
        {
            try
            {
                _wrapper.Dispose();
            }
            finally
            {
                base.Dispose(disposing);
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