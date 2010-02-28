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
            _hintLabel.Visible = !Focused && string.IsNullOrEmpty(base.Text);

            // Show clear button only if it is enabled and the field is not empty
            _buttonClear.Visible = _clearButton && !string.IsNullOrEmpty(base.Text);
            
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

        private bool _clearButton;
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
        /// Controls whether the clear button is shown when <see cref="TextBox.Text"/> is empty.
        /// </summary>
        [DefaultValue(false), Category("Appearance"), Description("Controls whether the clear button is shown when Text is empty.")]
        public bool ClearButton
        {
            get { return _clearButton; }
            set
            {
                _clearButton = value;

                // Show clear button only if it is enabled and the field is not empty
                _buttonClear.Visible = value && !string.IsNullOrEmpty(base.Text);
            }
        }
        #endregion

        #region Constructor
        public HintTextBox()
        {
            _hintLabel.Click += delegate { Focus(); };
            _buttonClear.Click += delegate { Text = ""; Focus(); };

            Controls.Add(_hintLabel);
            Controls.Add(_buttonClear);
        }
        #endregion
    }
}