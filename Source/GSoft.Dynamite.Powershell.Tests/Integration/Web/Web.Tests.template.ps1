$here = Split-Path -Parent $MyInvocation.MyCommand.Path

# Script under test (sut)
$sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path).Replace(".Tests.", ".")
$sutPath = "$here\..\..\GSoft.Dynamite.Scripts\$sut"
$sourceFolderPath = (Get-Location).Path + "\source"
$destFolderPath = (Get-Location).Path + "\destination"

# ----------------------
# Tests configuration
# ----------------------
$webXmlSchema         =	 Join-Path -Path "$here" -ChildPath "[[DSP_XmlSchema]]"
$outputFileName       =  Join-Path -Path "$here" -ChildPath "[[DSP_OutputFileName]]"
$inputFileName       =  Join-Path -Path "$here" -ChildPath "[[DSP_InputFileName]]"
$inputFileNameOverwrite  =  Join-Path -Path "$here" -ChildPath "[[DSP_InputFileNameOverwrite]]"
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
	$subWeb1 = if ((Get-SPWeb $subWeb1Url -ErrorAction SilentlyContinue) -eq $null) { New-SPWeb $subWeb1Url -Template "STS#0" -Name "SubWeb1"  } else { return Get-SPWeb $subWeb1Url }
	$subWeb2 = if ((Get-SPWeb $subWeb2Url -ErrorAction SilentlyContinue) -eq $null) { New-SPWeb $subWeb2Url -Template "STS#0" -Name "SubWeb2"  } else { return Get-SPWeb $subWeb2Url }

	$subWeb11Url = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($subWeb1.Url, "subweb11"))
	$subWeb11 = if ((Get-SPWeb $subWeb11Url -ErrorAction SilentlyContinue) -eq $null) { New-SPWeb $subWeb11Url -Template "STS#0" -Name "SubWeb11"  } else { return Get-SPWeb $subWeb11Url }

	$webs+=$subWeb1
	$webs+=$subWeb2
	$webs+=$subWeb11

	return $webs		
}

function CreateSingleSiteNoSubsitesNoVariations {
		
	return CreateSingleSite
}
		
function CreateSiteWithSubsitesNoVariations {

	$site = CreateSingleSite
	CreateSubWebs -SourceWeb $site.RootWeb | Out-Null

	return Get-SPSite $site.Url
}

function CreateSiteWithSubsitesAndVariations{

	$site = CreateSingleSite
	[xml]$config = Get-Content $variationsConfigFile
		
	# Create hierarchies on the root site
	New-DSPSiteVariations -Config $config.Variations -Site $site

	Set-VariationHierarchy -Site $site.Url

	Start-Sleep -s 5

	$sourceVariationSiteUrl = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($site.RootWeb.Url, "en"))

	# Sync Sub webs
	CreateSubWebs -SourceWeb (Get-SPWeb $sourceVariationSiteUrl) | ForEach-Object {$_ | Sync-DSPWeb -LabelToSync 'fr'}

	$webApp = Get-SPWebApplication $webApplication
	Wait-SPTimerJob -Name "VariationsSpawnSites" -WebApplication $webApp

	return Get-SPSite $site.Url
}


Describe "Export-DSPWebStructure" -Tags "Local", "Slow" {

	# ----------------------
	# Export-DSPWebStructure
	# ----------------------
	Context "The source web doesn't exist" 	{
		It "should throw an error " {

			{ Export-DSPWebStructureStructure -SourceWeb "http:///%!" } | Should Throw
		}
	}

	Context "The source web exist"	{

		AfterEach {
			Write-Host "     --Test Teardown--"
			Remove-Item $outputFileName -Force -Confirm:$false
		}

		Write-Host "     --Test Setup--"

		# Create site hierarchy
		$site = CreateSingleSiteNoSubsitesNoVariations
		

		It "should output a XML file with the correct XSD schema" {

			# Execute the command
			Export-DSPWebStructure -SourceWeb $site.RootWeb.Url -OutputFileName $outputFileName

			# A file should be generated
			$outputFileName | Should Exist

			# This file should be match the correct schema
			Test-Xml $outputFileName -SchemaPath $webXmlSchema | Should Be $true
		}
	}
	
	Context "The source web has no subsites"	{

		AfterEach {
			Write-Host "     --Test Teardown--"
			Remove-Item $outputFileName -Force -Confirm:$false
		}

		Write-Host "     --Test Setup--"

		# Create site hierarchy
		$site = CreateSingleSiteNoSubsitesNoVariations

		It "should export only the source web" {
			
			# Execute the command
			Export-DSPWebStructure -SourceWeb $site.RootWeb.Url -OutputFileName $outputFileName

			# Search for the web node which contains the web url
			if (Test-Path $outputFileName)
			{
				[xml]$xml = Get-Content $outputFileName

				$allNodes = Select-Xml -Xml $xml -XPath "//Web"
				$testNode = Select-Xml -Xml $xml -XPath ("/Configuration/Web[@Path='/'][@IsRoot='True'][@Template='BLANKINTERNET#0'][@Owner='" + $currentAccountName + "']")
			}
		   
			$testNode | Should Not Be $null
			$allNodes.Length | Should Be 1            
		}
	}

	Context "The source web has multiple subsites"	{
	
		AfterEach {
			Write-Host "     --Test Teardown--"
			Remove-Item $outputFileName -Force -Confirm:$false
		}

		Write-Host "     --Test Setup--"

		# Create site hierarchy
		$site = CreateSiteWithSubsitesNoVariations

		It "should export all webs and sub webs" {
			
			# Execute the command
			Export-DSPWebStructure -SourceWeb $site.RootWeb.Url -OutputFileName $outputFileName

			# Search for the web node which contains the web url
			if (Test-Path $outputFileName)
			{
				[xml]$xml = Get-Content $outputFileName

				$allNodes     = Select-Xml -Xml $xml -XPath "//Web"
				$rootNode     = Select-Xml -Xml $xml -XPath ("/Configuration/Web[@Path='/'][@IsRoot='True'][@Template='BLANKINTERNET#0'][@Name='RootWeb'][@Owner='" + $currentAccountName + "'][@Language='1033']")
				$subweb1Node  = Select-Xml -Xml $xml -XPath "/Configuration/Web/Web[@Path='subweb1'][@Template='STS#0'][@Name='SubWeb1'][@Language='1033']"
				$subweb2Node  = Select-Xml -Xml $xml -XPath "/Configuration/Web/Web[@Path='subweb2'][@Template='STS#0'][@Name='SubWeb2'][@Language='1033']"
				$subweb11Node = Select-Xml -Xml $xml -XPath "/Configuration/Web/Web/Web[@Path='subweb11'][@Template='STS#0'][@Name='SubWeb11'][@Language='1033']"            
			}
		   
			$rootNode | Should Not Be $null
			$subweb1Node | Should Not Be $null
			$subweb2Node | Should Not Be $null
			$subweb11Node | Should Not Be $null
			$allNodes.Length | Should Be 4          
		}

		It "should export all webs and sub webs without the source web if -ExcludeRootWeb is specified" {

			# Execute the command
			Export-DSPWebStructure -SourceWeb $site.RootWeb.Url -OutputFileName $outputFileName -ExludeRootWeb

			# Search for the web node which contains the web url
			if (Test-Path $outputFileName)
			{
				[xml]$xml = Get-Content $outputFileName

				$allNodes = Select-Xml -Xml $xml -XPath "//Web"
				$rootNode = Select-Xml -Xml $xml -XPath ("/Configuration/Web[@Path='/'][@IsRoot='True'][@Template='BLANKINTERNET#0'][@Name='RootWeb'][@Owner='" + $currentAccountName + "'][@Language='1033']")
				$subweb1Node = Select-Xml -Xml $xml -XPath "/Configuration/Web[@Path='subweb1'][@Template='STS#0'][@Name='SubWeb1'][@Language='1033']"
				$subweb2Node = Select-Xml -Xml $xml -XPath "/Configuration/Web[@Path='subweb2'][@Template='STS#0'][@Name='SubWeb2'][@Language='1033']"
				$subweb11Node = Select-Xml -Xml $xml -XPath "/Configuration/Web/Web[@Path='subweb11'][@Template='STS#0'][@Name='SubWeb11'][@Language='1033']"            
			}
		   
			$rootNode | Should Be $null
			$subweb1Node | Should Not Be $null
			$subweb2Node | Should Not Be $null
			$subweb11Node | Should Not Be $null
			$allNodes.Length | Should Be 3       		
		}
	}

	Context "SharePoint variations are activated on the source web with multiple sites" {
	
		AfterEach {
			Write-Host "     --Test Teardown--"
			Remove-Item $outputFileName -Force -Confirm:$false
		}

		Write-Host "     --Test Setup--"

		# Create site hierarchy
		$site = CreateSiteWithSubsitesAndVariations		

		It "should export the original webs and sub webs URL structure without automatically generated variations sites (including variations root sites and target sites)" {

			# Execute the command
			Export-DSPWebStructure -SourceWeb $site.RootWeb.Url -OutputFileName $outputFileName

			# Search for the web node which contains the web url
			if (Test-Path $outputFileName)
			{
				[xml]$xml = Get-Content $outputFileName

				$allNodes = Select-Xml -Xml $xml -XPath "//Web"
				$rootNode = Select-Xml -Xml $xml -XPath ("/Configuration/Web[@Path='/'][@IsRoot='True'][@Template='BLANKINTERNET#0'][@Name='RootWeb'][@Owner='" + $currentAccountName + "'][@Language='1033']")
				$subweb1Node = Select-Xml -Xml $xml -XPath "/Configuration/Web/Web[@Path='subweb1'][@Template='STS#0'][@Name='SubWeb1'][@Language='1033']"
				$subweb2Node = Select-Xml -Xml $xml -XPath "/Configuration/Web/Web[@Path='subweb2'][@Template='STS#0'][@Name='SubWeb2'][@Language='1033']"
				$subweb11Node = Select-Xml -Xml $xml -XPath "/Configuration/Web/Web/Web[@Path='subweb11'][@Template='STS#0'][@Name='SubWeb11'][@Language='1033']"            
			}
		   
			$rootNode | Should Not Be $null
			$subweb1Node | Should Not Be $null
			$subweb2Node | Should Not Be $null
			$subweb11Node | Should Not Be $null
			$allNodes.Length | Should Be 4                 
		}

		It "shouldn't export any webs if the command is run on a site on the a target variation label branch" {

			# Execute the command
			Export-DSPWebStructure -SourceWeb ($site.RootWeb.Url + "/fr/subweb1") -OutputFileName $outputFileName

			# Search for the web node which contains the web url
			if (Test-Path $outputFileName)
			{
				[xml]$xml = Get-Content $outputFileName

				$allNodes = Select-Xml -Xml $xml -XPath "//Web"
			}
		   
			$allNodes.Length | Should Be 0
		}
	}

	Write-Host "     --Tests Teardown--"
	Remove-SPSite $siteUrl -Confirm:$false
}

Describe "Import-DSPWebStructure" -Tag "Slow" {
	
	# ----------------------
	# Import-DSPWebStructure
	# ----------------------
	Context "Parameters are invalid" 	{

		It "should throw an error if parent url is invalid" {

			{ Import-DSPWebStructure -InputFileName $inputFileName -ParentUrl "http:///%!" } | Should Throw
		}

		It "should throw an error if input file is invalid" {

			{ Import-DSPWebStructure -InputFileName "C:\dontexist.xml" -ParentUrl $siteUrl } | Should Throw
		}
	}

	Context "The XML structure contains multiple webs and sub webs" {
	
		It "Import all webs under the parent url (webs don't already exist)" {

			Write-Host "     --Test Setup--"

			# Create site hierarchy
			$site = CreateSingleSiteNoSubsitesNoVariations

			$subweb1Url = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($site.Url, "subweb1"))
			$subweb2Url = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($site.Url, "subweb2"))
			$subweb11Url = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($subweb1Url, "subweb11"))

			Import-DSPWebStructure -InputFileName $inputFileName -ParentUrl $site.Url 

			$subweb1 = Get-SPWeb $subweb1Url -ErrorAction SilentlyContinue
			$subweb2 = Get-SPWeb $subweb2Url -ErrorAction SilentlyContinue
			$subweb11 = Get-SPWeb $subweb11Url -ErrorAction SilentlyContinue

			$subweb1 | Should Not Be $null
			$subweb2 | Should Not Be $null
			$subweb11 | Should Not Be $null		

			Write-Host "     --Tests Teardown--"
			Remove-SPSite $siteUrl -Confirm:$false			
		}

		It "Import and overwrite all existing webs under the parent url if Overwrite paramater is specified" {

			Write-Host "     --Test Setup--"

			# Create site hierarchy
			$site = CreateSiteWithSubsitesNoVariations

			$subweb1Url = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($site.Url, "subweb1"))
			$subweb2Url = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($site.Url, "subweb2"))
			$subweb11Url = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($subweb1Url, "subweb11"))

			Import-DSPWebStructure -InputFileName $inputFileNameOverwrite -ParentUrl $site.Url -Overwrite

			$subweb1 = Get-SPWeb $subweb1Url -ErrorAction SilentlyContinue
			$subweb2 = Get-SPWeb $subweb2Url -ErrorAction SilentlyContinue
			$subweb11 = Get-SPWeb $subweb11Url -ErrorAction SilentlyContinue

			$subweb1 | Should Not Be $null
			$subweb2 | Should Not Be $null
			$subweb11 | Should Not Be $null		

			$subweb1.Title -match "Overwrite" | Should Be $true
			$subweb2.Title -match "Overwrite" | Should Be $true
			$subweb11.Title -match "Overwrite" | Should Be $true

			Write-Host "     --Tests Teardown--"
			$ConfirmPreference = "High"		
		}
	}
}