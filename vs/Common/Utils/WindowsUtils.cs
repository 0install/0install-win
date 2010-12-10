/*
 * Based on Windows API Code Pack for Microsoft .NET Framework 1.0.1
 * Copyright 2009 Microsoft Corporation
 * License: http://code.msdn.microsoft.com/WindowsAPICodePack/Project/License.aspx
 *
 * Modified for better project integration
 * Copyright 2010 Bastian Eicher
 */

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;

namespace Common.Utils
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
        MouseFirst = LeftButtonDown, // Skip mouse move, it happens a lot and there is another message for that
        MouseLast = XButtonDoubleClick,

        // Sizing
        EnterSizeMove = 0x0231,
        ExitSizeMove = 0x0232,
        Size = 0x0005,
    }

    /// <summary>
    /// Represents the thumbnail progress bar state.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags", Justification = "These enum values are mutually exclusive and not meant to be ORed like flags")]
    public enum TaskbarProgressBarState
    {
        /// <summary>
        /// No progress is displayed.
        /// </summary>
        NoProgress = 0,

        /// <summary>
        /// The progress is indeterminate (marquee).
        /// </summary>
        Indeterminate = 0x1,

        /// <summary>
        /// Normal progress is displayed.
        /// </summary>
        Normal = 0x2,

        /// <summary>
        /// An error occurred (red).
        /// </summary>
        Error = 0x4,

        /// <summary>
        /// The operation is paused (yellow).
        /// </summary>
        Paused = 0x8
    }
    #endregion

    /// <summary>
    /// Easily access non-Framework Windows DLLs.
    /// </summary>
    public static class WindowsUtils
    {
        [SuppressUnmanagedCodeSecurity]
        private static class SafeNativeMethods
        {
            #region Platform
            [DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool IsWow64Process([In] IntPtr hProcess, [Out, MarshalAs(UnmanagedType.Bool)] out bool lpSystemInfo);
            #endregion

            #region Performance counters
            [DllImport("kernel32")]
            [return : MarshalAs(UnmanagedType.Bool)]
            internal static extern bool QueryPerformanceFrequency(out long lpFrequency);

            [DllImport("kernel32")]
            [return : MarshalAs(UnmanagedType.Bool)]
            internal static extern bool QueryPerformanceCounter(out long lpCounter);
            #endregion

            #region Window messages
            [DllImport("user32", CharSet = CharSet.Auto)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool PeekMessage(out WinMessage msg, IntPtr hWnd, uint messageFilterMin, uint messageFilterMax, uint flags);

            [StructLayout(LayoutKind.Sequential)]
            internal struct WinMessage
            {
// ReSharper disable InconsistentNaming
                public IntPtr hWnd;
                public IntPtr wParam;
                public IntPtr lParam;
                public uint time;
                public Point p;
// ReSharper restore InconsistentNaming
            }

            [DllImport("user32", CharSet = CharSet.Auto)]
            internal static extern short GetAsyncKeyState(uint key);

            [DllImport("user32", CharSet = CharSet.Auto)]
            public static extern int GetCaretBlinkTime();

            [DllImport("user32", CharSet = CharSet.Auto)]
            public static extern IntPtr SetCapture(IntPtr handle);

            [DllImport("user32", CharSet = CharSet.Auto)]
            [return : MarshalAs(UnmanagedType.Bool)]
            public static extern bool ReleaseCapture();
            #endregion

            #region Taskbar
            [DllImport("shell32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
            public static extern void SetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] string appID);
            #endregion
        }

        #region Platform
        /// <summary>
        /// <see langword="true"/> if the current operating system is a modern desktop Windows version (9x- or NT-based); <see langword="false"/> otherwise.
        /// </summary>
        public static bool IsWindows
        { get { return Environment.OSVersion.Platform == PlatformID.Win32Windows || Environment.OSVersion.Platform == PlatformID.Win32NT; } }

        /// <summary>
        /// <see langword="true"/> if the current operating system is Windows 7 or newer; <see langword="false"/> otherwise.
        /// </summary>
        public static bool IsWindows7
        { get { return Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version >= new Version(6, 1); } }

        /// <summary>
        /// <see langword="true"/> if the current operating system is 64-bit capable; <see langword="false"/> otherwise.
        /// </summary>
        public static bool Is64BitOperatingSystem
        { get { return Is64BitProcess || Is32BitProcessOn64BitOperatingSystem; } }

        /// <summary>
        /// <see langword="true"/> if the current process is 64-bit; <see langword="false"/> otherwise.
        /// </summary>
        public static bool Is64BitProcess
        { get { return IntPtr.Size == 8; } }

        /// <summary>
        /// <see langword="true"/> if the current process is 32-bit but the operating system is 64-bit capable; <see langword="false"/> otherwise.
        /// </summary>
        /// <remarks>Can only detect WOW on Windows XP and newer</remarks>
        public static bool Is32BitProcessOn64BitOperatingSystem
        {
            get
            {
                // Can only detect WOW on Windows XP or newer
                if (Environment.OSVersion.Platform != PlatformID.Win32NT || Environment.OSVersion.Version < new Version(5, 1)) return false;

                bool retVal;
                SafeNativeMethods.IsWow64Process(Process.GetCurrentProcess().Handle, out retVal);
                return retVal;
            }
        }
        #endregion

        #region Performance counter
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
        #endregion

        #region Window messages
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
                    SafeNativeMethods.WinMessage msg;
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
            get {
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
            return IsWindows ? SafeNativeMethods.SetCapture(handle) : IntPtr.Zero;
        }

        /// <summary>
        /// Releases the mouse cursor after it was locked by <see cref="SetCapture"/>.
        /// </summary>
        /// <returns><see langword="true"/> if successful; <see langword="false"/> otherwise.</returns>
        /// <remarks>Will always return <see langword="false"/> on non-Windows OSes.</remarks>
        public static bool ReleaseCapture()
        {
            return IsWindows ? SafeNativeMethods.ReleaseCapture() : false;
        }
        #endregion

        #region TaskBar
        /// <summary>
        /// Sets the current process' explicit application user model ID.
        /// </summary>
        /// <param name="appId">The application ID.</param>
        /// <remarks>The application ID is used to group related windows in the taskbar.</remarks>
        public static void SetCurrentProcessAppId(string appId)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(appId)) throw new ArgumentNullException("appId");
            #endregion

            if (IsWindows7)
                SafeNativeMethods.SetCurrentProcessExplicitAppUserModelID(appId);
        }

        private static readonly object _syncLock = new object();

        private static ITaskbarList4 _taskbarList;
        /// <summary>
        /// Singleton COM object for taskbar operations.
        /// </summary>
        private static ITaskbarList4 TaskbarList
        {
            get
            {
                if (_taskbarList == null)
                {
                    // Create a new instance of ITaskbarList3
                    lock (_syncLock)
                    {
                        if (_taskbarList == null)
                        {
                            _taskbarList = (ITaskbarList4)new CTaskbarList();
                            _taskbarList.HrInit();
                        }
                    }
                }

                return _taskbarList;
            }
        }

        /// <summary>
        /// Sets the type and state of the progress indicator displayed on a taskbar button 
        /// of the given window handle 
        /// </summary>
        /// <param name="windowHandle">The handle of the window whose associated taskbar button is being used as a progress indicator.
        /// This window belong to a calling process associated with the button's application and must be already loaded.</param>
        /// <param name="state">Progress state of the progress button</param>
        public static void SetProgressState(TaskbarProgressBarState state, IntPtr windowHandle)
        {
            if (IsWindows7)
                TaskbarList.SetProgressState(windowHandle, (TBPFLAG)state);
        }

        /// <summary>
        /// Displays or updates a progress bar hosted in a taskbar button of the given window handle 
        /// to show the specific percentage completed of the full operation.
        /// </summary>
        /// <param name="windowHandle">The handle of the window whose associated taskbar button is being used as a progress indicator.
        /// This window belong to a calling process associated with the button's application and must be already loaded.</param>
        /// <param name="currentValue">An application-defined value that indicates the proportion of the operation that has been completed at the time the method is called.</param>
        /// <param name="maximumValue">An application-defined value that specifies the value <paramref name="currentValue"/> will have when the operation is complete.</param>
        public static void SetProgressValue(int currentValue, int maximumValue, IntPtr windowHandle)
        {
            if (IsWindows7)
                TaskbarList.SetProgressValue(windowHandle, Convert.ToUInt32(currentValue), Convert.ToUInt32(maximumValue));
        }
        #endregion
    }
}
