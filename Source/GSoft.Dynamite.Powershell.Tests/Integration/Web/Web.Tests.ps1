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
$inputFileName       =  Join-Path -Path "$here" -ChildPath ".\ImportWebStructure.xml"
$inputFileNameOverwrite  =  Join-Path -Path "$here" -ChildPath ".\ImportWebStructure_Overwrite.xml"
$tempSiteCollection   = "sites/exporttest"
$webApplication       = "http://franck-vm2013"
$currentAccountName   = ("OFFICE\franck.cornu").ToLower()
$variationsConfigFile   = Join-Path -Path "$here" -ChildPath "./TestVariationsSettings.xml"
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

	return Get-SPSite $site.Url
}

# TODO: Figure out why this test only work in dev Local but not on TeamCity build :(
Describe "Export-DSPWebStructure" -Tags "Local", "Slow" {



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
			Export-DSPWebStructure -SourceWebUrl $site.RootWeb.Url -OutputFileName $outputFileName

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

		It "should export webs and sub webs even if the command is run on a target variation label branch site" {

			# Execute the command
			Export-DSPWebStructure -SourceWebUrl ($site.RootWeb.Url + "/fr") -OutputFileName $outputFileName

			# Search for the web node which contains the web url
			if (Test-Path $outputFileName)
			{
				[xml]$xml = Get-Content $outputFileName

				$allNodes = Select-Xml -Xml $xml -XPath "//Web"
				$rootNode = Select-Xml -Xml $xml -XPath ("/Configuration/Web[@Path='fr'][@Template='CMSPUBLISHING'][@Name='French-FR'][@Language='1036']")
				$subweb1Node = Select-Xml -Xml $xml -XPath "/Configuration/Web[@Path='subweb1'][@Template='STS#0'][@Name='SubWeb1'][@Language='1036']"
				$subweb2Node = Select-Xml -Xml $xml -XPath "/Configuration/Web[@Path='subweb2'][@Template='STS#0'][@Name='SubWeb2'][@Language='1036']"
				$subweb11Node = Select-Xml -Xml $xml -XPath "/Configuration/Web/Web[@Path='subweb11'][@Template='STS#0'][@Name='SubWeb11'][@Language='1036']"            
			}
		   
			$rootNode | Should Not Be $null
			$subweb1Node | Should Not Be $null
			$subweb2Node | Should Not Be $null
			$subweb11Node | Should Not Be $null
			$allNodes.Length | Should Be 4  
		}
	}


}
