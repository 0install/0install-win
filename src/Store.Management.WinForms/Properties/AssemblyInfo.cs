using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security.Permissions;

// Assembly info
[assembly: AssemblyTitle("Zero Install Store Management WinForms GUI")]
[assembly: AssemblyDescription("Manages caches of Zero Install implementations via a WinForms GUI.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("0install.de")]
[assembly: AssemblyProduct("Zero Install")]
[assembly: AssemblyCopyright("Copyright 2010-2011 Bastian Eicher")]
[assembly: NeutralResourcesLanguage("en")]

// Version information
[assembly: AssemblyVersion("1.1.1")]
[assembly: AssemblyFileVersion("1.1.1")]

// Security settings
[assembly: FileIOPermission(SecurityAction.RequestMinimum, Unrestricted = true)]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, UnmanagedCode = true)]
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
