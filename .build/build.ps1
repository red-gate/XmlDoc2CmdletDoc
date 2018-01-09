# This file cannot be invoked directly; it simply contains a bunch of Invoke-Build tasks. To use it, invoke
# _init.ps1 which declares three global functions (build, clean, rebuild), then invoke one of those functions.

[CmdletBinding()]
param([string]$Configuration = 'Release')


# Useful paths used by multiple tasks.
$RepositoryRoot = "$PsScriptRoot\.." | Resolve-Path
$SolutionPath = "$RepositoryRoot\XmlDoc2CmdletDoc.sln" | Resolve-Path
$NuGetPath = "$PsScriptRoot\nuget.exe" | Resolve-Path
$DistPath = "$RepositoryRoot\dist"


# Helper function for clearer logging of each task.
function Write-Info {
    [CmdletBinding()]
    param ([string] $Message)

    Write-Host "## $Message ##" -ForegroundColor Magenta
}


# Environment-specific configuration should happen here (and only here!)
task Init {
    Write-Info 'Establishing build properties'

    # Establish IsAutobuild property.
    $script:IsAutomatedBuild = $env:BRANCH_NAME -and $env:BUILD_NUMBER
    Write-Host "Is automated build = $IsAutomatedBuild"
    
    # Load the release notes and parse the version number.
    $Notes = Get-ReleaseNotes
    $script:ReleaseNotes = [string] $Notes.Content
    $script:SemanticVersion = [version] $Notes.Version
    Write-Host "Semantic version = '$SemanticVersion'"
    
    # Establish assembly version number
    $script:AssemblyVersion = [version] "$($SemanticVersion.Major).0.0.0"
    $script:AssemblyFileVersion = [version] "$SemanticVersion.0"
    if ($env:BUILD_NUMBER) {
        $VersionSuffix = $env:BUILD_NUMBER # Build server override.
        $script:AssemblyFileVersion = [version] "$SemanticVersion.$VersionSuffix"
    }
    Write-Host "Assembly version = '$AssemblyVersion'"
    Write-Host "Assembly file version = '$AssemblyFileVersion'"
    TeamCity-SetBuildNumber $AssemblyFileVersion
    
    # Establish NuGet package version.
    $BranchName = Get-BranchName
    $IsDefaultBranch = $BranchName -eq 'master'
    $script:NuGetPackageVersion = New-SemanticNuGetPackageVersion -Version $AssemblyFileVersion -BranchName $BranchName -IsDefaultBranch $IsDefaultBranch
    Write-Host "NuGet package version = $NuGetPackageVersion"
    
    # Establish whether or not to sign the assemblies.
    if ($env:SigningServiceUrl) { # We sign if and only if the SigningServiceUrl environment variable is set.
        $script:AssemblySigningEnabled = $True
        Write-Host 'Assembly signing enabled'
    } else {
        $script:AssemblySigningEnabled = $False
        Write-Host 'Assembly signing disabled (SigningServiceUrl environment variable is not set)'
    }
}

function Get-ReleaseNotes {
    $ReleaseNotesPath = "$RepositoryRoot\release-notes.md" | Resolve-Path
    $Lines = [System.IO.File]::ReadAllLines($ReleaseNotesPath, [System.Text.Encoding]::UTF8)
    $Result = @()
    $Version = $Null
    $Lines | ForEach-Object {
        $Line = $_.Trim()
        if (-not $Version) {
            $Match = [regex]::Match($Line, '[0-9]+\.[0-9]+\.[0-9]+')
            if ($Match.Success) {
                $Version = $Match.Value
            }
        }
        if ($Version) {
            $Result += $Line
        }
    }
    if (-not $Version) {
        throw "Failed to parse release notes: $ReleaseNotesPath"
    }
    return @{
        Content = $Result -join [System.Environment]::NewLine
        Version = [version] $Version
    }
}

function Get-BranchName {
    # If the branch name is specified via an environment variable (i.e. on TeamCity), use it.
    if ($env:BRANCH_NAME) {
        return $env:BRANCH_NAME
    }

    # If the .git folder is present, try to get the current branch using Git.
    $DotGitDirPath = "$RepositoryRoot\.git"
    if (Test-Path $DotGitDirPath) {
        Add-Type -Path ("$PsScriptRoot\packages\GitSharp\lib\GitSharp.dll" | Resolve-Path)
        Add-Type -Path ("$PsScriptRoot\packages\SharpZipLib\lib\20\ICSharpCode.SharpZipLib.dll" | Resolve-Path)
        Add-Type -Path ("$PsScriptRoot\packages\Tamir.SharpSSH\lib\Tamir.SharpSSH.dll" | Resolve-Path)
        Add-Type -Path ("$PsScriptRoot\packages\Winterdom.IO.FileMap\lib\Winterdom.IO.FileMap.dll" | Resolve-Path)
    
        $Repository = New-Object 'GitSharp.Repository' $DotGitDirPath
        return $Repository.CurrentBranch.Name
    }

    # Otherwise, assume 'master'
    Write-Warning "Unable to determine the current branch name using either git or the BRANCH_NAME environment variable. Defaulting to 'master'."
    return 'master'
}


# Clean task, deletes all build output folders.
task Clean {
    Write-Info 'Cleaning build output'

    Get-ChildItem $RepositoryRoot -Exclude @('packages') -Include @('dist', 'bin', 'obj') -Directory -Recurse | ForEach-Object {
        Write-Host "Deleting $_"
        Remove-Item $_ -Force -Recurse
    }
}


# RestorePackages task, restores all the NuGet packages.
task RestorePackages {
    Write-Info "Restoring NuGet packages for solution $SolutionPath"

    & $NuGetPath @('restore', $SolutionPath)
}


# UpdateAssemblyInfo task, updates the AssemblyVersion, AssemblyFileVersion and AssemblyInformationlVersion attributes in the source code.
task UpdateAssemblyInfo  Init, {
    Write-Info 'Updating assembly information'

    "$RepositoryRoot\SolutionInfo.cs" | Resolve-Path | Update-AssemblyVersion -Version $SemanticVersion -InformationalVersion $NuGetPackageVersion
}


# Compile task, runs MSBuild to build the solution.
task Compile  UpdateAssemblyInfo, RestorePackages, {
    Write-Info "Compiling solution $SolutionPath"

    $MSBuildPath = Get-MSBuildPath
    $Parameters = @(
        $SolutionPath,
        '/nodeReuse:False',
        '/target:Build',
        "/property:Configuration=$Configuration"
    )
    exec {
        & $MSBuildPath $Parameters
    }
}

function Get-MSBuildPath {
    $MSBuildPath = @(
        "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe",
        "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\MSBuild.exe",
        "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe"
    ) |
        Where-Object {
            Write-Host "Checking $_"
            return Test-Path $_
        } |
    Select-Object -First 1
    if ($MSBuildPath) {
        return $MSBuildPath
    } else {
        throw 'Failed to locate MSBuild.exe'
    }
}


# Create a forced 32-bit version of the tool.
task CorFlags  Compile, {
    Write-Info 'Using CorFlags.exe to create a 32-bit forced version of XmlDoc2CmdletDoc.exe'

    copy -Force "$RepositoryRoot\XmlDoc2CmdletDoc\bin\$Configuration\XmlDoc2CmdletDoc.exe" "$RepositoryRoot\XmlDoc2CmdletDoc\bin\$Configuration\XmlDoc2CmdletDoc32.exe"

    $CorFlagsPath = Get-CorFlagsPath
    Write-Host "CorFlagsPath = $CorFlagsPath"
    $Parameters = @(
        "$RepositoryRoot\XmlDoc2CmdletDoc\bin\$Configuration\XmlDoc2CmdletDoc32.exe"
        '/32BITREQ+'
    )

    exec {
        & $CorFlagsPath $Parameters
    }
}

function Get-CorFlagsPath {
    $ProgramFiles = ${env:ProgramFiles(x86)}
    $ProgramFiles = if ($ProgramFiles) { $ProgramFiles } else { $env:ProgramFiles }
    $Root = "$ProgramFiles\Microsoft SDKs\Windows"
    if (-not (Test-Path $Root)) { throw "Path not found: $Root" }

    $Files =  $Root |
        Get-ChildItem -File -Recurse -Filter CorFlags.exe |
        Sort-Object -Descending { $_.VersionInfo.ProductVersion }
    if ($Files.Count -eq 0) { throw 'Failed to locate CorFlags.exe' }

    return $Files[0].FullName
}


# Sign the files (note that this is signing, not strong-naming)
task Sign  CorFlags, {
    if (-not $AssemblySigningEnabled) {
        Write-Info 'Skipping assembly signing'
    } else {
        Write-Info 'Signing Redgate assemblies'
        
        "$RepositoryRoot\XmlDoc2CmdletDoc\bin\$Configuration" |
            Get-ChildItem -File |
            Where-Object { $_.Extension -eq '.dll' -or $_.Extension -eq '.exe'} |
            ForEach-Object {
                $_.FullName | Invoke-SigningService
            }
    }
}


# Test task, runs the NUnit tests.
task Test  Sign, {
    Write-Info 'Running tests'

    $AssemblyPath = "$RepositoryRoot\XmlDoc2CmdletDoc.Tests\bin\$Configuration\XmlDoc2CmdletDoc.Tests.dll" | Resolve-Path
    Invoke-NUnitForAssembly -AssemblyPath $AssemblyPath `
                            -NUnitVersion '2.6.3' `
                            -FrameworkVersion 'net-4.5'
}

# Package task, create the NuGet package.
task Package  Test, {
    Write-Info 'Generating NuGet package'

    # Make sure the output folder exists.
    if (-not (Test-Path $DistPath)) {
        $Null = mkdir $DistPath
    }

    # Run NuGet pack.
    $NuSpecPath = "$RepositoryRoot\XmlDoc2CmdletDoc\XmlDoc2CmdletDoc.nuspec" | Resolve-Path
    $Parameters = @(
        'pack',
        "$NuSpecPath",
        '-Version', $NuGetPackageVersion,
        '-OutputDirectory', $DistPath,
        '-Properties', "configuration=$Configuration;releaseNotes=$ReleaseNotes"
    )
    & $NuGetPath $Parameters
}


task Build  Package
task Rebuild  Clean, Build
task Default  Build