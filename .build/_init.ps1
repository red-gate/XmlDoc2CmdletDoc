$VerbosePreference = 'Continue'          # Want useful output in our build log files.
$ProgressPreference = 'SilentlyContinue' # Progress logging slows down TeamCity when downloading files with Invoke-WebRequest.
$ErrorActionPreference = 'Stop'          # Abort quickly on error.

function global:Build
{
    [CmdletBinding()]
    param(
        [string[]] $Task = @('Default'),

        [ValidateSet('Release', 'Debug')]
        [string] $Configuration = 'Release'
    )

    Push-Location $PsScriptRoot -Verbose
    try
    {
        # Obtain nuget.exe
        $NuGetVersion = [version] '3.2.0'
        $NuGetPath = '.\nuget.exe'
        if (-not (Test-Path $NuGetPath) -or (Get-Item $NuGetPath).VersionInfo.ProductVersion -ne $NuGetVersion)
        {
            $NuGetUrl = "https://dist.nuget.org/win-x86-commandline/v$NuGetVersion/nuget.exe"
            Write-Host "Downloading $NuGetUrl"
            Invoke-WebRequest $NuGetUrl -OutFile $NuGetPath
        }
        
        # Clean out any existing 'build-level' packages from the .build/packages folder.
        # Restoring them will be quick thanks to the nuget local cache.
        if (Test-Path 'packages')
        {
            Remove-Item 'packages' -Force -Recurse
        }

        # Restore the 'build-level' nuget packages into .build/packages
        $NuGetConfigXml = [xml](Get-Content 'packages.config')
        $NuGetConfigXml.packages.package | ForEach-Object {
            & $NuGetPath install $_.id `
                -Version $_.Version `
                -OutputDirectory 'packages' `
                -ExcludeVersion `
                -PackageSaveMode nuspec
        }

        # Import the RedGate.Build module.
        Import-Module '.\packages\RedGate.Build\tools\RedGate.Build.psm1' -Force

        # Call the actual build script.
        & '.\packages\Invoke-Build\tools\Invoke-Build.ps1' -File .\build.ps1 -Task $Task -Configuration $Configuration
    }
    finally
    {
        Pop-Location
    }
}

function global:Clean 
{
    Build -Task Clean
}

function global:Rebuild
{
    [CmdletBinding()]
    param([ValidateSet('Release', 'Debug')] [string] $Configuration = 'Release')

    Build -Task Rebuild -Configuration $Configuration
}

Write-Host 'This is the XmlDoc2CmdletDoc repo. Here are the available commands:' -ForegroundColor Magenta
Write-Host "    Build [-Task <task-list>] [-Configuration <Debug|Release>]" -ForegroundColor Green
Write-Host "    Clean" -ForegroundColor Green
Write-Host "    Rebuild [-Configuration <Debug|Release>]" -ForegroundColor Green