using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security.Permissions;

// Assembly info
[assembly: AssemblyTitle("Zero Install Central GTK# GUI")]
[assembly: AssemblyDescription("A GTK#-based GUI for Zero Install, for discovering and installing new applications, managing and launching installed applications, etc.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCopyright("Copyright © Bastian Eicher 2010")]

// Security settings
[assembly: FileIOPermission(SecurityAction.RequestMinimum, Unrestricted = true)]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, UnmanagedCode = true)]
[assembly: CLSCompliant(false)]
[assembly: ComVisible(false)]
