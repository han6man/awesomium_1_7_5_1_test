Imports System.Reflection
Imports System.Runtime.InteropServices
Imports Awesomium.Core

' General Information about an assembly is controlled through the following 
' set of attributes. Change these attribute values to modify the information
' associated with an assembly.

' Review the values of the assembly attributes

<Assembly: AssemblyTitle("Awesomium.NET WPF Sample (VB.NET)")> 
<Assembly: AssemblyDescription("Awesomium.NET WPF Sample (VB.NET)")> 
<Assembly: AssemblyCompany(ReleaseInfo.PRODUCT_COMPANY)> 
<Assembly: AssemblyProduct("Awesomium.NET v" & ReleaseInfo.LIBRARY_VERSION)> 
<Assembly: AssemblyCopyright("Copyright " & ReleaseInfo.PRODUCT_COPYRIGHT)> 
<Assembly: AssemblyTrademark(ReleaseInfo.PRODUCT_TRADEMARK)> 
<Assembly: ComVisible(false)>

'In order to begin building localizable applications, set 
'<UICulture>CultureYouAreCodingWith</UICulture> in your .vbproj file
'inside a <PropertyGroup>.  For example, if you are using US English 
'in your source files, set the <UICulture> to "en-US".  Then uncomment the
'NeutralResourceLanguage attribute below.  Update the "en-US" in the line
'below to match the UICulture setting in the project file.

'<Assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.Satellite)> 


'The ThemeInfo attribute describes where any theme specific and generic resource dictionaries can be found.
'1st parameter: where theme specific resource dictionaries are located
'(used if a resource is not found in the page, 
' or application resource dictionaries)

'2nd parameter: where the generic resource dictionary is located
'(used if a resource is not found in the page, 
'app, and any theme specific resource dictionaries)
<Assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)>



'The following GUID is for the ID of the typelib if this project is exposed to COM
<Assembly: Guid("098af4f2-676a-49a9-b4c6-de6118ca6030")> 

' Version information for an assembly consists of the following four values:
'
'      Major Version
'      Minor Version 
'      Build Number
'      Revision
'
' You can specify all the values or you can default the Build and Revision Numbers 
' by using the '*' as shown below:
' <Assembly: AssemblyVersion("1.0.*")> 

<Assembly: AssemblyVersion(ReleaseInfo.ASSEMBLY_VERSION)> 
<Assembly: AssemblyFileVersion(ReleaseInfo.ASSEMBLY_VERSION)> 
