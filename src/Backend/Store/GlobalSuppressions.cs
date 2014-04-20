// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the 
// Error List, point to "Suppress Message(s)", and click 
// "In Project Suppression File".
// You do not need to add suppressions to this file manually.

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Interface", Scope = "namespace", Target = "ZeroInstall.Store.Feeds")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1701:ResourceStringCompoundWordsShouldBeCasedCorrectly", MessageId = "newline", Scope = "resource", Target = "ZeroInstall.Store.Properties.Resources.resources")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId = "gz", Scope = "resource", Target = "ZeroInstall.Store.Properties.Resources.resources")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId = "os-cpu", Scope = "resource", Target = "ZeroInstall.Store.Properties.Resources.resources")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Scope = "member", Target = "ZeroInstall.Store.Implementations.Archives.MicrosoftExtractor.#Microsoft.Deployment.Compression.IUnpackStreamContext.OpenArchiveReadStream(System.Int32,System.String,Microsoft.Deployment.Compression.CompressionEngine)", Justification = "Interface only provides callbacks for external library.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Scope = "member", Target = "ZeroInstall.Store.Implementations.Archives.MicrosoftExtractor.#Microsoft.Deployment.Compression.IUnpackStreamContext.CloseArchiveReadStream(System.Int32,System.String,System.IO.Stream)", Justification = "Interface only provides callbacks for external library.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Scope = "member", Target = "ZeroInstall.Store.Implementations.Archives.MicrosoftExtractor.#Microsoft.Deployment.Compression.IUnpackStreamContext.CloseFileWriteStream(System.String,System.IO.Stream,System.IO.FileAttributes,System.DateTime)", Justification = "Interface only provides callbacks for external library.")]
