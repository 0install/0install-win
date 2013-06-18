/*
 * Copyright 2006-2013 Bastian Eicher
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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Common.Properties;
using ICSharpCode.TextEditor.Document;

namespace Common.Controls
{
    /// <summary>
    /// A text editor that automatically validates changes using an external callback after a short period of no input.
    /// </summary>
    public partial class LiveEditor : UserControl
    {
        private bool _suppressUpdate;

        /// <summary>
        /// Raised when changes have accumulated after a short period of no input.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Cannot rename System.Action<T>")]
        [Description("Raised when changes have accumulated after a short period of no input.")]
        public event Action<string> ContentChanged;

        public LiveEditor()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Sets a new text to be edited.
        /// </summary>
        /// <param name="text">The text to set.</param>
        /// <param name="format">The format named used to determine the highlighting scheme (e.g. XML).</param>
        public void SetContent(string text, string format)
        {
            _suppressUpdate = true;
            textEditor.BeginUpdate();
            textEditor.Document.TextContent = text;
            textEditor.Document.HighlightingStrategy = HighlightingStrategyFactory.CreateHighlightingStrategy(format);
            textEditor.Document.UndoStack.ClearAll();
            textEditor.EndUpdate();
            textEditor.Refresh();
            _suppressUpdate = false;

            SetStatus(Resources.Info, "OK");
        }

        private void textEditor_TextChanged(object sender, EventArgs e)
        {
            textEditor.Document.MarkerStrategy.RemoveAll(marker => true);
            if (_suppressUpdate) return;

            if (timer.Enabled) timer.Stop();
            else SetStatus(null, "Changed...");
            timer.Start();
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Invalid user input may cause arbitrary exceptions.")]
        private void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                ValidateContent();
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Invalid user input may cause arbitrary exceptions.")]
        private void textEditor_Validating(object sender, CancelEventArgs e)
        {
            if (timer.Enabled)
            { // Ensure pending validation is not lost
                try
                {
                    ValidateContent();
                }
                catch (Exception ex)
                {
                    HandleError(ex);
                    e.Cancel = true;
                }
            }
        }

        private void ValidateContent()
        {
            timer.Stop();

            if (ContentChanged != null) ContentChanged(textEditor.Text);
            SetStatus(Resources.Info, "OK");
            textEditor.Document.UndoStack.ClearAll();
        }

        private void HandleError(Exception ex)
        {
            SetStatus(Resources.Error, ex.Message);

            if (ex is InvalidDataException && ex.Source == "System.Xml" && ex.InnerException != null)
            {
                // Parse exception message for position of the error
                int lineStart = ex.Message.LastIndexOf('(') + 1;
                int lineLength = ex.Message.LastIndexOf(',') - lineStart;
                int charStart = ex.Message.LastIndexOf(' ') + 1;
                int charLength = ex.Message.LastIndexOf(')') - charStart;
                int lineNumber = int.Parse(ex.Message.Substring(lineStart, lineLength)) - 1;
                int charNumber = int.Parse(ex.Message.Substring(charStart, charLength)) - 1;

                int lineOffset = textEditor.Document.GetLineSegment(lineNumber).Offset;
                textEditor.Document.MarkerStrategy.AddMarker(
                    new TextMarker(lineOffset + charNumber, 10, TextMarkerType.WaveLine) {ToolTip = ex.InnerException.Message});
                textEditor.Refresh();
            }
        }

        private void SetStatus(Image image, string message)
        {
            labelStatus.Image = image;
            labelStatus.Text = message;
        }
    }
}
