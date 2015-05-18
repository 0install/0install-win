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
    /// A common base for <see cref="ISummaryContainer"/> editors.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="ISummaryContainer"/> to edit.</typeparam>
    public class SummaryEditor<T> : DescriptionEditor<T>
        where T : class, ISummaryContainer
    {
        protected readonly LocalizableTextBox TextBoxSummary;

        public SummaryEditor()
        {
            TextBoxSummary = new LocalizableTextBox
            {
                Location = new Point(0, 0),
                Size = new Size(Width, 23),
                Multiline = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                HintText = "Summary",
                TabIndex = 0
            };
            RegisterControl(TextBoxSummary, () => Target.Summaries);

            // Shift existing controls down
            SuspendLayout();
            TextBoxDescription.Top = TextBoxSummary.Bottom + 6;
            EditorControl.Top = TextBoxDescription.Bottom + 6;
            EditorControl.Height = Height - TextBoxDescription.Bottom - 6;
            ResumeLayout();
        }
    }
}
