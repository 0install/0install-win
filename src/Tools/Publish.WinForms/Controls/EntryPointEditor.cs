/*
 * Copyright 2010 Simon E. Silva Lauinger
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

using System.Drawing;
using System.Windows.Forms;
using NanoByte.Common.Controls;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Publish.WinForms.Controls
{
    /// <summary>
    /// Edits <see cref="EntryPoint"/> instances.
    /// </summary>
    public sealed class EntryPointEditor : SummaryEditor<EntryPoint>
    {
        public EntryPointEditor()
        {
            var textBoxNames = new LocalizableTextBox
            {
                Location = new Point(0, 0),
                Size = new Size(Width, 23),
                Multiline = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                HintText = "Names",
                TabIndex = 0
            };
            RegisterControl(textBoxNames, () => Target.Names);

            // Shift existing controls down
            SuspendLayout();
            TextBoxSummary.Top = textBoxNames.Bottom + 6;
            TextBoxDescription.Top = TextBoxSummary.Bottom + 6;
            EditorControl.Top = TextBoxDescription.Bottom + 6;
            EditorControl.Height = Height - TextBoxDescription.Bottom - 6;
            ResumeLayout();
        }
    }
}
