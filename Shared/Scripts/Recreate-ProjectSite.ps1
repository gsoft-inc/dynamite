# Reference script to delete an existing site collection and recreate it.
# Change the variables below to suit your needs.

# Load external scripts
. .\Configure-ProjectTaxonomyFields.ps1

# Init script variables
$siteURL = "http://sptao/sites/client"
$owner = "SPSDEV\administrator"
$secondary = "SPSDEV\dev"

# Delete old site collection
Write-Host @"

Deleting site...
"@

Remove-SPSite -Identity $siteUrl

# Recreate the site collection from the iO site definition
Write-Host @"

Recreating site...
"@

$startTime = Get-Date
$teamTemplate = Get-SPWebTemplate "STS#0"
New-SPSite -Url $siteURL -Name "Client Project" -Language 1033 -OwnerAlias $owner -SecondaryOwnerAlias $secondary -Template $teamTemplate
$elapsedTime = ($(get-date) - $StartTime).TotalSeconds
Write-Host "Took $elapsedTime sec."

Write-Host @"

Configuring site...
"@

# Add site columns
Enable-SPFeature -Identity "GSoft.Dynamite.Examples.Definitions_Wall Content Types" -URL $siteURL

# Connect site columns to term sets in Managed Metadata Service
Configure-ProjectTaxonomyFields

# Provision the lists (after configuring site columns because otherwise the list fields won't be connected to Managed Metadata Service)
Enable-SPFeature -Identity "GSoft.Dynamite.Examples.Definitions_Wall Lists Code" -URL $siteURL

# Enable web parts
Enable-SPFeature -Identity "GSoft.Dynamite.Examples.Parts_Basic Wall" -URL $siteURL

# Javascript
Enable-SPFeature -Identity "GSoft.Dynamite.Examples.Parts_Client Scripts" -URL $siteURL

# Branding
Enable-SPFeature -Identity "GSoft.Dynamite.Examples.Branding_Project Brand" -URL $siteURL