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
    /// A common base for <see cref="IDependencyContainer"/> editors.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="IDependencyContainer"/> to edit.</typeparam>
    public class DescriptionEditor<T> : UserControl, IEditorControl<T>
        where T : class, IDescriptionContainer
    {
        #region Properties
        /// <inheritdoc/>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual T Target
        {
            get { return EditorControl.Target; }
            set
            {
                EditorControl.Target = value;
                TextBoxDescription.Target = value.Descriptions;
            }
        }

        /// <inheritdoc/>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual ICommandExecutor CommandExecutor
        {
            get { return EditorControl.CommandExecutor; }
            set
            {
                EditorControl.CommandExecutor = value;
                TextBoxDescription.CommandExecutor = value;
            }
        }
        #endregion

        #region Constructor
        protected readonly LocalizableTextBox TextBoxDescription;
        protected readonly EditorControl<T> EditorControl;

        public DescriptionEditor()
        {
            Controls.Add(TextBoxDescription = new LocalizableTextBox
            {
                Location = new Point(0, 0),
                Size = new Size(Width, 76),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                TabIndex = 1,
                HintText = "Description"
            });

            Controls.Add(EditorControl = new EditorControl<T>
            {
                Location = new Point(0, TextBoxDescription.Bottom + 6),
                Size = new Size(Width, Height - TextBoxDescription.Bottom - 6),
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom,
                TabIndex = 2
            });
        }
        #endregion

        public override void Refresh()
        {
            TextBoxDescription.Refresh();
            EditorControl.Refresh();
            base.Refresh();
        }
    }
}
