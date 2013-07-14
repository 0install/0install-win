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

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Common.Controls;
using ZeroInstall.Model;
using ICommandExecutor = Common.Undo.ICommandExecutor;

namespace ZeroInstall.Publish.WinForms.Controls
{
    /// <summary>
    /// A common base for <see cref="ISummaryContainer"/> editors.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="ISummaryContainer"/> to edit.</typeparam>
    public class SummaryEditor<T> : DescriptionEditor<T>
        where T : class, ISummaryContainer
    {
        #region Properties
        /// <inheritdoc/>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override T Target
        {
            set
            {
                _textBoxSummary.Target = value.Summaries;
                base.Target = value;
            }
        }

        /// <inheritdoc/>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override ICommandExecutor CommandExecutor
        {
            set
            {
                _textBoxSummary.CommandExecutor = value;
                base.CommandExecutor = value;
            }
        }
        #endregion

        #region Constructor
        private readonly LocalizableTextBox _textBoxSummary;

        public SummaryEditor()
        {
            Controls.Add(_textBoxSummary = new LocalizableTextBox
            {
                Location = new Point(0, 0),
                Size = new Size(Width, 23),
                Multiline = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                HintText = "Summary",
                TabIndex = 0
            });

            // Shift existing controls down
            TextBoxDescription.Location = new Point(0, _textBoxSummary.Bottom + 6);
            EditorControl.Location = new Point(0, TextBoxDescription.Bottom + 6);
            EditorControl.Size = new Size(Width, Height - TextBoxDescription.Bottom - 6);
        }
        #endregion

        public override void Refresh()
        {
            _textBoxSummary.Refresh();
            base.Refresh();
        }
    }
}
