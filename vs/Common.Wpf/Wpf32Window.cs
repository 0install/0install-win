using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Interop;
using System.Windows;

namespace Common.Wpf
{
    public class Wpf32Window : System.Windows.Forms.IWin32Window
    {
        public IntPtr Handle { get; private set; }

        public Wpf32Window(Window wpfWindow)
        {
            Handle = new WindowInteropHelper(wpfWindow).Handle;
        }
    }
}
