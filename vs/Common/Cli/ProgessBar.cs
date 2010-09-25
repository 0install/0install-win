/*
 * Copyright 2006-2010 Simon E. Silva Lauinger, Bastian Eicher
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
using Common.Utils;
using Common.Properties;

namespace Common.Cli
{
    /// <summary>
    /// A progress bar rendered on the <see cref="Console"/>.
    /// </summary>
    public class ProgessBar
    {
        #region Properties
        private ProgressState _state;
        /// <summary>
        /// The current status of the task.
        /// </summary>
        [Description("The current status of the task.")]
        public ProgressState State
        {
            get { return _state; }
            set
            {
                #region Sanity checks
                if (!Enum.IsDefined(typeof(ProgressState), value))
                    throw new InvalidEnumArgumentException("value", (int)value, typeof(ProgressState));
                #endregion

                UpdateHelper.Do(ref _state, value, Draw);
            }
        }

        private int _maximum = 20;
        /// <summary>
        /// The maximum valid value for <see cref="Value"/>; must be greater than 0. Determines the length of the progress bar in console characters.
        /// </summary>
        [DefaultValue(20), Description("The maximum valid value for Value; must be greater than 0. Determines the length of the progress bar in console characters.")]
        public int Maximum
        {
            get { return _maximum; }
            set
            {
                #region Sanity checks
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("value", Resources.ArgMustBeGreaterThanZero);
                #endregion

                UpdateHelper.Do(ref _maximum, value, Draw);
                if (Value > Maximum) Value = Maximum;
            }
        }

        private int _value;
        /// <summary>
        /// The progress of the task as a value between 0 and <see cref="Maximum"/>; -1 when unknown.
        /// </summary>
        [DefaultValue(0), Description("The progress of the task as a value between 0 and Maximum; -1 when unknown.")]
        public int Value
        {
            get { return _value; }
            set
            {
                #region Sanity checks
                if (value < -1 || value > Maximum)
                    throw new ArgumentOutOfRangeException("value");
                #endregion

                UpdateHelper.Do(ref _value, value, Draw);
            }
        }
        #endregion

        //--------------------//

        #region Draw
        /// <summary>
        /// Draws the progress-bar to <see cref="Console.Error"/>.
        /// </summary>
        /// <remarks>The current line is overwritten.</remarks>
        public void Draw()
        {
            // Draw start of progress bar
            Console.CursorLeft = 0;
            Console.Error.Write(@"[");

            // Draw filled part
            Console.ForegroundColor = ConsoleColor.DarkGray;
            for (int i = 0; i < Value; i++)
                Console.Error.Write(@"=");
            Console.ResetColor();

            // Draw end of progress bar
            Console.CursorLeft = Maximum + 1;
            Console.Error.Write(@"]");

            // Write status
            Console.Error.Write(@" ");
            switch (State)
            {
                case ProgressState.Header:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Error.Write("Getting headers");
                    break;

                case ProgressState.Data:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Error.Write("Processing data");
                    break;

                case ProgressState.Complete:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Error.Write("Complete");
                    break;

                case ProgressState.WebError:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.Write("Web error");
                    break;

                case ProgressState.IOError:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.Write("IO error");
                    break;
            }
            Console.ResetColor();

            // Blanks at the end to overwrite any excess
            Console.Error.Write(@"          ");
            Console.CursorLeft -= 10;
        }
        #endregion
    }
}
