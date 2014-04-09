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
using System.Security.Permissions;
using System.Windows.Forms;
using NanoByte.Common.Utils;

namespace NanoByte.Common.Controls
{
    /// <summary>
    /// Represents a panel that reacts to touch input on Windows 7 or newer.
    /// </summary>
    public class TouchPanel : Panel, ITouchControl
    {
        #region Events
        /// <inheritdoc/>
        public event EventHandler<TouchEventArgs> TouchDown;

        /// <inheritdoc/>
        public event EventHandler<TouchEventArgs> TouchUp;

        /// <inheritdoc/>
        public event EventHandler<TouchEventArgs> TouchMove;
        #endregion

        #region Message handling
        protected override CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                CreateParams createParams = base.CreateParams;
                createParams.ExStyle |= 0x00000020; // WS_EX_TRANSPARENT
                return createParams;
            }
        }

        protected override void CreateHandle()
        {
            base.CreateHandle();
            WindowsUtils.RegisterTouchWindow(this);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            WindowsUtils.HandleTouchMessage(ref m, this, TouchDown, TouchMove, TouchUp);
            base.WndProc(ref m);
        }
        #endregion
    }
}
