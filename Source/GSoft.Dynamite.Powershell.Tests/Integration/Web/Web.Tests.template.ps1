$here = Split-Path -Parent $MyInvocation.MyCommand.Path

# Script under test (sut)
$sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path).Replace(".Tests.", ".")
$sutPath = "$here\..\..\GSoft.Dynamite.Scripts\$sut"
$sourceFolderPath = (Get-Location).Path + "\source"
$destFolderPath = (Get-Location).Path + "\destination"

# ----------------------
# Export-DSPWeb test data
# ----------------------
$webXmlSchema         =	 Join-Path -Path "$here" -ChildPath "[[DSP_XmlSchema]]"
$outputFileName       =  Join-Path -Path "$here" -ChildPath "[[DSP_OutputFileName]]"
$tempSiteCollection   = "[[DSP_TempSiteCollection]]"
$webApplication       = "[[DSP_WebApplicationUrl]]"
$currentAccountName   = ("[[DSP_CurrentAccount]]").ToLower()
$variationsConfigFile   = Join-Path -Path "$here" -ChildPath "[[DSP_VariationsConfigFile]]"
$siteUrl = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($webApplication, $tempSiteCollection))

Describe "Web.ps1" {

	# Tests functions
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
		CreateSubWebs -SourceWeb $site.RootWeb

		return Get-SPSite $site.Url
	}

	function CreateSiteWithSubsitesAndVariations{

		$site = CreateSingleSite
		[xml]$config = Get-Content $variationsConfigFile
		
		# Create hierarchies on the root site
		New-DSPSiteVariations -Config $config.Variations -Site $site

		$sourceVariationSiteUrl = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($site.RootWeb.Url, "en"))

		# Sync Sub webs
		CreateSubWebs -SourceWeb (Get-SPWeb $sourceVariationSiteUrl) | ForEach-Object {$_ | Sync-DSPWeb -LabelToSync 'fr'}

		$webApp = Get-SPWebApplication $webApplication
		Wait-SPTimerJob -Name "VariationsSpawnSites" -WebApplication $webApp

		return Get-SPSite $site.Url
	}

	# ----------------------
	# Export-DSPWeb
	# ----------------------
	Context "The source web doesn't exist" 	{
		It "should throw an error " {

			{ Export-DSPWeb -SourceWeb "http:///%!" } | Should Throw
		}
	}

	Context "The source web exist"	{

		Write-Host "     --Test Setup--"

		# Create site hierarchy
		$site = CreateSingleSiteNoSubsitesNoVariations
		

		It "should output a XML file with the correct XSD schema" {

			# Execute the command
			Export-DSPWeb -SourceWeb $site.RootWeb.Url -OutputFileName $outputFileName

			# A file should be generated
			$outputFileName | Should Exist

			# This file should be match the correct schema
			Test-Xml $outputFileName -SchemaPath $webXmlSchema | Should Be $true
		}
	}
	
	Context "The source web has no subsites"	{

		Write-Host "     --Test Setup--"

		# Create site hierarchy
		$site = CreateSingleSiteNoSubsitesNoVariations

		It "should export only the source web" {
			
			# Execute the command
			Export-DSPWeb -SourceWeb $site.RootWeb.Url -OutputFileName $outputFileName

			# Search for the web node which contains the web url
			if (Test-Path $outputFileName)
			{
				[xml]$xml = Get-Content $outputFileName

				$allNodes = Select-Xml -Xml $xml -XPath "//Web"
				$testNode = Select-Xml -Xml $xml -XPath ("/Configuration/Web[@Path='/sites/test'][@IsRoot='True'][@Template='BLANKINTERNET#0'][@Owner='" + $currentAccountName + "']")
			}
		   
			$testNode | Should Not Be $null
			$allNodes.Length | Should Be 1            
		}
	}

	Context "The source web has multiple subsites"	{
	
		Write-Host "     --Test Setup--"

		# Create site hierarchy
		$site = CreateSiteWithSubsitesNoVariations

		It "should export all webs and sub webs" {
			
			# Execute the command
			Export-DSPWeb -SourceWeb $site.RootWeb.Url -OutputFileName $outputFileName

			#Remove-SPSite $site -Confirm:$false

			# Search for the web node which contains the web url
			if (Test-Path $outputFileName)
			{
				[xml]$xml = Get-Content $outputFileName

				$allNodes = Select-Xml -Xml $xml -XPath "//Web"
				$rootNode = Select-Xml -Xml $xml -XPath ("/Configuration/Web[@Path='/sites/test'][@IsRoot='True'][@Template='BLANKINTERNET#0'][@Name='RootWeb'][@Owner='" + $currentAccountName + "']")
				$subweb1Node = Select-Xml -Xml $xml -XPath "/Configuration/Web/Web[@Path='/sites/test/subweb1'][@Template='STS#0'][@Name='SubWeb1']"
				$subweb2Node = Select-Xml -Xml $xml -XPath "/Configuration/Web/Web[@Path='/sites/test/subweb2'][@Template='STS#0'][@Name='SubWeb2']"
				$subweb11Node = Select-Xml -Xml $xml -XPath "/Configuration/Web/Web/Web[@Path='/sites/test/subweb1/subweb11'][@Template='STS#0'][@Name='SubWeb11']"            
			}
		   
			$rootNode | Should Not Be $null
			$subweb1Node | Should Not Be $null
			$subweb2Node | Should Not Be $null
			$subweb11Node | Should Not Be $null
			$allNodes.Length | Should Be 4          
		}

		It "should export all webs and sub webs without the source web if -ExcludeSourceWeb is specified" {

			# Execute the command
			Export-DSPWeb -SourceWeb $site.RootWeb.Url -OutputFileName $outputFileName -ExludeSourceWeb

			# Search for the web node which contains the web url
			if (Test-Path $outputFileName)
			{
				[xml]$xml = Get-Content $outputFileName

				$allNodes = Select-Xml -Xml $xml -XPath "//Web"
				$rootNode = Select-Xml -Xml $xml -XPath ("/Configuration/Web[@Path='/sites/test'][@IsRoot='True'][@Template='BLANKINTERNET#0'][@Name='RootWeb'][@Owner='" + $currentAccountName + "']")
				$subweb1Node = Select-Xml -Xml $xml -XPath "/Configuration/Web[@Path='/sites/test/subweb1'][@Template='STS#0'][@Name='SubWeb1']"
				$subweb2Node = Select-Xml -Xml $xml -XPath "/Configuration/Web[@Path='/sites/test/subweb2'][@Template='STS#0'][@Name='SubWeb2']"
				$subweb11Node = Select-Xml -Xml $xml -XPath "/Configuration/Web/Web[@Path='/sites/test/subweb1/subweb11'][@Template='STS#0'][@Name='SubWeb11']"            
			}
		   
			$rootNode | Should Be $null
			$subweb1Node | Should Not Be $null
			$subweb2Node | Should Not Be $null
			$subweb11Node | Should Not Be $null
			$allNodes.Length | Should Be 3       		
		}
	}

	Context "SharePoint variations are activated on the source web with multiple sites" {
	
		Write-Host "     --Test Setup--"

		# Create site hierarchy
		$site = CreateSiteWithSubsitesAndVariations		

		It "should export the original webs and sub webs URL structure without automatically generated variations sites (including variations root sites and target sites)" {

			# Execute the command
			Export-DSPWeb -SourceWeb $site.RootWeb.Url -OutputFileName $outputFileName

			# Search for the web node which contains the web url
			if (Test-Path $outputFileName)
			{
				[xml]$xml = Get-Content $outputFileName

				$allNodes = Select-Xml -Xml $xml -XPath "//Web"
				$rootNode = Select-Xml -Xml $xml -XPath ("/Configuration/Web[@Path='/sites/test'][@IsRoot='True'][@Template='BLANKINTERNET#0'][@Name='RootWeb'][@Owner='" + $currentAccountName + "']")
				$subweb1Node = Select-Xml -Xml $xml -XPath "/Configuration/Web/Web[@Path='/sites/test/subweb1'][@Template='STS#0'][@Name='SubWeb1']"
				$subweb2Node = Select-Xml -Xml $xml -XPath "/Configuration/Web/Web[@Path='/sites/test/subweb2'][@Template='STS#0'][@Name='SubWeb2']"
				$subweb11Node = Select-Xml -Xml $xml -XPath "/Configuration/Web/Web/Web[@Path='/sites/test/subweb1/subweb11'][@Template='STS#0'][@Name='SubWeb11']"            
			}
		   
			$rootNode | Should Not Be $null
			$subweb1Node | Should Not Be $null
			$subweb2Node | Should Not Be $null
			$subweb11Node | Should Not Be $null
			$allNodes.Length | Should Be 4                 
		}

		It "shouldn't export any webs if the command is run on a site on the a target variation label branch" {

			# Execute the command
			Export-DSPWeb -SourceWeb ($site.RootWeb.Url + "/fr/subweb1") -OutputFileName $outputFileName

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
	Remove-Item $outputFileName -Force -Confirm:$false
}