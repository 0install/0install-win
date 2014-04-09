/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using Gtk;

namespace NanoByte.Common.Gtk
{
    /// <summary>
    /// Provides easier access to typical <see cref="MessageDialog"/> configurations and automatically logs error messages.
    /// </summary>
    public static class Msg
    {
        #region Inform
        /// <summary>
        /// Displays a message to the user using a <see cref="MessageDialog"/>.
        /// </summary>
        /// <param name="owner">The parent window the displayed window is modal to.</param>
        /// <param name="text">The message to be displayed; must not be <see langword="null"/>.</param>
        /// <param name="severity">How severe/important the message is.</param>
        public static void Inform(Window owner, string text, MsgSeverity severity)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(text)) throw new ArgumentNullException("text");
            #endregion

            #region Logging
            switch (severity)
            {
                case MsgSeverity.Warn:
                    Log.Warn(text);
                    break;
                case MsgSeverity.Error:
                    Log.Error(text);
                    break;
            }
            #endregion

            ShowMessageDialog(owner, text, severity, ButtonsType.Ok);
        }
        #endregion

        #region Ask
        /// <summary>
        /// Asks the user a OK/cancel-question using a <see cref="MessageDialog"/>.
        /// </summary>
        /// <param name="owner">The parent window the displayed window is modal to.</param>
        /// <param name="text">The message to be displayed; must not be <see langword="null"/>.</param>
        /// <param name="severity">How severe/important the message is.</param>
        /// <returns><see langword="true"/> if Yes was selected, <see langword="false"/> if No was selected.</returns>
        public static bool Ask(Window owner, string text, MsgSeverity severity)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(text)) throw new ArgumentNullException("text");
            #endregion

            #region Logging
            switch (severity)
            {
                case MsgSeverity.Warn:
                    Log.Warn(text);
                    break;
                case MsgSeverity.Error:
                    Log.Error(text);
                    break;
            }
            #endregion

            return ShowMessageDialog(owner, text, severity, ButtonsType.OkCancel) == ResponseType.Ok;
        }
        #endregion

        #region Select
        /// <summary>
        /// Asks the user to choose between two options (yes/no) using a <see cref="MessageDialog"/>.
        /// </summary>
        /// <param name="owner">The parent window the displayed window is modal to.</param>
        /// <param name="text">The message to be displayed; must not be <see langword="null"/>.</param>
        /// <param name="severity">How severe/important the message is.</param>
        /// <param name="allowCancel">Can the user also cancel / choose neither of the two?</param>
        /// <param name="option1">The title and a short description (separated by a linebreak) of the <see cref="ResponseType.Yes"/> option; must not be <see langword="null"/>.</param>
        /// <param name="option2">The title and a short description (separated by a linebreak) of the <see cref="ResponseType.No"/> option; must not be <see langword="null"/>.</param>
        /// <returns><see cref="ResponseType.Yes"/> if <paramref name="option1"/> was chosen,
        /// <see cref="ResponseType.No"/> if <paramref name="option2"/> was chosen,
        /// <see cref="ResponseType.Cancel"/> otherwise.</returns>
        /// <remarks>If a <see cref="MessageDialog"/> is used, <paramref name="option1"/> and <paramref name="option2"/> are not display to the user, so don't rely on them!</remarks>
        public static ResponseType Choose(Window owner, string text, MsgSeverity severity, bool allowCancel, string option1, string option2)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(text)) throw new ArgumentNullException("text");
            if (string.IsNullOrEmpty(option1)) throw new ArgumentNullException("option1");
            if (string.IsNullOrEmpty(option2)) throw new ArgumentNullException("option2");
            #endregion

            #region Logging
            switch (severity)
            {
                case MsgSeverity.Warn:
                    Log.Warn(text);
                    break;
                case MsgSeverity.Error:
                    Log.Error(text);
                    break;
            }
            #endregion

            // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
            return ShowMessageDialog(owner, text, severity, allowCancel ? ButtonsType.YesNo | ButtonsType.Cancel : ButtonsType.YesNo);
        }
        #endregion

        //--------------------//

        #region MessageDialog
        /// <summary>Displays a message using a <see cref="MessageDialog"/>.</summary>
        /// <param name="owner">The parent window the displayed window is modal to.</param>
        /// <param name="text">The message to be displayed; must not be <see langword="null"/>.</param>
        /// <param name="severity">How severe/important the message is.</param>
        /// <param name="buttons">The buttons the user can click.</param>
        private static ResponseType ShowMessageDialog(Window owner, string text, MsgSeverity severity, ButtonsType buttons)
        {
            // Select icon based on message severity
            MessageType type;
            switch (severity)
            {
                case MsgSeverity.Warn:
                    type = MessageType.Warning;
                    break;
                case MsgSeverity.Error:
                    type = MessageType.Error;
                    break;
                default:
                case MsgSeverity.Info:
                    // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
                    type = buttons.HasFlag(ButtonsType.YesNo) ? MessageType.Question : MessageType.Info;
                    break;
            }

            // Display MessageDialog
            using (var dialog = new MessageDialog(owner, DialogFlags.Modal, type, ButtonsType.YesNo, text))
                return (ResponseType)dialog.Run();
        }
        #endregion
    }
}
