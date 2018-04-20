// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Windows.Forms;
using NanoByte.Common;
using NanoByte.Common.Tasks;

namespace ZeroInstall.Commands.WinForms
{
    /// <summary>
    /// Like <see cref="SilentTaskHandler"/> but with <see cref="Msg"/> for <see cref="ITaskHandler.Ask(string)"/>.
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
        protected override bool Ask(string question, MsgSeverity severity)
        {
            bool result = false;
            Log.Debug("Question: " + question);
            _owner.Invoke(new Action(() => result = Msg.YesNo(_owner, question, severity)));
            Log.Debug("Answer: " + result);
            return result;
        }
    }
}
