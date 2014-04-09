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

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using NanoByte.Common.Utils;
using ICommandExecutor = NanoByte.Common.Undo.ICommandExecutor;

namespace NanoByte.Common.Controls
{
    /// <summary>
    /// Common base class for <see cref="IEditorControl{T}"/> implementations.
    /// </summary>
    /// <typeparam name="T">The type of element to edit.</typeparam>
    public abstract class EditorControlBase<T> : UserControl, IEditorControl<T> where T : class
    {
        #region Properties
        private T _target;

        /// <inheritdoc/>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual T Target
        {
            get { return _target; }
            set
            {
                _target = value;
                if (TargetChanged != null) TargetChanged();
                Refresh();
            }
        }

        /// <summary>
        /// Is raised when <see cref="Target"/> has been changed.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Is not really an event but rather a hook.")]
        protected event Action TargetChanged;

        private ICommandExecutor _commandExecutor;

        /// <inheritdoc/>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual ICommandExecutor CommandExecutor
        {
            get { return _commandExecutor; }
            set
            {
                _commandExecutor = value;
                if (CommandExecutorChanged != null) CommandExecutorChanged();
            }
        }

        /// <summary>
        /// Is raised when <see cref="CommandExecutor"/> has been changed.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Is not really an event but rather a hook.")]
        protected event Action CommandExecutorChanged;
        #endregion

        #region Constructor
        protected EditorControlBase(bool showDescriptionBox = true)
        {
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            AutoScroll = true;

            if (showDescriptionBox) AddDescriptionBox();
        }

        private void AddDescriptionBox()
        {
            var description = AttributeUtils.GetAttributes<DescriptionAttribute, T>().FirstOrDefault();
            if (description == null) return;

            var descriptionLabel = new Label
            {
                Text = description.Description, AutoEllipsis = true,
                AutoSize = false, Height = 35, Dock = DockStyle.Bottom,
                BorderStyle = BorderStyle.FixedSingle
            };
            descriptionLabel.Click += delegate { Msg.Inform(this, description.Description, MsgSeverity.Info); };
            Controls.Add(descriptionLabel);

            new ToolTip().SetToolTip(descriptionLabel, description.Description);
        }
        #endregion

        //--------------------//

        #region Refresh
        /// <summary>
        /// Is raised when <see cref="Refresh"/> is called.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Is not really an event but rather a hook.")]
        protected event Action OnRefresh;

        public override void Refresh()
        {
            if (OnRefresh != null) OnRefresh();
            base.Refresh();
        }
        #endregion

        #region Register
        /// <summary>
        /// Hooks a WinForms control in to the live editing and Undo system.
        /// </summary>
        /// <param name="control">The control to hook up (is automatically added to <see cref="Control.Controls"/>).</param>
        /// <param name="pointer">Read/write access to the value the <paramref name="control"/> represents.</param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The set-value callback method may throw any kind of exception.")]
        protected void RegisterControl(Control control, PropertyPointer<string> pointer)
        {
            Controls.Add(control);

            control.Validated += delegate
            {
                string text = string.IsNullOrEmpty(control.Text) ? null : control.Text;
                if (text == pointer.Value) return;

                try
                {
                    if (CommandExecutor == null) pointer.Value = text;
                    else CommandExecutor.Execute(new Undo.SetValueCommand<string>(pointer, text));
                }
                    #region Error handling
                catch (Exception ex)
                {
                    Msg.Inform(this, ex.Message, MsgSeverity.Warn);
                    control.Text = pointer.Value;
                }
                #endregion
            };

            // ReSharper disable once RedundantCheckBeforeAssignment
            OnRefresh += () => { if (control.Text != pointer.Value) control.Text = pointer.Value; };
        }

        /// <summary>
        /// Hooks a <see cref="ComboBox"/> in to the live editing and Undo system.
        /// </summary>
        /// <param name="control">The control to hook up (is automatically added to <see cref="Control.Controls"/>).</param>
        /// <param name="pointer">Read/write access to the value the <paramref name="control"/> represents.</param>
        protected void RegisterControl(ComboBox control, PropertyPointer<string> pointer)
        {
            // Setting ComboBox.Text will only work reliably if the value is in the Items list
            OnRefresh += () =>
            {
                if (pointer.Value != null && !control.Items.Contains(pointer.Value))
                    control.Items.Add(pointer.Value);
            };

            RegisterControl((Control)control, pointer);
        }

        /// <summary>
        /// Hooks a <see cref="UriTextBox"/> in to the live editing and Undo system.
        /// </summary>
        /// <param name="control">The control to hook up (is automatically added to <see cref="Control.Controls"/>).</param>
        /// <param name="pointer">Read/write access to the value the <paramref name="control"/> represents.</param>
        protected void RegisterControl(UriTextBox control, PropertyPointer<Uri> pointer)
        {
            Controls.Add(control);

            control.Validated += delegate
            {
                if (!control.IsValid || control.Uri == pointer.Value) return;

                if (CommandExecutor == null) pointer.Value = control.Uri;
                else CommandExecutor.Execute(new Undo.SetValueCommand<Uri>(pointer, control.Uri));
            };

            OnRefresh += () => control.Uri = pointer.Value;
        }

        /// <summary>
        /// Hooks up a <see cref="IEditorControl{T}"/> as child editor.
        /// </summary>
        /// <typeparam name="TControl">The specific <see cref="IEditorControl{T}"/> type.</typeparam>
        /// <typeparam name="TChild">The type the child editor handles.</typeparam>
        /// <param name="control">The control to hook up (is automatically added to <see cref="Control.Controls"/>).</param>
        /// <param name="getTarget">Callback to retrieve the (child) target of the <paramref name="control"/>.</param>
        protected void RegisterControl<TControl, TChild>(TControl control, Func<TChild> getTarget)
            where TControl : Control, IEditorControl<TChild>
        {
            Controls.Add(control);

            TargetChanged += () => control.Target = getTarget();
            CommandExecutorChanged += () => control.CommandExecutor = CommandExecutor;
            OnRefresh += control.Refresh;
        }

        /// <summary>
        /// Hooks a <see cref="CheckBox"/> in to the live editing and Undo system.
        /// </summary>
        /// <param name="control">The control to hook up (is automatically added to <see cref="Control.Controls"/>).</param>
        /// <param name="pointer">Read/write access to the value the <paramref name="control"/> represents.</param>
        protected void RegisterControl(CheckBox control, PropertyPointer<bool> pointer)
        {
            Controls.Add(control);

            control.CheckedChanged += delegate
            {
                if (control.Checked == pointer.Value) return;

                if (CommandExecutor == null) pointer.Value = control.Checked;
                else CommandExecutor.Execute(new Undo.SetValueCommand<bool>(pointer, control.Checked));
            };

            OnRefresh += () => control.Checked = pointer.Value;
        }
        #endregion
    }
}
