// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Windows.Forms;
using NanoByte.Common;
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
        public override bool Ask(string question, bool? defaultAnswer = null, string? alternateMessage = null)
        {
            bool result = false;
            Log.Debug("Question: " + question);
            _owner.Invoke(new Action(() => result = Msg.YesNo(_owner, question, MsgSeverity.Info)));
            Log.Debug("Answer: " + result);
            return result;
        }
    }
}
