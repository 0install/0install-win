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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using NanoByte.Common.Controls;
using NanoByte.Common.Values;

namespace NanoByte.Common.Utils
{
    /// <summary>
    /// Provides helper methods and API calls specific to the Windows platform.
    /// </summary>
    static partial class WindowsUtils
    {
        // Note: The following code is based on Windows API Code Pack for Microsoft .NET Framework 1.0.1

        #region Enumerations
        [Flags]
        private enum TouchEvents
        {
            Move = 0x0001, // TOUCHEVENTF_MOVE
            Down = 0x0002, // TOUCHEVENTF_DOWN
            Up = 0x0004, // TOUCHEVENTF_UP
            InRange = 0x0008, // TOUCHEVENTF_INRANGE
            Primary = 0x0010, // TOUCHEVENTF_PRIMARY
            NoCoalesce = 0x0020, // TOUCHEVENTF_NOCOALESCE
            Palm = 0x0080 // TOUCHEVENTF_PALM
        }
        #endregion

        #region Structures
        [StructLayout(LayoutKind.Sequential)]
        private struct TouchInput
        {
            public int x, y;
            public IntPtr hSource;
            public int dwID;
            public TouchEvents dwFlags;
            public TouchEventMask dwMask;
            public int dwTime;
            public IntPtr dwExtraInfo;
            public int cxContact;
            public int cyContact;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Points
        {
            public short x, y;
        }
        #endregion

        /// <summary>
        /// Registers a control as a receiver for touch events.
        /// </summary>
        /// <param name="control">The control to register.</param>
        public static void RegisterTouchWindow(Control control)
        {
            #region Sanity checks
            if (control == null) throw new ArgumentNullException("control");
            #endregion

            if (IsWindows7) SafeNativeMethods.RegisterTouchWindow(control.Handle, 0);
        }

// ReSharper disable InconsistentNaming
        private const int WM_TOUCHMOVE = 0x0240, WM_TOUCHDOWN = 0x0241, WM_TOUCHUP = 0x0242;
// ReSharper restore InconsistentNaming
        private static readonly int _touchInputSize = Marshal.SizeOf(new TouchInput());

        /// <summary>
        /// Handles touch-related <see cref="Control.WndProc"/> <see cref="Message"/>s.
        /// </summary>
        /// <param name="m">The message to handle.</param>
        /// <param name="sender">The object to send possible events from.</param>
        /// <param name="onTouchDown">The event handler to call for touch down events; may be <see langword="null"/>.</param>
        /// <param name="onTouchMove">The event handler to call for touch move events; may be <see langword="null"/>.</param>
        /// <param name="onTouchUp">The event handler to call for touch up events; may be <see langword="null"/>.</param>
        public static void HandleTouchMessage(ref Message m, object sender, EventHandler<TouchEventArgs> onTouchDown, EventHandler<TouchEventArgs> onTouchMove, EventHandler<TouchEventArgs> onTouchUp)
        {
            if (!IsWindows7) return;
            if (m.Msg != WM_TOUCHDOWN && m.Msg != WM_TOUCHMOVE && m.Msg != WM_TOUCHUP) return;

            // More than one touchinput may be associated with a touch message,
            // so an array is needed to get all event information.
            short inputCount = (short)(m.WParam.ToInt32() & 0xffff); // Number of touch inputs, actual per-contact messages

            if (inputCount < 0) return;
            var inputs = new TouchInput[inputCount];

            // Unpack message parameters into the array of TOUCHINPUT structures, each
            // representing a message for one single contact.
            //Exercise2-Task1-Step3
            if (!SafeNativeMethods.GetTouchInputInfo(m.LParam, inputCount, inputs, _touchInputSize))
                return;

            // For each contact, dispatch the message to the appropriate message
            // handler.
            // Note that for WM_TOUCHDOWN you can get down & move notifications
            // and for WM_TOUCHUP you can get up & move notifications
            // WM_TOUCHMOVE will only contain move notifications
            // and up & down notifications will never come in the same message
            for (int i = 0; i < inputCount; i++)
            {
                TouchInput ti = inputs[i];

                // Assign a handler to this message.
                EventHandler<TouchEventArgs> handler = null;     // Touch event handler
                if (ti.dwFlags.HasFlag(TouchEvents.Down)) handler = onTouchDown;
                else if (ti.dwFlags.HasFlag(TouchEvents.Up)) handler = onTouchUp;
                else if (ti.dwFlags.HasFlag(TouchEvents.Move)) handler = onTouchMove;

                // Convert message parameters into touch event arguments and handle the event.
                if (handler != null)
                {
                    // TOUCHINFO point coordinates and contact size is in 1/100 of a pixel; convert it to pixels.
                    // Also convert screen to client coordinates.
                    var te = new TouchEventArgs
                    {
                        ContactY = ti.cyContact / 100,
                        ContactX = ti.cxContact / 100,
                        ID = ti.dwID,
                        LocationX = ti.x / 100,
                        LocationY = ti.y / 100,
                        Time = ti.dwTime,
                        Mask = ti.dwMask,
                        InRange = ti.dwFlags.HasFlag(TouchEvents.InRange),
                        Primary = ti.dwFlags.HasFlag(TouchEvents.Primary),
                        NoCoalesce = ti.dwFlags.HasFlag(TouchEvents.NoCoalesce),
                        Palm = ti.dwFlags.HasFlag(TouchEvents.Palm)
                    };

                    handler(sender, te);

                    m.Result = new IntPtr(1); // Indicate to Windows that the message was handled
                }
            }

            SafeNativeMethods.CloseTouchInputHandle(m.LParam);
        }
    }
}
