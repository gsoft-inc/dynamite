$here = Split-Path -Parent $MyInvocation.MyCommand.Path

# Script under test (sut)
$sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path).Replace(".Tests.", ".")
$sutPath = "$here\..\..\GSoft.Dynamite.PowerShell\$sut"

# ----------------------
# Tests configuration
# ----------------------
$webApplicationUrl = "[[DSP_WebApplicationUrl]]"
$currentAccount = "[[DSP_CurrentAccount]]"

Describe "New-DSPStructure" -Tags "Slow" {

    Context "When creating a site collection"	{

        It "should create the site collection at the given relative path" {
			# Arrange
			$structureXml = [xml]"<WebApplication Url=`"$webApplicationUrl`"><Site Name=`"Dynamite PowerShell Integration Tests`" OwnerAlias=`"$currentAccount`" Language=`"1033`" Template=`"STS#0`" ContentDatabase=`"SP2013_Content_DynamitePSTests`" RelativePath=`"DynamiteTest`" /></WebApplication>"
			
			# Act
			New-DSPStructure -XmlConfig $structureXml

			# Assert
			$SiteCollection = Get-SPSite "$($webApplicationUrl.trimEnd('/'))/DynamiteTest"
			$SiteCollection | Should Not BeNullOrEmpty

			# Clean Up
			$SiteCollection | Remove-SPSite -Confirm:$false
			Remove-SPManagedPath -Identity "DynamiteTest" -WebApplication $webApplicationUrl -confirm:$false
        }

		It "should create the site collection at the given Managed path" {
			# Arrange
			$structureXml = [xml]"<WebApplication Url=`"$webApplicationUrl`"><Site Name=`"Dynamite PowerShell Integration Tests`" OwnerAlias=`"$currentAccount`" Language=`"1033`" Template=`"STS#0`" ContentDatabase=`"SP2013_Content_DynamitePSTests`" ManagedPath=`"DynamiteTest`" /></WebApplication>"
			
			# Act
			New-DSPStructure -XmlConfig $structureXml

			# Assert
			$SiteCollection = Get-SPSite "$($webApplicationUrl.trimEnd('/'))/DynamiteTest"
			$SiteCollection | Should Not BeNullOrEmpty

			# Clean Up
			$SiteCollection | Remove-SPSite -Confirm:$false
			Remove-SPManagedPath -Identity "DynamiteTest" -WebApplication $webApplicationUrl -confirm:$false
        }

		It "Should create the site collection using the given a cofiguration file" {
			# Arrange
			$inputFileName = Join-Path -Path "$here" -ChildPath ".\ImportSiteStructure.xml"
			
			# Act
			New-DSPStructure -XmlPath $inputFileName

			# Assert
			$SiteCollection = Get-SPSite "$($webApplicationUrl.trimEnd('/'))/DynamiteTest"
			$SiteCollection | Should Not BeNullOrEmpty

			# Clean Up
			$SiteCollection | Remove-SPSite -Confirm:$false
			Remove-SPManagedPath -Identity "DynamiteTest" -WebApplication $webApplicationUrl -confirm:$false
		}
    }
}