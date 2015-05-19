﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.0
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WebSocket.Portable.Resources {
    using System;
    using System.Reflection;
    
    
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
    internal class ErrorMessages {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ErrorMessages() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("WebSocket.Portable.Resources.ErrorMessages", typeof(ErrorMessages).GetTypeInfo().Assembly);
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
        ///   Looks up a localized string similar to A non data frame is compressed..
        /// </summary>
        internal static string CompressedNonDataFrame {
            get {
                return ResourceManager.GetString("CompressedNonDataFrame", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An web socket extension with the same name is already registered:.
        /// </summary>
        internal static string ExtensionsAlreadyRegistered {
            get {
                return ResourceManager.GetString("ExtensionsAlreadyRegistered", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A control frame is fragmented..
        /// </summary>
        internal static string FragmentedControlFrame {
            get {
                return ResourceManager.GetString("FragmentedControlFrame", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid response line:.
        /// </summary>
        internal static string InvalidResponseLine {
            get {
                return ResourceManager.GetString("InvalidResponseLine", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid scheme:.
        /// </summary>
        internal static string InvalidScheme {
            get {
                return ResourceManager.GetString("InvalidScheme", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid state:.
        /// </summary>
        internal static string InvalidState {
            get {
                return ResourceManager.GetString("InvalidState", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Must not be null or empty..
        /// </summary>
        internal static string MustNotBeNullOrEmpty {
            get {
                return ResourceManager.GetString("MustNotBeNullOrEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Must not contain a fragment..
        /// </summary>
        internal static string MustNotContainAFragment {
            get {
                return ResourceManager.GetString("MustNotContainAFragment", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No header lines..
        /// </summary>
        internal static string NoHeaderLines {
            get {
                return ResourceManager.GetString("NoHeaderLines", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Not an absolute uri..
        /// </summary>
        internal static string NotAnAbsoluteUri {
            get {
                return ResourceManager.GetString("NotAnAbsoluteUri", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The payload data length of a control frame is greater than 125 bytes..
        /// </summary>
        internal static string PayloadLengthControlFrame {
            get {
                return ResourceManager.GetString("PayloadLengthControlFrame", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Secure connections are not yet supported..
        /// </summary>
        internal static string SecureConnectionsAreNotYetSupported {
            get {
                return ResourceManager.GetString("SecureConnectionsAreNotYetSupported", resourceCulture);
            }
        }
    }
}
