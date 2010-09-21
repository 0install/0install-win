/*
 * Copyright 2006-2010 Bastian Eicher
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
using System.Globalization;
using System.Windows.Forms;
using TaskDialog;

namespace Common
{
    #region Enumerations
    /// <summary>
    /// How severe/important a message is
    /// </summary>
    public enum MsgSeverity
    {
        Information, Warning, Error
    }
    #endregion

    /// <summary>
    /// Provides easier access to typical <see cref="MessageBox"/> configurations and automatically logs error messages.
    /// </summary>
    public static class Msg
    {
        #region Inform
        /// <summary>
        /// Displays a message to the user using a <see cref="MessageBox"/> or <see cref="TaskDialog"/>.
        /// </summary>
        /// <param name="owner">The parent window the displayed window is modal to.</param>
        /// <param name="text">The message to be displayed; must not be <see langword="null"/>.</param>
        /// <param name="severity">How severe/important the message is.</param>
        public static void Inform(IWin32Window owner, string text, MsgSeverity severity)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(text)) throw new ArgumentNullException("text");
            #endregion

            #region Logging
            switch (severity)
            {
                case MsgSeverity.Warning: Log.Warn(text); break;
                case MsgSeverity.Error: Log.Error(text); break;
            }
            #endregion

            // Use TaskDialog if possibe, otherwise fall back to MessageBox
            if (TaskDialog.TaskDialog.IsAvailable)
            {
                try { ShowTaskDialog(GetTaskDialog(text, severity), owner); }
                catch (BadImageFormatException) { ShowMesageBox(owner, text, severity, MessageBoxButtons.OK); }
                catch (EntryPointNotFoundException) { ShowMesageBox(owner, text, severity, MessageBoxButtons.OK); }
            }
            else ShowMesageBox(owner, text, severity, MessageBoxButtons.OK);
        }
        #endregion

        #region Ask
        /// <summary>
        /// Asks the user a OK/cancel-question using a <see cref="MessageBox"/> or <see cref="TaskDialog"/>.
        /// </summary>
        /// <param name="owner">The parent window the displayed window is modal to.</param>
        /// <param name="text">The message to be displayed; must not be <see langword="null"/>.</param>
        /// <param name="severity">How severe/important the message is.</param>
        /// <param name="option1">The title and a short description (separated by a linebreak) of the <see cref="DialogResult.OK"/> option; must not be <see langword="null"/>.</param>
        /// <param name="option2">The title and a short description (separated by a linebreak) of the <see cref="DialogResult.Cancel"/> option; may be <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if <paramref name="option1"/> was selected, <see langword="false"/> if <paramref name="option2"/> was selected.</returns>
        /// <remarks>If a <see cref="MessageBox"/> is used, <paramref name="option1"/> and <paramref name="option2"/> are not display to the user, so don't rely on them!</remarks>
        public static bool Ask(IWin32Window owner, string text, MsgSeverity severity, string option1, string option2)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(text)) throw new ArgumentNullException("text");
            if (string.IsNullOrEmpty(option1)) throw new ArgumentNullException("option1");
            #endregion

            #region Logging
            switch (severity)
            {
                case MsgSeverity.Warning: Log.Warn(text); break;
                case MsgSeverity.Error: Log.Error(text); break;
            }
            #endregion

            // Use TaskDialog if possibe, otherwise fall back to MessageBox
            if (TaskDialog.TaskDialog.IsAvailable)
            {
                #region TaskDialog
                var taskDialog = GetTaskDialog(text, severity);

                // Display default names with custom explanations
                taskDialog.UseCommandLinks = true;

                if (string.IsNullOrEmpty(option2))
                { // Default cancel button
                    taskDialog.Buttons = new[]
                    {
                        new TaskDialogButton((int)DialogResult.OK, option1.Replace("\r\n", "\n")),
                    };
                    taskDialog.CommonButtons = TaskDialogCommonButtons.Cancel;
                }
                else
                { // Custom cancel button
                    taskDialog.Buttons = new[]
                    {
                        new TaskDialogButton((int)DialogResult.OK, option1.Replace("\r\n", "\n")),
                        new TaskDialogButton((int)DialogResult.Cancel, option2.Replace("\r\n", "\n"))
                    };
                }

                // Only Infos should default to OK
                if (severity >= MsgSeverity.Warning) taskDialog.DefaultButton = (int)DialogResult.Cancel;

                try { return ShowTaskDialog(taskDialog, owner) == DialogResult.OK; }
                catch (BadImageFormatException) { return ShowMesageBox(owner, text, severity, MessageBoxButtons.OKCancel) == DialogResult.OK; }
                catch (EntryPointNotFoundException) { return ShowMesageBox(owner, text, severity, MessageBoxButtons.OKCancel) == DialogResult.OK; }

                #endregion
            }

            // MessgeBox
            return ShowMesageBox(owner, text, severity, MessageBoxButtons.OKCancel) == DialogResult.OK;
        }
        #endregion

        #region Select
        /// <summary>
        /// Asks the user to choose between two options (yes/no) using a <see cref="MessageBox"/> or <see cref="TaskDialog"/>.
        /// </summary>
        /// <param name="owner">The parent window the displayed window is modal to.</param>
        /// <param name="text">The message to be displayed; must not be <see langword="null"/>.</param>
        /// <param name="severity">How severe/important the message is.</param>
        /// <param name="allowCancel">Can the user also cancel / choose neither of the two?</param>
        /// <param name="option1">The title and a short description (separated by a linebreak) of the <see cref="DialogResult.Yes"/> option; must not be <see langword="null"/>.</param>
        /// <param name="option2">The title and a short description (separated by a linebreak) of the <see cref="DialogResult.No"/> option; must not be <see langword="null"/>.</param>
        /// <returns><see cref="DialogResult.Yes"/> if <paramref name="option1"/> was chosen,
        /// <see cref="DialogResult.No"/> if <paramref name="option2"/> was chosen,
        /// <see cref="DialogResult.Cancel"/> otherwise.</returns>
        /// <remarks>If a <see cref="MessageBox"/> is used, <paramref name="option1"/> and <paramref name="option2"/> are not display to the user, so don't rely on them!</remarks>
        public static DialogResult Choose(IWin32Window owner, string text, MsgSeverity severity, bool allowCancel, string option1, string option2)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(text)) throw new ArgumentNullException("text");
            if (string.IsNullOrEmpty(option1)) throw new ArgumentNullException("option1");
            if (string.IsNullOrEmpty(option2)) throw new ArgumentNullException("option2");
            #endregion

            #region Logging
            switch (severity)
            {
                case MsgSeverity.Warning: Log.Warn(text); break;
                case MsgSeverity.Error: Log.Error(text); break;
            }
            #endregion

            // Use TaskDialog if possibe, otherwise fall back to MessageBox
            if (TaskDialog.TaskDialog.IsAvailable)
            {
                #region TaskDialog
                var taskDialog = GetTaskDialog(text, severity);

                // Display fully customized text
                taskDialog.UseCommandLinks = true;
                taskDialog.Buttons = new[]
                {
                    new TaskDialogButton((int)DialogResult.Yes, option1.Replace("\r\n", "\n")),
                    new TaskDialogButton((int)DialogResult.No, option2.Replace("\r\n", "\n"))
                };

                // Add cancel support if desired
                if (allowCancel)
                {
                    taskDialog.AllowDialogCancellation = true;
                    taskDialog.CommonButtons = TaskDialogCommonButtons.Cancel;

                    // Infos and Warnings (like Save) should default to yes
                    if (severity >= MsgSeverity.Error) taskDialog.DefaultButton = (int)DialogResult.Cancel;
                }

                try { return ShowTaskDialog(taskDialog, owner); }
                catch (BadImageFormatException) { return ShowMesageBox(owner, text, severity, allowCancel ? MessageBoxButtons.YesNoCancel : MessageBoxButtons.YesNo); }
                catch (EntryPointNotFoundException) { return ShowMesageBox(owner, text, severity, allowCancel ? MessageBoxButtons.YesNoCancel : MessageBoxButtons.YesNo); }
                #endregion
            }

            return ShowMesageBox(owner, text, severity, allowCancel ? MessageBoxButtons.YesNoCancel : MessageBoxButtons.YesNo);
        }
        #endregion

        //--------------------//

        #region MsgBox
        /// <summary>Displays a message using a <see cref="MessageBox"/>.</summary>
        /// <param name="owner">The parent window the displayed window is modal to.</param>
        /// <param name="text">The message to be displayed; must not be <see langword="null"/>.</param>
        /// <param name="severity">How severe/important the message is.</param>
        /// <param name="buttons">The buttons the user can click.</param>
        private static DialogResult ShowMesageBox(IWin32Window owner, string text, MsgSeverity severity, MessageBoxButtons buttons)
        {
            // Handle RTL systems
            MessageBoxOptions localizedOptions;
            if (CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft) localizedOptions = MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign;
            else localizedOptions = 0;

            // Select icon based on message severity
            MessageBoxIcon icon;
            switch (severity)
            {
                case MsgSeverity.Warning: icon = MessageBoxIcon.Warning; break;
                case MsgSeverity.Error: icon = MessageBoxIcon.Error; break;
                //case MsgSeverity.Information:
                default: icon = MessageBoxIcon.Information; break;
            }

            // Display message-box
            return MessageBox.Show(owner, text, Application.ProductName, buttons, icon, MessageBoxDefaultButton.Button1, localizedOptions);
        }
        #endregion

        #region TaskDialog
        /// <summary>Displays a message using a <see cref="TaskDialog"/>.</summary>
        /// <param name="text">The message to be displayed; must not be <see langword="null"/>.</param>
        /// <param name="severity">How severe/important the message is.</param>
        private static TaskDialog.TaskDialog GetTaskDialog(string text, MsgSeverity severity)
        {
            // Split everything from the second line onwards off from the main text
            string[] split = text.Replace("\r\n", "\n").Split(new[] { '\n' }, 2);
            var taskDialog = new TaskDialog.TaskDialog { MainInstruction = split[0], WindowTitle = Application.ProductName };
            if (split.Length == 2) taskDialog.Content = split[1];

            // Handle RTL systems
            taskDialog.RightToLeftLayout = CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft;

            // Select icon based on message severity
            switch (severity)
            {
                case MsgSeverity.Warning:
                    taskDialog.MainIcon = TaskDialogIcon.Warning;
                    taskDialog.AllowDialogCancellation = true;
                    break;

                case MsgSeverity.Error:
                    taskDialog.MainIcon = TaskDialogIcon.Error;
                    taskDialog.AllowDialogCancellation = false; // Real errors shouldn't be easily ESCed away
                    break;

                //case MsgSeverity.Information:
                default:
                    taskDialog.MainIcon = TaskDialogIcon.Information;
                    taskDialog.AllowDialogCancellation = true;
                    break;
            }

            return taskDialog;
        }

        /// <summary>
        /// Displays a <see cref="TaskDialog"/>.
        /// </summary>
        /// <param name="taskDialog">The <see cref="TaskDialog"/> to display.</param>
        /// <param name="owner">The parent window the displayed window is modal to.</param>
        /// <returns>Indicates the button the user pressed.</returns>
        /// <exception cref="BadImageFormatException">Thrown if the task-dialog DLL could not be loaded.</exception>
        /// <exception cref="EntryPointNotFoundException">Thrown if the task-dialog DLL routine could not be called.</exception>
        private static DialogResult ShowTaskDialog(TaskDialog.TaskDialog taskDialog, IWin32Window owner)
        {
            // Note: If you get an EntryPointNotFoundException here, add this to your application manifest and test outside the IDE:
            // <dependency>
            //   <dependentAssembly>
            //     <assemblyIdentity type="win32" name="Microsoft.Windows.Common-Controls" version="6.0.0.0" processorArchitecture="*" publicKeyToken="6595b64144ccf1df" language="*" />
            //   </dependentAssembly>
            // </dependency>

            int result = (owner == null) ? taskDialog.Show() : taskDialog.Show(owner);
            return (DialogResult)result;
        }
        #endregion
    }
}