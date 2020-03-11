# -----------------------------------------
#.SYNOPSIS
# Install the solution.
#
#.DESCRIPTION
# This sript uses the remote provisioning engine for a SharePoint Online site collection. XML definition used is based on PnP Schema definition.
# PREREQUISITE: You must install the PnP Cmdlet in order for this script to work properly.
#				  https://github.com/OfficeDev/PnP-PowerShell/blob/master/Binaries/PnPPowerShellCommands16.msi?raw=true
#
# 1) Creates the O365 Site Collection if it does not already exist.
# 2) Provision configurations and artifacts to the site collection.
# 3) Output a timestamped log file. 
#
#.PARAMETER TokensFilePath
# The relative or full path of the tokens file to use. Default: ".\Tokens\Tokens.$([System.Net.Dns]::GetHostName()).xml"

Param
(
	[ValidateScript({Test-Path $_})]
	[String]$TokensFilePath = ".\Tokens\Tokens.$([System.Net.Dns]::GetHostName()).xml"
)

# Make sure a transcript is not already running.
try{
  stop-transcript|out-null
}
catch [System.InvalidOperationException]{}

# Start a new transcript
$logPath = "$(get-location)\Transcript-Install-All-$($env:computername)-$(Get-Date -Format yyyy'-'MM'-'dd'T'HH'-'mm'-'ss).log"
Start-Transcript -path $logPath

# Prep token file
$tokensFileFullPath = Resolve-Path $TokensFilePath
$tokens = [xml](Get-Content $tokensFileFullPath)
Write-Host "Using token file '$tokensFileFullPath'..."

try {
	Write-Host " "
	Write-Host "################################" -ForegroundColor Green
	Write-Host "#   Creating site Collection   #" -ForegroundColor Green
	Write-Host "################################" -ForegroundColor Green

	./Configuration/New-Site.ps1 -Tokens $tokens

	Write-Host " "
	Write-Host "##############################" -ForegroundColor Green
	Write-Host "#   Provision site content   #" -ForegroundColor Green
	Write-Host "##############################" -ForegroundColor Green

	./Configuration/Provision-Site.ps1 -Tokens $tokens

} catch {
	Write-Host "Failed!" -ForegroundColor Red
	Write-Error $_.Exception.Message
}

# End the transcript
Stop-Transcript