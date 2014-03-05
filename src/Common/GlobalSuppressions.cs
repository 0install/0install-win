// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the 
// Error List, point to "Suppress Message(s)", and click 
// "In Project Suppression File".
// You do not need to add suppressions to this file manually.

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Scope = "member", Target = "Common.Cli.ProgressBar.#System.IDisposable.Dispose()", Justification = "IDisposable is only implemented here to support using() blocks.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope = "type", Target = "Common.Utils.WindowsUtils+ShellLink", Justification = "Necessary to duplicate visibility issues.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope = "type", Target = "Common.Utils.WindowsUtils+TaskbarProgressBarState", Justification = "Necessary to duplicate visibility issues.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Scope = "member", Target = "Common.Cli.ProgressBar.#System.IDisposable.Dispose()", Justification = "IDisposable is only implemented here to support using() blocks.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly", Scope = "member", Target = "Common.Cli.ProgressBar.#System.IDisposable.Dispose()", Justification = "IDisposable is only implemented here to support using() blocks.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId = "canonicalized", Scope = "resource", Target = "Common.Properties.Resources.resources")]
