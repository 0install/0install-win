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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace NanoByte.Common.Utils
{
    #region Enumerations
    [CLSCompliant(false)]
    [SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32", Justification = "uint required for Win32 API")]
    public enum WindowMessage : uint
    {
        Empty = 0,

        // Misc messages
        Destroy = 0x0002,
        Close = 0x0010,
        Quit = 0x0012,
        Paint = 0x000F,
        SetCursor = 0x0020,
        ActivateApplication = 0x001C,
        EnterMenuLoop = 0x0211,
        ExitMenuLoop = 0x0212,
        NonClientHitTest = 0x0084,
        PowerBroadcast = 0x0218,
        SystemCommand = 0x0112,
        GetMinMax = 0x0024,

        // Keyboard messages
        KeyDown = 0x0100,
        KeyUp = 0x0101,
        Character = 0x0102,
        SystemKeyDown = 0x0104,
        SystemKeyUp = 0x0105,
        SystemCharacter = 0x0106,

        // Mouse messages
        MouseMove = 0x0200,
        LeftButtonDown = 0x0201,
        LeftButtonUp = 0x0202,
        LeftButtonDoubleClick = 0x0203,
        RightButtonDown = 0x0204,
        RightButtonUp = 0x0205,
        RightButtonDoubleClick = 0x0206,
        MiddleButtonDown = 0x0207,
        MiddleButtonUp = 0x0208,
        MiddleButtonDoubleClick = 0x0209,
        MouseWheel = 0x020a,
        XButtonDown = 0x020B,
        XButtonUp = 0x020c,
        XButtonDoubleClick = 0x020d,
        MouseFirst = LeftButtonDown,
        MouseLast = XButtonDoubleClick,

        // Sizing
        EnterSizeMove = 0x0231,
        ExitSizeMove = 0x0232,
        Size = 0x0005,
    }
    #endregion

    /// <summary>
    /// Provides helper methods and API calls specific to the Windows platform.
    /// </summary>
    static partial class WindowsUtils
    {
        #region Structures
        [StructLayout(LayoutKind.Sequential)]
        private struct WinMessage
        {
// ReSharper disable InconsistentNaming
            public IntPtr hWnd;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public Point p;
// ReSharper restore InconsistentNaming
        }
        #endregion

        /// <summary>
        /// Determines whether this application is currently idle.
        /// </summary>
        /// <returns><see langword="true"/> if idle, <see langword="false"/> if handling window events.</returns>
        /// <remarks>Will always return <see langword="true"/> on non-Windows OSes.</remarks>
        public static bool AppIdle
        {
            get
            {
                if (IsWindows)
                {
                    WinMessage msg;
                    return !SafeNativeMethods.PeekMessage(out msg, IntPtr.Zero, 0, 0, 0);
                }
                return true; // Not supported on non-Windows OSes
            }
        }

        /// <summary>
        /// Determines whether <paramref name="key"/> is pressed right now.
        /// </summary>
        /// <remarks>Will always return <see langword="false"/> on non-Windows OSes.</remarks>
        public static bool IsKeyDown(Keys key)
        {
            if (IsWindows) return (SafeNativeMethods.GetAsyncKeyState((uint)key) & 0x8000) != 0;
            return false; // Not supported on non-Windows OSes
        }

        /// <summary>
        /// Text-box caret blink time in seconds.
        /// </summary>
        public static float CaretBlinkTime
        {
            get
            {
                return IsWindows
                    ? SafeNativeMethods.GetCaretBlinkTime() / 1000f
                    : 0.5f; // Default to 0.5 seconds on non-Windows OSes
            }
        }

        /// <summary>
        /// Prevents the mouse cursor from leaving a specific window.
        /// </summary>
        /// <param name="handle">The handle to the window to lock the mouse cursor into.</param>
        /// <returns>A handle to the window that had previously captured the mouse</returns>
        /// <remarks>Will do nothing on non-Windows OSes.</remarks>
        public static IntPtr SetCapture(IntPtr handle)
        {
            return IsWindows ? UnsafeNativeMethods.SetCapture(handle) : IntPtr.Zero;
        }

        /// <summary>
        /// Releases the mouse cursor after it was locked by <see cref="SetCapture"/>.
        /// </summary>
        /// <returns><see langword="true"/> if successful; <see langword="false"/> otherwise.</returns>
        /// <remarks>Will always return <see langword="false"/> on non-Windows OSes.</remarks>
        public static bool ReleaseCapture()
        {
            return IsWindows ? UnsafeNativeMethods.ReleaseCapture() : false;
        }

        //--------------------//

        private static long _performanceFrequency;

        /// <summary>
        /// A time index in seconds that continuously increases.
        /// </summary>
        /// <remarks>Depending on the operating system this may be the time of the system clock or the time since the system booted.</remarks>
        public static double AbsoluteTime
        {
            get
            {
                // Use high-accuracy kernel timing methods on NT
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    if (_performanceFrequency == 0)
                        SafeNativeMethods.QueryPerformanceFrequency(out _performanceFrequency);

                    long time;
                    SafeNativeMethods.QueryPerformanceCounter(out time);
                    return time / (double)_performanceFrequency;
                }

                return Environment.TickCount / 1000f;
            }
        }
    }
}
