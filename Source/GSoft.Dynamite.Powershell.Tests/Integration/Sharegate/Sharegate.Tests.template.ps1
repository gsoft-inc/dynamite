$here = Split-Path -Parent $MyInvocation.MyCommand.Path

# Script under test (sut)
$sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path).Replace(".Tests.", ".")
$sutPath = "$here\..\..\GSoft.Dynamite.PowerShell\$sut"
$sourceFolderPath = (Get-Location).Path + "\source"
$destFolderPath = (Get-Location).Path + "\destination"

# ----------------------
# Tests configuration
# ----------------------

$tempSiteCollection   = "[[DSP_TempSiteCollection]]"
$webApplication       = "[[DSP_WebApplicationUrl]]"
$currentAccountName   = ("[[DSP_CurrentAccount]]").ToLower()
$variationsConfigFile   = Join-Path -Path "$here" -ChildPath "[[DSP_VariationsConfigFile]]"
$siteUrl = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($webApplication, $tempSiteCollection))

# ----------------------
# Utility functions
# ----------------------
function CreateSingleSite {

	$site = Get-SPSite $siteUrl -ErrorAction SilentlyContinue
	if ($site -ne $null)
	{
		Remove-SPSite $site -Confirm:$false 	
	}

	if($webApplication -ne $null)
	{
		# Create test structure with the current account login to avoid access denied
		$site = New-SPSite $siteUrl -OwnerAlias $currentAccountName -Template "BLANKINTERNET#0" -Name "RootWeb"
	}

	return $site
}

function CreateSubWebs {
	param
	(	
		[Parameter(Mandatory=$true, Position=0)]
		[ValidateNotNullOrEmpty()]
		[Microsoft.SharePoint.SPWeb]$SourceWeb
	)
	$webs = @()

	$subWeb1Url = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($SourceWeb.Url, "subweb1"))
	$subWeb2Url = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($SourceWeb.Url, "subweb2"))
	  
	# Create a site hierarchy
	$subWeb1 = if ((Get-SPWeb $subWeb1Url -ErrorAction SilentlyContinue) -eq $null) { New-SPWeb $subWeb1Url -Template "BLANKINTERNET#0" -Name "SubWeb1"  } else { return Get-SPWeb $subWeb1Url }
	$subWeb2 = if ((Get-SPWeb $subWeb2Url -ErrorAction SilentlyContinue) -eq $null) { New-SPWeb $subWeb2Url -Template "BLANKINTERNET#0" -Name "SubWeb2"  } else { return Get-SPWeb $subWeb2Url }

	$subWeb11Url = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($subWeb1.Url, "subweb11"))
	$subWeb11 = if ((Get-SPWeb $subWeb11Url -ErrorAction SilentlyContinue) -eq $null) { New-SPWeb $subWeb11Url -Template "BLANKINTERNET#0" -Name "SubWeb11"  } else { return Get-SPWeb $subWeb11Url }

	$webs+=$subWeb1
	$webs+=$subWeb2
	$webs+=$subWeb11

	return $webs		
}

function CreateSingleSiteNoSubsitesNoVariationsWithCustomLists {
		
    $site = CreateSingleSite
    CreateCustomList -Web $site.RootWeb -ListName "CustomList" -TemplateName "GenericList"
    CreateCustomList -Web $site.RootWeb -ListName "CustomLibrary" -TemplateName "DocumentLibrary"

    return $site
}

function CreateSingleSiteNoSubsitesNoVariationsWithoutCustomLists {
		
    $site = CreateSingleSite

    return $site
}
		
function CreateSiteWithSubsitesAndVariationsWithCustomLists{

	$site = CreateSingleSite
	[xml]$config = Get-Content $variationsConfigFile
		
	$webApp = Get-SPWebApplication $webApplication

	# Create hierarchies on the root site
	New-DSPSiteVariations -Config $config.Variations -Site $site

	Set-VariationHierarchy -Site $site.Url

	Start-Sleep -s 5

	$sourceVariationSiteUrl = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($site.RootWeb.Url, "en"))

	# Sync Sub webs
	$SubWebs = CreateSubWebs -SourceWeb (Get-SPWeb $sourceVariationSiteUrl) 

    $SubWebs| ForEach-Object {

            $_ | Sync-DSPWeb -LabelToSync 'fr'        
    }

    Wait-SPTimerJob -Name "VariationsSpawnSites" -WebApplication $webApp
    Write-Warning "Waiting for 'VariationsSpawnSites' timer job to finish..."
    Start-Sleep -Seconds 60

    $SubWebs| ForEach-Object {

        CreateCustomList -Web $_ -ListName "CustomList" -TemplateName "GenericList" | Sync-DSPList -LabelToSync 'fr'    
        CreateCustomList -Web $_ -ListName "CustomLibrary" -TemplateName "DocumentLibrary" | Sync-DSPList -LabelToSync 'fr'         
    }  

    Wait-SPTimerJob -Name "VariationsSpawnSites" -WebApplication $webApp
    Write-Warning "Waiting for 'VariationsSpawnSites' timer job to finish..."
    Start-Sleep -Seconds 60

	return Get-SPSite $site.Url
}

function CreateCustomList {

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

function GetListItem {

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

Describe "Import-DSPData" -Tags "Local", "Slow" {

    Import-Module Sharegate

	Context "Parameters are invalid" 	{

		It "should throw an error if folder path if invalid " {		
            { Import-DSPData -FromFolder "C:\DoesntExist" -ToUrl $siteUrl } | Should Throw
		}

        It "should throw an error if target URL is invalid " {
			{ Import-DSPData -FromFolder "C:\Users" -ToUrl "http:///%!" } | Should Throw
		}
	}

    Context "Mirror structure between folders and site and lists" 	{

		It "[Single site] should import images, documents and list items including custom items and reusable content items into the target URL site" {
       
            Write-Host "     --Test Setup--"
            $folderPath = Join-Path -Path "$here" -ChildPath ".\SingleSite"

            # Create site hierarchy
		    CreateSingleSiteNoSubsitesNoVariationsWithCustomLists
            
        	Import-DSPData -FromFolder $folderPath -ToUrl $siteUrl		

            $Web = Get-SPWeb $siteUrl

            # Check for an image
            GetListItem -Web $Web -ListName "Images" -ItemTitle "TestImage" | Should Not be $null

            # Check for a page
            GetListItem -Web $Web -ListName "Pages" -ItemTitle "TestPage" | Should Not be $null

            # Check for a reusable content
            GetListItem -Web $Web -ListName "Reusable Content" -ItemTitle "TestReusableContent" | Should Not be $null

            # Check for a list item into a custom list
            GetListItem -Web $Web -ListName "CustomList" -ItemTitle "TestListItem" | Should Not be $null

            # Check for a document item into a custom library
            GetListItem -Web $Web -ListName "CustomLibrary" -ItemTitle "TestDocument" | Should Not be $null   
            
            Write-Host "     --Tests Teardown--"
	        Remove-SPSite $siteUrl -Confirm:$false   
		}

        It "[Multiples sites and sub sites with variations] should import images, documents and list items including custom items and reusable content items across the whole site structure"   {            
            Write-Host "     --Test Setup--"
            $folderPath = Join-Path -Path "$here" -ChildPath ".\MultipleSites"

		    # Create site hierarchy
		    CreateSiteWithSubsitesAndVariationsWithCustomLists
            
            Import-DSPData -FromFolder $folderPath -ToUrl $siteUrl		

            $RootWeb = Get-SPWeb $siteUrl

            # French URL to check
            $subWeb1UrlFr = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($siteUrl, "fr/subweb1"))
	        $subWeb2UrlFr = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($siteUrl, "fr/subweb2"))
            $subWeb11Fr = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($siteUrl, "fr/subweb1/subweb11"))

            # English URL to check
            $subWeb1UrlEn = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($siteUrl, "en/subweb1"))
	        $subWeb2UrlEn = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($siteUrl, "en/subweb2"))
            $subWeb11En = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($siteUrl, "en/subweb1/subweb11"))

            $SubWebs = @($subWeb1UrlFr, $subWeb2UrlFr, $subWeb11Fr, $subWeb1UrlEn, $subWeb2UrlEn, $subWeb11En)

            # Check for a reusable content at root web
            GetListItem -Web $RootWeb -ListName "Reusable Content" -ItemTitle "TestReusableContent" | Should Not be $null

            $SubWebs | ForEach-Object {

                $CurrentWeb = Get-SPWeb $_

                # Check for an image
                GetListItem -Web $CurrentWeb -ListName "Images" -ItemTitle "TestImage" | Should Not be $null

                # Check for a page
                GetListItem -Web $CurrentWeb -ListName "Pages" -ItemTitle "TestPage" | Should Not be $null

                # Check for a list item into a custom list
                GetListItem -Web $CurrentWeb -ListName "CustomList" -ItemTitle "TestListItem" | Should Not be $null

                # Check for a document item into a custom library
                GetListItem -Web $CurrentWeb -ListName "CustomLibrary" -ItemTitle "TestDocument" | Should Not be $null
            }            Write-Host "     --Tests Teardown--"
	        Remove-SPSite $siteUrl -Confirm:$false        }
	}

    Context "Duplicates behavior" {
      
        It "[Single site] should duplicate items if no custom keys specified" {

            Write-Host "     --Test Setup--"

            # Create site hierarchy
		    CreateSingleSiteNoSubsitesNoVariationsWithCustomLists
            $folderPath = Join-Path -Path "$here" -ChildPath ".\SingleCustomList"

            Import-DSPData -FromFolder $folderPath -ToUrl $siteUrl

            $folderPath = Join-Path -Path "$here" -ChildPath ".\SingleCustomListDuplicates"

            Import-DSPData -FromFolder $folderPath -ToUrl $siteUrl

            $Web = Get-SPWeb $siteUrl

            # Check for a list item into a custom list#
            GetListItem -Web $Web -ListName "CustomList" -ItemTitle "TestListItem" | Should Not be $null

            GetListItem -Web $Web -ListName "CustomList" -ItemTitle "TestListItemDuplicate" | Should Not be  $null
            
            Write-Host "     --Tests Teardown--"
	        Remove-SPSite $siteUrl -Confirm:$false             
        }

        It "[Single site] should not duplicate items with the same custom keys passed as parameter even if the title and created date are different" {

            Write-Host "     --Test Setup--"

            # Create site hierarchy
		    CreateSingleSiteNoSubsitesNoVariationsWithCustomLists
            $folderPath = Join-Path -Path "$here" -ChildPath ".\SingleCustomList"

            Import-DSPData -FromFolder $folderPath -ToUrl $siteUrl

            $folderPath = Join-Path -Path "$here" -ChildPath ".\SingleCustomListDuplicates"

            Import-DSPData -FromFolder $folderPath -ToUrl $siteUrl -Keys "ID","ContentType"

            $Web = Get-SPWeb $siteUrl

            # Check for a list item into a custom list#
            GetListItem -Web $Web -ListName "CustomList" -ItemTitle "TestListItem" | Should Not be $null

            GetListItem -Web $Web -ListName "CustomList" -ItemTitle "TestListItemDuplicate" | Should be  $null
            
            Write-Host "     --Tests Teardown--"
	        Remove-SPSite $siteUrl -Confirm:$false             
        }

        It "[Single site] should not ignore property if no property template is specified (Sharegate default behavior)" {

            Write-Host "     --Test Setup--"

            # Create site hierarchy
		    CreateSingleSiteNoSubsitesNoVariationsWithCustomLists
            $folderPath = Join-Path -Path "$here" -ChildPath ".\SingleSite"

            Import-DSPData -FromFolder $folderPath -ToUrl $siteUrl 
            $Web = Get-SPWeb $siteUrl
            
            $Item = GetListItem -Web $Web -ListName "Pages" -ItemTitle "TestPage" 

            # Check for a list item into a custom list#
            $Item | Should Not be $null
            $Item["Comments"] | Should be "TestIgnore"
            
            Write-Host "     --Tests Teardown--"
	        Remove-SPSite $siteUrl -Confirm:$false             
        }

        It "[Single site] should ignore property configured in the property template file" {

            Write-Host "     --Test Setup--"

            # Create site hierarchy
		    CreateSingleSiteNoSubsitesNoVariationsWithCustomLists
            $folderPath = Join-Path -Path "$here" -ChildPath ".\SingleSite"
            $propertytemplateFile = Join-Path -Path "$here" -ChildPath ".\TestPropertyTemplate.sgt"

            Import-DSPData -FromFolder $folderPath -ToUrl $siteUrl -PropertyTemplateFile $propertytemplateFile -TemplateName "TestTemplate"

            $Web = Get-SPWeb $siteUrl
            
            $Item = GetListItem -Web $Web -ListName "Pages" -ItemTitle "TestPage" 

            # Check for a list item into a custom list#
            $Item | Should Not be $null
            $Item["Comments"] | Should BeNullOrEmpty 
            
            Write-Host "     --Tests Teardown--"
	        Remove-SPSite $siteUrl -Confirm:$false             
        }
    }

    Context "Non mirror structure between folders and sites" {
    
        It "[Single site] should ignore not found lists and webs but import other found artefacts" {

            Write-Host "     --Test Setup--"
            $folderPath = Join-Path -Path "$here" -ChildPath ".\NonMirror"

            # Create site hierarchy
		    CreateSingleSiteNoSubsitesNoVariationsWithoutCustomLists

        	Import-DSPData -FromFolder $folderPath -ToUrl $siteUrl		

            $Web = Get-SPWeb $siteUrl

            # Check for an image
            GetListItem -Web $Web -ListName "Images" -ItemTitle "TestImage" | Should Not be $null

            # Check for a page
            GetListItem -Web $Web -ListName "Pages" -ItemTitle "TestPage" | Should Not be $null

            # Check for a list item into a custom list
            GetListItem -Web $Web -ListName "CustomList" -ItemTitle "TestListItem" | Should Be $null

            # Check for a document item into a custom library
            GetListItem -Web $Web -ListName "CustomLibrary" -ItemTitle "TestDocument" | Should Be $null

            Write-Host "     --Tests Teardown--"
	        Remove-SPSite $siteUrl -Confirm:$false 
        }
    }
}
