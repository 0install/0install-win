using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security.Permissions;

// Assembly info
[assembly: AssemblyTitle("Zero Install Central WinForms GUI")]
[assembly: AssemblyDescription("The main GUI for Zero Install, for discovering and installing new applications, managing and launching installed applications, etc.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCopyright("Copyright 2010-2011 Bastian Eicher")]

// Security settings
[assembly: FileIOPermission(SecurityAction.RequestMinimum, Unrestricted = true)]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, UnmanagedCode = true)]
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
