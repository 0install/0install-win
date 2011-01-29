﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ZeroInstall.Injector.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("ZeroInstall.Injector.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The feed {0} could not be located in the interface cache. Looked for file at: {1}.
        /// </summary>
        internal static string FeedNotInCache {
            get {
                return ResourceManager.GetString("FeedNotInCache", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The interface &apos;{0}&apos; doesn&apos;t start with &apos;http(s):&apos; and isn&apos;t a an absolute local path either..
        /// </summary>
        internal static string InvalidInterfaceID {
            get {
                return ResourceManager.GetString("InvalidInterfaceID", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No interface was specified..
        /// </summary>
        internal static string MissingInterfaceID {
            get {
                return ResourceManager.GetString("MissingInterfaceID", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Missing / after hostname in URI &apos;{0}&apos;..
        /// </summary>
        internal static string MissingSlashInUri {
            get {
                return ResourceManager.GetString("MissingSlashInUri", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No commands in the selection..
        /// </summary>
        internal static string NoCommands {
            get {
                return ResourceManager.GetString("NoCommands", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to At least one implementation must be passed..
        /// </summary>
        internal static string NoImplementationsPassed {
            get {
                return ResourceManager.GetString("NoImplementationsPassed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The interface &apos;{0}&apos; doesn&apos;t start with &apos;http:&apos; and doesn&apos;t exist as a file &apos;{1}&apos; either..
        /// </summary>
        internal static string NotInterfaceID {
            get {
                return ResourceManager.GetString("NotInterfaceID", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Parse hasn&apos;t been called yet..
        /// </summary>
        internal static string NotParsed {
            get {
                return ResourceManager.GetString("NotParsed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The value is not a valid domain name..
        /// </summary>
        internal static string NotValidDomain {
            get {
                return ResourceManager.GetString("NotValidDomain", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Run in batch mode: don&apos;t display any progress reports to the user and silently answer all questions with &quot;No&quot;..
        /// </summary>
        internal static string OptionBatch {
            get {
                return ResourceManager.GetString("OptionBatch", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Chosen implementation&apos;s version number must be earlier than {VERSION}. i.e., force the use of an old version the program..
        /// </summary>
        internal static string OptionBefore {
            get {
                return ResourceManager.GetString("OptionBefore", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Instead of executing the default command, use {COMMAND} instead. Possible command names are defined in the program&apos;s interface..
        /// </summary>
        internal static string OptionCommand {
            get {
                return ResourceManager.GetString("OptionCommand", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Forces the solver to target the CPU {CPU}.
        ///Supported values: i386, i486, i586, i686, x86_64, ppc, ppc64.
        /// </summary>
        internal static string OptionCpu {
            get {
                return ResourceManager.GetString("OptionCpu", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Show the built-in help text..
        /// </summary>
        internal static string OptionHelp {
            get {
                return ResourceManager.GetString("OptionHelp", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Run the specified executable {MAIN} instead of the default. If it starts with &apos;/&apos; or &apos;\&apos; then the path is relative to the implementation&apos;s top-level directory, whereas otherwise it is relative to the directory containing the default main program..
        /// </summary>
        internal static string OptionMain {
            get {
                return ResourceManager.GetString("OptionMain", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Chosen implementation&apos;s version number must not be earlier than {VERSION}. E.g., if you want to run version 2.0 or later, use --not-before=2.0..
        /// </summary>
        internal static string OptionNotBefore {
            get {
                return ResourceManager.GetString("OptionNotBefore", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Immediately returns once the chosen program has been launched instead of waiting for it to finish executing..
        /// </summary>
        internal static string OptionNoWait {
            get {
                return ResourceManager.GetString("OptionNoWait", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Run in off-line mode, overriding the default setting. In off-line mode, no interfaces are refreshed even if they are out-of-date, and newer versions of programs won&apos;t be downloaded even if the injector already knows about them (e.g. from a previous refresh)..
        /// </summary>
        internal static string OptionOffline {
            get {
                return ResourceManager.GetString("OptionOffline", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Forces the solver to target the operating system {OS}.
        ///Supported values: Linux, Solaris, MacOSX, Windows.
        /// </summary>
        internal static string OptionOS {
            get {
                return ResourceManager.GetString("OptionOS", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Fetch a fresh copy of all used interfaces..
        /// </summary>
        internal static string OptionRefresh {
            get {
                return ResourceManager.GetString("OptionRefresh", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Options:.
        /// </summary>
        internal static string Options {
            get {
                return ResourceManager.GetString("Options", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The implementation of the main (root) interface must have an architecture of the form &apos;*-src&apos; (normally a literal &quot;*&quot;, but could be a compatible OS). Dependencies are normal implementations, not source ones. See 0compile for details..
        /// </summary>
        internal static string OptionSource {
            get {
                return ResourceManager.GetString("OptionSource", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to More verbose output. Use twice for even more verbose output..
        /// </summary>
        internal static string OptionVerbose {
            get {
                return ResourceManager.GetString("OptionVerbose", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Display version information..
        /// </summary>
        internal static string OptionVersion {
            get {
                return ResourceManager.GetString("OptionVersion", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Add {DIR} to the list of implementation caches to search.
        ///However, new downloads will not be written to this directory..
        /// </summary>
        internal static string OptionWithStore {
            get {
                return ResourceManager.GetString("OptionWithStore", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Instead of executing the chosen program directly, run {COMMAND} PROGRAM ARGS. This is useful for running debuggers and tracing tools on the program (rather than on Zero Install!). Note that the wrapper is executed in the environment selected by the program; hence, this mechanism cannot be used for sandboxing..
        /// </summary>
        internal static string OptionWrapper {
            get {
                return ResourceManager.GetString("OptionWrapper", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The output of the Python solver could not be processed..
        /// </summary>
        internal static string PythonSolverOutputErrror {
            get {
                return ResourceManager.GetString("PythonSolverOutputErrror", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The solver encountered an unexpected problem..
        /// </summary>
        internal static string SolverProblem {
            get {
                return ResourceManager.GetString("SolverProblem", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unknown option..
        /// </summary>
        internal static string UnknownOption {
            get {
                return ResourceManager.GetString("UnknownOption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Usage:.
        /// </summary>
        internal static string Usage {
            get {
                return ResourceManager.GetString("Usage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The working directory has already been changed by a previous command..
        /// </summary>
        internal static string WokringDirDuplicate {
            get {
                return ResourceManager.GetString("WokringDirDuplicate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The working directory contains an invalid paths (potentially a security risk)..
        /// </summary>
        internal static string WorkingDirInvalidPath {
            get {
                return ResourceManager.GetString("WorkingDirInvalidPath", resourceCulture);
            }
        }
    }
}
