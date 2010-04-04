using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security.Permissions;

// Assembly info
[assembly: AssemblyTitle("Common functions")]
[assembly: AssemblyDescription("Common functions used by desktop applications.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("NanoByte")]
[assembly: AssemblyProduct("OmegaEngine helper functions")]
[assembly: AssemblyCopyright("Copyright 2006-2010 Bastian Eicher")]
[assembly: NeutralResourcesLanguage("en")]

// Always default version
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

// Security settings
[assembly: FileIOPermission(SecurityAction.RequestMinimum, Unrestricted = true)]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, UnmanagedCode = true)]
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
