using System;
using System.Windows.Forms;

namespace Common.Controls
{
    /// <summary>
    /// Shows a simple dialog asking the user to input some text.
    /// </summary>
    public partial class InputBox : Form
    {
        #region Constructor
        public InputBox()
        {
            InitializeComponent();
        }
        #endregion

        /// <summary>
        /// Displays an input box asking the the user to input some text.
        /// </summary>
        /// <param name="prompt">The prompt to display.</param>
        /// <param name="title">The window title to use.</param>
        /// <param name="defaultText">The default text to show pre-entered in the input field.</param>
        /// <returns>The text the user entered if he pressed OK; otherwise <see langword="null"/>.</returns>
        public static string Show(string prompt, string title, string defaultText)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(prompt)) throw new ArgumentNullException("prompt");
            if (string.IsNullOrEmpty(title)) throw new ArgumentNullException("title");
            #endregion

            var inputBox = new InputBox
            {
                Text = title,
                labelPrompt = {Text = prompt},
                textInput = {Text = defaultText}
            };

            return (inputBox.ShowDialog() == DialogResult.OK) ? inputBox.textInput.Text : null;
        }
        
        /// <summary>
        /// Displays an input box asking the the user to input some text.
        /// </summary>
        /// <param name="prompt">The prompt to display.</param>
        /// <param name="title">The window title to use.</param>
        /// <returns>The text the user entered if he pressed OK; otherwise <see langword="null"/>.</returns>
        public static string Show(string prompt, string title)
        {
            return Show(prompt, title, "");
        }
    }
}