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

namespace ZeroInstall.Publish.WinForms.Controls
{
    public partial class FeedEditor : UserControl, IEditorControl<Feed>
    {
        #region Properties
        /// <inheritdoc/>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Feed Target
        {
            get { return _editorControl.Target; }
            set
            {
                _editorControl.Target = value;
                textBoxSummary.Target = value.Summaries;
                textBoxDescription.Target = value.Descriptions;
            }
        }

        /// <inheritdoc/>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Common.Undo.ICommandExecutor CommandExecutor
        {
            get { return _editorControl.CommandExecutor; }
            set
            {
                _editorControl.CommandExecutor = value;
                textBoxSummary.CommandExecutor = textBoxDescription.CommandExecutor = value;
            }
        }
        #endregion

        #region Constructor
        private readonly EditorControl<Feed> _editorControl;

        public FeedEditor()
        {
            InitializeComponent();
            _editorControl = new EditorControl<Feed>
            {
                Location = new Point(0, textBoxDescription.Bottom + 6),
                Size = new Size(Width, Height - textBoxDescription.Bottom - 6),
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom,
                TabIndex = textBoxDescription.TabIndex + 1
            };
            Controls.Add(_editorControl);
        }
        #endregion

        public override void Refresh()
        {
            textBoxDescription.Refresh();
            textBoxSummary.Refresh();
            _editorControl.Refresh();
            base.Refresh();
        }
    }
}
