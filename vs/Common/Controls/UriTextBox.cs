/*
 * Copyright 2010 Bastian Eicher
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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Common.Controls
{
    /// <summary>
    /// A <see cref="HintTextBox"/> designed specifically for entering URIs.
    /// </summary>
    /// <remarks>Will turn red for invalid input and green for valid input. Will not allow focus to be lost for invalid input.</remarks>
    [Description("A HintTextBox designed specifically for entering URIs.")]
    public class UriTextBox : HintTextBox
    {
        #region Events
        protected override void OnTextChanged(EventArgs e)
        {
            ForeColor = ValidateUri() ? Color.Green : Color.Red;
            
            base.OnTextChanged(e);
        }

        protected override void OnValidating(CancelEventArgs e)
        {
            e.Cancel = !ValidateUri();

            base.OnValidating(e);
        }
        #endregion

        #region Properties
        /// <summary>
        /// The <see cref="Uri"/> represented by this text box.
        /// </summary>
        /// <exception cref="UriFormatException">Thrown when trying to read while <see cref="TextBox.Text"/> is not a well-formed <see cref="Uri"/>.</exception>
        /// <remarks>It is always safe to set this property. It is safe to read this property after validation has been performed.</remarks>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Uri Uri
        {
            get { return string.IsNullOrEmpty(Text) ? null : new Uri(Text, UriKind.Absolute); }
            set { Text = (value == null) ? null : value.ToString(); }
        }

        /// <summary>
        /// When set to <see langword="true"/> only URIs starting with "http:" or "https:" will be considered valid.
        /// </summary>
        [DefaultValue(false), Category("Behavior"), Description("When set to true only URIs starting with \"http:\" or \"https:\" will be considered valid.")]
        public bool HttpOnly { get; set; }
        #endregion

        //--------------------//

        #region Helpers
        /// <summary>
        /// Checks if <see cref="TextBox.Text"/> currently represents a valid <see cref="Uri"/>.
        /// </summary>
        protected virtual bool ValidateUri()
        {
            // Allow empty input
            if (string.IsNullOrEmpty(Text)) return true;

            Uri temp;
            // Check URI is well-formed
            if (!Uri.TryCreate(Text, UriKind.Absolute, out temp)) return false;

            // Check URI is HTTP(S) if that was requested
            if (HttpOnly) return Text.StartsWith("http:") || Text.StartsWith("https:");

            return true;
        }
        #endregion
    }
}
