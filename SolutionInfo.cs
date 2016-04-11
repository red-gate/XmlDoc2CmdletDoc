using System.Reflection;
using System.Runtime.InteropServices;

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: AssemblyCompany("Redgate")]
[assembly: AssemblyProduct("XmlDoc2CmdletDoc")]
[assembly: AssemblyCopyright("Copyright © 2014-2016, Red Gate Software Ltd.")]

[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]

// The following attributes are automatically set by the build scripts. Please do not modify them manually.
// Instead, change the value defined in version-number.txt.
[assembly: AssemblyVersion("0.2.0")]
[assembly: AssemblyFileVersion("0.2.0")]
[assembly: AssemblyInformationalVersion("0.2.0-build-scripts")]
