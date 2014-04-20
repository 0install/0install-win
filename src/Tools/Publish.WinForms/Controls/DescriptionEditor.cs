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
    /// A common base for <see cref="IDependencyContainer"/> editors.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="IDependencyContainer"/> to edit.</typeparam>
    public class DescriptionEditor<T> : EditorControlBase<T>
        where T : class, IDescriptionContainer
    {
        protected readonly LocalizableTextBox TextBoxDescription;
        protected readonly GenericEditorControl<T> EditorControl;

        public DescriptionEditor()
        {
            TextBoxDescription = new LocalizableTextBox
            {
                Location = new Point(0, 0),
                Size = new Size(Width, 76),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                TabIndex = 1,
                HintText = "Description"
            };
            RegisterControl(TextBoxDescription, () => Target.Descriptions);

            EditorControl = new GenericEditorControl<T>(showDescriptionBox: false)
            {
                Location = new Point(0, TextBoxDescription.Bottom + 6),
                Size = new Size(Width, Height - TextBoxDescription.Bottom - 6),
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom,
                TabIndex = 2
            };
            RegisterControl(EditorControl, () => Target);
        }
    }
}
