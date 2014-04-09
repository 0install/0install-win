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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NanoByte.Common.Utils
{
    static partial class WindowsUtils
    {
        #region Structures
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct PropertyKey
        {
            Guid formatID;
            Int32 propertyID;

            public PropertyKey(Guid formatID, int propertyID)
            {
                this.formatID = formatID;
                this.propertyID = propertyID;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PropertyVariant : IDisposable
        {
            ushort valueType;
            ushort wReserved1, wReserved2, wReserved3;
            IntPtr valueData;
            Int32 valueDataExt;

            public PropertyVariant(string value)
            {
                valueType = (ushort)VarEnum.VT_LPWSTR;
                wReserved1 = 0;
                wReserved2 = 0;
                wReserved3 = 0;
                valueData = Marshal.StringToCoTaskMemUni(value);
                valueDataExt = 0;
            }

            public void Dispose()
            {
                PropertyVariant var = this;
                UnsafeNativeMethods.PropVariantClear(ref var);

                valueType = (ushort)VarEnum.VT_EMPTY;
                wReserved1 = wReserved2 = wReserved3 = 0;
                valueData = IntPtr.Zero;
                valueDataExt = 0;
            }
        }
        #endregion

        #region COM
        private const string PropertyStoreGuid = "886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99";

        [ComImport, Guid(PropertyStoreGuid), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IPropertyStore
        {
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetCount([Out] out uint cProps);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetAt([In] uint iProp, out PropertyKey pkey);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetValue([In] ref PropertyKey key, out PropertyVariant pv);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), PreserveSig]
            [return: MarshalAs(UnmanagedType.I4)]
            int SetValue([In] ref PropertyKey key, [In] ref PropertyVariant pv);

            [PreserveSig]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint Commit();
        }
        #endregion
        
        /// <summary>
        /// Retrieves the property store for a window.
        /// </summary>
        /// <param name="hwnd">A handle to the window to retrieve the property store for.</param>
        private static IPropertyStore GetWindowPropertyStore(IntPtr hwnd)
        {
            IPropertyStore propStore;
            var guid = new Guid(PropertyStoreGuid);
            int rc = UnsafeNativeMethods.SHGetPropertyStoreForWindow(hwnd, ref guid, out propStore);
            if (rc != 0) throw Marshal.GetExceptionForHR(rc);
            return propStore;
        }

        /// <summary>
        /// Sets a property value.
        /// </summary>
        /// <param name="propertyStore">The property store to set the property in.</param>
        /// <param name="property">The property to set.</param>
        /// <param name="value">The value to set the property to.</param>
        private static void SetPropertyValue(IPropertyStore propertyStore, PropertyKey property, string value)
        {
            var variant = new PropertyVariant(value);
            propertyStore.SetValue(ref property, ref variant);
            variant.Dispose();
        }
    }
}
