using Awesomium.Core;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle( "BasicAsyncSample" )]
[assembly: AssemblyDescription( "Awesomium.NET Basic Async Programming Sample (CSharp)" )]
[assembly: AssemblyConfiguration( "" )]
[assembly: AssemblyCompany( ReleaseInfo.PRODUCT_COMPANY )]
[assembly: AssemblyProduct( "Awesomium.NET v" + ReleaseInfo.LIBRARY_VERSION )]
[assembly: AssemblyCopyright( "Copyright " + ReleaseInfo.PRODUCT_COPYRIGHT )]
[assembly: AssemblyTrademark( ReleaseInfo.PRODUCT_TRADEMARK )]
[assembly: AssemblyCulture( "" )]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible( false )]

// Code in this application is sure to never attempt to make
// illegal cross-thread calls to Awesomium API. Bypass thread
// affinity checks performed by Awesomium.NET.
[assembly: ThreadAffinityEnsured]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid( "f89f76bf-60ca-4356-b380-cde2bedf799a" )]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion( ReleaseInfo.ASSEMBLY_VERSION )]
[assembly: AssemblyFileVersion( ReleaseInfo.ASSEMBLY_VERSION )]

class AssemblyInfo
{
    #region Assembly Attribute Accessors
    public static string AssemblyTitle
    {
        get
        {
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes( typeof( AssemblyTitleAttribute ), false );
            if ( attributes.Length > 0 )
            {
                AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[ 0 ];
                if ( titleAttribute.Title != "" )
                    return titleAttribute.Title;
            }
            return Path.GetFileNameWithoutExtension( Assembly.GetExecutingAssembly().CodeBase );
        }
    }

    public static string AssemblyVersion
    {
        get
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }

    public static string AssemblyDescription
    {
        get
        {
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes( typeof( AssemblyDescriptionAttribute ), false );
            if ( attributes.Length == 0 )
                return String.Empty;
            return ( (AssemblyDescriptionAttribute)attributes[ 0 ] ).Description;
        }
    }

    public static string AssemblyProduct
    {
        get
        {
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes( typeof( AssemblyProductAttribute ), false );
            if ( attributes.Length == 0 )
                return String.Empty;
            return ( (AssemblyProductAttribute)attributes[ 0 ] ).Product;
        }
    }

    public static string AssemblyCopyright
    {
        get
        {
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes( typeof( AssemblyCopyrightAttribute ), false );
            if ( attributes.Length == 0 )
                return String.Empty;
            return ( (AssemblyCopyrightAttribute)attributes[ 0 ] ).Copyright;
        }
    }

    public static string AssemblyCompany
    {
        get
        {
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes( typeof( AssemblyCompanyAttribute ), false );
            if ( attributes.Length == 0 )
                return String.Empty;
            return ( (AssemblyCompanyAttribute)attributes[ 0 ] ).Company;
        }
    }
    #endregion
}