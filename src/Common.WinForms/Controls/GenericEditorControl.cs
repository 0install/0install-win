/*
 * Copyright 2006-2014 Bastian Eicher
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

using System.Drawing;
using System.Windows.Forms;
using NanoByte.Common.Undo;
using NanoByte.Common.Utils;

namespace NanoByte.Common.Controls
{
    /// <summary>
    /// Edits arbitrary types of elements using a <see cref="PropertyGrid"/>. Provides optional <see cref="Common.Undo"/> support.
    /// </summary>
    /// <typeparam name="T">The type of element to edit.</typeparam>
    public sealed class GenericEditorControl<T> : EditorControlBase<T> where T : class
    {
        public GenericEditorControl()
        {
            AddPropertyGrid();
        }

        public GenericEditorControl(bool showDescriptionBox) : base(showDescriptionBox)
        {
            AddPropertyGrid();
        }

        private void AddPropertyGrid()
        {
            var propertyGrid = new ResettablePropertyGrid
            {
                ToolbarVisible = false,
                Size = Size - new Size(0, 45),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            Controls.Add(propertyGrid);

            TargetChanged += () => propertyGrid.SelectedObject = Target;
            OnRefresh += propertyGrid.Refresh;

            SetupUndoTracking(propertyGrid);
        }

        private void SetupUndoTracking(PropertyGrid propertyGrid)
        {
            if (UnixUtils.IsUnix)
            { // WORKAROUND: e.OldValue is not reliable on Mono, use MultiPropertyTracker instead
                var tracker = new MultiPropertyTracker(propertyGrid);
                propertyGrid.PropertyValueChanged += delegate(object sender, PropertyValueChangedEventArgs e)
                {
                    if (CommandExecutor != null)
                        CommandExecutor.Execute(tracker.GetCommand(e.ChangedItem));
                };
            }
            else
            {
                propertyGrid.PropertyValueChanged += delegate(object sender, PropertyValueChangedEventArgs e)
                {
                    if (CommandExecutor != null)
                        CommandExecutor.Execute(new PropertyChangedCommand(Target, e));
                };
            }
        }
    }
}
