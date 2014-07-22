<#
.SYNOPSIS
	Creates new SPWebs from an xml definition.
	
.DESCRIPTION
	Creates new SPWebs from an xml definition. The XML definitions is a collection of sites to be created.
	SiteCollections and Webs can contain webs.
	EX. Definition: <Webs><Web Name="Blogue" Path="blogue" Template="BLOG#0" /><Webs>

    --------------------------------------------------------------------------------------
    Module 'Dynamite.PowerShell.Toolkit'
    by: GSoft, Team Dynamite.
    > GSoft & Dynamite : http://www.gsoft.com
    > Dynamite Github : https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit
    > Documentation : https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit/wiki
    --------------------------------------------------------------------------------------
	
.PARAMETER Webs
	The XmlElement representing a collection of sites you want to create.
	
.PARAMETER ParentUrl
	The Url of the site the sub sites will be created under.
	
.PARAMETER UseParentTopNav
	Specifies that the same top-level navigation is to be used in all sites.
        
  .LINK
    GSoft, Team Dynamite on Github
    > https://github.com/GSoft-SharePoint
    
    Dynamite PowerShell Toolkit on Github
    > https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit
    
    Documentation
    > https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit/wiki
    
#>
function New-GSPWebXml()
{
	[CmdletBinding()]
	param
	(
		[Parameter(Mandatory=$true, Position=0)]
		[System.Xml.XmlElement]$Webs,
		
		[Parameter(Mandatory=$true, Position=1)]
		[string]$ParentUrl,
		
		[Parameter(Position=2)]
		[switch]$UseParentTopNav
	)
		
	foreach ($web in $Webs.Web)
	{
		[string]$Name = $web.Name
		[string]$Path = $web.Path
		[string]$Template = $web.Template
		[string]$Language = $web.Language
		
		if ($Language.Length -eq 0)
		{
			$parentWeb = Get-SPWeb $ParentUrl
			$Language = $parentWeb.Language
		}
		
		$ParentUrl = $ParentUrl.TrimEnd('/')
		$Url = "$ParentUrl/$Path"

		if ((Get-SPWeb -Identity $Url -ErrorAction SilentlyContinue) -eq $null)
		{
			$newWeb = $null
			try {
				#If we can't find the web template in the Get-SPWebTemplate command but it exists in the site, we create the site and apply it after.
				if(((Get-SPWebTemplate -Identity "$Template" -ErrorAction SilentlyContinue) -eq $null) -and (($parentWeb.Site.GetWebTemplates($Language) | where {$_.Name -eq "$Template"}) -ne $null )) 
				{
					$newWeb = New-SPWeb -Url $Url -Name $Name -UseParentTopNav:$UseParentTopNav -Language $Language
					$newWeb.ApplyWebTemplate("$Template")
				}
				else
				{
					$newWeb = New-SPWeb -Url $Url -Template "$Template" -Name $Name -UseParentTopNav:$UseParentTopNav -Language $Language	
				}
			} finally {
				if($newWeb -ne $null){
					$newWeb.Dispose()
				}
			}

			Write-Verbose "The web $Url was created."
		}
		else
		{
			Write-Verbose "Another web already exists at $Url"
		}
		
		if ($web.Groups -ne $null)
		{
			Set-GSPWebPermissionInheritance -Web $Url -Break
			Add-GSPGroupByXml -Web $Url -Group $web.Groups
		}
		
		if($Web.Webs -ne $null)
		{
			New-GSPWebXml -Webs $web.Webs -ParentUrl $Url -UseParentTopNav
		}		
	}
}