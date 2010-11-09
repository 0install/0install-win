using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security.Permissions;

// Assembly info
[assembly: AssemblyTitle("Zero Install Central WinForms GUI")]
[assembly: AssemblyDescription("A WinForms-based GUI for Zero Install, for discovering and installing new applications, managing and launching installed applications, etc.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("0install.net")]
[assembly: AssemblyProduct("Zero Install")]
[assembly: AssemblyCopyright("Copyright © Bastian Eicher 2010")]
[assembly: NeutralResourcesLanguage("en")]

// Version information
[assembly: AssemblyVersion("0.50.2")]
[assembly: AssemblyFileVersion("0.50.2")]

// Security settings
[assembly: FileIOPermission(SecurityAction.RequestMinimum, Unrestricted = true)]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, UnmanagedCode = true)]
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
