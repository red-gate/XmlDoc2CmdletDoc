# XmlDoc2CmdletDoc

XmlDoc2CmdletDoc is a tool that can create a .dll-Help.xml help file for a binary PowerShell module, given the binary module and its corresponding .xml doc comments file. This allows you to keep the documentation of the cmdlets close to the source code for those cmdlets, helping to minimize the risk of them becoming out of sync.

XmlDoc2CmdletDoc has a handful of NuGet package dependencies, one of which is not available via the official public NuGet repository - RedGate.ThirdParty.JoltCore. Nonetheless, the NuGet packages are already included in this repository, and the source for RedGate.ThirdParty.JoltCore is publicly available at https://github.com/red-gate/JoltNet-core. Both Jolt.NET and XmlDoc2CmdletDoc are released under the same BSD licence.

Here are some examples of how to document your cmdlets:

## Cmdlet synopsis and description

The synopsis and description for a cmdlet are defined using \<para\> elements in the XML doc comment for the cmdlet class. The relevant \<para\> elements should be tagged with a *type="synopsis"* or *type="description"* attribute indicating whether the \<para\> is part of the synopsis or description. Multiple \<para\> elements can be used for both the synopsis and the description, though it's customary for a cmdlet synopsis to be a brief one line explanation of the cmdlet.

```
/// <summary>
/// <para type="synopsis">This is the cmdlet synopsis.</para>
/// <para type="description">This is part of the cmdlet description.</para>
/// <para type="description">This is also part of the cmdlet description.</para>
/// </summary>
[Cmdlet("Test", "MyExample")]
public class TestMyExampleCommand : Cmdlet
{
    ...
}
```

## Parameter description

The description for the parameter of a cmdlet is defined using \<para\> elements in the XML doc comment for the parameter's field or property. The relevant \<para\> elements should be tagged with a *type="description"* attribute.

```
[Cmdlet("Test", "MyExample")]
public class TestMyExampleCommand : Cmdlet
{
    /// <summary>
    /// <para type="description">This is part of the parameter description.</para>
    /// <para type="description">This is also part of the parameter description.</para>
    /// </summary>
    public string MyParameter {get; set;}
    
    ...
}
```

