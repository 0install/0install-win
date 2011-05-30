using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security.Permissions;

// Assembly info
[assembly: AssemblyTitle("Zero Install Store Service")]
[assembly: AssemblyDescription("Windows service for managing the secure shared cache of Zero Install implementations.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("0install.de")]
[assembly: AssemblyProduct("Zero Install")]
[assembly: AssemblyCopyright("Copyright © Bastian Eicher 2010-2011")]
[assembly: NeutralResourcesLanguage("en")]

// Version information
[assembly: AssemblyVersion("0.54.4")]
[assembly: AssemblyFileVersion("0.54.4")]

// Security settings
[assembly: FileIOPermission(SecurityAction.RequestMinimum, Unrestricted = true)]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, UnmanagedCode = true)]
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
