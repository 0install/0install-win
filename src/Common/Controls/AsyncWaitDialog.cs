/*
 * Copyright 2006-2013 Bastian Eicher
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
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Common.Utils;

namespace Common.Controls
{
    /// <summary>
    /// Displays a "Loading..." dialog box in a separate GUI thread for cases where the main message pump is blocked.
    /// </summary>
    public sealed partial class AsyncWaitDialog : Form
    {
        #region Variables
        private readonly Thread _thread;

        /// <summary>A barrier that blocks threads until the window handle is ready.</summary>
        private readonly ManualResetEvent _handleReady = new ManualResetEvent(false);
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new asynchrnous waiting dialog.
        /// </summary>
        /// <param name="title">The title of th dialog to display.</param>
        /// <param name="icon">The icon for the dialog to display in the task bar; may be <see langword="null"/>.</param>
        public AsyncWaitDialog(string title, Icon icon = null)
        {
            InitializeComponent();

            Text = title;
            Icon = icon;

            HandleCreated += delegate { _handleReady.Set(); };
            HandleDestroyed += delegate { _handleReady.Reset(); };

            _thread = new Thread(() => Application.Run(this));
        }
        #endregion

        #region Event handlers
        private void AsyncWaitDialog_Shown(object sender, EventArgs e)
        {
            WindowsUtils.SetProgressState(Handle, WindowsUtils.TaskbarProgressBarState.Indeterminate);
        }

        private void AsyncWaitDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            WindowsUtils.SetProgressState(Handle, WindowsUtils.TaskbarProgressBarState.NoProgress);
        }
        #endregion

        #region Control
        /// <summary>
        /// Starts a new message pump with this dialog in a new thread.
        /// </summary>
        public void Start()
        {
            _thread.Start();
            _handleReady.WaitOne();
            Application.DoEvents();
        }

        /// <summary>
        /// Closes the dialog and stops the separate message pump.
        /// </summary>
        public void Stop()
        {
            Invoke(new Action(Close));
            _thread.Join();
        }
        #endregion
    }
}
