/*
 * Based on Windows API Code Pack for Microsoft .NET Framework 1.0.1
 * Copyright 2009 Microsoft Corporation
 * License: http://code.msdn.microsoft.com/WindowsAPICodePack/Project/License.aspx
 *
 * Modified for better project integration
 * Copyright 2010 Bastian Eicher
 */

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Common.Utils
{
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
    /// <summary>
    /// HRESULT Wrapper
    /// This is intended for Library Internal use only.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "HRESULT"), SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32", Justification = "The base type for all of these value is uint")]
    internal enum HRESULT : uint
    {
        /// <summary>
        /// S_FALSE
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "FALSE")]
        S_FALSE = 0x0001,

        /// <summary>
        /// S_OK
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "OK")]
        S_OK = 0x0000,

        /// <summary>
        /// E_INVALIDARG
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "INVALIDARG")]
        E_INVALIDARG = 0x80070057,

        /// <summary>
        /// E_OUTOFMEMORY
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "OUTOFMEMORY")]
        E_OUTOFMEMORY = 0x8007000E,

        /// <summary>
        /// E_NOINTERFACE
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "NOINTERFACE")]
        E_NOINTERFACE = 0x80004002,

        /// <summary>
        /// E_FAIL
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "FAIL")]
        E_FAIL = 0x80004005,

        /// <summary>
        /// E_ELEMENTNOTFOUND
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ELEMENTNOTFOUND")]
        E_ELEMENTNOTFOUND = 0x80070490,

        /// <summary>
        /// TYPE_E_ELEMENTNOTFOUND
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "TYPE")]
        TYPE_E_ELEMENTNOTFOUND = 0x8002802B,


        /// <summary>
        /// NO_OBJECT
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "NO_OBJECT")]
        NO_OBJECT = 0x800401E5,

        /// <summary>
        /// Win32 Error code: ERROR_CANCELLED
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ERROR")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "CANCELLED")]
        ERROR_CANCELLED = 1223,

        /// <summary>
        /// ERROR_CANCELLED
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ERROR")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "CANCELLED")]
        E_ERROR_CANCELLED = 0x800704C7,

        /// <summary>
        /// The requested resource is in use
        /// </summary>
        RESOURCE_IN_USE = 0x800700AA,
    }

    [Flags]
    internal enum TBPFLAG
    {
        TBPF_NOPROGRESS = 0,
        TBPF_INDETERMINATE = 0x1,
        TBPF_NORMAL = 0x2,
        TBPF_ERROR = 0x4,
        TBPF_PAUSED = 0x8
    }

    internal enum THBMASK
    {
        THB_BITMAP = 0x1,
        THB_ICON = 0x2,
        THB_TOOLTIP = 0x4,
        THB_FLAGS = 0x8
    }

    [Flags]
    internal enum THBFLAGS
    {
        THBF_ENABLED = 0x00000000,
        THBF_DISABLED = 0x00000001,
        THBF_DISMISSONCLICK = 0x00000002,
        THBF_NOBACKGROUND = 0x00000004,
        THBF_HIDDEN = 0x00000008,
        THBF_NONINTERACTIVE = 0x00000010
    }

    [Flags]
    internal enum STPFLAG
    {
        STPF_NONE = 0x0,
        STPF_USEAPPTHUMBNAILALWAYS = 0x1,
        STPF_USEAPPTHUMBNAILWHENACTIVE = 0x2,
        STPF_USEAPPPEEKALWAYS = 0x4,
        STPF_USEAPPPEEKWHENACTIVE = 0x8
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal struct THUMBBUTTON
    {
        /// <summary>
        /// WPARAM value for a THUMBBUTTON being clicked.
        /// </summary>
        internal const int THBN_CLICKED = 0x1800;

        [MarshalAs(UnmanagedType.U4)]
        internal THBMASK dwMask;
        internal uint iId;
        internal uint iBitmap;
        internal IntPtr hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        internal string szTip;
        [MarshalAs(UnmanagedType.U4)]
        internal THBFLAGS dwFlags;
    }

    [ComImportAttribute]
    [GuidAttribute("c43dc798-95d1-4bea-9030-bb99e2983a1a")]
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ITaskbarList4
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
        void MarkFullscreenWindow(
            IntPtr hwnd,
            [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);

        // ITaskbarList3
        [PreserveSig]
        void SetProgressValue(IntPtr hwnd, UInt64 ullCompleted, UInt64 ullTotal);
        [PreserveSig]
        void SetProgressState(IntPtr hwnd, TBPFLAG tbpFlags);
        [PreserveSig]
        void RegisterTab(IntPtr hwndTab, IntPtr hwndMDI);
        [PreserveSig]
        void UnregisterTab(IntPtr hwndTab);
        [PreserveSig]
        void SetTabOrder(IntPtr hwndTab, IntPtr hwndInsertBefore);
        [PreserveSig]
        void SetTabActive(IntPtr hwndTab, IntPtr hwndInsertBefore, uint dwReserved);
        [PreserveSig]
        HRESULT ThumbBarAddButtons(
            IntPtr hwnd,
            uint cButtons,
            [MarshalAs(UnmanagedType.LPArray)] THUMBBUTTON[] pButtons);
        [PreserveSig]
        HRESULT ThumbBarUpdateButtons(
            IntPtr hwnd,
            uint cButtons,
            [MarshalAs(UnmanagedType.LPArray)] THUMBBUTTON[] pButtons);
        [PreserveSig]
        void ThumbBarSetImageList(IntPtr hwnd, IntPtr himl);
        [PreserveSig]
        void SetOverlayIcon(
          IntPtr hwnd,
          IntPtr hIcon,
          [MarshalAs(UnmanagedType.LPWStr)] string pszDescription);
        [PreserveSig]
        void SetThumbnailTooltip(
            IntPtr hwnd,
            [MarshalAs(UnmanagedType.LPWStr)] string pszTip);
        [PreserveSig]
        void SetThumbnailClip(
            IntPtr hwnd,
            IntPtr prcClip);

        // ITaskbarList4
        void SetTabProperties(IntPtr hwndTab, STPFLAG stpFlags);
    }

    [GuidAttribute("56FDF344-FD6D-11d0-958A-006097C9A090")]
    [ClassInterfaceAttribute(ClassInterfaceType.None)]
    [ComImportAttribute]
    internal class CTaskbarList { }
// ReSharper restore InconsistentNaming
// ReSharper restore UnusedMember.Global
}
