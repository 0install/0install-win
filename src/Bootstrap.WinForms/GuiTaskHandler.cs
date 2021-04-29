// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using NanoByte.Common;
using NanoByte.Common.Tasks;
using NanoByte.Common.Threading;

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
