using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security.Permissions;

// Assembly info
[assembly: AssemblyTitle("Zero Install Store Management CLI")]
[assembly: AssemblyDescription("Manages caches of Zero Install implementations via the command-line.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCopyright("Copyright 2010-2011 Bastian Eicher")]

// Security settings
[assembly: FileIOPermission(SecurityAction.RequestMinimum, Unrestricted = true)]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, UnmanagedCode = true)]
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
