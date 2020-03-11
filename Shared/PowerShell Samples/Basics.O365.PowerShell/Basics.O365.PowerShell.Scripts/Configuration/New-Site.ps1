# -----------------------------------------
#.SYNOPSIS
# Creates an O365 Site Collection if it does not already exist.
#
#.DESCRIPTION
# This sript uses the remote provisioning engine for a SharePoint Online site collection. XML definition used is based on PnP Schema definition.
# PREREQUISITE: You must install the PnP Cmdlet in order for this script to work properly.
#				  https://github.com/OfficeDev/PnP-PowerShell/blob/master/Binaries/PnPPowerShellCommands16.msi?raw=true
#
#.PARAMETER Tokens
# The contents of a tokens file as an xml object. ex.: $tokens = [xml](Get-Content $tokensFileFullPath)
Param
(
	[Parameter(Mandatory=$true)]
	[xml]$Tokens
)

Try
{
	# Connecto the the Admin tenant
	$AdminTenantUrl = $Tokens.Tokens.AdminTenantUrl;
	Write-Host "Trying to connect to $AdminTenantUrl..." -ForeGroundColor Yellow -NoNewLine
	Connect-SPOnline -Url $AdminTenantUrl
	Write-Host "Success!" -ForegroundColor Green

	Try 
	{
		# Get current user
		$context = Get-SPOContext
		$currentUser = $context.Credentials.UserName

		# Get site collection information
		$url = $Tokens.Tokens.SiteCollection.Url
		$title = $Tokens.Tokens.SiteCollection.Title

		$existingSite = Get-SPOTenantSite -Url $url -ErrorAction SilentlyContinue
		if($existingSite -eq $null) {
			Write-Host "Creating '$url'..." -ForeGroundColor Yellow -NoNewline
			New-SPOTenantSite -Title $title -Url $url -Lcid 1033 -Owner $currentUser -TimeZone 10 -Template STS#0 -Wait -RemoveDeletedSite
			Write-Host "Success!" -ForegroundColor Green
		} else {
			Write-Host "Site collection at url '$url' already exists." -ForeGroundColor Yellow
		}		
	}
	Catch
	{
		Write-Error $_.Exception.Message
	}
}
Catch
{
	Write-Host "Failed!" -ForegroundColor Red
	Write-Error $_.Exception.Message
}