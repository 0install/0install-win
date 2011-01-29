using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security.Permissions;

// Assembly info
[assembly: AssemblyTitle("Zero Install Launcher WinForms GUI")]
[assembly: AssemblyDescription("A WinForms-based GUI for Zero Install, for launching applications. A speced down version of 0install-win.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("0install.net")]
[assembly: AssemblyProduct("Zero Install")]
[assembly: AssemblyCopyright("Copyright © Bastian Eicher 2010-2011")]
[assembly: NeutralResourcesLanguage("en")]

// Version information
[assembly: AssemblyVersion("0.51.2")]
[assembly: AssemblyFileVersion("0.51.2")]

// Security settings
[assembly: FileIOPermission(SecurityAction.RequestMinimum, Unrestricted = true)]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, UnmanagedCode = true)]
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
