// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System.Windows.Forms;
using NanoByte.Common;
using NanoByte.Common.Controls;
using NanoByte.Common.Tasks;

namespace ZeroInstall.Central.WinForms
{
    /// <summary>
    /// Like <see cref="SilentTaskHandler"/> but with <see cref="Msg"/> for <see cref="ITaskHandler.Ask"/>.
    /// </summary>
    public class MinimalTaskHandler : GuiTaskHandlerBase
    {
        private readonly Control _owner;

        /// <summary>
        /// Creates a new minimal handler.
        /// </summary>
        /// <param name="owner">The parent window owning the handler.</param>
        public MinimalTaskHandler(Control owner)
        {
            _owner = owner;
        }

        /// <inheritdoc/>
        protected override bool AskInteractive(string question, bool defaultAnswer)
        {
            bool result = false;
            Log.Debug("Question: " + question);
            _owner.Invoke(() => result = Msg.YesNo(_owner, question, MsgSeverity.Info));
            Log.Debug("Answer: " + result);
            return result;
        }

        /// <summary>
        /// Cancels currently running <see cref="ITask"/>s.
        /// </summary>
        public void Cancel()
            => CancellationTokenSource.Cancel();
    }
}
