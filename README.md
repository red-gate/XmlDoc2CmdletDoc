# XmlDoc2CmdletDoc

It's easy to write good help documentation for PowerShell *script* modules (those written in the PowerShell script language). You just write specially formatted comments alongside the source code for your cmdlets, and the PowerShell host automatically uses those comments to provide good inline help for your cmdlets' users. **XmlDoc2CmdletDoc** brings this same functionality to PowerShell *binary* modules (those written in C# or VB.NET). You no longer need to use *CmdletHelpEditor* or *PowerShell Cmdlet Help Editor* to manually edit a separate help file. Instead, this tool will automatically generate your PowerShell module's help file from XML Doc comments in your source code.

For more details, [Michael Sorens](https://www.simple-talk.com/author/michael-sorens/) has written a [comprehensive guide to documenting your PowerShell binary cmdlets](https://www.simple-talk.com/dotnet/software-tools/documenting-your-powershell-binary-cmdlets/) using XmlDoc2CmdletDoc.

To create a .dll-Help.xml file for your binary PowerShell module, simply call:

```batchfile
XmlDoc2CmdletDoc.exe C:\Full\Path\To\MyPowerShellModule.dll
```

Here are some examples of how to document your cmdlets:


## Cmdlet synopsis and description

The cmdlet's synopsis and description are defined using `<para>` elements in the cmdlet class's XML doc comment. Tag the `<para>` elements with a `type="synopsis"` or `type="description"` attribute, showing whether `<para>` is part of the synopsis or description. 

You can use multiple `<para>` elements for both the synopsis and the description, but a cmdlet synopsis is usually just one sentence.

```c#
/// <summary>
/// <para type="synopsis">This is the cmdlet synopsis.</para>
/// <para type="description">This is part of the longer cmdlet description.</para>
/// <para type="description">This is also part of the longer cmdlet description.</para>
/// </summary>
[Cmdlet("Test", "MyExample")]
public class TestMyExampleCommand : Cmdlet
{
    ...
}
```

For guidance on writing the cmdlet synopsis, see http://msdn.microsoft.com/en-us/library/bb525429.aspx.
For guidance on writing the cmdlet description, see http://msdn.microsoft.com/en-us/library/bb736332.aspx.


## Parameter description

The description for a cmdlet parameter is defined using `<para>` elements in the XML doc comment for the parameter's field or property. Tag the `<para>` elements with a `type="description"` attribute.

```c#
[Cmdlet("Test", "MyExample")]
public class TestMyExampleCommand : Cmdlet
{
    /// <summary>
    /// <para type="description">This is part of the parameter description.</para>
    /// <para type="description">This is also part of the parameter description.</para>
    /// </summary>
    [Parameter]
    public string MyParameter {get; set;}
    
    ...
}

```

For guidance on writing the parameter description, see http://msdn.microsoft.com/en-us/library/bb736339.aspx.


## Type description

You can document a parameter's input type or a cmdlet's output type, using `<para>` elements in the type's XML doc comment. As before, tag the `<para>` elements with a `type="description"` attribute. 

You can only document types defined in the PowerShell module like this.

```c#
[Cmdlet("Test", "MyExample")]
public class TestMyExampleCommand : Cmdlet
{
    [Parameter]
    public MyType MyParameter {get; set;}
    
    ...
}

/// <summary>
/// <para type="description">This is part of the type description.</para>
/// <para type="description">This is also part of the type description.</para>
/// </summary>
public class MyType
{
    ...
}
```


## Notes

You can add notes to a cmdlet's help section using a `<list>` element with a `type="alertSet"` attribute. Each `<item>` sub-element corresponds to a single note. 

Inside each `<item>` element, specify the note's title with the `<term>` sub-element, and the note's body text with the `<description>` sub-element. The `<description>` element can directly contain the note's body text, or you can split the note's body text into multiple paragraphs, using `<para>` elements.

```c#
/// <list type="alertSet">
///   <item>
///     <term>First note title</term>
///     <description>
///     This is the entire body text for the first note.
///     </description>
///   </item>
///   <item>
///     <term>Second note title</term>
///     <description>
///       <para>The first paragraph of the body text for the second note.</para>
///       <para>The second paragraph of the body text for the second note.</para>
///     </description>
///   </item>
/// </list>
[Cmdlet("Test", "MyExample")]
public class TestMyExampleCommand : Cmdlet
{
    ...
}
```

For guidance on writing cmdlet notes, see http://msdn.microsoft.com/en-us/library/bb736330.aspx.


## Examples

Cmdlet examples are defined using `<example>` elements in the XML doc comment for the cmdlet class. 

The example's code body is taken from the `<code>` element. Any `<para>` elements before the `<code>` element become the example's introduction. Any `<para>` elements  after the `<code>` element become the example's remarks. The introduction and remarks are both optional. 

To add multiple cmdlet examples, use multiple `<example>` elements.

```c#
/// <example>
///   <para>This is part of the example's introduction.</para>
///   <para>This is also part of the example's introduction.</para>
///   <code>Test-MyExample | Wrte-Host</code>
///   <para>This is part of the example's remarks.</para>
///   <para>This is also part of the example's remarks.</para>
/// </example>
[Cmdlet("Test", "MyExample")]
public class TestMyExampleCommand : Cmdlet
{
    ...
}
```

For guidance on writing cmdlet examples, see http://msdn.microsoft.com/en-us/library/bb736335.aspx.


## Related links

Related links are defined using `<para>` elements in the XML doc comment for the cmdlet class. Tag the relevant `<para>` elements with a `type="link"` attribute. The link text for each navigation link is taken from the body of the `<para>` element. If you want to include a uri, specify a uri attribute in the `<para>` element.

```c#
/// <summary>
///   <para type="link">This is the text of the first link.</para>
///   <para type="link">This is the text of the second link.</para>
///   <para type="link" uri="https://github.com/red-gate/XmlDoc2CmdletDoc/">The XmlDoc2CmdletDoc website.</para>
/// </summary>
[Cmdlet("Test", "MyExample")]
public class TestMyExampleCommand : Cmdlet
{
    ...
}
```

For guidance on writing related links, see http://msdn.microsoft.com/en-us/library/bb736334.aspx.

# Developer notes 

XmlDoc2CmdletDoc has a handful of NuGet package dependencies. One of them, **RedGate.ThirdParty.JoltCore**, isn't yet available via the official public NuGet repository. Nonetheless, the NuGet packages are included in this repository, and the source for RedGate.ThirdParty.JoltCore is [publicly available](https://github.com/red-gate/JoltNet-core). Jolt.NET and XmlDoc2CmdletDoc are released under the same [BSD licence](LICENSE).

# Contributors

- [Chris Lambrou](https://github.com/chrislambrou) (Redgate)
- [Michael Sorens](https://github.com/msorens)