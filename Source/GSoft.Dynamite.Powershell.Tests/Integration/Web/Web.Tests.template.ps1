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
$inputFileName       =  Join-Path -Path "$here" -ChildPath "[[DSP_InputFileName]]"
$inputFileNameOverwrite  =  Join-Path -Path "$here" -ChildPath "[[DSP_InputFileNameOverwrite]]"
$tempSiteCollection   = "[[DSP_TempSiteCollection]]"
$webApplication       = "[[DSP_WebApplicationUrl]]"
$currentAccountName   = ("[[DSP_CurrentAccount]]").ToLower()
$variationsConfigFile   = Join-Path -Path "$here" -ChildPath "[[DSP_VariationsConfigFile]]"
$siteUrl = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($webApplication, $tempSiteCollection))

# TODO: Figure out why this test only work in dev Local but not on TeamCity build :(
Describe "Export-DSPWebStructure" -Tags "Local", "Slow" {

	# ----------------------
	# Export-DSPWebStructure
	# ----------------------
	Context "Test parameters" 	{
		It "should throw an error if 'IgnoreVariations' parameter is specified on a non root web site collection" {
			
			$site = New-SiteWithSubsitesNoVariations -SiteUrl $siteUrl -SubWebsTemplateID "STS#0"
			$subweb1Url = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($site.Url, "subweb1"))

			{ Export-DSPWebStructure -SourceWebUrl $site.RootWeb.Url -OutputFileName $outputFileName -IgnoreVariations } | Should Not Throw		
			{ Export-DSPWebStructure -SourceWebUrl $subweb1Url -OutputFileName $outputFileName -IgnoreVariations } | Should Throw
		}
	}

	Context "The source web doesn't exist" 	{
		It "should throw an error " {

			{ Export-DSPWebStructure -SourceWebUrl "http:///%!" } | Should Throw
		}
	}

	Context "The source web exist"	{

		AfterEach {
			Write-Host "     --Test Teardown--"
			Remove-Item $outputFileName -Force -Confirm:$false
		}

		Write-Host "     --Test Setup--"

		# Create site hierarchy
		$site = New-SingleSiteNoSubsitesNoVariationsWithoutCustomLists -SiteUrl $siteUrl
		

		It "should output a XML file with the correct XSD schema" {

			# Execute the command
			Export-DSPWebStructure -SourceWebUrl $site.RootWeb.Url -OutputFileName $outputFileName

			# A file should be generated
			$outputFileName | Should Exist

			# This file should be match the correct schema
			Test-Xml $outputFileName -SchemaPath $webXmlSchema | Should Be $true
		}
	}
	
	Context "The source web has no subsites"	{

		AfterEach {
			Write-Host "     --Test Teardown--"

			if (Test-Path -PathType 'Leaf' $outputFileName)
			{
				Remove-Item $outputFileName -Force -Confirm:$false
			}
		}

		Write-Host "     --Test Setup--"

		# Create site hierarchy
		$site = New-SingleSiteNoSubsitesNoVariationsWithoutCustomLists -SiteUrl $siteUrl

		It "should export only the source web" {
			
			# Execute the command
			Export-DSPWebStructure -SourceWebUrl $site.RootWeb.Url -OutputFileName $outputFileName

			# Search for the web node which contains the web url
			if (Test-Path $outputFileName)
			{
				[xml]$xml = Get-Content $outputFileName

				$allNodes = Select-Xml -Xml $xml -XPath "//Web"
				$testNode = Select-Xml -Xml $xml -XPath ("/Configuration/Web[@Path='/'][@IsRoot='True'][@Template='CMSPUBLISHING#0'][@Owner='" + $currentAccountName + "']")
			}
		   
			$testNode | Should Not Be $null
			$allNodes.Length | Should Be 1            
		}
	}

	Context "The source web has multiple subsites"	{
	
		AfterEach {

			Write-Host "     --Test Teardown--"

			if (Test-Path -PathType 'Leaf' $outputFileName)
			{
				Remove-Item $outputFileName -Force -Confirm:$false
			}
		}

		Write-Host "     --Test Setup--"

		# Create site hierarchy
		$site = New-SiteWithSubsitesNoVariations -SiteUrl $siteUrl -SubWebsTemplateID "STS#0"

		It "should export all webs and sub webs including the root site" {
			
			# Execute the command
			Export-DSPWebStructure -SourceWebUrl $site.RootWeb.Url -OutputFileName $outputFileName

			# Search for the web node which contains the web url
			if (Test-Path $outputFileName)
			{
				[xml]$xml = Get-Content $outputFileName

				$allNodes     = Select-Xml -Xml $xml -XPath "//Web"
				$rootNode     = Select-Xml -Xml $xml -XPath ("/Configuration/Web[@Path='/'][@IsRoot='True'][@Template='CMSPUBLISHING#0'][@Name='RootWeb'][@Owner='" + $currentAccountName + "'][@Language='1033']")
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

		It "should exclude webs where title match regex tokens" {
			
			# Execute the command
			Export-DSPWebStructure -SourceWebUrl $site.RootWeb.Url -OutputFileName $outputFileName -WebExclusionPatterns @("\bSubWeb1\b","SubWeb2")

			# Search for the web node which contains the web url
			if (Test-Path $outputFileName)
			{
				[xml]$xml = Get-Content $outputFileName

				$allNodes     = Select-Xml -Xml $xml -XPath "//Web"
				$rootNode     = Select-Xml -Xml $xml -XPath ("/Configuration/Web[@Path='/'][@IsRoot='True'][@Template='CMSPUBLISHING#0'][@Name='RootWeb'][@Owner='" + $currentAccountName + "'][@Language='1033']")
				$subweb11Node = Select-Xml -Xml $xml -XPath "/Configuration/Web/Web[@Path='subweb11'][@Template='STS#0'][@Name='SubWeb11'][@Language='1033']"            
			}
		   
			$rootNode | Should Not Be $null
			$subweb11Node | Should Not Be $null
			$allNodes.Length | Should Be 2        
		}
	}

	Context "SharePoint variations are activated on the source web with multiple sites" {
	
		AfterEach {
			Write-Host "     --Test Teardown--"

			if (Test-Path -PathType 'Leaf' $outputFileName)
			{
				Remove-Item $outputFileName -Force -Confirm:$false
			}
		}

		Write-Host "     --Test Setup--"

		# Create site hierarchy
		$site = New-SiteWithSubsitesAndVariationsWithCustomLists -SiteUrl $siteUrl -SubWebsTemplateID "STS#0" -VariationConfigFilePath $variationsConfigFile 	 

		It "should export the original webs and sub webs URL structure form root web site colelction with all generated variations sites if '-IgnoreVariations' is not specified" {

			# Execute the command
			Export-DSPWebStructure -SourceWebUrl $site.RootWeb.Url -OutputFileName $outputFileName

			# Search for the web node which contains the web url
			if (Test-Path $outputFileName)
			{
				[xml]$xml = Get-Content $outputFileName

				$allNodes = Select-Xml -Xml $xml -XPath "//Web"
				$rootNode = Select-Xml -Xml $xml -XPath ("/Configuration/Web[@Path='/'][@IsRoot='True'][@Template='CMSPUBLISHING#0'][@Name='RootWeb'][@Owner='" + $currentAccountName + "'][@Language='1033']")
				$enRootNode = Select-Xml -Xml $xml -XPath "/Configuration/Web/Web[@Path='en'][@Template='CMSPUBLISHING#0'][@Name='English-US'][@Language='1033']"
				$frRootNode = Select-Xml -Xml $xml -XPath "/Configuration/Web/Web[@Path='fr'][@Template='CMSPUBLISHING#0'][@Name='French-FR'][@Language='1036']"
				$subweb1Node = Select-Xml -Xml $xml -XPath "//Web[@Path='subweb1'][@Template='STS#0'][@Name='SubWeb1']"
				$subweb2Node = Select-Xml -Xml $xml -XPath "//Web[@Path='subweb2'][@Template='STS#0'][@Name='SubWeb2']"
				$subweb11Node = Select-Xml -Xml $xml -XPath "//Web[@Path='subweb11'][@Template='STS#0'][@Name='SubWeb11']"            
			}
		   
			$rootNode | Should Not Be $null
			$enRootNode | Should Not Be $null
			$frRootNode | Should Not Be $null
			$subweb1Node.Length | Should Be 2
			$subweb2Node.Length | Should Be 2
			$subweb11Node.Length | Should Be 2
			$allNodes.Length | Should Be 9 
		}

		It "should ignore all SharePoint sites generated by variation system if '-IgnoreVariations' is specified" {

			# Execute the command
			Export-DSPWebStructure -SourceWebUrl $site.RootWeb.Url -OutputFileName $outputFileName -IgnoreVariations

			# Search for the web node which contains the web url
			if (Test-Path $outputFileName)
			{
				[xml]$xml = Get-Content $outputFileName

				$rootNode = Select-Xml -Xml $xml -XPath ("/Configuration/Web[@Path='/'][@IsRoot='True'][@Template='CMSPUBLISHING#0'][@Name='RootWeb'][@Owner='" + $currentAccountName + "'][@Language='1033']")
				$subweb1Node = Select-Xml -Xml $xml -XPath "/Configuration/Web/Web[@Path='subweb1'][@Template='STS#0'][@Name='SubWeb1'][@Language='1033']"
				$subweb1VariationNode = Select-Xml -Xml $xml -XPath "/Configuration/Web/Web[@Path='subweb1'][@Template='STS#0'][@Name='SubWeb1'][@Language='1033'][@Label='en']/Variations/TargetWeb[@Path='subweb1'][@Name='SubWeb1'][@Language='1036'][@Label='fr']"

				$subweb2Node = Select-Xml -Xml $xml -XPath "/Configuration/Web/Web[@Path='subweb2'][@Template='STS#0'][@Name='SubWeb2'][@Language='1033']"
				$subweb2VariationNode = Select-Xml -Xml $xml -XPath "/Configuration/Web/Web[@Path='subweb2'][@Template='STS#0'][@Name='SubWeb2'][@Language='1033'][@Label='en']/Variations/TargetWeb[@Path='subweb2'][@Name='SubWeb2'][@Language='1036'][@Label='fr']"

				$subweb11Node = Select-Xml -Xml $xml -XPath "/Configuration/Web/Web/Web[@Path='subweb11'][@Template='STS#0'][@Name='SubWeb11'][@Language='1033']"    
				$subweb11VariationNode = Select-Xml -Xml $xml -XPath "/Configuration/Web/Web/Web[@Path='subweb11'][@Template='STS#0'][@Name='SubWeb11'][@Language='1033'][@Label='en']/Variations/TargetWeb[@Path='subweb11'][@Name='SubWeb11'][@Language='1036'][@Label='fr']"      
			}
		   
			$rootNode | Should Not Be $null
			$subweb1Node | Should Not Be $null
			$subweb1VariationNode  | Should Not Be $null
			$subweb2Node | Should Not Be $null
			$subweb2VariationNode  | Should Not Be $null
			$subweb11Node | Should Not Be $null
			$subweb11VariationNode  | Should Not Be $null
		}

		It "should export webs and sub webs even if the command is run on a target variation label branch site and '-IgnoreVariations' is not specified" {

			# Execute the command
			Export-DSPWebStructure -SourceWebUrl ($site.RootWeb.Url + "/fr") -OutputFileName $outputFileName

			# Search for the web node which contains the web url
			if (Test-Path $outputFileName)
			{
				[xml]$xml = Get-Content $outputFileName

				$allNodes = Select-Xml -Xml $xml -XPath "//Web"
				$rootNode = Select-Xml -Xml $xml -XPath ("/Configuration/Web[@Path='fr'][@Template='CMSPUBLISHING#0'][@Name='French-FR'][@Language='1036']")
				$subweb1Node = Select-Xml -Xml $xml -XPath "/Configuration/Web/Web[@Path='subweb1'][@Template='STS#0'][@Name='SubWeb1'][@Language='1036']"
				$subweb2Node = Select-Xml -Xml $xml -XPath "/Configuration/Web/Web[@Path='subweb2'][@Template='STS#0'][@Name='SubWeb2'][@Language='1036']"
				$subweb11Node = Select-Xml -Xml $xml -XPath "/Configuration/Web/Web/Web[@Path='subweb11'][@Template='STS#0'][@Name='SubWeb11'][@Language='1036']"            
			}
		   
			$rootNode | Should Not Be $null
			$subweb1Node | Should Not Be $null
			$subweb2Node | Should Not Be $null
			$subweb11Node | Should Not Be $null
			$allNodes.Length | Should Be 4  
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
			$site =  New-SingleSiteNoSubsitesNoVariationsWithoutCustomLists -SiteUrl $siteUrl

			$subweb1Url = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($site.Url, "subweb1"))
			$subweb2Url = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($site.Url, "subweb2"))
			$subweb11Url = ([Microsoft.SharePoint.Utilities.SPUtility]::ConcatUrls($subweb1Url, "subweb11"))

			Import-DSPWebStructure -InputFileName $inputFileName -ParentUrl $site.RootWeb.Url 

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
			$site = New-SingleSiteNoSubsitesNoVariationsWithoutCustomLists -SiteUrl $siteUrl

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