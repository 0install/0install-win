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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Common.Properties;

namespace Common.Controls
{
    /// <summary>
    /// A special <see cref="TextBox"/> that displays a <see cref="HintText"/> when <see cref="TextBox.Text"/> is empty and a clear button when it is not.
    /// </summary>
    [Description("A special TextBox that displays a hint text when Text is empty.")]
    public class HintTextBox : TextBox
    {
        #region Events
        protected override void OnEnter(EventArgs e)
        {
            // No hint when the cursor is in the text box
            _hintLabel.Visible = false;

            base.OnEnter(e);
        }

        protected override void OnLeave(EventArgs e)
        {
            // Restore hint when the cursor leaves the text box and the field is empty
            _hintLabel.Visible = string.IsNullOrEmpty(base.Text);

            base.OnLeave(e);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            // Show hint only when the cursor is not in the text box and the field is empty
            _hintLabel.Visible = HintTextVisible;

            // Show clear button only if it is enabled and the field is not empty
            _buttonClear.Visible = _showClearButton && !string.IsNullOrEmpty(base.Text);

            base.OnTextChanged(e);
        }

        protected override void OnFontChanged(EventArgs e)
        {
            _hintLabel.Font = Font;

            base.OnFontChanged(e);
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            _buttonClear.BackColor = BackColor;

            base.OnBackColorChanged(e);
        }
        #endregion

        #region Variables
        private readonly Label _hintLabel = new Label
        {
            Location = new Point(-2, 1), Size = new Size(85, 18), Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom,
            ForeColor = SystemColors.GrayText
        };

        private readonly PictureBox _buttonClear = new PictureBox
        {
            Visible = false, Cursor = Cursors.Default,
            Location = new Point(79, -1), Size = new Size(18, 18), Anchor = AnchorStyles.Right,
            BackColor = SystemColors.Window, BackgroundImageLayout = ImageLayout.Center, BackgroundImage = Resources.ClearButton
        };
        #endregion

        #region Properties
        /// <summary>
        /// A text to be displayed in <see cref="SystemColors.GrayText"/> when <see cref="TextBox.Text"/> is empty.
        /// </summary>
        [Category("Appearance"), Description("A text to be displayed in gray when Text is empty."), Localizable(true)]
        public string HintText
        {
            get { return _hintLabel.Text; } set { _hintLabel.Text = value; }
        }

        /// <summary>
        /// Indicates whether the <see cref="HintText"/> is currently visible.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool HintTextVisible
        {
            get { return !Focused && string.IsNullOrEmpty(base.Text); }
        }

        private bool _showClearButton;
        /// <summary>
        /// Controls whether the clear button is shown when <see cref="TextBox.Text"/> is not empty.
        /// </summary>
        [DefaultValue(false), Category("Appearance"), Description("Controls whether the clear button is shown when Text is not empty.")]
        public bool ShowClearButton
        {
            get { return _showClearButton; }
            set
            {
                _showClearButton = value;

                // Show clear button only if it is enabled and the field is not empty
                _buttonClear.Visible = value && !string.IsNullOrEmpty(base.Text);
            }
        }
        #endregion

        #region Constructor
        public HintTextBox()
        {
            _hintLabel.Click += delegate { Focus(); };
            _buttonClear.Click += delegate
            {
                Focus();

                // Only clear the text if focus change was possible (might be prevented by validation)
                if (Focused) Clear(); };
            }

            Controls.Add(_hintLabel);
            Controls.Add(_buttonClear);
        }
        #endregion
    }
}