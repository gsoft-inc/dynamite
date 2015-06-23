$here = Split-Path -Parent $MyInvocation.MyCommand.Path

# Script under test (sut)
$sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path).Replace(".Tests.", ".")
$sutPath = "$here\..\..\GSoft.Dynamite.PowerShell\$sut"
$sourceFolderPath = (Get-Location).Path + "\source"
$destFolderPath = (Get-Location).Path + "\destination"

# ----------------------
# Tests configuration
# ----------------------
$webXmlSchema         =	 Join-Path -Path "$here" -ChildPath ".\TestSchema.xsd"
$outputFileName       =  Join-Path -Path "$here" -ChildPath ".\Output.xml"
$tempSiteCollection   = "sites/exporttest"
$webApplication       = "http://franck-vm2013"
$currentAccountName   = ("OFFICE\franck.cornu").ToLower()
$variationsConfigFile   = Join-Path -Path "$here" -ChildPath "./TestVariationsSettings.xml"
$siteUrl = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($webApplication, $tempSiteCollection))

Describe "Export-DSPWebStructureAsTaxonomy" -Tags "Local", "Slow" {

	# ----------------------
	# Export-DSPWebStructureAsTaxonomy
	# ----------------------
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
		$site = New-SinglePublishingSiteNoSubsitesNoVariationsWithPagesAndFolders -SiteUrl $siteUrl

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
			Export-DSPWebStructureAsTaxonomy -SourceWeb $site.RootWeb.Url -OutputFileName $outputFileName -TermSetName "Navigation" -PageExclusionPatterns "default.aspx","NotFound"

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
			Export-DSPWebStructureAsTaxonomy -SourceWeb $site.RootWeb.Url -OutputFileName $outputFileName -TermSetName "Navigation" -PageExclusionPatterns "default.aspx","NotFound"

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
		$site = New-TeamSiteWithSubSites -SiteUrl $siteUrl

	    It "[Team sites] should considering the Site Pages library instead of the Pages library" {
        
            # Execute the command
		    Export-DSPWebStructureAsTaxonomy -SourceWeb $site.RootWeb.Url -OutputFileName $outputFileName -TermSetName "Navigation" -PageExclusionPatterns "default.aspx","NotFound" 
        
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
        $site = New-PublishingSiteWithSubsitesNoVariationsWithPagesAndFolders -SiteUrl $siteUrl

        It "[No variations] should export a taxonomy structure corresponding to the site, sub sites, pages and folders" {
        
            # Execute the command
		    Export-DSPWebStructureAsTaxonomy -SourceWeb $site.RootWeb.Url -OutputFileName $outputFileName -TermSetName "Navigation" -PageExclusionPatterns "default.aspx","NotFound"

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
        $site = New-PublishingSiteWithSubsitesWithVariationsWithPagesAndFolders  -SiteUrl $siteUrl -SubWebsTemplateID "STS#0" -VariationConfigFilePath $variationsConfigFile 	

        It "[Variations] It should exclude root variation sites and sites that are not in the source branch and create a new taxonomy term set" {
        
            # Execute the command
		    Export-DSPWebStructureAsTaxonomy -SourceWeb $site.RootWeb.Url -OutputFileName $outputFileName -TermSetName "Navigation" -PageExclusionPatterns "default.aspx","NotFound"
           
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
		    Export-DSPWebStructureAsTaxonomy -SourceWeb $site.RootWeb.Url -OutputFileName $outputFileName -TermSetName "Navigation" -PageExclusionPatterns "default.aspx","NotFound"
           
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
