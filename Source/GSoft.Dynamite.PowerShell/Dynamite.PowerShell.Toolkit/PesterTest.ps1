# ----------------------
# Utility functions
# ----------------------
function New-SingleSite {

	Param
	(	
		[Parameter(Mandatory=$true, Position=0)]
		[ValidateNotNullOrEmpty()]
		[string]$SiteUrl,

		[Parameter(Mandatory=$false)]
		[string]$RootTemplateID = "BLANKINTERNET#0",

		[Parameter(Mandatory=$false)]
		[string]$OwnerAlias = [System.Security.Principal.WindowsIdentity]::GetCurrent().Name.ToLower()
	)

	$site = Get-SPSite $SiteUrl -ErrorAction SilentlyContinue
	if ($site -ne $null)
	{
		Remove-SPSite $site -Confirm:$false 
	}

	# Create test structure with the current account login to avoid access denied
	$site = New-SPSite $SiteUrl -OwnerAlias $OwnerAlias -Template $RootTemplateID -Name "RootWeb"
	
	return $site
}

function New-SubWebs {

	Param
	(	
		[Parameter(Mandatory=$true, Position=0)]
		[ValidateNotNullOrEmpty()]
		[Microsoft.SharePoint.SPWeb]$SourceWeb,

		[Parameter(Mandatory=$false)]
		[string]$TemplateID	= "BLANKINTERNET#0"
	)

	$webs = @()

	$subWeb1Url = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($SourceWeb.Url, "subweb1"))
	$subWeb2Url = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($SourceWeb.Url, "subweb2"))
	  
	# Create a site hierarchy
	$subWeb1 = if ((Get-SPWeb $subWeb1Url -ErrorAction SilentlyContinue) -eq $null) { New-SPWeb $subWeb1Url -Template $TemplateID -Name "SubWeb1"  } else { return Get-SPWeb $subWeb1Url }
	$subWeb2 = if ((Get-SPWeb $subWeb2Url -ErrorAction SilentlyContinue) -eq $null) { New-SPWeb $subWeb2Url -Template $TemplateID -Name "SubWeb2"  } else { return Get-SPWeb $subWeb2Url }

	$subWeb11Url = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($subWeb1.Url, "subweb11"))
	$subWeb11 = if ((Get-SPWeb $subWeb11Url -ErrorAction SilentlyContinue) -eq $null) { New-SPWeb $subWeb11Url -Template $TemplateID -Name "SubWeb11"  } else { return Get-SPWeb $subWeb11Url }

	$webs+=$subWeb1
	$webs+=$subWeb2
	$webs+=$subWeb11

	return $webs		
}

function New-SingleSiteNoSubsitesNoVariationsWithCustomLists {

	Param
	(	
		[Parameter(Mandatory=$true, Position=0)]
		[ValidateNotNullOrEmpty()]
		[string]$SiteUrl
	)
		
    $site = New-SingleSite -SiteUrl $SiteUrl
    New-CustomList -Web $site.RootWeb -ListName "CustomList" -TemplateName "GenericList"
    New-CustomList -Web $site.RootWeb -ListName "CustomLibrary" -TemplateName "DocumentLibrary"

    return $site
}

function New-SiteWithSubsitesNoVariations {

	Param
	(	
		[Parameter(Mandatory=$true, Position=0)]
		[ValidateNotNullOrEmpty()]
		[string]$SiteUrl,

		[Parameter(Mandatory=$true, Position=1)]
		[ValidateNotNullOrEmpty()]
		[string]$SubWebsTemplateID	
	)

	$site = New-SingleSite $SiteUrl
	New-SubWebs -SourceWeb $site.RootWeb -TemplateID $SubWebsTemplateID | Out-Null

	return Get-SPSite $site.Url
}

function New-SingleSiteNoSubsitesNoVariationsWithoutCustomLists {
		
	Param
	(	
		[Parameter(Mandatory=$true, Position=0)]
		[ValidateNotNullOrEmpty()]
		[string]$SiteUrl
	)	

    $site = New-SingleSite -SiteUrl $SiteUrl

    return $site
}
		
function New-SiteWithSubsitesAndVariationsWithCustomLists {

	Param
	(	
		[Parameter(Mandatory=$true, Position=0)]
		[ValidateNotNullOrEmpty()]
		[string]$SiteUrl,

		[Parameter(Mandatory=$true)]
		[ValidateNotNullOrEmpty()]
		[string]$SubWebsTemplateID,

		[Parameter(Mandatory=$true)]
		[ValidateScript({Test-Path $_ -PathType 'Leaf'})] 
		[string]$VariationConfigFilePath
	)
	
	$site = New-SingleSite $SiteUrl

	[xml]$config = Get-Content $VariationConfigFilePath
		
	# Create hierarchies on the root site
	New-DSPSiteVariations -Config $config.Variations -Site $site

	Set-VariationHierarchy -Site $site.Url

	Start-Sleep -s 5

	$sourceVariationSiteUrl = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($site.RootWeb.Url, "en"))

	# Sync Sub webs
	$SubWebs = New-SubWebs -SourceWeb (Get-SPWeb $sourceVariationSiteUrl) -TemplateID $SubWebsTemplateID

    $SubWebs| ForEach-Object {

            $_ | Sync-DSPWeb -LabelToSync 'fr'        
    }

    Wait-SPTimerJob -Name "VariationsSpawnSites" -Site $site
    Write-Warning "Waiting for 'VariationsSpawnSites' timer job to finish..."
    Start-Sleep -Seconds 60

    $SubWebs| ForEach-Object {

        New-CustomList -Web $_ -ListName "CustomList" -TemplateName "GenericList" | Sync-DSPList -LabelToSync 'fr'    
        New-CustomList -Web $_ -ListName "CustomLibrary" -TemplateName "DocumentLibrary" | Sync-DSPList -LabelToSync 'fr'         
    }  

    Wait-SPTimerJob -Name "VariationsSpawnSites" -Site $site
    Write-Warning "Waiting for 'VariationsSpawnSites' timer job to finish..."
    Start-Sleep -Seconds 60

	return Get-SPSite $site.Url
}

function New-CustomList {

    Param
	(
        [Parameter(Mandatory=$true, ValueFromPipeline=$true)]
		[Microsoft.SharePoint.SPWeb]$Web,

        [Parameter(Mandatory=$true)]
		[string]$ListName,
        
        [Parameter(Mandatory=$true)]
		[string]$TemplateName
	)

    $ListGuid = $Web.Lists.Add($ListName, [string]::Empty, [Microsoft.SharePoint.SPListTemplateType]$TemplateName);
    $Web.Update()

    return (Get-SPWeb $Web.Url).Lists[$ListGuid]
}

function Get-ListItem {

    Param
	(
        [Parameter(Mandatory=$true, ValueFromPipeline=$true)]
		[Microsoft.SharePoint.SPWeb]$Web,

        [Parameter(Mandatory=$true)]
		[string]$ListName,
        
        [Parameter(Mandatory=$true)]
		[string]$ItemTitle

	)

    $List = $Web.Lists.TryGetList($ListName)

    if ($List -ne $null)
    {
        $CamlQuery = New-Object -TypeName Microsoft.SharePoint.SPQuery
        $CamlQuery.Query = "<Where><Eq><FieldRef Name='Title' /><Value Type='Text'>" + $ItemTitle + "</Value></Eq></Where>"
        $CamlQuery.RowLimit = 1
        $Items = $List.GetItems($CamlQuery) 

        return $Items
    }
    else
    {
        return $null
    }
}

function New-SinglePublishingSiteNoSubsitesNoVariationsWithPagesAndFolders {
		
	Param
	(	
		[Parameter(Mandatory=$true, Position=0)]
		[ValidateNotNullOrEmpty()]
		[string]$SiteUrl
	)

	$site = New-SingleSite -SiteUrl $SiteUrl -RootTemplateID "BLANKINTERNET#0"
    
    New-PublishingPagesAndFolders $site.RootWeb "TestPage" -WelcomePageTitle "TestHomePage"

    return $site
}

function New-TeamSiteWithSubSites {

	Param
	(	
		[Parameter(Mandatory=$true, Position=0)]
		[ValidateNotNullOrEmpty()]
		[string]$SiteUrl
	)

	$site = New-SingleSite -SiteUrl $SiteUrl -RootTemplateID "STS#0"
	$subwebs = New-SubWebs -SourceWeb $site.RootWeb -TemplateID "STS#0"

    return $site
}

function New-PublishingPagesAndFolders {
    
    Param
    (
        [Parameter(Mandatory=$true)]
		[Microsoft.SharePoint.SPWeb]$Web,

        [Parameter(Mandatory=$true)]
		[array]$Pages,

        [Parameter(Mandatory=$true)]
		[string]$WelcomePageTitle    
    )
    
    $PageItems = @()
    $Folders = @()

    $PubWeb = [Microsoft.SharePoint.Publishing.PublishingWeb]::GetPublishingWeb($Web)
    $PagesLib = $PubWeb.PagesList

    $PubSite = New-Object Microsoft.SharePoint.Publishing.PublishingSite($Web.Site)

    $PageLayoutRelUrl = "/_catalogs/masterpage/BlankWebPartPage.aspx"

    # Get the PageLayouts Installed on the Publishing Site
    $Layouts = $PubSite.GetPageLayouts($False)

    $Folder1 = $PagesLib.AddItem("", [Microsoft.SharePoint.SPFileSystemObjectType]::Folder,"Folder1")
    $Folder1["Title"] = "Folder1"
    $Folder1.Update()

    $Folders += $Folder1
    $Folders += $PagesLib.RootFolder

     # Get our PageLayout
    $PageLayout = $Layouts | Where-Object { $_.ServerRelativeUrl -match $PageLayoutRelUrl }

    $Folders | ForEach-Object {

        $CurrentFolder = $_

        $Pages | ForEach-Object {

            $PageTitle = $_

            if ($CurrentFolder.Folder -ne $null)
            {
                $CurrentFolder = $CurrentFolder.Folder
            }
            
            # Add pages
            $Page = $PubWeb.AddPublishingPage($PageTitle +".aspx", $PageLayout, $CurrentFolder)
            $Page.Title = $PageTitle
            # Be careful, items without content will be ignored by the SharePoint variation system ;)
            $Page.ListItem[[Microsoft.SharePoint.Publishing.FieldId]::PublishingPageContent] = "Dummy Content"
            $Page.Update()

            $PageItems += $Page.ListItem

            # Check in the Page with Comments
            $Page.CheckIn("Test Comment")

            # Publish the Page With Comments
            $Page.ListItem.File.Publish("Test Publish Comment")
        }
    }

    # Add Welcome page
    $WelcomePage = $PubWeb.AddPublishingPage($WelcomePageTitle +".aspx", $PageLayout,  $PagesLib.RootFolder)
    $WelcomePage.Title = $WelcomePageTitle
    $WelcomePage.ListItem[[Microsoft.SharePoint.Publishing.FieldId]::PublishingPageContent] = "Dummy Content"
    $WelcomePage.Update()
    # Check in the Page with Comments
    $WelcomePage.CheckIn("Test Comment")
    # Publish the Page With Comments
    $WelcomePage.ListItem.File.Publish("Test Publish Comment")

    $PageItems += $WelcomePage.ListItem

    $newDefaultPageFile = $PubWeb.Web.GetFile($WelcomePage.Url);
    $PubWeb.DefaultPage = $newDefaultPageFile 

    $PubWeb.Update()

    return $PageItems
}
		
function New-PublishingSiteWithSubsitesNoVariationsWithPagesAndFolders {

	Param
	(	
		[Parameter(Mandatory=$true, Position=0)]
		[ValidateNotNullOrEmpty()]
		[string]$SiteUrl
	)

	$site = New-SingleSite -SiteUrl $SiteUrl -RootTemplateID "BLANKINTERNET#0"
	$subwebs = New-SubWebs -SourceWeb $site.RootWeb -TemplateID "BLANKINTERNET#0"
    
    $i = 1
    $subwebs | ForEach-Object {
    
        New-PublishingPagesAndFolders $_ "TestPage" -WelcomePageTitle "TestHomePageSubWeb$i"
        $i++
    }

	return Get-SPSite $site.Url
}

function New-PublishingSiteWithSubsitesWithVariationsWithPagesAndFolders {

	Param
	(	
		[Parameter(Mandatory=$true, Position=0)]
		[ValidateNotNullOrEmpty()]
		[string]$SiteUrl,

		[Parameter(Mandatory=$true)]
		[ValidateNotNullOrEmpty()]
		[string]$SubWebsTemplateID,

		[Parameter(Mandatory=$true)]
		[ValidateScript({Test-Path $_ -PathType 'Leaf'})] 
		[string]$VariationConfigFilePath
	)
	
	$site = New-SingleSite $SiteUrl

	[xml]$config = Get-Content $VariationConfigFilePath
		
	# Create hierarchies on the root site
	New-DSPSiteVariations -Config $config.Variations -Site $site

	Set-VariationHierarchy -Site $site.Url

	Start-Sleep -s 5

	$sourceVariationSiteUrl = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($site.RootWeb.Url, "en"))

	# Sync Sub webs
	$SubWebs = New-SubWebs -SourceWeb (Get-SPWeb $sourceVariationSiteUrl) -TemplateID "BLANKINTERNET#0"

    $SubWebs| ForEach-Object {

            $_ | Sync-DSPWeb -LabelToSync 'fr'        
    }

    Wait-SPTimerJob -Name "VariationsSpawnSites" -Site $site
    Write-Warning "Waiting for 'VariationsSpawnSites' timer job to finish..."
    Start-Sleep -Seconds 60

    $i = 1
    $SubWebs | ForEach-Object {
		
		$Web = $_
        $Items = New-PublishingPagesAndFolders $Web "TestPage" -WelcomePageTitle "TestHomePageSubWeb$i"

        $Items | ForEach-Object {

			$Item = $_
            Sync-DSPItem -VariationListItem $Item 
        }
        $i++
    }

    # Be careful, you must run the timer job VariationsPropagatePage instead of VariationsPropagateListItem
    # Dont' forget to set EnableAutoSpawn="true" in variations settings to set the "Publish" action on pages as a trigger for synchronization
    Wait-SPTimerJob -Name "VariationsPropagatePage" -Site $site
	Write-Verbose "Waiting for 'VariationsPropagatePage' timer job to finish..."
	Start-Sleep -Seconds 60

	return Get-SPSite $site.Url
}

function Add-Document {

	Param
	(	
        [Parameter(Mandatory=$true, ValueFromPipeline=$true)]
		[Microsoft.SharePoint.SPWeb]$Web,

        [Parameter(Mandatory=$true)]
		[string]$DocumentLibraryName,

		[Parameter(Mandatory=$true)]
		$File
	)

    $FileStream = ([System.IO.FileInfo] (Get-Item $File.FullName)).OpenRead()

    #Add file
    $List =  $Web.Lists.TryGetList("$DocumentLibraryName")

    if ($List -ne $null)
    {
        $Folder = $List.RootFolder
        $FileUrl = $Folder.Url + "/" + $File.Name

        $UploadedFile = $Folder.Files.Add($FileUrl, [System.IO.Stream]$FileStream, $true)

        #Close file stream
        $FileStream.Close();
    }

    return $UploadedFile
}


        

