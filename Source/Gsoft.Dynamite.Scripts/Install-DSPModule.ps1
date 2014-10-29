cls

# Unblock files if they're from another computer
gci -Recurse | Unblock-File 

$moduleName = "Dynamite.PowerShell.Toolkit"
Write-Host ([string]::Format("[LOG] Installing Module {0}", $moduleName)) -ForegroundColor Yellow

# Create profile if not exist
if (!(Test-path $PROFILE))
{
	Write-Host ([string]::Format("[LOG] Creating the following profile {0}", $PROFILE)) 
	New-item -type file -force $PROFILE
}

$file = Get-Item $PROFILE
$modulePath = [io.Path]::Combine($file.Directory, "Modules")

# Create the Modules Folder
if (!(Test-path $modulePath))
{
	Write-Host "[LOG] Creating the Modules Folder."
	New-Item -ItemType directory -Path $modulePath
}

# Copy DSPModule 
$currentPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Host ([string]::Format("[LOG] Copying {0} to {1}", "$currentPath\$moduleName", $modulePath))

# Remove if exist (old stuff)
if (Test-Path $modulePath\$moduleName)
{
	Write-Host ([string]::Format("[LOG] Deleting the Module @ {0}", "$modulePath\$moduleName"))
	Remove-Item $modulePath\$moduleName -Recurse -Force
}

Copy-Item "$currentPath\$moduleName" $modulePath -recurse

# Reload DSPModule
if (Get-Module -ListAvailable -Name $moduleName)
{
	Write-Host ([string]::Format("[LOG] Importing module {0}", $moduleName))
	$currentModule = Get-Module -Name $moduleName
	if($currentModule -ne $null) {
		Remove-Module $currentModule
	}
	
	Import-Module $moduleName
}

# Load Binary Modules
$binaryModules = Get-ChildItem $modulePath -Recurse | Where-Object {$_.Extension -eq ".dll"}

$content = Get-Content $PROFILE

# Adding Import Line to $PROFILE
if (($content -eq $null) -or !([string]::Join("`n", $content)).Contains("Import-Module $moduleName"))
{
	Write-Host ([string]::Format("[LOG] Adding Module Import to {0}", $PROFILE))
	Add-Content $PROFILE "`n# Import $moduleName `nImport-Module $moduleName"
	$binaryModules | ForEach-Object {
	
		$binaryModule = $_.FullName
		Add-Content $PROFILE "# Import $binaryModule `nImport-Module $binaryModule"
	}
}

# Adding Get-DSPCommand out of module scope into $PROFILE
if (($content -eq $null) -or !([string]::Join("`n", $content)).Contains("function Get-DSPCommand(){Get-Command -Module (Get-DSPModuleName)}"))
{
	Write-Host ([string]::Format("[LOG] Adding Get-DSPCommand function to {0}", $PROFILE))
	Add-Content $PROFILE "`nfunction Get-DSPCommand(){Get-Command -Module (Get-DSPModuleName)}"
}

# Adding the PS-Snapin for Sharepoint
if (($content -eq $null) -or !([string]::Join("`n", $content)).Contains("Add-PSSnapin Microsoft.SharePoint.PowerShell"))
{
	Write-Host ([string]::Format("[LOG] Adding Module Import to {0}", $PROFILE))
  Add-Content $PROFILE "`nIf ((Get-PSSnapin |?{`$_.Name -eq `"Microsoft.SharePoint.PowerShell`"})-eq `$null)"
  Add-Content $PROFILE "{"
  Add-Content $PROFILE "`tWrite-Host `"Loading SharePoint PowerShell Snapin...`""
  Add-Content $PROFILE "`tAdd-PSSnapin Microsoft.SharePoint.PowerShell"
  Add-Content $PROFILE "}"
}

Write-Host ([string]::Format("[LOG] Installation finished. Module {0} is ready to use", $moduleName)) -ForegroundColor Green