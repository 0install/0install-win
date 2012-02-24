/*
 * Copyright 2010-2012 Bastian Eicher
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
using System.Windows.Forms;
using Common;
using ZeroInstall.Injector;

namespace ZeroInstall.Commands.WinForms
{
    /// <summary>
    /// Like <see cref="SilentHandler"/> but with <see cref="Msg"/> for <see cref="IHandler.AskQuestion"/>.
    /// </summary>
    public class MinimalHandler : SilentHandler
    {
        private readonly IWin32Window _owner;

        /// <summary>
        /// Creates a new minimal handler.
        /// </summary>
        /// <param name="owner">The parent window owning the handler.</param>
        public MinimalHandler(IWin32Window owner)
        {
            _owner = owner;
        }

        /// <inheritdoc/>
        public override bool AskQuestion(string question, string batchInformation)
        {
            switch (Msg.YesNoCancel(_owner, question, MsgSeverity.Info))
            {
                case DialogResult.Yes:
                    return true;
                case DialogResult.No:
                    return false;
                default:
                    throw new OperationCanceledException();
            }
        }
    }
}
