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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace NanoByte.Common.Utils
{
    static partial class WindowsUtils
    {
        #region Enumerations
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

        #region Structs
        /// <summary>
        /// Represents a shell link targeting a file.
        /// </summary>
        public struct ShellLink
        {
            /// <summary>The title/name of the task link.</summary>
            public readonly string Title;

            /// <summary>The target path the link shall point to.</summary>
            public readonly string Path;

            /// <summary>Additional arguments for <see cref="Title"/>; may be <see langword="null"/>.</summary>
            public readonly string Arguments;

            /// <summary>The path of the icon for the link.</summary>
            public readonly string IconPath;

            /// <summary>The resouce index within the file specified by <see cref="IconPath"/>.</summary>
            public readonly int IconIndex;

            /// <summary>
            /// Creates a new shell link structure.
            /// </summary>
            /// <param name="title">The title/name of the task link.</param>
            /// <param name="path">The target path the link shall point to and to get the icon from.</param>
            /// <param name="arguments">Additional arguments for <paramref name="title"/>; may be <see langword="null"/>.</param>
            public ShellLink(string title, string path, string arguments = null)
            {
                #region Sanity checks
                if (string.IsNullOrEmpty(title)) throw new ArgumentNullException("title");
                if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
                #endregion

                Title = title;
                IconPath = Path = path;
                Arguments = arguments;
                IconIndex = 0;
            }

            /// <summary>
            /// Creates a new shell link structure
            /// </summary>
            /// <param name="title">The title/name of the task link.</param>
            /// <param name="path">The target path the link shall point to.</param>
            /// <param name="arguments">Additional arguments for <paramref name="title"/>; may be <see langword="null"/>.</param>
            /// <param name="iconPath">The path of the icon for the link.</param>
            /// <param name="iconIndex">The resouce index within the file specified by <paramref name="iconPath"/>.</param>
            public ShellLink(string title, string path, string arguments, string iconPath, int iconIndex)
            {
                #region Sanity checks
                if (string.IsNullOrEmpty(title)) throw new ArgumentNullException("title");
                if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
                #endregion

                Title = title;
                Path = path;
                Arguments = arguments;
                IconPath = iconPath;
                IconIndex = iconIndex;
            }
        }
        #endregion

        #region Enumerations
        private enum ThumbnailMask
        {
            Bitmap = 0x1,
            Icon = 0x2,
            Tooltip = 0x4,
            Flags = 0x8
        }

        [Flags]
        private enum ThumbnailFlags
        {
            Enabled = 0x0,
            ThbfDisabled = 0x1,
            ThbfDismissonclick = 0x2,
            ThbfNobackground = 0x4,
            ThbfHidden = 0x8,
            ThbfNoninteractive = 0x10
        }

        [Flags]
        private enum StpFlag
        {
            None = 0x0,
            UseThumbnailalways = 0x1,
            UseThumbnailWhenActive = 0x2,
            UsePeekAlways = 0x4,
            UsePeekWhenActive = 0x8
        }

        private enum KnownDestinationCategory
        {
            Frequent = 1,
            Recent
        }
        #endregion

        #region Structures
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct ThumbnailButton
        {
            [MarshalAs(UnmanagedType.U4)]
            private ThumbnailMask dwMask;

            private uint iId;
            private uint iBitmap;
            private IntPtr hIcon;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            private string szTip;

            [MarshalAs(UnmanagedType.U4)]
            private ThumbnailFlags dwFlags;
        }
        #endregion

        #region COM
        [ComImport, Guid("c43dc798-95d1-4bea-9030-bb99e2983a1a"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface ITaskbarList4
        {
            // ITaskbarList
            [PreserveSig]
            void HrInit();

            [PreserveSig]
            void AddTab(IntPtr hwnd);

            [PreserveSig]
            void DeleteTab(IntPtr hwnd);

            [PreserveSig]
            void ActivateTab(IntPtr hwnd);

            [PreserveSig]
            void SetActiveAlt(IntPtr hwnd);

            // ITaskbarList2
            [PreserveSig]
            void MarkFullscreenWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);

            // ITaskbarList3
            [PreserveSig]
            void SetProgressValue(IntPtr hwnd, UInt64 ullCompleted, UInt64 ullTotal);

            [PreserveSig]
            void SetProgressState(IntPtr hwnd, TaskbarProgressBarState tbpFlags);

            [PreserveSig]
            void RegisterTab(IntPtr hwndTab, IntPtr hwndMDI);

            [PreserveSig]
            void UnregisterTab(IntPtr hwndTab);

            [PreserveSig]
            void SetTabOrder(IntPtr hwndTab, IntPtr hwndInsertBefore);

            [PreserveSig]
            void SetTabActive(IntPtr hwndTab, IntPtr hwndInsertBefore, uint dwReserved);

            [PreserveSig]
            uint ThumbBarAddButtons(IntPtr hwnd, uint cButtons, [MarshalAs(UnmanagedType.LPArray)] ThumbnailButton[] pButtons);

            [PreserveSig]
            uint ThumbBarUpdateButtons(IntPtr hwnd, uint cButtons, [MarshalAs(UnmanagedType.LPArray)] ThumbnailButton[] pButtons);

            [PreserveSig]
            void ThumbBarSetImageList(IntPtr hwnd, IntPtr himl);

            [PreserveSig]
            void SetOverlayIcon(IntPtr hwnd, IntPtr hIcon, [MarshalAs(UnmanagedType.LPWStr)] string pszDescription);

            [PreserveSig]
            void SetThumbnailTooltip(IntPtr hwnd, [MarshalAs(UnmanagedType.LPWStr)] string pszTip);

            [PreserveSig]
            void SetThumbnailClip(IntPtr hwnd, IntPtr prcClip);

            // ITaskbarList4
            void SetTabProperties(IntPtr hwndTab, StpFlag stpFlags);
        }

        [ComImport, Guid("56FDF344-FD6D-11d0-958A-006097C9A090"), ClassInterface(ClassInterfaceType.None)]
        private class CTaskbarList
        {}

        [ComImport, Guid("6332DEBF-87B5-4670-90C0-5E57B408A49E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface ICustomDestinationList
        {
            void SetAppID([MarshalAs(UnmanagedType.LPWStr)] string pszAppID);

            [PreserveSig]
            uint BeginList(out uint cMaxSlots, ref Guid riid, [Out, MarshalAs(UnmanagedType.Interface)] out object ppvObject);

            [PreserveSig]
            uint AppendCategory([MarshalAs(UnmanagedType.LPWStr)] string pszCategory, [MarshalAs(UnmanagedType.Interface)] IObjectArray poa);

            void AppendKnownCategory([MarshalAs(UnmanagedType.I4)] KnownDestinationCategory category);

            [PreserveSig]
            uint AddUserTasks([MarshalAs(UnmanagedType.Interface)] IObjectArray poa);

            void CommitList();
            void GetRemovedDestinations(ref Guid riid, [Out, MarshalAs(UnmanagedType.Interface)] out object ppvObject);
            void DeleteList([MarshalAs(UnmanagedType.LPWStr)] string pszAppID);
            void AbortList();
        }

        [ComImport, Guid("77F10CF0-3DB5-4966-B520-B7C54FD35ED6"), ClassInterface(ClassInterfaceType.None)]
        private class CDestinationList
        {}

        [ComImport, Guid("92CA9DCD-5622-4BBA-A805-5E9F541BD8C9"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IObjectArray
        {
            void GetCount(out uint cObjects);
            void GetAt(uint iIndex, ref Guid riid, [Out, MarshalAs(UnmanagedType.Interface)] out object ppvObject);
        }

        [ComImport, Guid("5632B1A4-E38A-400A-928A-D4CD63230295"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IObjectCollection
        {
            // IObjectArray
            [PreserveSig]
            void GetCount(out uint cObjects);

            [PreserveSig]
            void GetAt(uint iIndex, ref Guid riid, [Out, MarshalAs(UnmanagedType.Interface)] out object ppvObject);

            // IObjectCollection
            void AddObject([MarshalAs(UnmanagedType.Interface)] object pvObject);
            void AddFromArray([MarshalAs(UnmanagedType.Interface)] IObjectArray poaSource);
            void RemoveObject(uint uiIndex);
            void Clear();
        }

        [ComImport, Guid("2D3468C1-36A7-43B6-AC24-D3F02FD9607A"), ClassInterface(ClassInterfaceType.None)]
        private class CEnumerableObjectCollection
        {}

        [ComImport, Guid("000214F9-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellLinkW
        {
            void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, IntPtr pfd, uint fFlags);
            void GetIDList(out IntPtr ppidl);
            void SetIDList(IntPtr pidl);
            void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxName);
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
            void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
            void GetHotKey(out short wHotKey);
            void SetHotKey(short wHotKey);
            void GetShowCmd(out uint iShowCmd);
            void SetShowCmd(uint iShowCmd);
            void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] out StringBuilder pszIconPath, int cchIconPath, out int iIcon);
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);
            void Resolve(IntPtr hwnd, uint fFlags);
            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }

        [ComImport, Guid("00021401-0000-0000-C000-000000000046"), ClassInterface(ClassInterfaceType.None)]
        private class CShellLink
        {}

        private static readonly ITaskbarList4 _taskbarList;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Must perform COM call during init")]
        static WindowsUtils()
        {
            if (WindowsUtils.IsWindows7)
            {
                _taskbarList = (ITaskbarList4)new CTaskbarList();
                _taskbarList.HrInit();
            }
        }
        #endregion

        /// <summary>
        /// Sets the state of the taskbar progress indicator.
        /// </summary>
        /// <param name="handle">The handle of the window whose taskbar button contains the progress indicator.</param>
        /// <param name="state">The state of the progress indicator.</param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "COM calls throw unpredictable exceptions and this methods successful execution is not critical.")]
        public static void SetProgressState(IntPtr handle, TaskbarProgressBarState state)
        {
            if (!IsWindows7) return;
            try
            {
                lock (_taskbarList)
                    _taskbarList.SetProgressState(handle, state);
            }
            catch
            {}
        }

        /// <summary>
        /// Sets the value of the taskbar progress indicator.
        /// </summary>
        /// <param name="handle">The handle of the window whose taskbar button contains the progress indicator.</param>
        /// <param name="currentValue">The current value of the progress indicator.</param>
        /// <param name="maximumValue">The value <paramref name="currentValue"/> will have when the operation is complete.</param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "COM calls throw unpredictable exceptions and this methods successful execution is not critical.")]
        public static void SetProgressValue(IntPtr handle, int currentValue, int maximumValue)
        {
            if (!IsWindows7) return;
            try
            {
                lock (_taskbarList)
                    _taskbarList.SetProgressValue(handle, Convert.ToUInt32(currentValue), Convert.ToUInt32(maximumValue));
            }
            catch
            {}
        }

        /// <summary>
        /// Sets a specific window's explicit application user model ID.
        /// </summary>
        /// <param name="hwnd">A handle to the window to set the ID for.</param>
        /// <param name="appID">The application ID to set.</param>
        /// <param name="relaunchCommand">The command to use for relaunching this specific window if it was pinned to the taskbar; may be <see langword="null"/>.</param>
        /// <param name="relaunchIcon">The icon to use for pinning this specific window to the taskbar (written as Path,ResourceIndex); may be <see langword="null"/>.</param>
        /// <param name="relaunchName">The user-friendly name to associate with <paramref name="relaunchCommand"/>; may be <see langword="null"/>.</param>
        /// <remarks>The application ID is used to group related windows in the taskbar.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "COM calls throw unpredictable exceptions and this methods successful execution is not critical.")]
        public static void SetWindowAppID(IntPtr hwnd, string appID, string relaunchCommand = null, string relaunchIcon = null, string relaunchName = null)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(appID)) throw new ArgumentNullException("appID");
            #endregion

            if (!IsWindows7) return;

            try
            {
                IPropertyStore propertyStore = GetWindowPropertyStore(hwnd);

                var stringFormat = new Guid("{9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3}");
                SetPropertyValue(propertyStore, new PropertyKey(stringFormat, 5), appID);
                if (!string.IsNullOrEmpty(relaunchCommand)) SetPropertyValue(propertyStore, new PropertyKey(stringFormat, 2), relaunchCommand);
                if (!string.IsNullOrEmpty(relaunchIcon)) SetPropertyValue(propertyStore, new PropertyKey(stringFormat, 3), relaunchIcon);
                if (!string.IsNullOrEmpty(relaunchName)) SetPropertyValue(propertyStore, new PropertyKey(stringFormat, 4), relaunchName);

                Marshal.ReleaseComObject(propertyStore);
            }
            catch
            {}
        }

        /// <summary>
        /// Adds user-task links to the taskbar jumplist. Any existing task links are removed.
        /// </summary>
        /// <param name="appID">The application ID of the jumplist to add the task to.</param>
        /// <param name="links">The links to add to the jumplist.</param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "COM calls throw unpredictable exceptions and this methods successful execution is not critical.")]
        public static void AddTaskLinks(string appID, IEnumerable<ShellLink> links)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(appID)) throw new ArgumentNullException("appID");
            if (links == null) throw new ArgumentNullException("links");
            #endregion

            if (!IsWindows7) return;

            try
            {
                var customDestinationList = (ICustomDestinationList)new CDestinationList();
                customDestinationList.SetAppID(appID);

                var objectArray = new Guid("92CA9DCD-5622-4BBA-A805-5E9F541BD8C9");
                object removedItems;
                uint maxSlots;
                customDestinationList.BeginList(out maxSlots, ref objectArray, out removedItems);

                var taskContent = (IObjectCollection)new CEnumerableObjectCollection();
                foreach (var shellLink in links)
                    taskContent.AddObject(ConvertShellLink(shellLink));

                customDestinationList.AddUserTasks((IObjectArray)taskContent);
                customDestinationList.CommitList();
            }
            catch
            {}
        }

        /// <summary>
        /// Converts a managed shell link structure to a COM object.
        /// </summary>
        private static IShellLinkW ConvertShellLink(ShellLink shellLink)
        {
            var nativeShellLink = (IShellLinkW)new CShellLink();
            var nativePropertyStore = (IPropertyStore)nativeShellLink;

            nativeShellLink.SetPath(shellLink.Path);
            if (!string.IsNullOrEmpty(shellLink.Arguments)) nativeShellLink.SetArguments(shellLink.Arguments);
            if (!string.IsNullOrEmpty(shellLink.IconPath)) nativeShellLink.SetIconLocation(shellLink.IconPath, shellLink.IconIndex);

            nativeShellLink.SetShowCmd(1); // Normal window state

            SetPropertyValue(nativePropertyStore, new PropertyKey(new Guid("{F29F85E0-4FF9-1068-AB91-08002B27B3D9}"), 2), shellLink.Title);
            nativePropertyStore.Commit();

            return nativeShellLink;
        }
    }
}
