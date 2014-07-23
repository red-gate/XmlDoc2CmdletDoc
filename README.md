# XmlDoc2CmdletDoc

XmlDoc2CmdletDoc is a tool that can create a .dll-Help.xml help file for a binary PowerShell module, given the binary module and its corresponding .xml doc comments file. This allows you to keep the documentation of the cmdlets close to the source code for those cmdlets, helping to minimize the risk of them becoming out of sync.

XmlDoc2CmdletDoc has a handful of NuGet package dependencies, one of which is not available via the official public NuGet repository - RedGate.ThirdParty.JoltCore. Nonetheless, the NuGet packages are already included in this repository, and the source for RedGate.ThirdParty.JoltCore is publicly available at https://github.com/red-gate/JoltNet-core. Both Jolt.NET and XmlDoc2CmdletDoc are released under the same BSD licence.
