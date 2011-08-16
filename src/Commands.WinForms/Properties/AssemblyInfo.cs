using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security.Permissions;

// Assembly info
[assembly: AssemblyTitle("Zero Install Command WinForms GUI")]
[assembly: AssemblyDescription("A WinForms-based GUI for Zero Install, for installing and launching applications, managing caches, etc.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("0install.de")]
[assembly: AssemblyProduct("Zero Install")]
[assembly: AssemblyCopyright("Copyright 2010-2011 Bastian Eicher")]
[assembly: NeutralResourcesLanguage("en")]

// Version information
[assembly: AssemblyVersion("1.1.2")]
[assembly: AssemblyFileVersion("1.1.2")]

// Security settings
[assembly: FileIOPermission(SecurityAction.RequestMinimum, Unrestricted = true)]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, UnmanagedCode = true)]
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
