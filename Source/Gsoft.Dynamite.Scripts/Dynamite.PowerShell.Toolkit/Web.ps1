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
function New-DSPWebXml()
{
	[CmdletBinding()]
	param
	(
		[Parameter(Mandatory=$true, Position=0)]
		[System.Xml.XmlElement]$Webs,
		
		[Parameter(Mandatory=$true, Position=1)]
		[string]$ParentUrl,
		
		[Parameter(Position=2)]
		[switch]$UseParentTopNav,
		
		[Parameter(Position=3)]
		[switch]$Overwrite
		
		
	)
		
	foreach ($web in $Webs.Web)
	{
		[string]$Name = $web.Name
		[string]$Path = $web.Path
		[string]$Template = $web.Template
		[string]$Language = $web.Language
		
		$allWebs = @()
		
		if ($Language.Length -eq 0)
		{
			$parentWeb = Get-SPWeb $ParentUrl
			$Language = $parentWeb.Language
		}
		
        $name = $web.Name

		Write-Verbose "Processing $name"
		
		$ParentUrl = $ParentUrl.TrimEnd('/')
		$Url = "$ParentUrl/$Path"

		$newWeb = Get-SPWeb -Identity $Url -ErrorAction SilentlyContinue
		
		if ($newWeb -ne $null)
		{
			Write-Verbose "Another web already exists at $Url"
			if($Overwrite)
			{
				Remove-SPWeb -Identity $Url
				$newWeb = $null
			}
			else
			{
				$allWebs += $newWeb
			}
		}
		
		if ($newWeb -eq $null)
		{
			#If we can't find the web template in the Get-SPWebTemplate command but it exists in the site, we create the site and apply it after.
			if(((Get-SPWebTemplate -Identity "$Template" -ErrorAction SilentlyContinue) -eq $null) -and (($parentWeb.Site.GetWebTemplates($Language) | where {$_.Name -eq "$Template"}) -ne $null )) 
			{
				$newWeb = New-SPWeb -Url $Url -Name $Name -UseParentTopNav:$UseParentTopNav -Language $Language
				$newWeb.ApplyWebTemplate("$Template")					
				$allWebs += $newWeb
			}
			else
			{
				$newWeb = New-SPWeb -Url $Url -Template "$Template" -Name $Name -UseParentTopNav:$UseParentTopNav -Language $Language						
				$allWebs += $newWeb
			}

		    Write-Verbose "The web $Url was created."	
		}
		
		# Groups
		if ($web.Groups -ne $null)
		{
            $resetExistingPermissions = [System.Convert]::ToBoolean($web.Groups.ClearExistingPermissions)
            Set-DSPWebPermissionInheritance -Web $Url -Break -CopyRoleAssignments:(-not $clearExistingPermissions)
			Add-DSPGroupByXml -Web $Url -Group $web.Groups
		}
		
		# Features 
		if($web.Features -ne $null)
		{
			Initialize-DSPFeatures $web.Features $newWeb.Url
		}
		
		if($Web.Webs -ne $null)
		{
			New-DSPWebXml -Webs $web.Webs -ParentUrl $Url -UseParentTopNav
		}

        Write-Output $allWebs
	}
}

<#
    .SYNOPSIS
	    Configures Web Search Settings
	
    .DESCRIPTION
	    Configures the Web search settings (URLs and navigation links)

    --------------------------------------------------------------------------------------
    Module 'Dynamite.PowerShell.Toolkit'
    by: GSoft, Team Dynamite.
    > GSoft & Dynamite : http://www.gsoft.com
    > Dynamite Github : https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit
    > Documentation : https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit/wiki
    --------------------------------------------------------------------------------------
		
    .PARAMETER XmlPath
	    The search configuration XML configuration file path.

    .NOTES
        Here is the Structure XML schema.

        <Configuration>
          <Web Url="http://intranet.contoso.com">
            <Search
	        ResultPageURL="http://intranet.contoso.com/recherche"
	        SearchCenterURL="">
              <NavigationLinks>
                <Link Title="Tous" Url="http://intranet.contoso.com/recherche" IsExternal="TRUE"/>
                <Link Title="Nouvelles" Url="http://intranet.contoso.com/recherche/nouvelles" IsExternal="TRUE"/>
              </NavigationLinks>
            </Search>
          </Web>
        </Configuration>

    .EXAMPLE
		    PS C:\> Set-DSPWebSearchSettings -XmlPath "D:\WebSearchSettings.xml" 

	.OUTPUTS
		n/a. 

    .LINK
    GSoft, Team Dynamite on Github
    > https://github.com/GSoft-SharePoint
    
    Dynamite PowerShell Toolkit on Github
    > https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit
    
    Documentation
    > https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit/wiki
    
#>
function Set-DSPWebSearchSettings
{
    [CmdletBinding()]
	param
	(
		[Parameter(Mandatory=$true, Position=0)]
		$XmlPath		
	)

    $Configuration = [xml](Get-Content $XmlPath)

    $Configuration.Configuration.Web | ForEach-Object {

        $Web = Get-SPWeb $_.Url

        if ($Web)
        {
            $SearchConfig = $_.Search

            $ResultPageUrl = $SearchConfig.ResultPageURL
            $SearchCenterUrl = $SearchConfig.SearchCenterURL
    
            if ($ResultPageUrl)
            {
                Write-Verbose "Set result page URL to $ResultPageUrl on $SiteUrl"
                $Web.AllProperties["SRCH_SB_SET_WEB"] = '{"Inherit":false,"ResultsPageAddress":"'+$ResultPageUrl+'","ShowNavigation":false}'
            }

            if ($SearchCenterUrl)
            {
                Write-Verbose "Set search center URL to $SearchCenterUrl on $SiteUrl"
                $web.AllProperties["SRCH_ENH_FTR_URL_WEB"] = $SearchCenterUrl
            }

            $SearchConfig.NavigationLinks.Link | ForEach-Object {

                $LinkUrl = $_.Url
                $Title = $_.Title
                $nodes = $Web.Navigation.SearchNav | Where-Object {$_.Title -eq $Title}
                
                if ($nodes)
                {
                    Write-Verbose "`tNode $Title already exists. Deleting..."

                    $nodes | ForEach-Object{
                        $_.Delete()
                    }
                }

                $node = New-Object Microsoft.SharePoint.Navigation.SPNavigationNode($_.Title, $LinkUrl , [System.Convert]::ToBoolean($_.IsExternal));

                Write-Verbose "`tAdding search navigation link $LinkUrl"
                $Web.Navigation.AddToSearchNav($node);
            }   

            $Web.Update()
        }   
    }   
}

<#
    .SYNOPSIS
	    Export a web structure
	
    .DESCRIPTION
	    Export the web structure under the source web.
        Cases were webs are not exported:
         - The current web is the source variation site created by SharePoint (e.g /sites/<sitename>/en)
         - The current web is a peer variation site automatically created by SharePoint for a variation target label (e.g /sites/<sitename>/fr/subweb where 'en' label is the variation source)
         In all cases, it makes no sense to export a web generated automatically by SharePoint. To reproduce the same structure, you have to create the original structure and synchronize again.
    --------------------------------------------------------------------------------------
    Module 'Dynamite.PowerShell.Toolkit'
    by: GSoft, Team Dynamite.
    > GSoft & Dynamite : http://www.gsoft.com
    > Dynamite Github : https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit
    > Documentation : https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit/wiki
    --------------------------------------------------------------------------------------
		
    .PARAMETER SourceWeb
	    The source web at which to start exporting. By defaut, the source web in included.

    .EXAMPLE
		    PS C:\> Export-DSPWeb -SourceWeb "http://<site>/sites/test/" -OutputFileName "C:\Test.xml" 

	.OUTPUTS
		Here is the output Structure XML schema.

        <!--This file was auto-generated by the Export-DSPWeb cmdlet at 02/27/2015 09:38:46-->
        <Configuration>
          <Web Name="Home" Path="/sites/test" Template="BLANKINTERNET#0" IsRoot="True" Owner="office\joe.blow">
            <Web Name="Sub Web 1" Path="/sites/test/subweb1" Template="CMSPUBLISHING#0">
              <Web Name="Sub Sub Web 1" Path="/sites/test/subweb1/subweb11" Template="CMSPUBLISHING#0">
              </Web>
            </Web>
            <Web Name="Sub Web 2" Path="/sites/test/subweb2" Template="CMSPUBLISHING#0">
            </Web>
          </Web>
        </Configuration>

    .LINK
    GSoft, Team Dynamite on Github
    > https://github.com/GSoft-SharePoint
    
    Dynamite PowerShell Toolkit on Github
    > https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit
    
    Documentation
    > https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit/wiki
    
#>
function Export-DSPWeb {
    
    [CmdletBinding()]
	param
	(
		[Parameter(Mandatory=$true, Position=0)]
		[string]$SourceWeb,

		[Parameter(Mandatory=$false, Position=1)]
		[string]$OutputFileName,

		[Parameter(Mandatory=$false)]
		[switch]$ExludeSourceWeb
	)
  
    function CreateWebNode
    {
        Param
	    (
            [Parameter(Mandatory=$true, Position=0)]
		    [System.XML.XMLDocument]$XmlDocument,

            [Parameter(Mandatory=$false, Position=1)]
            [System.XML.XMLElement]$ParentXMLElement,

		    [Parameter(Mandatory=$true, Position=2)]
		    [Microsoft.SharePoint.SPWeb]$Web,

            [Parameter(Mandatory=$false, Position=3)]
		    [bool]$IgnoreWebNode
        )

        $WebUrl = $Web.ServerRelativeUrl

        # To know if variations are enabled on a site, we need to cast the current Web to a PublishingWeb (works for all web templates) and check the Label property
        $publishingWeb = [Microsoft.SharePoint.Publishing.PublishingWeb]::GetPublishingWeb($web)

        if ($publishingWeb -ne $null)
        {
            if ($publishingWeb.Label -ne $null)
            {
                # Cases were webs are not exported:
                # - The current web is the source variation site created by SharePoint (e.g /sites/<sitename>/en)
                # - The current web is a peer variation site automatically created by SharePoint for a variation target label (e.g /sites/<sitename>/fr/subweb where 'en' label is the variation source)
                # In all cases, it makes no sense to export a web generated automatically by SharePoint. To reproduce the same structure, you have to create the original structure and synchronize again.
                if (([System.IO.Path]::GetFileNameWithoutExtension($Web.ServerRelativeUrl) -eq $publishingWeb.Label.Title) -or $publishingWeb.Label.IsSource -ne $true)
                {
                    $url =$Web.Url
                    Write-Warning "Web with URL '$url' appears to be a variation generated site. Skipping export..."
                    $IgnoreWebNode = $true
                }
                else
                {
                    # Remove the variation label to get the original web URL
                    $WebUrl = $WebUrl.Replace(("/" +$publishingWeb.Label.Title), [string]::Empty)  
                }              
            }
        }

        if($IgnoreWebNode -eq $false)
        {
            # New Node
            [System.XML.XMLElement]$oXMLElement= $XmlDocument.CreateElement("Web")
            $oXMLElement.SetAttribute("Name", $Web.Title)
            $oXMLElement.SetAttribute("Path", $WebUrl)
            $oXMLElement.SetAttribute("Template", $Web.WebTemplate + "#" + $Web.Configuration)

            if ($Web.IsRootWeb -eq $true)
            {
               $oXMLElement.SetAttribute("IsRoot", "True") 

               # Remove claims identifier if present (no claims in MOSS 2007)
               $ownerLoginName = ($Web.Site.Owner.LoginName -replace "^(.*)[|]",[string]::Empt).ToLower()

               $oXMLElement.SetAttribute("Owner", $ownerLoginName)           
            }

            if ($IgnoreWebNode -eq $false)
            {
                $ParentXMLElement.appendChild($oXMLElement)           
            }
        }
        
        # Create sub web nodes
        $web.Webs | ForEach-Object {

            CreateWebNode $XmlDocument $oXMLElement $_      
        }
    }

    # Load SharePoint assembly to be backward compatible with MOSS 2007
    [System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SharePoint")
	[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SharePoint.Publishing")
    Try
    {
        $site = New-Object Microsoft.SharePoint.SPSite($SourceWeb)
        $web = $site.OpenWeb()        

        # Create a new XML File
        [System.XML.XMLDocument]$oXmlDocument = New-Object System.XML.XMLDocument

        # Write header
        $header = $oXmlDocument.CreateComment("This file was auto-generated by the Export-DSPWeb cmdlet at "+ (Get-Date))
        $oXmlDocument.AppendChild($header)

        # Create the root node (an XML file must have a root node even if there is no web to export)
        [System.XML.XMLElement]$oXMLElement= $oXmlDocument.CreateElement("Configuration")
        $oXMLDocument.appendChild($oXMLElement)

        # Recursively create nodes
        CreateWebNode $oXmlDocument $oXMLElement $web $ExludeSourceWeb.IsPresent
        
        # Save File
        $oXmlDocument.Save($OutputFileName)

        $web.Dispose()
        $site.Dispose();
    }
    Catch
    {
        $ErrorMessage = $_.Exception.Message
        throw $ErrorMessage
    }
}