// VERSION 1.4 | BUILD x.x.x with build excluded.
using System.Reflection;
using System.Resources;

// General Information
[assembly: AssemblyTitle("ModApi")]
[assembly: AssemblyDescription("ModApi v0.2.0 BUILD 176")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("eightyseven")]
[assembly: AssemblyProduct("ModApi")]
[assembly: AssemblyCopyright("eightyseven Copyright © 2024")]
[assembly: NeutralResourcesLanguage("en-AU")]

// Version information
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("0.2.0.176")]

namespace eightyseven.ModApi
{
    
    /// <summary>
    /// Represents the version info for ModApi
    /// </summary>
    public class VersionInfo
    {
        /// <summary>
        /// Represents latest release version date. Format: dd:MM:yyyy hh:mm tt
        /// </summary>
	    public static readonly string lastestRelease = "01.11.2024 07:20 PM";

        /// <summary>
        /// Represents current version. (Excluding build number)
        /// </summary>
	    public static readonly string version = "0.2.0";
        /// <summary>
        /// Represents current (constant) version. (Excluding build number)
        /// </summary>
	    public const string VERSION = "0.2.0";

        /// <summary>
        /// Represents current full version . (including build number)
        /// </summary>
	    public static readonly string fullVersion = "0.2.0.176";
        /// <summary>
        /// Represents current (constant) full version . (including build number)
        /// </summary>
	    public const string FULL_VERSION = "0.2.0.176";
        
        /// <summary>
        /// Represents current build number. (excludes major, minor and revision numbers)
        /// </summary>
	    public static readonly string build = "176";
        /// <summary>
        /// Represents current (const) build number. (excludes major, minor and revision numbers)
        /// </summary>
	    public const string BUILD = "176";
        
        /// <summary>
        /// Represents if the mod has been complied for x64
        /// </summary>
        #if x64
            internal static readonly bool is64bit = true;
        #else
            internal static readonly bool is64bit = false;
        #endif
        /// <summary>
        /// Represents if the mod has been complied for x64
        /// </summary>
        #if x64
            internal const bool IS_64_BIT = true;
        #else
            internal const bool IS_64_BIT = false;
        #endif
        
        /// <summary>
        /// Represents if the mod has been complied in Debug mode
        /// </summary>
        #if DEBUG
            internal static readonly bool isDebugConfig = true;
        #else
            internal static readonly bool isDebugConfig = false;
        #endif
        /// <summary>
        /// Represents if the mod has been complied in Debug mode
        /// </summary>
        #if DEBUG
            internal const bool IS_DEBUG_CONFIG = true;
        #else
            internal const bool IS_DEBUG_CONFIG = false;
        #endif
    }
}
