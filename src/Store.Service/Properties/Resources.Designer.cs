﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18010
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ZeroInstall.Store.Service.Properties {
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("ZeroInstall.Store.Service.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to The user &apos;{0}&apos; failed to add the implementation &apos;{1}&apos; to &apos;{2}&apos;..
        /// </summary>
        internal static string FailedToAddImplementation {
            get {
                return ResourceManager.GetString("FailedToAddImplementation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to create a temporary directory for the user &apos;{0}&apos; in &apos;{1}&apos;..
        /// </summary>
        internal static string FailedToCreateTempDir {
            get {
                return ResourceManager.GetString("FailedToCreateTempDir", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The user &apos;{0}&apos; failed to removed the implementation &apos;{1}&apos; from &apos;{2}&apos;..
        /// </summary>
        internal static string FailedToRemoveImplementation {
            get {
                return ResourceManager.GetString("FailedToRemoveImplementation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to remove the temporary directory &apos;{1}&apos; (created for the user &apos;{0}&apos;)..
        /// </summary>
        internal static string FailedToRemoveTempDir {
            get {
                return ResourceManager.GetString("FailedToRemoveTempDir", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to install the Zero Install Store Service. See log file in installation directory for details..
        /// </summary>
        internal static string InstallFail {
            get {
                return ResourceManager.GetString("InstallFail", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Successfully installed the Zero Install Store Service..
        /// </summary>
        internal static string InstallSuccess {
            get {
                return ResourceManager.GetString("InstallSuccess", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Only administrators can remove elements from a shared store..
        /// </summary>
        internal static string MustBeAdminToRemove {
            get {
                return ResourceManager.GetString("MustBeAdminToRemove", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The service cannot run in portable mode!.
        /// </summary>
        internal static string NoPortableMode {
            get {
                return ResourceManager.GetString("NoPortableMode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Successfully started the Zero Install Store Service..
        /// </summary>
        internal static string StartSuccess {
            get {
                return ResourceManager.GetString("StartSuccess", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The Zero Install Store Service is runnig..
        /// </summary>
        internal static string StatusRunning {
            get {
                return ResourceManager.GetString("StatusRunning", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The Zero Install Store Service is not runnig..
        /// </summary>
        internal static string StatusStopped {
            get {
                return ResourceManager.GetString("StatusStopped", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Successfully stopped the Zero Install Store Service..
        /// </summary>
        internal static string StopSuccess {
            get {
                return ResourceManager.GetString("StopSuccess", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The user &apos;{0}&apos; successfully added the implementation &apos;{1}&apos; to &apos;{2}&apos;..
        /// </summary>
        internal static string SuccessfullyAddedImplementation {
            get {
                return ResourceManager.GetString("SuccessfullyAddedImplementation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The user &apos;{0}&apos; successfully removed the implementation &apos;{1}&apos; from &apos;{2}&apos;..
        /// </summary>
        internal static string SuccessfullyRemovedImplementation {
            get {
                return ResourceManager.GetString("SuccessfullyRemovedImplementation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to uninstall the Zero Install Store Service. See log file in installation directory for details..
        /// </summary>
        internal static string UninstallFail {
            get {
                return ResourceManager.GetString("UninstallFail", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Successfully uninstalled the Zero Install Store Service..
        /// </summary>
        internal static string UninstallSuccess {
            get {
                return ResourceManager.GetString("UninstallSuccess", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unknown command!
        ///Usage: {0}.
        /// </summary>
        internal static string UnkownCommand {
            get {
                return ResourceManager.GetString("UnkownCommand", resourceCulture);
            }
        }
    }
}
