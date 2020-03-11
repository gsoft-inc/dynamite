# -----------------------------------------
#.SYNOPSIS
# Deployment folder creation for the solution.
#
# .DESCRIPTION
# 1)	Create folder for output if it does not exist or enpty the existing folder if specified to.
# 2)	Copy most contents of current "Scripts" folder to Deployment folder (powershell scripts, .template files, folder structure, etc.)
# 3)	Take all WSP solution packages present in bin folders
# 4)	Take all WSP solutions present in Nuget folders
# 5)	Copy DSP PowerShell module so it can be installed on destination server
# 6)	Change directory into the Deployment folder if that's what the user asked for
#
#.PARAMETER NugetFolderPath
# The relative or full path of the NuGet packages. Default: "..\packages"
#
#.PARAMETER SolutionFolderPath
# The relative or full path of the folder that the .sln file with all the project folders are in. Default: ".."
#
#.PARAMETER OutputFolderPath
# The relative or full path of the pakage to be created. Default: "..\..\DeployPkg"
#
#.PARAMETER Override
# Switch parameter in order to delete the existing pakage folder. Default: false (De not delete existing folder)
#
#.PARAMETER Release
# Switch parameter in order to take the release or debug builds for the WSP files. Default: false (In other words, debug)
#
#.PARAMETER SetLocationToOutput
# Switch parameter in order to Set the current location to the output folder after publish. Default: true

# Publish-DeploymentFolder -NugetFolderPath ..\packages -OutputFolderPath ..\package -SolutionFolderPath .. -Override
Param (
	# The relative or full path to the nuget pakages folder
	[ValidateScript({Test-Path $_})]
	[String]$NugetFolderPath = "..\packages",
		
	# The relative or full path to the solution folder
	[ValidateScript({Test-Path $_})]
	[String]$SolutionFolderPath = "..",
		
	# The relative or full path to the folder the created package will be placed
	[String]$OutputFolderPath = "..\..\DeployPkg",
		
	# Override existing package
	[Switch]$Override = $false,
		
	# Use WSP files from the project output release folder
	[switch]$Release = $false,

	# Set location to output folder after publish
	[switch]$SetLocationToOutput = $true
)
    	
# Create folder for output if it does not exist	
if (-not (Test-Path $OutputFolderPath)) {
	New-Item $OutputFolderPath -ItemType directory | Out-Null
} elseif ($Override) {
    Get-ChildItem -Path $OutputFolderPath -Recurse | Remove-Item -force -recurse
} else {
	throw "Output folder already exists."
}
	
# Construct Full Paths
$OutputFolderFullPath = Resolve-Path $OutputFolderPath
$NugetFolderFullPath = Resolve-Path $NugetFolderPath
$SolutionFolderFullPath = Resolve-Path $SolutionFolderPath
		
# Tell user where we are working.
Write-Host "Working with Scripts folder: " -NoNewline
Write-Host "$(Get-Location)" -ForegroundColor Green
	
Write-Host "Working with Nuget folder: " -NoNewline
Write-Host "$NugetFolderFullPath" -ForegroundColor Green
		
Write-Host "Working with Solution folder: " -NoNewline
Write-Host "$SolutionFolderFullPath" -ForegroundColor Green
		
Write-Host "Working with Output folder: " -NoNewline
Write-Host "$OutputFolderFullPath" -ForegroundColor Green

# Copy most contents of current "Scripts" folder to Deployment folder (powershell scripts, .template files, folder structure, etc.)
Write-Verbose "Copying custom scripts and configuration from current folder (source: $(Get-Location))... "
Copy-DSPFiles $(Get-Location) $OutputFolderFullPath -Match @("*.ps1","*.template.*", "README*") -Exclude "Publish-DeploymentFolder.ps1"

# Copy all WSP files
if ($Release) { $binWspFilter = "*`\bin`\Release" } else { $binWspFilter = "*`\bin`\Debug" }
$WspDestinationPath = Join-Path $OutputFolderFullPath "Solutions"
Copy-DSPSolutions $SolutionFolderFullPath $WspDestinationPath $NugetFolderPath
Copy-DSPSolutions $SolutionFolderFullPath $WspDestinationPath $binWspFilter

# Copy DSP PowerShell module so it can be installed on destination server
$dynamiteFolder = Get-ChildItem -Path $NugetFolderFullPath -Recurse -Include "*GSoft.Dynamite.SP*" | where { $_.PSIsContainer } | sort Name | Select-Object -Last 1
$DSPDestinationPath = Join-Path $OutputFolderFullPath "DSP"
Copy-DSPFiles (Join-Path $dynamiteFolder "tools") $DSPDestinationPath

# Change directory into the Deployment folder if that's what the user asked for
if ($SetLocationToOutput -eq $true)
{
	Set-Location $OutputFolderFullPath
}