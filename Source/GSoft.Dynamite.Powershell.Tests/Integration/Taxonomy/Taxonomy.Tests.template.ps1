$here = Split-Path -Parent $MyInvocation.MyCommand.Path

# Script under test (sut)
$sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path).Replace(".Tests.", ".")
$sutPath = "$here\..\..\GSoft.Dynamite.PowerShell\$sut"
$sourceFolderPath = (Get-Location).Path + "\source"
$destFolderPath = (Get-Location).Path + "\destination"

# ----------------------
# Tests configuration
# ----------------------
$webXmlSchema         =	 Join-Path -Path "$here" -ChildPath "[[DSP_XmlSchema]]"
$outputFileName       =  Join-Path -Path "$here" -ChildPath "[[DSP_OutputFileName]]"
$tempSiteCollection   = "[[DSP_TempSiteCollection]]"
$webApplication       = "[[DSP_WebApplicationUrl]]"
$currentAccountName   = ("[[DSP_CurrentAccount]]").ToLower()
$variationsConfigFile   = Join-Path -Path "$here" -ChildPath "[[DSP_VariationsConfigFile]]"
$siteUrl = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($webApplication, $tempSiteCollection))

# ----------------------
# Utility functions
# ----------------------
function CreateSingleSite {

	Param
    (
        [Parameter(Mandatory=$true)]
		[ValidateNotNullOrEmpty()]
		[string]$TemplateName 
    )

	$site = Get-SPSite $siteUrl -ErrorAction SilentlyContinue
	if ($site -ne $null)
	{
		Remove-SPSite $site -Confirm:$false 	
	}

	if($webApplication -ne $null)
	{
		# Create test structure with the current account login to avoid access denied
		$site = New-SPSite $siteUrl -OwnerAlias $currentAccountName -Template $TemplateName -Name "RootWeb"
	}

	return $site
}

function CreateSubWebs {
	Param
	(	
		[Parameter(Mandatory=$true, Position=0)]
		[ValidateNotNullOrEmpty()]
		[Microsoft.SharePoint.SPWeb]$SourceWeb,

        [Parameter(Mandatory=$true)]
		[ValidateNotNullOrEmpty()]
		[string]$TemplateName
	)
	$webs = @()

	$subWeb1Url = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($SourceWeb.Url, "subweb1"))
	$subWeb2Url = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($SourceWeb.Url, "subweb2"))
	  
	# Create a site hierarchy
	$subWeb1 = if ((Get-SPWeb $subWeb1Url -ErrorAction SilentlyContinue) -eq $null) { New-SPWeb $subWeb1Url -Template $TemplateName -Name "SubWeb1"  } else { return Get-SPWeb $subWeb1Url }
	$subWeb2 = if ((Get-SPWeb $subWeb2Url -ErrorAction SilentlyContinue) -eq $null) { New-SPWeb $subWeb2Url -Template $TemplateName -Name "SubWeb2"  } else { return Get-SPWeb $subWeb2Url }

	$subWeb11Url = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($subWeb1.Url, "subweb11"))
	$subWeb11 = if ((Get-SPWeb $subWeb11Url -ErrorAction SilentlyContinue) -eq $null) { New-SPWeb $subWeb11Url -Template $TemplateName -Name "SubWeb11"  } else { return Get-SPWeb $subWeb11Url }

	$webs+=$subWeb1
	$webs+=$subWeb2
	$webs+=$subWeb11

	return $webs		
}

function CreateSinglePublishingSiteNoSubsitesNoVariationsWithPagesAndFolders {
		
	$site = CreateSingleSite -TemplateName "BLANKINTERNET#0"
    
    CreatePublishingPagesAndFolders $site.RootWeb "TestPage" -WelcomePageTitle "TestHomePage"

    return $site
}

function CreateTeamSiteWithSubistes {

	$site = CreateSingleSite -TemplateName "STS#0"
	$subwebs = CreateSubWebs -SourceWeb $site.RootWeb -TemplateName "STS#0"

    return $site
}

function CreatePublishingPagesAndFolders {
    
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
		
function CreatePublishingSiteWithSubsitesNoVariationsWithPagesAndFolders {

	$site = CreateSingleSite -TemplateName "BLANKINTERNET#0"
	$subwebs = CreateSubWebs -SourceWeb $site.RootWeb -TemplateName "BLANKINTERNET#0"
    
    $i = 1
    $subwebs | ForEach-Object {
    
        CreatePublishingPagesAndFolders $_ "TestPage" -WelcomePageTitle "TestHomePageSubWeb$i"
        $i++
    }

	return Get-SPSite $site.Url
}

function CreatePublishingSiteWithSubsitesWithVariationsWithPagesAndFolders{

	$site = CreateSingleSite -TemplateName "BLANKINTERNET#0"
	[xml]$config = Get-Content $variationsConfigFile
		
	$webApp = Get-SPWebApplication $webApplication

	# Create hierarchies on the root site
	New-DSPSiteVariations -Config $config.Variations -Site $site

	Set-VariationHierarchy -Site $site.Url

	Start-Sleep -s 5

	$sourceVariationSiteUrl = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($site.RootWeb.Url, "en"))

	# Sync Sub webs
	$SubWebs = CreateSubWebs -SourceWeb (Get-SPWeb $sourceVariationSiteUrl) -TemplateName "BLANKINTERNET#0"

    $SubWebs| ForEach-Object {

            $_ | Sync-DSPWeb -LabelToSync 'fr'        
    }

    Wait-SPTimerJob -Name "VariationsSpawnSites" -WebApplication $webApp
    Write-Warning "Waiting for 'VariationsSpawnSites' timer job to finish..."
    Start-Sleep -Seconds 60

    $i = 1
    $SubWebs | ForEach-Object {
		
		$Web = $_
        $Items = CreatePublishingPagesAndFolders $Web "TestPage" -WelcomePageTitle "TestHomePageSubWeb$i"

        $Items | ForEach-Object {

			$Item = $_
            Sync-DSPItem -VariationListItem $Item 
        }
        $i++
    }

    # Be careful, you must run the timer job VariationsPropagatePage instead of VariationsPropagateListItem
    # Dont' forget to set EnableAutoSpawn="true" in variations settings to set the "Publish" action on pages as a trigger for synchronization
    Wait-SPTimerJob -Name "VariationsPropagatePage" -WebApplication $webApp
	Write-Verbose "Waiting for 'VariationsPropagatePage' timer job to finish..."
	Start-Sleep -Seconds 60

	return Get-SPSite $site.Url
}

Describe "Export-DSPWebStructureAsTaxonomy" -Tags "Local", "Slow" {

	Context "The source web doesn't exist" 	{
		It "should throw an error " {

			{ Export-DSPWebStructureAsTaxonomy -SourceWeb "http:///%!" -OutputFileName $outputFileName -TermSetName "Navigation" } | Should Throw
		}
	}

    Context "The source web has no subsites"	{

		AfterEach {
			Write-Host "     --Test Teardown--"
			Remove-Item $outputFileName -Force -Confirm:$false
		}

		Write-Host "     --Test Setup--"

		# Create site hierarchy
		$site = CreateSinglePublishingSiteNoSubsitesNoVariationsWithPagesAndFolders

        It "should output a XML file with the correct schema" {

            # Execute the command
			Export-DSPWebStructureAsTaxonomy -SourceWeb $site.RootWeb.Url -OutputFileName $outputFileName -TermSetName "Navigation"

            # A file should be generated
			$outputFileName | Should Exist

			# This file should be match the correct schema
			Test-Xml $outputFileName -SchemaPath $webXmlSchema | Should Be $true
        }

		It "should export a taxonomy structure corresponding to pages and folders hierarchy under the home page of the web excluding specific tokens" {
			
			# Execute the command
			Export-DSPWebStructureAsTaxonomy -SourceWeb $site.RootWeb.Url -OutputFileName $outputFileName -TermSetName "Navigation" -ExcludePage "default.aspx","NotFound"

			# Search for the web node which contains the web url
			if (Test-Path $outputFileName)
			{
				[xml]$xml = Get-Content $outputFileName

				$allNodes = Select-Xml -Xml $xml -XPath "//Term" # /Folder1, /TestPage, /Folder1/TestPage
				$Folder1 = Select-Xml -Xml $xml -XPath ("//TermSet/Terms/Term[@Name='Folder1']")
                $TestPageInFolder1 = Select-Xml -Xml $xml -XPath ("//TermSet/Terms/Term[@Name='Folder1']/Terms/Term[@Name='TestPage']")
                $TestPage = Select-Xml -Xml $xml -XPath ("//TermSet/Terms/Term[@Name='TestPage']")
			}
		   
			$Folder1 | Should Not Be $null
            $TestPageInFolder1 | Should Not Be $null
            $TestPage | Should Not Be $null

			$allNodes.Length | Should Be 3           
		}

        It "should output a XML file allowing the creation of a taxonomy term set via the Gary Lapointe cmdlet Import-SPTerms" {
            
            # Execute the command
			Export-DSPWebStructureAsTaxonomy -SourceWeb $site.RootWeb.Url -OutputFileName $outputFileName -TermSetName "Navigation" -ExcludePage "default.aspx","NotFound"

            $TermStore = (Get-SPTaxonomySession -Site $siteUrl).TermStores[0]

            $TermGroup = $TermStore.Groups["TestGroup"]
            if ($TermGroup -ne $null)
            {
                Remove-DSPTermGroup $TermStore "TestGroup" | Out-Null
            }

            $TermGroup = $TermStore.CreateGroup("TestGroup")
            $TermStore.CommitAll()

            Import-SPTerms -ParentGroup $TermGroup -InputFile $outputFileName
            
            $TermGroup = $TermStore.Groups["TestGroup"]
            $TermSet = $TermGroup.TermSets["Navigation"]

            # Home pages
            $TestHomePage = $TermSet.Terms | Where-Object {$_.Name -eq "TestHomePage"} | Select-Object -First 1

            $Folder1Term = $TermSet.Terms | Where-Object {$_.Name -eq "Folder1"} | Select-Object -First 1
            $TestPageInFolder1Term = $Folder1Term.Terms | Where-Object {$_.Name -eq "TestPage"} | Select-Object -First 1
            $TestPage = $TermSet.Terms | Where-Object {$_.Name -eq "TestPage"} | Select-Object -First 1

            # Assertions
            $TestHomePage | Should be $null
            $Folder1Term | Should Not be $null
            $TestPageInFolder1Term | Should Not be $null
            $TestPage | Should Not be $null

			Remove-DSPTermGroup $TermStore "TestGroup" | Out-Null
        }

        Write-Host "     --Tests Teardown--"
	    Remove-SPSite $siteUrl -Confirm:$false
	}

    Context "The source web has multiple subsites"	{

		AfterEach {
			Remove-Item $outputFileName -Force -Confirm:$false
		}

	    Write-Host "     --Test Setup--"

		# Create a team site strucutre
		$site = CreateTeamSiteWithSubistes

	    It "[Team sites] should considering the Site Pages library instead of the Pages library" {
        
            # Execute the command
		    Export-DSPWebStructureAsTaxonomy -SourceWeb $site.RootWeb.Url -OutputFileName $outputFileName -TermSetName "Navigation" -ExcludePage "default.aspx","NotFound" 
        
            # Search for the web node which contains the web url
			if (Test-Path $outputFileName)
			{
				[xml]$xml = Get-Content $outputFileName

				(Select-Xml -Xml $xml -XPath "//Term").Length | Should Be 7
                # RootWeb
				(Select-Xml -Xml $xml -XPath ("//TermSet/Terms/Term[@Name='How To Use This Library']")) | Should Not Be $null
                # Subweb 1 and Subweb 2
                (Select-Xml -Xml $xml -XPath ("//TermSet/Terms/Term[@Name='Home']")).Length | Should be 2
                # Subweb 1 pages
                (Select-Xml -Xml $xml -XPath ("//TermSet/Terms/Term[@Name='Home']/Terms/Term[@Name='How To Use This Library']")) | Should Not Be $null
                # Subweb 11
                (Select-Xml -Xml $xml -XPath ("//TermSet/Terms/Term[@Name='Home']/Terms/Term[@Name='Home']")) | Should Not Be $null
                # Subweb 11 pages
                (Select-Xml -Xml $xml -XPath ("//TermSet/Terms/Term[@Name='Home']/Terms/Term[@Name='Home']/Terms/Term[@Name='How To Use This Library']")) | Should Not Be $null                 
			}
		   
			# This file should be match the correct schema
			Test-Xml $outputFileName -SchemaPath $webXmlSchema | Should Be $true		
		}

        Write-Host "     --Tests Teardown--"
	    Remove-SPSite $siteUrl -Confirm:$false

        Write-Host "     --Test Setup--"

        # Create web structure with sub webs without variations
        $site = CreatePublishingSiteWithSubsitesNoVariationsWithPagesAndFolders

        It "[No variations] should export a taxonomy structure corresponding to the site, sub sites, pages and folders" {
        
            # Execute the command
		    Export-DSPWebStructureAsTaxonomy -SourceWeb $site.RootWeb.Url -OutputFileName $outputFileName -TermSetName "Navigation" -ExcludePage "default.aspx","NotFound"

			# Test the number of XML in the output file
			if (Test-Path $outputFileName)
			{
				[xml]$xml = Get-Content $outputFileName

				$allNodes = Select-Xml -Xml $xml -XPath "//Term" 
				$allNodes.Length | Should Be 12  
			}

			# This file should be match the correct schema
			Test-Xml $outputFileName -SchemaPath $webXmlSchema | Should Be $true	

            # Configure term store
            $TermStore = (Get-SPTaxonomySession -Site $siteUrl).TermStores[0]

            $TermGroup = $TermStore.Groups["TestGroup"]
            if ($TermGroup -ne $null)
            {
                Remove-DSPTermGroup $TermStore "TestGroup" | Out-Null
            }

            $TermGroup = $TermStore.CreateGroup("TestGroup")
            $TermStore.CommitAll()

            Import-SPTerms -ParentGroup $TermGroup -InputFile $outputFileName
            
            $TermGroup = $TermStore.Groups["TestGroup"]
            $TermSet = $TermGroup.TermSets["Navigation"]

			# Root terms are terms which contain the same sub terms sequence 
            $RootTerms = @()
            $Subweb1HomePage = $TermSet.Terms | Where-Object {$_.Name -eq "TestHomePageSubWeb1"} | Select-Object -First 1
			$Subweb2HomePage = $TermSet.Terms | Where-Object {$_.Name -eq "TestHomePageSubWeb2"} | Select-Object -First 1
			$Subweb11HomePage = $Subweb1HomePage.Terms | Where-Object {$_.Name -eq "TestHomePageSubWeb3"} | Select-Object -First 1

            $Subweb1HomePage | Should Not be $null
			$Subweb2HomePage | Should Not be $null
			$Subweb11HomePage | Should Not be $null
	
            $RootTerms += $Subweb1HomePage
			$RootTerms += $Subweb2HomePage
			$RootTerms += $Subweb11HomePage

            $RootTerms | ForEach-Object {

                $Folder1Term = $_.Terms | Where-Object {$_.Name -eq "Folder1"} | Select-Object -First 1
                $Folder1Term | Should Not be $null

                ($Folder1Term.Terms | Where-Object {$_.Name -eq "TestPage"} | Select-Object -First 1) | Should Not be $null
                ($_.Terms | Where-Object {$_.Name -eq "TestPage"} | Select-Object -First 1) | Should Not be $null
            }        
        }

        Write-Host "     --Tests Teardown--"
	    Remove-SPSite $siteUrl -Confirm:$false

        Write-Host "     --Test Setup--"

        # Create web structure with sub webs and variations
        $site = CreatePublishingSiteWithSubsitesWithVariationsWithPagesAndFolders

        It "[Variations] It should exclude root variation sites and sites that are not in the source branch and create a new taxonomy term set" {
        
            # Execute the command
		    Export-DSPWebStructureAsTaxonomy -SourceWeb $site.RootWeb.Url -OutputFileName $outputFileName -TermSetName "Navigation" -ExcludePage "default.aspx","NotFound"
           
			# Test the number of XML in the output file
			if (Test-Path $outputFileName)
			{
				[xml]$xml = Get-Content $outputFileName

				$allNodes = Select-Xml -Xml $xml -XPath "//Term" 
				$allNodes.Length | Should Be 12  
			}

			# This file should be match the correct schema
			Test-Xml $outputFileName -SchemaPath $webXmlSchema | Should Be $true	

            # Configure term store
            $TermStore = (Get-SPTaxonomySession -Site $siteUrl).TermStores[0]

            $TermGroup = $TermStore.Groups["TestGroup"]
            if ($TermGroup -ne $null)
            {
                Remove-DSPTermGroup $TermStore "TestGroup" | Out-Null
            }

            $TermGroup = $TermStore.CreateGroup("TestGroup")
            $TermStore.CommitAll()

            Import-SPTerms -ParentGroup $TermGroup -InputFile $outputFileName
            
            $TermGroup = $TermStore.Groups["TestGroup"]
            $TermSet = $TermGroup.TermSets["Navigation"]

			# Root terms are terms which contain the same sub terms sequence 
            $RootTerms = @()
            $Subweb1HomePage = $TermSet.Terms | Where-Object {$_.Name -eq "TestHomePageSubWeb1"} | Select-Object -First 1
			$Subweb2HomePage = $TermSet.Terms | Where-Object {$_.Name -eq "TestHomePageSubWeb2"} | Select-Object -First 1
			$Subweb11HomePage = $Subweb1HomePage.Terms | Where-Object {$_.Name -eq "TestHomePageSubWeb3"} | Select-Object -First 1

            $Subweb1HomePage | Should Not be $null
			$Subweb2HomePage | Should Not be $null
			$Subweb11HomePage | Should Not be $null
	
            $RootTerms += $Subweb1HomePage
			$RootTerms += $Subweb2HomePage
			$RootTerms += $Subweb11HomePage

            $RootTerms | ForEach-Object {

                $Folder1Term = $_.Terms | Where-Object {$_.Name -eq "Folder1"} | Select-Object -First 1
                $Folder1Term | Should Not be $null

                ($Folder1Term.Terms | Where-Object {$_.Name -eq "TestPage"} | Select-Object -First 1) | Should Not be $null
                ($_.Terms | Where-Object {$_.Name -eq "TestPage"} | Select-Object -First 1) | Should Not be $null
            }    

			Remove-DSPTermGroup $TermStore "TestGroup" | Out-Null
        }

        It "[Variations] It should get the correct term labels for all variation target branches" {
       
            # Execute the command
		    Export-DSPWebStructureAsTaxonomy -SourceWeb $site.RootWeb.Url -OutputFileName $outputFileName -TermSetName "Navigation" -ExcludePage "default.aspx","NotFound"
           
			# Test the number of XML in the output file
			if (Test-Path $outputFileName)
			{
				[xml]$xml = Get-Content $outputFileName

				$allNodes = Select-Xml -Xml $xml -XPath "//Term"

				$allNodes | Foreach-Object {

				    if ($_.Node.Name -ne "Folder1")
                    {
					    $_.Node.Labels.Label.Length | Should be 2
                    }
				}
			}
        }

        Write-Host "     --Tests Teardown--"
	    Remove-SPSite $siteUrl -Confirm:$false
    }
}