###############################
# Dynamite.PowerShell.Toolkit #
###############################
# Installer based on the ChocoleyInstall of Pester. It will install the Module in the ProgramFiles so it will be available to all users
# It will also add the Module to the Environment Variable PSModulePath. 
# Add a Switch (-Profile) and it will ensure the Profile file for the user and add the Import-Module and the Add-PSSnappin for Dynamite and SharePoint

[CmdletBinding()]
Param (
        [Parameter(Mandatory=$false)]
        [switch]$Profile,
)

end
{
    $moduleName = "Dynamite.PowerShell.Toolkit"
    $modulePath = Join-Path $env:ProgramFiles WindowsPowerShell\Modules
    $targetDirectory = Join-Path $modulePath $moduleName

    $scriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent
    $sourceDirectory = Join-Path $scriptRoot $moduleName

    Update-Directory -Source $sourceDirectory -Destination $targetDirectory

    if ($PSVersionTable.PSVersion.Major -lt 4)
    {
        $modulePaths = [Environment]::GetEnvironmentVariable('PSModulePath', 'Machine') -split ';'
        if ($modulePaths -notcontains $modulePath)
        {
            Write-Verbose "Adding '$modulePath' to PSModulePath."

            $modulePaths = @(
                $modulePath
                $modulePaths
            )

            $newModulePath = $modulePaths -join ';'

            [Environment]::SetEnvironmentVariable('PSModulePath', $newModulePath, 'Machine')
            $env:PSModulePath += ";$modulePath"
        }
    }

    if ($Profile -eq $true)
    {
        # Create profile if not exist
        if (!(Test-path $PROFILE))
        {
            Write-Host ([string]::Format("[LOG] Creating the following profile {0}", $PROFILE)) 
            New-item -type file -force $PROFILE
        }

        $content = Get-Content $PROFILE

        # Adding Import Line to $PROFILE
        if (($content -eq $null) -or !([string]::Join("`n", $content)).Contains("Import-Module $moduleName"))
        {
            Write-Host ([string]::Format("[LOG] Adding Module Import to {0}", $PROFILE))
            Add-Content $PROFILE "`n# Import $moduleName `nImport-Module $moduleName"
        }

        # Adding Get-DSPCommand out of module scope into $PROFILE
        if (($content -eq $null) -or !([string]::Join("`n", $content)).Contains("function Get-DSPCommand(){Get-Command -Module (Get-DSPModuleName)}"))
        {
            Write-Host ([string]::Format("[LOG] Adding Get-DSPCommand function to {0}", $PROFILE))
            Add-Content $PROFILE "`nfunction Get-DSPCommand(){Get-Command -Module (Get-DSPModuleName)}"
        }

        # Adding the PS-Snapin for SharePoint
        if (($content -eq $null) -or !([string]::Join("`n", $content)).Contains("Add-PSSnapin Microsoft.SharePoint.PowerShell"))
        {
            Write-Host ([string]::Format("[LOG] Adding Module Import to {0}", $PROFILE))
            Add-Content $PROFILE "`nIf ((Get-PSSnapin |?{`$_.Name -eq `"Microsoft.SharePoint.PowerShell`"})-eq `$null)"
            Add-Content $PROFILE "{"
            Add-Content $PROFILE "`tWrite-Host `"Loading SharePoint PowerShell Snapin...`""
            Add-Content $PROFILE "`tAdd-PSSnapin Microsoft.SharePoint.PowerShell"
            Add-Content $PROFILE "}"
        }
    }

    Write-Host ([string]::Format("[LOG] Installation finished. Module {0} is ready to use", $moduleName)) -ForegroundColor Green
}

begin
{
    function Update-Directory
    {
        [CmdletBinding()]
        param (
            [Parameter(Mandatory = $true)]
            [string] $Source,

            [Parameter(Mandatory = $true)]
            [string] $Destination
        )

        $Source = $PSCmdlet.GetUnresolvedProviderPathFromPSPath($Source)
        $Destination = $PSCmdlet.GetUnresolvedProviderPathFromPSPath($Destination)

        if (-not (Test-Path -LiteralPath $Destination))
        {
            $null = New-Item -Path $Destination -ItemType Directory -ErrorAction Stop
        }

        try
        {
            $sourceItem = Get-Item -LiteralPath $Source -ErrorAction Stop
            $destItem = Get-Item -LiteralPath $Destination -ErrorAction Stop

            if ($sourceItem -isnot [System.IO.DirectoryInfo] -or $destItem -isnot [System.IO.DirectoryInfo])
            {
                throw 'Not Directory Info'
            }
        }
        catch
        {
            throw 'Both Source and Destination must be directory paths.'
        }

        $sourceFiles = Get-ChildItem -Path $Source -Recurse |
                       Where-Object { -not $_.PSIsContainer }

        foreach ($sourceFile in $sourceFiles)
        {
            $relativePath = Get-RelativePath $sourceFile.FullName -RelativeTo $Source
            $targetPath = Join-Path $Destination $relativePath

            $sourceHash = Get-FileHash -Path $sourceFile.FullName
            $destHash = Get-FileHash -Path $targetPath

            if ($sourceHash -ne $destHash)
            {
                $targetParent = Split-Path $targetPath -Parent

                if (-not (Test-Path -Path $targetParent -PathType Container))
                {
                    $null = New-Item -Path $targetParent -ItemType Directory -ErrorAction Stop
                }

                Write-Verbose "Updating file $relativePath to new version."
                Copy-Item $sourceFile.FullName -Destination $targetPath -Force -ErrorAction Stop
            }
        }

        $targetFiles = Get-ChildItem -Path $Destination -Recurse |
                       Where-Object { -not $_.PSIsContainer }
    
        foreach ($targetFile in $targetFiles)
        {
            $relativePath = Get-RelativePath $targetFile.FullName -RelativeTo $Destination
            $sourcePath = Join-Path $Source $relativePath        

            if (-not (Test-Path $sourcePath -PathType Leaf))
            {
                Write-Verbose "Removing unknown file $relativePath from module folder."
                Remove-Item -LiteralPath $targetFile.FullName -Force -ErrorAction Stop
            }
        }

    }

    function Get-RelativePath
    {
        param ( [string] $Path, [string] $RelativeTo )
        return $Path -replace "^$([regex]::Escape($RelativeTo))\\?"
    }

    function Get-FileHash
    {
        param ([string] $Path)

        if (-not (Test-Path -LiteralPath $Path -PathType Leaf))
        {
            return $null
        }

        $item = Get-Item -LiteralPath $Path
        if ($item -isnot [System.IO.FileSystemInfo])
        {
            return $null
        }

        $stream = $null

        try
        {
            $sha = New-Object System.Security.Cryptography.SHA256CryptoServiceProvider
            $stream = $item.OpenRead()
            $bytes = $sha.ComputeHash($stream)
            return [convert]::ToBase64String($bytes)
        }
        finally
        {
            if ($null -ne $stream) { $stream.Close() }
            if ($null -ne $sha)    { $sha.Clear() }
        }
    }
}