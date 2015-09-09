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
$variationsConfigFile   = Join-Path -Path $here -ChildPath "[[DSP_VariationsConfigFile]]"
$LogFilePath = Join-Path -Path $here -ChildPath "./Logs"
$siteUrl = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($webApplication, $tempSiteCollection))


Describe "Import-DSPData" -Tags "Local", "Slow" {

    # ----------------------
    # Import-DSPData
    # ----------------------
    Import-Module Sharegate

    Context "Parameters are invalid" 	{

        It "should throw an error if folder path if invalid " {
        
            { Import-DSPData -FromFolder "C:\DoesntExist" -ToUrl $siteUrl -LogFolder $LogFilePath } | Should Throw
        }

        It "should throw an error if target URL is invalid " {

            { Import-DSPData -FromFolder "C:\Users" -ToUrl "http:///%!" -LogFolder $LogFilePath } | Should Throw
        }
    }

    Context "Mirror structure between folders and site and lists" 	{

        It "[Single site] should import images, documents and list items including custom items and reusable content items into the target URL site" {
       
            Write-Host "     --Test Setup--"
            $folderPath = Join-Path -Path "$here" -ChildPath ".\SingleSite"

            # Create site hierarchy
            $site = New-SingleSiteNoSubsitesNoVariationsWithCustomLists -SiteUrl $siteUrl
            
            Import-DSPData -FromFolder $folderPath -ToUrl $siteUrl -LogFolder $LogFilePath	 

            $Web = Get-SPWeb $siteUrl

            # Check for an image
            Get-ListItem -Web $Web -ListName "Images" -ItemTitle "TestImage" | Should Not be $null

            # Check for a page
            Get-ListItem -Web $Web -ListName "Pages" -ItemTitle "TestPage" | Should Not be $null

            # Check for a reusable content
            Get-ListItem -Web $Web -ListName "Reusable Content" -ItemTitle "TestReusableContent" | Should Not be $null

            # Check for a list item into a custom list
            Get-ListItem -Web $Web -ListName "CustomList" -ItemTitle "TestListItem" | Should Not be $null

            # Check for a document item into a custom library
            Get-ListItem -Web $Web -ListName "CustomLibrary" -ItemTitle "TestDocument" | Should Not be $null   
            
            Write-Host "     --Tests Teardown--"
            Remove-SPSite $siteUrl -Confirm:$false  
            Remove-Item $LogFilePath -Recurse -Confirm:$false
        }

        It "[Multiples sites and sub sites with variations] should import images, documents and list items including custom items and reusable content items across the whole site structure"   {
            
            Write-Host "     --Test Setup--"
            $folderPath = Join-Path -Path "$here" -ChildPath ".\MultipleSites"

            # Create site hierarchy
            $site = New-SiteWithSubsitesAndVariationsWithCustomLists -SiteUrl $siteUrl -SubWebsTemplateID "BLANKINTERNET#0" -VariationConfigFilePath $variationsConfigFile
            
            Import-DSPData -FromFolder $folderPath -ToUrl $siteUrl -LogFolder $LogFilePath		

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
            Get-ListItem -Web $RootWeb -ListName "Reusable Content" -ItemTitle "TestReusableContent" | Should Not be $null

            $SubWebs | ForEach-Object {

                $CurrentWeb = Get-SPWeb $_

                # Check for an image
                Get-ListItem -Web $CurrentWeb -ListName "Images" -ItemTitle "TestImage" | Should Not be $null

                # Check for a page
                Get-ListItem -Web $CurrentWeb -ListName "Pages" -ItemTitle "TestPage" | Should Not be $null

                # Check for a list item into a custom list
                Get-ListItem -Web $CurrentWeb -ListName "CustomList" -ItemTitle "TestListItem" | Should Not be $null

                # Check for a document item into a custom library
                Get-ListItem -Web $CurrentWeb -ListName "CustomLibrary" -ItemTitle "TestDocument" | Should Not be $null
            }

            Write-Host "     --Tests Teardown--"
            Remove-SPSite $siteUrl -Confirm:$false
            Remove-Item $LogFilePath -Recurse -Confirm:$false
        }
    }

    Context "Duplicates behavior" {
      
        It "[Single site] should duplicate items if no custom composite key is specified" {

            Write-Host "     --Test Setup--"

            # Create site hierarchy
            $site = New-SingleSiteNoSubsitesNoVariationsWithCustomLists -SiteUrl $siteUrl

            $folderPath = Join-Path -Path "$here" -ChildPath ".\SingleCustomList"

            Import-DSPData -FromFolder $folderPath -ToUrl $siteUrl -LogFolder $LogFilePath

            $folderPath = Join-Path -Path "$here" -ChildPath ".\SingleCustomListDuplicates"

            Import-DSPData -FromFolder $folderPath -ToUrl $siteUrl -LogFolder $LogFilePath

            $Web = Get-SPWeb $siteUrl

            # Check for a list item into a custom list#
            Get-ListItem -Web $Web -ListName "CustomList" -ItemTitle "TestListItem" | Should Not be $null

            Get-ListItem -Web $Web -ListName "CustomList" -ItemTitle "TestListItemDuplicate" | Should Not be  $null
            
            Write-Host "     --Tests Teardown--"
            Remove-SPSite $siteUrl -Confirm:$false    
            Remove-Item $LogFilePath -Recurse -Confirm:$false        
        }

        It "[Single site] should not duplicate items with the same custom composite key when copy settings is set to 'Skip'" {

            Write-Host "     --Test Setup--"

            # Create site hierarchy
            $site = New-SingleSiteNoSubsitesNoVariationsWithCustomLists -SiteUrl $siteUrl

            $folderPath = Join-Path -Path "$here" -ChildPath ".\SingleCustomList"

            Import-DSPData -FromFolder $folderPath -ToUrl $siteUrl -LogFolder $LogFilePath

            $folderPath = Join-Path -Path "$here" -ChildPath ".\SingleCustomListDuplicates"

            # Copy settings
            $CopySettings = New-CopySettings -OnContentItemExists Skip

            # Custom property mapping settings
            $MappingSettings = New-MappingSettings 

            # Remove default keys
            Set-PropertyMapping -MappingSettings $MappingSettings -Source Title -Destination Title
            Set-PropertyMapping -MappingSettings $MappingSettings -Source Created -Destination Created

            # Add custom keys
            Set-PropertyMapping -MappingSettings $MappingSettings -Source ID -Destination ID -Key
            Set-PropertyMapping -MappingSettings $MappingSettings -Source ContentType -Destination ContentType -Key

            Import-DSPData -FromFolder $folderPath -ToUrl $siteUrl -MappingSettings $MappingSettings -CopySettings $CopySettings -LogFolder $LogFilePath

            $Web = Get-SPWeb $siteUrl

            # Check for a list item into a custom list#
            Get-ListItem -Web $Web -ListName "CustomList" -ItemTitle "TestListItem" | Should Not be $null

            Get-ListItem -Web $Web -ListName "CustomList" -ItemTitle "TestListItemDuplicate" | Should be $null
            
            Write-Host "     --Tests Teardown--"
            Remove-SPSite $siteUrl -Confirm:$false    
            Remove-Item $LogFilePath -Recurse -Confirm:$false        
        }
    }

    Context "Non mirror structure between folders and sites" {
    
        It "[Single site] should ignore not found lists and webs but import other found artefacts" {

            Write-Host "     --Test Setup--"
            $folderPath = Join-Path -Path "$here" -ChildPath ".\NonMirror"

            # Create site hierarchy
            $site = New-SingleSiteNoSubsitesNoVariationsWithCustomLists -SiteUrl $siteUrl

            Import-DSPData -FromFolder $folderPath -ToUrl $siteUrl -LogFolder $LogFilePath	

            $Web = Get-SPWeb $siteUrl

            # Check for an image
            Get-ListItem -Web $Web -ListName "Images" -ItemTitle "TestImage" | Should Not be $null

            # Check for a page
            Get-ListItem -Web $Web -ListName "Pages" -ItemTitle "TestPage" | Should Not be $null

            # Check for a list item into a custom list
            Get-ListItem -Web $Web -ListName "CustomList" -ItemTitle "TestListItem" | Should Not Be $null

            # Check for a document item into a custom library
            Get-ListItem -Web $Web -ListName "CustomLibrary" -ItemTitle "TestDocument" | Should Be $null

            Write-Host "     --Tests Teardown--"
            Remove-SPSite $siteUrl -Confirm:$false 
            Remove-Item $LogFilePath -Recurse -Confirm:$false
        }
    }
}

Describe "Copy-DSPData" -Tags "Local", "Slow" {

    # ----------------------
    # Import-DSPData
    # ----------------------
    Import-Module Sharegate

    Context "Parameters are invalid" 	{

        It "should throw an error if source URL if invalid " {
        
            { Copy-DSPData -FromUrl "http://webapp/fakesite" -ToUrl $siteUrl -LogFolder $LogFilePath } | Should Throw
        }

        It "should throw an error if destination URL is invalid " {

            { Copy-DSPData -FromUrl $siteUrl -ToUrl "http://webapp/fakesite" -LogFolder $LogFilePath } | Should Throw
        }
    }

    Context "Copy content between single sites" {

        Write-Host "     --Test Setup--"

        $SourceSiteUrl = $webApplication+ "/sites/sourcesite"
        $DestinationSiteUrl = $webApplication+ "/sites/destinationsite"
        
        $SourceSite = New-SingleSiteNoSubsitesNoVariationsWithoutCustomLists $SourceSiteUrl

        # Add a test page in source web
        New-PublishingPagesAndFolders $SourceSite.RootWeb "SourcePage1" -WelcomePageTitle "SourceHomePage"

        # Add a test document in source web
        $FilePath = Join-Path -Path "$here" -ChildPath ".\Document1.docx"
        $File = Add-Document $SourceSite.RootWeb "Documents" (Get-Item $FilePath)
        $File.Item["Title"] = "Document1"
        $File.Item.Update()

        # Add a test image in source web
        $FilePath = Join-Path -Path "$here" -ChildPath ".\Image1.jpg"
        $File = Add-Document $SourceSite.RootWeb "Images" (Get-Item $FilePath)
        $File.Item["Title"] = "Image1"
        $File.Item.Update()

        # Add custom lists with an item
        $List = New-CustomList $SourceSite.RootWeb "CustomList1" "GenericList"
        $NewItem = $List.AddItem()
        $NewItem["Title"] = "ItemList1"
        $NewItem.Update()

        $List = New-CustomList $SourceSite.RootWeb "CustomList2" "GenericList"
        $NewItem = $List.AddItem()
        $NewItem["Title"] = "ItemList2"
        $NewItem.Update()
    
        $DestinationSite = New-SingleSite $DestinationSiteUrl

        # Add a list with the same name as source in the destination site
        New-CustomList $DestinationSite.RootWeb "CustomList1" "GenericList" | Out-Null
        New-CustomList $DestinationSite.RootWeb "CustomList2" "GenericList" | Out-Null

        It "[Single Site] should only copy items for lists specified in the 'ListNames' parameter" {

            Copy-DSPData -FromUrl $SourceSiteUrl -ToUrl $DestinationSiteUrl -LogFolder $LogFilePath -ListNames "CustomList1"

            # Check for a list item into a custom list 1
            Get-ListItem -Web $DestinationSite.RootWeb -ListName "CustomList1" -ItemTitle "ItemList1" | Should Not be $null

            # Check for a list item into a custom list 2
            Get-ListItem -Web $DestinationSite.RootWeb -ListName "CustomList2" -ItemTitle "ItemList2" | Should be $null

            # Check for a document item into a custom library
            Get-ListItem -Web $DestinationSite.RootWeb -ListName "Pages" -ItemTitle "SourcePage1" | Should be $null
        }

        It "[Single Site] should by default copy pages, documents amd images into the destination site when libraries exists on both sides and 'ListNames' parameter is not set" {

            Copy-DSPData -FromUrl $SourceSiteUrl -ToUrl $DestinationSiteUrl -LogFolder $LogFilePath

            # Check for a list item into a custom list 2
            Get-ListItem -Web $DestinationSite.RootWeb -ListName "CustomList2" -ItemTitle "ItemList2" | Should be $null

            # Check for a page item into the pages library
            Get-ListItem -Web $DestinationSite.RootWeb -ListName "Pages" -ItemTitle "SourcePage1" | Should Not be $null

            # Check for a document item into the "Documents" library
            Get-ListItem -Web $DestinationSite.RootWeb -ListName "Documents" -ItemTitle "Document1" | Should Not be $null

            # Check for an image item into the "Images" library
            Get-ListItem -Web $DestinationSite.RootWeb -ListName "Images" -ItemTitle "Image1" | Should Not be $null
        }

        Write-Host "     --Tests Teardown--"
        Remove-SPSite $SourceSiteUrl -Confirm:$false
        Remove-SPSite $DestinationSiteUrl -Confirm:$false
        Remove-Item $LogFilePath -Recurse -Confirm:$false
    }

    Context "Copy content between mirror structure" {

        Write-Host "     --Test Setup--"

        $SourceSiteUrl = $webApplication+ "/sites/sourcesite"
        $DestinationSiteUrl = $webApplication+ "/sites/destinationsite"
            
        $SourceSite = New-SiteWithSubsitesNoVariations $SourceSiteUrl "BLANKINTERNET#0"
        $DestinationSite = New-SiteWithSubsitesNoVariations $DestinationSiteUrl "BLANKINTERNET#0"

        # Source URLs
        $SrcSubWeb1Url = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($SourceSiteUrl, "subweb1"))
        $SrcSubWeb2Url = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($SourceSiteUrl, "subweb2"))
        $SrcSubWeb11 = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($SourceSiteUrl, "subweb1/subweb11"))

        # Destnation URLs
        $DestSubWeb1Url = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($DestinationSiteUrl, "subweb1"))
        $DestSubWeb2Url = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($DestinationSiteUrl, "subweb2"))
        $DestSubWeb11 = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($DestinationSiteUrl, "subweb1/subweb11"))

        $SrcSubWebs = @($SourceSiteUrl, $SrcSubWeb1Url, $SrcSubWeb2Url, $SrcSubWeb11)
        $DestSubSwebs = @($DestinationSiteUrl, $DestSubWeb1Url , $DestSubWeb2Url , $DestSubWeb11)

        $SrcSubWebs | ForEach-Object {

            $CurrentWeb = Get-SPWeb $_
        
            # Add a test page in source web
            New-PublishingPagesAndFolders $CurrentWeb "SourcePage1" -WelcomePageTitle "SourceHomePage"

            # Add a test document in source web
            $FilePath = Join-Path -Path "$here" -ChildPath ".\Document1.docx"
            $File = Add-Document $CurrentWeb "Documents" (Get-Item $FilePath)
            $File.Item["Title"] = "Document1"
            $File.Item.Update()

            # Add a test image in source web
            $FilePath = Join-Path -Path "$here" -ChildPath ".\Image1.jpg"
            $File = Add-Document $CurrentWeb "Images" (Get-Item $FilePath)
            $File.Item["Title"] = "Image1"
            $File.Item.Update()

            # Add custom lists with an item
            $List = New-CustomList $CurrentWeb "CustomList1" "GenericList"
            $NewItem = $List.AddItem()
            $NewItem["Title"] = "ItemList1"
            $NewItem.Update()

            $List = New-CustomList $CurrentWeb "CustomList2" "GenericList"
            $NewItem = $List.AddItem()
            $NewItem["Title"] = "ItemList2"
            $NewItem.Update()
        }

        $DestSubSwebs | ForEach-Object {

            $CurrentWeb = Get-SPWeb $_

            # Add a list with the same name as source in the destination site
            New-CustomList $CurrentWeb "CustomList1" "GenericList" | Out-Null
            New-CustomList $CurrentWeb "CustomList2" "GenericList" | Out-Null
        }       
        
        It "[Multiple Sites] should only copy items for lists specified in the 'ListNames' parameter in all natching subwebs" {

            Copy-DSPData -FromUrl $SourceSiteUrl -ToUrl $DestinationSiteUrl -LogFolder $LogFilePath -ListNames "CustomList1" -IncludeChildren

            $DestSubSwebs | ForEach-Object {

                $CurrentWeb = Get-SPWeb $_

                # Check for a list item into a custom list 1
                Get-ListItem -Web $CurrentWeb -ListName "CustomList1" -ItemTitle "ItemList1" | Should Not be $null

                # Check for a list item into a custom list 2
                Get-ListItem -Web $CurrentWeb -ListName "CustomList2" -ItemTitle "ItemList2" | Should be $null

                # Check for a document item into a custom library
                Get-ListItem -Web $CurrentWeb -ListName "Pages" -ItemTitle "SourcePage1" | Should be $null
            }
        }

        It "[Multiple Sites] should by default copy pages, documents amd images into destination sites when libraries exists on both sides and 'ListNames' parameter is not set" {

            Copy-DSPData -FromUrl $SourceSiteUrl -ToUrl $DestinationSiteUrl -LogFolder $LogFilePath -IncludeChildren

            $DestSubSwebs | ForEach-Object {

                $CurrentWeb = Get-SPWeb $_

                # Check for a list item into a custom list 2
                Get-ListItem -Web $CurrentWeb -ListName "CustomList2" -ItemTitle "ItemList2" | Should be $null

                # Check for a page item into the pages library
                Get-ListItem -Web $CurrentWeb -ListName "Pages" -ItemTitle "SourcePage1" | Should Not be $null

                # Check for a document item into the "Documents" library
                Get-ListItem -Web $CurrentWeb -ListName "Documents" -ItemTitle "Document1" | Should Not be $null

                # Check for an image item into the "Images" library
                Get-ListItem -Web $CurrentWeb -ListName "Images" -ItemTitle "Image1" | Should Not be $null
            }
        }  

        Write-Host "     --Tests Teardown--"
        Remove-SPSite $SourceSiteUrl -Confirm:$false
        Remove-SPSite $DestinationSiteUrl -Confirm:$false
        Remove-Item $LogFilePath -Recurse -Confirm:$false
    }
}