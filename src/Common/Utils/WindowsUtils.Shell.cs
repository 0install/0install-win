/*
 * Copyright 2006-2011 Bastian Eicher
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
using System.Text;

namespace Common.Utils
{
    #region Structures
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

    public static partial class WindowsUtils
    {
        #region COM
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
        private class CShellLink { }
        #endregion

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

        /// <summary>
        /// Informs the Windows shell that changes were made to the file association data in the registry.
        /// </summary>
        /// <remarks>This should be called immediatley after the changes in order to trigger a refresh of the Explorer UI.</remarks>
        public static void NotifyAssocChanged()
        {
            if (!IsWindows) return;

            const uint SHCNE_ASSOCCHANGED = 0x08000000;
            const uint SHCNF_IDLIST = 0;
            UnsafeNativeMethods.SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
        }

        /// <summary>
        /// Informs all GUI applications that changes where made to the environment variables (e.g. PATH) and that they should re-pull them.
        /// </summary>
        public static void NotifyEnvironmentChanged()
        {
            if (!IsWindows) return;

            var HWND_BROADCAST = new IntPtr(0xFFFF);
            const uint WM_SETTINGCHANGE = 0x001A;
            const uint SMTO_ABORTIFHUNG = 0x0002;
            UIntPtr result;
            UnsafeNativeMethods.SendMessageTimeout(HWND_BROADCAST, WM_SETTINGCHANGE, UIntPtr.Zero, "Environment", SMTO_ABORTIFHUNG, 10000, out result);
        }
    }
}
