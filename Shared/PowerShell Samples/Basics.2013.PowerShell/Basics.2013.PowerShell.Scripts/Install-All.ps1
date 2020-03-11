# Should not be run from the scripts folder in order to avoid replacing tokens in this folder.
if((Get-Location).Path.Contains("Scripts")){
	throw "This script should not be run from this folder."
}

# Make sure a transcript is not already running.
try{
  stop-transcript|out-null
}
catch [System.InvalidOperationException]{}

# Start a new transcript
$logPath = "$(get-location)\Transcript-Install-All-$($env:computername)-$(Get-Date -Format yyyy'-'MM'-'dd'T'HH'-'mm'-'ss).log"
Start-Transcript -path $logPath

# Update tokens in the template files.
Update-DSPTokens -Path .\Tokens

Write-Host " "
Write-Host "################################################" -ForegroundColor Green
Write-Host "#      Creating site Collection structure      #" -ForegroundColor Green
Write-Host "################################################" -ForegroundColor Green

./Configuration/New-SiteStructure.ps1

Write-Host " "
Write-Host "###############################" -ForegroundColor Green
Write-Host "#   Re-enable Site features   #" -ForegroundColor Green
Write-Host "###############################" -ForegroundColor Green

./Configuration/Enable-SiteFeatures.ps1

# End the transcript
Stop-Transcript