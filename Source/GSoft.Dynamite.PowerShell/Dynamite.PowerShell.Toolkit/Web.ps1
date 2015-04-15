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
	    Remove a SPWeb
	
    .DESCRIPTION
	    If -Recurse is specified, all subwebs are removed
    --------------------------------------------------------------------------------------
    Module 'Dynamite.PowerShell.Toolkit'
    by: GSoft, Team Dynamite.
    > GSoft & Dynamite : http://www.gsoft.com
    > Dynamite Github : https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit
    > Documentation : https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit/wiki
    --------------------------------------------------------------------------------------
		
    .PARAMETER WebUrl
	    The source web url

    .PARAMETER Recurse
	    Removes all subsites recursively

    .EXAMPLE
		    PS C:\> Remove-DSPWeb -WebUrl "http://<site>/sites/test/" -Recurse 

    .LINK
    GSoft, Team Dynamite on Github
    > https://github.com/GSoft-SharePoint
    
    Dynamite PowerShell Toolkit on Github
    > https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit
    
    Documentation
    > https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit/wiki
    
#>
function Remove-DSPWeb {

    [CmdletBinding()]
	param
	(
        [Parameter(Mandatory=$true)]
		[string]$WebUrl,

        [Parameter(Mandatory=$false)]
		[switch]$Recurse
    )

    $Web = Get-SPWeb $WebUrl -ErrorAction SilentlyContinue

    if ($Web)
    {
        if($Recurse)
        {
            $Web.Webs | ForEach-Object {
        
                Remove-DSPWeb -WebUrl $_.Url -Recurse:$Recurse
            }
        }

        Remove-SPWeb -Identity $Web -Confirm:$false
    }
}

<#
    .SYNOPSIS
	    Export a SharePoint web structure as XML. 
	
    .DESCRIPTION
	    Export the SharePoint web structure under the source web as an XML file. This cmdlet is compatible with MOSS 2007
 
    --------------------------------------------------------------------------------------
    Module 'Dynamite.PowerShell.Toolkit'
    by: GSoft, Team Dynamite.
    > GSoft & Dynamite : http://www.gsoft.com
    > Dynamite Github : https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit
    > Documentation : https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit/wiki
    --------------------------------------------------------------------------------------
		
    .PARAMETER $SourceWebUrl
	    [REQUIRED] The source web url to start exporting form. Be careful, if the source web is also a variation root site, all webs under target branches will be ignored.

    .PARAMETER OutputFileName
	    [REQUIRED] The output file name in XML format.

    .PARAMETER WebExclusionPatterns
	    [OPTIONAL] List of tokens to exclude. Applied on the title and template ID of each web. You can pass an array of string if you have multiple tokens. Regex expressions are supported.

	.PARAMETER ExcludeVariationTargetWebs
	    [OPTIONAL] If this parameter is specified, variations root branch sites (e.g /en, /fr) all sub sites generated by SharePoint variations mecanism for target labels 
				  (e.g /sites/<sitename>/fr/subweb where 'en' label is the variation source) will be ignored.

    .EXAMPLE
		    PS C:\> Export-DSPWebStructure -SourceWeb "http://<site>/sites/test/" -OutputFileName "C:\Test.xml" -WebExclusionPatterns "*STS#0","^Search" -ExcludeVariationTargetWebs

	.OUTPUTS
		Here is the output Structure XML schema.

        <!--This file was auto-generated by the Export-DSPWeb cmdlet at 02/27/2015 09:38:46-->
        <Configuration>
          <Web Name="Home" Path="/" Template="BLANKINTERNET#0" IsRoot="True" Owner="office\joe.blow">
            <Web Name="Sub Web 1" Path="subweb1" Template="CMSPUBLISHING#0">
              <Web Name="Sub Sub Web 1" Path="subweb11" Template="CMSPUBLISHING#0">
              </Web>
            </Web>
            <Web Name="Sub Web 2" Path="subweb2" Template="CMSPUBLISHING#0">
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
function Export-DSPWebStructure {
    
    [CmdletBinding()]
	param
	(
		[ValidateScript({(Get-SPWeb $_) -ne $null})]
		[Parameter(Mandatory=$true, Position=0)]
		[string]$SourceWebUrl,

		[Parameter(Mandatory=$true, Position=1)]
		[string]$OutputFileName,
      
        [Parameter(Mandatory=$false)]
		[System.Array]$WebExclusionPatterns,

		[Parameter(Mandatory=$false)]
		[switch]$ExcludeVariationTargetWebs
	)   

    function Process-WebNode
    {
        Param
	    (
            [Parameter(Mandatory=$false, Position=0)]
            [System.XML.XMLElement]$ParentXMLElement,

		    [Parameter(Mandatory=$true, Position=1)]
		    [Microsoft.SharePoint.SPWeb]$Web
        )

        $IsExcluded = $false

        # Check exclusion regex patterns
        if ($WebExclusionPatterns -ne $null)
        {
			$WebTemplateId = $Web.WebTemplate + "#" + $Web.Configuration
            if (($Web.Title | Select-String -Pattern $WebExclusionPatterns) -ne $null -or (($WebTemplateId | Select-String -Pattern $WebExclusionPatterns) -ne $null))
            {
                $Url =$Web.Url
                $Title = $Web.Title
                $Tokens = $WebExclusionPatterns -Join ","
                Write-Warning "Web with URL '$Url' with title '$Title' matches one of exclusion tokens '$Tokens'. Skipping export..."
                $IsExcluded = $true
            }
        }

        $WebUrl = $Web.ServerRelativeUrl

		if($IsVariationsEnabled)
		{
			# To know acces variations properties on a site, we need to cast the current Web to a PublishingWeb (works for all web templates) and check the Label property
			$CurrentPublishingWeb = [Microsoft.SharePoint.Publishing.PublishingWeb]::GetPublishingWeb($Web)

			if ($CurrentPublishingWeb -ne $null)
			{	
				if ($CurrentPublishingWeb.Label -ne $null)
				{
					if ($CurrentPublishingWeb.Label.IsSource)
					{
						# Case: The current web is the source variation site created by SharePoint (e.g /sites/<sitename>/en)	 
						if ([System.IO.Path]::GetFileNameWithoutExtension($Web.ServerRelativeUrl) -eq $CurrentPublishingWeb.Label.Title)
						{
								if ($ExcludeVariationTargetWebs.IsPresent)
								{
									$url =$Web.Url
									Write-Warning "Web with URL '$url' is a variation generated site. Skipping export..."
									$IsExcluded = $true 
								}                 
						}
						else
						{
							# Remove the variation label in the web URL to get the original one
							$WebUrl = $WebUrl.Replace(("/" +$CurrentPublishingWeb.Label.Title), [string]::Empty)
						}
					}
					else
					{	
						# Case: The current web is a peer variation site automatically created by SharePoint for a variation target label (e.g /sites/<sitename>/fr/subweb where 'en' label is the variation source)
						if ($ExcludeVariationTargetWebs.IsPresent)
						{
							$url =$Web.Url
							Write-Warning "Web with URL '$url' is a variation generated site. Skipping export..."
							$IsExcluded = $true
						}
					}              
				}		            
			}
		}
        
        if($IsExcluded -eq $false)
        {           
            # New Node
            [System.XML.XMLElement]$WebXMLElement= $XMLDocument.CreateElement("Web")
            $WebXMLElement.SetAttribute("Name", $Web.Title)
            $WebXMLElement.SetAttribute("Path", [System.IO.Path]::GetFileNameWithoutExtension($Web.ServerRelativeUrl))
            $WebXMLElement.SetAttribute("Template", $Web.WebTemplate + "#" + $Web.Configuration)
            $WebXMLElement.SetAttribute("Language", $Web.Language)

            if ($Web.IsRootWeb -eq $true)
            {
               $WebXMLElement.SetAttribute("IsRoot", "True") 
               $WebXMLElement.SetAttribute("Path", "/")

               # Remove claims identifier if present (no claims in MOSS 2007)
               $ownerLoginName = ($Web.Site.Owner.LoginName -replace "^(.*)[|]",[string]::Empty).ToLower()

               $WebXMLElement.SetAttribute("Owner", $ownerLoginName)           
            }

            $ParentXMLElement.appendChild($WebXMLElement)  
            $ParentXMLElement = $WebXMLElement                               
        }
        
        # Create sub web nodes
        $web.Webs | ForEach-Object {

            Process-WebNode $ParentXMLElement $_      
        }
    }

    # Load SharePoint assembly to be backward compatible with MOSS 2007
    [System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SharePoint")
	[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SharePoint.Publishing")
    Try
    {
        $Site = New-Object Microsoft.SharePoint.SPSite($SourceWebUrl)
        $SourceWeb = $Site.OpenWeb()

		if($SourceWeb.IsRootWeb)
        {
            $RootWeb = $SourceWeb
        }
        else
        {
            $RootWeb = $SourceWeb.Site.RootWeb
        }  

        # Check if the current web is a variation root site
        $VariationLabels =  $RootWeb | Get-VariationLabels
		$RootWebUrl = $RootWeb.Url

        if( $VariationLabels.Count -gt 0)
        {
			Write-Warning "SharePoint variations are enabled on web $RootWebUrl"
            $IsVariationsEnabled = $true
        }
        else
        {
			Write-Warning "SharePoint variations are not enabled on web $RootWebUrl"
            $IsVariationsEnabled = $false
        }

        # Create a new XML File
        [System.Xml.XmlDocument]$XMLDocument = New-Object System.Xml.XmlDocument
        $XmlDeclaration = $XMLDocument.CreateXmlDeclaration("1.0","UTF-8",$null)
        $XMLDocument.appendChild($XmlDeclaration)

        # Write header
        $XMLHeader = $XMLDocument.CreateComment("This file was auto-generated by the Export-DSPWebStructure cmdlet at "+ (Get-Date))
        $XMLDocument.AppendChild($XMLHeader)

        # Create the root node (an XML file must have a root node even if there is no web to export)
        [System.XML.XMLElement]$RootXMLElement= $XMLDocument.CreateElement("Configuration")
        $XMLDocument.appendChild($RootXMLElement)

        # Recursively create nodes
        Process-WebNode $RootXMLElement $SourceWeb

        # Create StreamWriter for encoding

        $StreamWriter = New-Object System.IO.StreamWriter($OutputFileName, $false, [System.Text.Encoding]::UTF8)

        # Save File
        $XMLDocument.Save($StreamWriter)

        $StreamWriter.Close()
        $RootWeb.Dispose()
        $Site.Dispose();
    }
    Catch
    {
        $ErrorMessage = $_.Exception.Message
        throw $ErrorMessage
    }
}

<#
    .SYNOPSIS
	    Import a web structure.
	
    .DESCRIPTION
	    Import a web structure under a parent web URL. Works with the same XML schema a the cmdlet Export-DSPWebStructure.
        Notes that site collection root web is ignored.
       
    --------------------------------------------------------------------------------------
    Module 'Dynamite.PowerShell.Toolkit'
    by: GSoft, Team Dynamite.
    > GSoft & Dynamite : http://www.gsoft.com
    > Dynamite Github : https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit
    > Documentation : https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit/wiki
    --------------------------------------------------------------------------------------
		
    .PARAMETER ParentUrl
	    [REQUIRED] The parent web URL web at which to start importing. The URL must be a valid SPWeb.

    .PARAMETER InputFileName
	    [REQUIRED] The web structure to import in XML format. Note that RootWeb node is ignored is present

    .PARAMETER Overwrite
	    [Optional] Removes a web and all its subwebs if already exists. By default, keep the existing web.

    .EXAMPLE
		    PS C:\> Import-DSPWebStructure -ParentUrl "http://<site>/sites/test/" -InputFileName "C:\Test.xml" -Overwrite

	.OUTPUTS

		Returns a list of created SPWeb. Note that existing webs are not returned.

		Here is the input Structure XML schema.

        <!--This file was auto-generated by the Export-DSPWeb cmdlet at 02/27/2015 09:38:46-->
        <Configuration>
          <Web Name="Home" Path="/" Template="BLANKINTERNET#0" IsRoot="True" Owner="office\joe.blow">
            <Web Name="Sub Web 1" Path="subweb1" Template="CMSPUBLISHING#0">
              <Web Name="Sub Sub Web 1" Path="subweb11" Template="CMSPUBLISHING#0">
              </Web>
            </Web>
            <Web Name="Sub Web 2" Path="subweb2" Template="CMSPUBLISHING#0">
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
function Import-DSPWebStructure {
    
    [CmdletBinding()]
	param
	(
		[ValidateScript({(Get-SPWeb $_) -ne $null})]
		[Parameter(Mandatory=$true)]
		[string]$ParentUrl,

		[ValidateScript({Test-Path $_ -PathType 'Leaf'})]
        [Parameter(Mandatory=$true)]
		[string]$InputFileName,

		[Parameter(Mandatory=$false)]
		[switch]$Overwrite
	)

    function CreateWeb
    {
        Param
	    (
            [Parameter(Mandatory=$true, Position=0)]
            $WebXMLElement,

            [Parameter(Mandatory=$true, Position=1)]
		    [string]$SourceUrl
        )

        if ($WebXMLElement.Web -ne $null)
        {
            $WebXMLElement.Web | Foreach-Object {

                $Path = $_.Path
                $Name = $_.Name
                $Template = $_.Template
                $IsRoot = [System.Convert]::ToBoolean($_.IsRoot)
                $Owner = $_.Owner
                $Language = $_.Language

                $Url = (([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($SourceUrl, $Path))).TrimEnd('/')
            
                if ($IsRoot)
                {
                    Write-Warning "The XML definition for web '$Name' is flagged as a site collection root web. Ignore..."   
                }
                else
                {
                    $web = Get-SPWeb -Identity $Url -ErrorAction SilentlyContinue
                    if ($web)
                    {
                       Write-Warning "Web $Url already exists"
                       if ($Overwrite)
                       {
                            Write-Host "'Overwrite' parameter was specified. Removing web $Url and its subwebs..." -NoNewline     
                            Remove-DSPWeb -WebUrl $web.Url -Recurse
                            Write-Host "Done!" -ForegroundColor Green
                            $newWeb = $true
                       }
                       else
                       {
                            $newWeb = $false
                       }
                    }
                    else
                    {
                        Write-Warning "Web $Url does not exist"
                        $newWeb = $true
                    }

                    if ($newWeb)
                    {        
                        Try
                        {                        
                            Write-Host "Creating $Url..." -NoNewline                              
                            $web = New-SPWeb -Url $Url -Template $Template -Language $Language -Name $Name
                            Write-Host "Done!" -ForegroundColor Green

							$script:WebList += $web
                        }
                        Catch
                        {
                            $ErrorMessage = $_.Exception.Message
			                Throw $ErrorMessage
                        }
                    } 
                } 

                # Create web recursively
                CreateWeb -WebXMLElement $_ -SourceUrl $Url
            }
        }
    }

    Try
    {
		$script:WebList = @()	

        [xml]$config = Get-Content $InputFileName
 
        CreateWeb -WebXMLElement $config.Configuration -SourceUrl $ParentUrl
		
		return $script:WebList        
	}
	Catch
    {
        $ErrorMessage = $_.Exception.Message
        Throw $ErrorMessage
    }	
}