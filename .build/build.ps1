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
        return exec {
            git rev-parse --abbrev-ref HEAD
        }
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


# UpdateAssemblyInfo task, updates the AssemblyVersion, AssemblyFileVersion and AssemblyInformationlVersion attributes in the source code.
task UpdateAssemblyInfo  Init, {
    Write-Info 'Updating assembly information'

    "$RepositoryRoot\SolutionInfo.cs" | Resolve-Path | Update-AssemblyVersion -Version $SemanticVersion -InformationalVersion $NuGetPackageVersion
}


# Compile task, runs MSBuild to build the solution.
task Compile  UpdateAssemblyInfo, {
    Write-Info "Compiling solution $SolutionPath"

    exec {
        dotnet build --configuration $Configuration $SolutionPath
    }

    @('net461', 'net472', 'net48', 'netcoreapp2.1', 'netcoreapp3.1') | ForEach-Object {
        exec {
            dotnet publish --configuration $Configuration --framework $_ --self-contained false "$RepositoryRoot\XmlDoc2CmdletDoc\XmlDoc2CmdletDoc.csproj"
        }
    }
}

# Create a forced 32-bit version of the tool.
task CorFlags  Compile, {
    Write-Info 'Using CorFlags.exe to create a 32-bit forced versions of XmlDoc2CmdletDoc.exe'
    
    $CorFlagsPath = Get-CorFlagsPath
    Write-Host "CorFlagsPath = $CorFlagsPath"

    @('net461', 'net472', 'net48') | ForEach-Object {
        $Input = "$RepositoryRoot\XmlDoc2CmdletDoc\bin\$Configuration\$_\publish\XmlDoc2CmdletDoc.exe"
        $Output = "$RepositoryRoot\XmlDoc2CmdletDoc\bin\$Configuration\$_\publish\XmlDoc2CmdletDoc32.exe"

        copy -Force $Input $Output

        $Parameters = @(
            $Output
            '/32BITREQ+'
        )

        exec {
            & $CorFlagsPath $Parameters
        }
        
        Copy-Item "$Input.config" "$Output.config"
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

        @('net461', 'net472', 'net48', 'netcoreapp2.1', 'netcoreapp3.1') | ForEach-Object {
            "$RepositoryRoot\XmlDoc2CmdletDoc\bin\$Configuration\$_\publish" |
                Get-ChildItem -File |
                Where-Object { $_.Name -match '^XmlDoc2CmdletDoc' -and $_.Extension -match '\.(exe|dll)$' } |
                ForEach-Object {
                    $_.FullName | Invoke-SigningService
                }
        }
    }
}


# Test task, runs the NUnit tests.
task Test  Sign, {
    Write-Info 'Running tests'

    @('net461', 'net472', 'net48', 'netcoreapp2.1', 'netcoreapp3.1') | ForEach-Object {
        $AssemblyPath = "$RepositoryRoot\XmlDoc2CmdletDoc.Tests\bin\$Configuration\$_\XmlDoc2CmdletDoc.Tests.dll" | Resolve-Path
        exec {
            dotnet vstest $AssemblyPath
        }
    }
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
    exec {
        & $NuGetPath $Parameters
    }
}


task Build  Package
task Rebuild  Clean, Build
task Default  Build