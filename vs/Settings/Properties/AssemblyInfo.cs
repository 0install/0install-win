using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security.Permissions;

// Assembly info
[assembly: AssemblyTitle("Epic Evolution Settings")]
[assembly: AssemblyDescription("Stores settings for Epic Evolution and AlphaEditor in an XML file.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("NanoByte")]
[assembly: AssemblyProduct("Epic Evolution")]
[assembly: AssemblyCopyright("Copyright 2006-2010 Bastian Eicher")]
[assembly: NeutralResourcesLanguage("en")]

// Game Version
[assembly: AssemblyVersion("0.5.0")]
[assembly: AssemblyFileVersion("0.5.0")]

// Security settings
[assembly: FileIOPermission(SecurityAction.RequestMinimum, Unrestricted = true)]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, UnmanagedCode = true)]
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
