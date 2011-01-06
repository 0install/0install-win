using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security.Permissions;

// Assembly info
[assembly: AssemblyTitle("Zero Install Launcher CLI")]
[assembly: AssemblyDescription("Launches Zero Install implementations and displays a command-line interface.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("0install.net")]
[assembly: AssemblyProduct("Zero Install")]
[assembly: AssemblyCopyright("Copyright © Bastian Eicher 2010")]
[assembly: NeutralResourcesLanguage("en")]

// Version information
[assembly: AssemblyVersion("0.51.1")]
[assembly: AssemblyFileVersion("0.51.1")]

// Security settings
[assembly: FileIOPermission(SecurityAction.RequestMinimum, Unrestricted = true)]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, UnmanagedCode = true)]
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
