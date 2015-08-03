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

	if ($UseParentTopNav)
	{
		Write-Warning "Usage of 'UseParentTopNav' as switch parameter is obsolete.  Please specify this parameter on the web XML configuration."
	}
		
	foreach ($web in $Webs.Web)
	{
		[string]$Name = $web.Name
		[string]$Path = $web.Path
		[string]$Template = $web.Template
		[string]$Language = $web.Language
		[string]$WebUseParentTopNavString = $web.UseParentTopNav
		[string]$UniquePermissionsString = $web.UniquePermissions

        # Parse boolean strings
        $WebUseParentTopNav = $false
        $UniquePermissions = $false
        [bool]::TryParse($WebUseParentTopNavString, [ref]$WebUseParentTopNav) | Out-Null
        [bool]::TryParse($UniquePermissionsString, [ref]$UniquePermissions) | Out-Null

        # Support legacy switch parameter for parent top navigation
        $WebUseParentTopNav = $UseParentTopNav
		
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
				$newWeb = New-SPWeb -Url $Url -Name $Name -UseParentTopNav:$WebUseParentTopNav -Language $Language -UniquePermissions:$UniquePermissions
				$newWeb.ApplyWebTemplate("$Template")					
				$allWebs += $newWeb
			}
			else
			{
				$newWeb = New-SPWeb -Url $Url -Template "$Template" -Name $Name -UseParentTopNav:$WebUseParentTopNav -Language $Language -UniquePermissions:$UniquePermissions						
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

	.PARAMETER IgnoreVariations
	    [OPTIONAL] If this parameter is specified, variations root branch sites (e.g /en, /fr) all sub sites generated by SharePoint variations mecanism for target labels 
				  (e.g /sites/<sitename>/fr/subweb where 'en' label is the variation source) will be ignored. The output structure reflects the orginal structure of root variation branch.

	.PARAMETER OutputFolderStructure
		[OPTIONAL] Generates a folder structure according to sites and subsites structure (base on title property). The strucuture in created under the parent directory of the OutputFileName.
				   Be careful, use the CurrentUICulture.

    .EXAMPLE
		    PS C:\> Export-DSPWebStructure -SourceWeb "http://<site>/sites/test/" -OutputFileName "C:\Test.xml" -WebExclusionPatterns "*STS#0","^Search" -IgnoreVariations

	.OUTPUTS
		Here is the output Structure XML schema for a standard export without variations

		<!--This file was auto-generated by the Export-DSPWeb cmdlet at 02/27/2015 09:38:46-->
        <Configuration>
          <Web Name="Home" Path="/" Template="BLANKINTERNET#0" IsRoot="True" Owner="office\joe.blow" WelcomePage="Pages/default.aspx">
            <Web Name="Sub Web 1" Path="subweb1" Template="CMSPUBLISHING#0" Language="1033" Label="en" WelcomePage="Pages/Home.aspx">
				<Variations>
					<TargetWeb Name="Sous Site 1" Path="soussite1" Language="1036" Label="fr" WelcomePage="Pages/Accueil.aspx"/>
				</Variations>
              <Web Name="Sub Sub Web 1" Path="subweb11" Template="CMSPUBLISHING#0" Language="1033" Label="en" WelcomePage="Pages/Home.aspx">
				<Variations>
					<TargetWeb Name="Sous Site 11" Path="soussite11" Language="1036" Label="fr" WelcomePage="Pages/Accueil.aspx"/>
				</Variations>
              </Web>
            </Web>
            <Web Name="Sub Web 2" Path="subweb2" Template="CMSPUBLISHING#0" Language="1033" Label="en" WelcomePage="Pages/Home.aspx">
				<Variations>
					<TargetWeb Name="Sous Site 2" Path="soussite2" Language="1036" Label="fr" WelcomePage="Pages/Accueil.aspx"/>
				</Variations>
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
		[Parameter(Mandatory=$true, Position=0)]
		[string]$SourceWebUrl,

		[Parameter(Mandatory=$true, Position=1)]
		[string]$OutputFileName,
      
        [Parameter(Mandatory=$false)]
		[System.Array]$WebExclusionPatterns,

		[Parameter(Mandatory=$false)]
		[ValidateScript({
			$Site = New-Object Microsoft.SharePoint.SPSite($SourceWebUrl)
			$SourceWeb = $Site.OpenWeb()
			If ($SourceWeb.IsRootWeb){			
				Return $true
			}
			Else {
				Throw "You can only use this parameter on a root web of a site collection"
			}
		})]

		[switch]$IgnoreVariations,

		[switch]$OutputFolderStructure
	)   

    function Process-WebNode
    {
        Param
	    (
            [Parameter(Mandatory=$false, Position=0)]
            [System.XML.XMLElement]$ParentXMLElement,

		    [Parameter(Mandatory=$true, Position=1)]
		    [Microsoft.SharePoint.SPWeb]$Web,

			[Parameter(Mandatory=$false, Position=2)]
		    $ParentFolder			
        )

        $IsExcluded = $false
		$VariationsWebs = @()
		
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
							if ($IgnoreVariations.IsPresent)
							{
								$url =$Web.Url
								Write-Warning "Web with URL '$url' is a variation generated site. Skipping export..."
								$IsExcluded = $true
							}                 
						}
						else
						{
							if ($IgnoreVariations.IsPresent)
							{
								if ($CurrentPublishingWeb.VariationPublishingWebUrls -ne $null)
								{
									# Get all peers URL
									$CurrentPublishingWeb.VariationPublishingWebUrls | ForEach-Object {
                        
										$PeerSite = New-Object Microsoft.SharePoint.SPSite($_)
										$PeerWeb = $PeerSite.OpenWeb()
										$PeerPublishingWeb = [Microsoft.SharePoint.Publishing.PublishingWeb]::GetPublishingWeb($PeerWeb)

										# Add variations sub node
										$VariatedWeb = New-Object -TypeName PSObject
										$VariatedWeb | Add-Member -Type NoteProperty -Name "Name" -Value $PeerWeb.Title
										$VariatedWeb | Add-Member -Type NoteProperty -Name "Path" -Value ([System.IO.Path]::GetFileNameWithoutExtension($PeerWeb.ServerRelativeUrl))
										$VariatedWeb | Add-Member -Type NoteProperty -Name "Language" -Value $PeerWeb.Language
										$VariatedWeb | Add-Member -Type NoteProperty -Name "Label" -Value $PeerPublishingWeb.Label.Title
                                        $VariatedWeb | Add-Member -Type NoteProperty -Name "WelcomePage" -Value $PeerWeb.RootFolder.WelcomePage

										$VariationsWebs += $VariatedWeb
							
										$PeerWeb.Dispose()
										$PeerSite.Dispose();
									}
								}
							}

							# Remove the variation label in the web URL to get the original one
							$WebUrl = $WebUrl.Replace(("/" +$CurrentPublishingWeb.Label.Title), [string]::Empty)
						}
					}
					else
					{	
						# Case: The current web is a peer variation site automatically created by SharePoint for a variation target label (e.g /sites/<sitename>/fr/subweb where 'en' label is the variation source)
						if ($IgnoreVariations.IsPresent)
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
			# Create a new folder if $GenerateFolderStructure switch parameter is on
			if ($ParentFolder -ne $null)
			{
				if (Test-Path $ParentFolder)
				{
					$ParentFolder = New-Item -ItemType Directory -Name $Web.Title -Path $ParentFolder -Force
				}
			}	

            # New Node
            [System.XML.XMLElement]$WebXMLElement= $XMLDocument.CreateElement("Web")
            $WebXMLElement.SetAttribute("Name", $Web.Title)
            $WebXMLElement.SetAttribute("Path", [System.IO.Path]::GetFileNameWithoutExtension($Web.ServerRelativeUrl))
            $WebXMLElement.SetAttribute("Template", $Web.WebTemplate + "#" + $Web.Configuration)
            $WebXMLElement.SetAttribute("Language", $Web.Language)
            $WebXMLElement.SetAttribute("WelcomePage", $Web.RootFolder.WelcomePage)

			if ($IsVariationsEnabled -and $IgnoreVariations.IsPresent)
			{
				$WebXMLElement.SetAttribute("Label", $CurrentPublishingWeb.Label.Title )
			}

            if ($Web.IsRootWeb -eq $true)
            {
               $WebXMLElement.SetAttribute("IsRoot", "True") 
               $WebXMLElement.SetAttribute("Path", "/")

               # Remove claims identifier if present (no claims in MOSS 2007)
               $ownerLoginName = ($Web.Site.Owner.LoginName -replace "^(.*)[|]",[string]::Empty).ToLower()

               $WebXMLElement.SetAttribute("Owner", $ownerLoginName)           
            }

			if ($VariationsWebs.Count -gt 0)
			{
				[System.XML.XMLElement]$VariationsXMLElement = $XMLDocument.CreateElement("Variations")

				$VariationsWebs | Foreach-Object {

						[System.XML.XMLElement]$VariationXMLElement = $XMLDocument.CreateElement("TargetWeb")	
						
						$VariationXMLElement.SetAttribute("Name", $_.Name)
						$VariationXMLElement.SetAttribute("Path", $_.Path)
						$VariationXMLElement.SetAttribute("Language", $_.Language)
						$VariationXMLElement.SetAttribute("Label", $_.Label)
                        $VariationXMLElement.SetAttribute("WelcomePage", $_.WelcomePage)
							
						$VariationsXMLElement.appendChild($VariationXMLElement)  	
				}

				$WebXMLElement.appendChild($VariationsXMLElement)				
			}

            $ParentXMLElement.appendChild($WebXMLElement)  
            $ParentXMLElement = $WebXMLElement                               
        }
        
        # Create sub web nodes
        $web.Webs | ForEach-Object {

            Process-WebNode $ParentXMLElement $_ $ParentFolder 
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
		if ($OutputFolderStructure)
		{
			 $ParentFolder = (Split-Path $OutputFileName -Parent)
		}
		else
		{
			$ParentFolder = $null
		}

        Process-WebNode $RootXMLElement $SourceWeb $ParentFolder

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

    .PARAMETER VariationLabel
	    [Optional] Update webs with properties of associated variated web for a specified label. This parameter is typically used after a variations synchronization.

    .PARAMETER UpdateWelcomePages
	    [Optional] Set the welcome page for webs according to the definition file. Works for new webs and updates with VariationLabel parameter. 
                   Be careful, a welcome page can be set even if it doesn't exists. Make sure it will be provisioned correctly later.

    .EXAMPLE
            # Overwrite a web structure according to the XML definition
		    PS C:\> Import-DSPWebStructure -ParentUrl "http://<site>/sites/test/" -InputFileName "C:\Test.xml" -Overwrite

            # Update existing webs with properties of peer webs according to the XML definition for the variation label 'fr' 
            PS C:\> Import-DSPWebStructure -ParentUrl "http://<site>/sites/test/" -InputFileName "C:\Test.xml" -VariationLabel 'fr'

            # Update welcome pages for web that don't exists
            PS C:\> Import-DSPWebStructure -ParentUrl "http://<site>/sites/test/" -InputFileName "C:\Test.xml" -UpdateWelcomePages

            # Update existing webs with properties of peer webs according to the XML definition for the variation label 'fr' (including welcome page) 
            PS C:\> Import-DSPWebStructure -ParentUrl "http://<site>/sites/test/" -InputFileName "C:\Test.xml" -UpdateWelcomePages -VariationLabel 'fr'

	.OUTPUTS

		Returns a list of created SPWeb. Note that existing webs are not returned.

		Here is the input Structure XML schema.

		<!--This file was auto-generated by the Export-DSPWeb cmdlet at 02/27/2015 09:38:46-->
        <Configuration>
          <Web Name="Home" Path="/" Template="BLANKINTERNET#0" IsRoot="True" Owner="office\joe.blow" WelcomePage="Pages/default.aspx">
            <Web Name="Sub Web 1" Path="subweb1" Template="CMSPUBLISHING#0" Language="1033" Label="en" WelcomePage="Pages/Home.aspx">
				<Variations>
					<TargetWeb Name="Sous Site 1" Path="soussite1" Language="1036" Label="fr" WelcomePage="Pages/Accueil.aspx"/>
				</Variations>
              <Web Name="Sub Sub Web 1" Path="subweb11" Template="CMSPUBLISHING#0" Language="1033" Label="en" WelcomePage="Pages/Home.aspx">
				<Variations>
					<TargetWeb Name="Sous Site 11" Path="soussite11" Language="1036" Label="fr" WelcomePage="Pages/Accueil.aspx"/>
				</Variations>
              </Web>
            </Web>
            <Web Name="Sub Web 2" Path="subweb2" Template="CMSPUBLISHING#0" Language="1033" Label="en" WelcomePage="Pages/Home.aspx">
				<Variations>
					<TargetWeb Name="Sous Site 2" Path="soussite2" Language="1036" Label="fr" WelcomePage="Pages/Accueil.aspx"/>
				</Variations>
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
    
    [CmdletBinding(DefaultParameterSetName='Overwrite')]
	param
	(
		[ValidateScript({(Get-SPWeb $_) -ne $null})]
        [Parameter(Mandatory=$true, ParameterSetName='Overwrite')]
        [Parameter(Mandatory=$true, ParameterSetName='Update')]
		[string]$ParentUrl,

		[ValidateScript({Test-Path $_ -PathType 'Leaf'})]
        [Parameter(Mandatory=$true, ParameterSetName='Overwrite')]
        [Parameter(Mandatory=$true, ParameterSetName='Update')]
		[string]$InputFileName,

        [Parameter(Mandatory=$false, ParameterSetName='Overwrite')]
		[switch]$Overwrite,
    
        [Parameter(Mandatory=$false, ParameterSetName='Update')]
		[string]$VariationLabel,
    
        [Parameter(Mandatory=$false, ParameterSetName='Overwrite')]
        [Parameter(Mandatory=$false, ParameterSetName='Update')]
		[switch]$UpdateWelcomePages
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
                $WelcomePageUrl = $_.WelcomePage			

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
							if ([string]::IsNullOrEmpty($VariationLabel) -eq $false)
							{
								# Looking for a web matching the specified variation label
								$TargetWeb = $_.Variations.TargetWeb | Where-Object {$_.Label -eq $VariationLabel}
				
								if ($TargetWeb -ne $null)	
								{
									$NewUrl = (([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($SourceUrl, $TargetWeb.Path))).TrimEnd('/')
									$NewWeb = Get-SPWeb -Identity $NewUrl -ErrorAction SilentlyContinue

									# Test if the site not already exists in the the site collection
									if($NewWeb -eq $null)
									{
										Write-Warning "Updating properties for web '$Url'"
										# Update web propeties
										$web.Title = $TargetWeb.Name
										$web.ServerRelativeUrl = $TargetWeb.Path
										$web.Update()

										#Update the URL
										$Url = $NewUrl
									}
									else
									{
										Write-Warning "Web with url '$NewUrl' already exists in the site collection. Update only the title..."  
																				
										$web.Title = $TargetWeb.Name
										$web.Update()
									}

                                    if ($UpdateWelcomePages)
                                    {
                                        $WelcomePageUrl = $TargetWeb.WelcomePage
                                    
                                        Write-Warning "Updating welcome page to '$WelcomePageUrl'..."
                                        $rootFolder = $web.RootFolder
                                        $rootFolder.WelcomePage = $WelcomePageUrl
                                        $rootFolder.Update()
                                        $web.Update()
							        }
								}	
								else
								{
									Write-Warning "Unable to find web with variation label '$VariationLabel' in the XML file. Skipping..."  
								}
							}
							
							$newWeb = $false
                       }
                    }
                    # If we specify a variation label, make sure we're only creating new webs of source label
                    elseif ([string]::IsNullOrEmpty($VariationLabel) -or $VariationLabel -eq $_.Label)
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

                            if ($UpdateWelcomePages)
                            {                                    
                                Write-Warning "Updating welcome page to '$WelcomePageUrl'..."
                                $rootFolder = $web.RootFolder
                                $rootFolder.WelcomePage = $WelcomePageUrl
                                $rootFolder.Update()
                                $web.Update()
							}

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