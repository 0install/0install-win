/*
 * Copyright 2006-2010 Bastian Eicher
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
using System.Windows.Forms;
using Common.Helpers;
using LuaInterface;

namespace Common.Controls
{
    /// <summary>
    /// Provides a generic debug console powered by the <see cref="Log"/> system and Lua scripting.
    /// </summary>
    public partial class DebugConsole : Form
    {
        #region Variables
        private string _lastCommand;
        #endregion

        #region Properties
        /// <summary>
        /// The Lua instance used to parse the commands entered in the console.
        /// </summary>
        [CLSCompliant(false)]
        public Lua Lua { get; private set; }
        #endregion

        #region Constructor
        public DebugConsole()
        {
            InitializeComponent();

            // Prepare Lua interpreter
            Lua = new Lua();

            // Keep the text-box in sync with the Log while the window is open
            Log.NewEntry += UpdateLog;
            Closing += delegate { Log.NewEntry -= UpdateLog; };
        }
        #endregion

        #region Startup
        private void DebugForm_Load(object sender, EventArgs e)
        {
            // Copy Lua globals list to auto-complete collection
            var autoComplete = new AutoCompleteStringCollection();
            foreach (string global in Lua.Globals)
                autoComplete.Add(global);

            // Set text-box to use auto-complete collection
            inputBox.AutoCompleteCustomSource = autoComplete;
        }

        private void DebugForm_Shown(object sender, EventArgs e)
        {
            UpdateLog();
            inputBox.Focus();
        }
        #endregion

        //--------------------//

        #region Update
        private void UpdateLog()
        {
            var deleg = new SimpleEventHandler(() =>
            {   // Update the text-box display
                outputBox.Text = Log.Content.Trim();
                outputBox.Select(outputBox.Text.Length, 0);
                outputBox.ScrollToCaret();
                Application.DoEvents();
            });

            // Defer the execution to the UI thread if necessary
            if (outputBox.InvokeRequired) outputBox.BeginInvoke(deleg);
            deleg();
        }
        #endregion

        #region Run
        private void runButton_Click(object sender, EventArgs e)
        {
            string command = inputBox.Text.Trim();
            if (string.IsNullOrEmpty(command)) return;

            if (!inputBox.AutoCompleteCustomSource.Contains(command))
                inputBox.AutoCompleteCustomSource.Add(command);
            _lastCommand = command;
            Log.Echo("> " + command);

            try
            {
                // Execute the command and capture its result
                if (command.Contains("=")) Lua.DoString(command);
                else Lua.DoString("DebugResult = " + command);
                if (Lua["DebugResult"] != null)
                {
                    // Output the result as a string if possible
                    string result = Lua["DebugResult"].ToString();
                    if (!string.IsNullOrEmpty(result))
                        Log.Echo("==> " + result);
                    Lua["DebugResult"] = null;
                }
            }
            catch (LuaScriptException ex)
            {
                // Unwrap .NET exceptions inside Lua exceptions
                string message = ex.IsNetException ? ex.InnerException.Message : ex.Message;

                // Output exception message
                Log.Echo("==> " + message);
            }

            inputBox.Text = "";
        }
        #endregion

        #region Retreive last command
        private void inputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up && string.IsNullOrEmpty(inputBox.Text))
                inputBox.Text = _lastCommand;
        }
        #endregion
    }
}