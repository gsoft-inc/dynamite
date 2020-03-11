# Should not be run from the scripts folder in order to avoid replacing tokens in this folder.
if((Get-Location).Path.Contains("Scripts")){
	throw "This script should not be run from this folder."
}

# Update tokens in the template files.
Update-DSPTokens -Path .\Tokens

# Deploy all the wsp files
Deploy-DSPSolution ./Solutions/Solutions.xml