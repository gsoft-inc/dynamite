$here = Split-Path -Parent $MyInvocation.MyCommand.Path

# Script under test (sut)
$sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path).Replace(".Tests.", ".")
$sutPath = "$here\..\..\GSoft.Dynamite.PowerShell\$sut"

$webApplicationUrl = "[[DSP_WebApplicationUrl]]"

Describe "ManagedPath.ps1" {

	Context "When creating a managed path" {
		It "Should be an explicit managed path by default" {
			# Arrange
			$relativeURL = "DSPTestPath"
			
			# Act
			New-DSPManagedPath -RelativeURL $relativeURL -WebApplication $webApplicationUrl

			# Assert
			$existingManagedPath = Get-SPManagedPath -WebApplication $webApplicationUrl | Where {$_.Name -eq $relativeURL}

			$existingManagedPath | Should Not BeNullOrEmpty
			$existingManagedPath.Type | Should Be "ExplicitInclusion"

			# Clean Up
			Remove-SPManagedPath -Identity $relativeURL -WebApplication $webApplicationUrl -confirm:$false
		}
	}

	Context "When creating an explicit managed path for a given Web Application" {
	    It "Should create an explicit managed path in that Web Application" {
			# Arrange
			$relativeURL = "DSPTestPath"
			
			# Act
			New-DSPManagedPath -RelativeURL $relativeURL -WebApplication $webApplicationUrl -Wildcard:$false

			# Assert
			$existingManagedPath = Get-SPManagedPath -WebApplication $webApplicationUrl | Where {$_.Name -eq $relativeURL}

			$existingManagedPath | Should Not BeNullOrEmpty
			$existingManagedPath.Type | Should Be "ExplicitInclusion"

			# Clean Up
			Remove-SPManagedPath -Identity $relativeURL -WebApplication $webApplicationUrl -confirm:$false
		}
	}

	Context "When creating a wildcard managed path for a given Web Application" {
		It "Should create a wildcard managed path in that Web Application" {
			# Arrange
			$relativeURL = "DSPTestPath"
			
			# Act
			New-DSPManagedPath -RelativeURL $relativeURL -WebApplication $webApplicationUrl -Wildcard

			# Assert
			$existingManagedPath = Get-SPManagedPath -WebApplication $webApplicationUrl | Where {$_.Name -eq $relativeURL}

			$existingManagedPath | Should Not BeNullOrEmpty
			$existingManagedPath.Type | Should Be "WildcardInclusion"

			# Clean Up
			Remove-SPManagedPath -Identity $relativeURL -WebApplication $webApplicationUrl -confirm:$false
		}
	}

	Context "When creating an explicit managed path for no Web Application" {
		It "Should create an explicit managed path for all host header site collections" {
			# Arrange
			$relativeURL = "DSPTestPath"
			
			# Act
			New-DSPManagedPath -RelativeURL $relativeURL

			# Assert
			$existingManagedPath = Get-SPManagedPath -HostHeader | Where {$_.Name -eq $relativeURL}

			$existingManagedPath | Should Not BeNullOrEmpty
			$existingManagedPath.Type | Should Be "ExplicitInclusion"

			# Clean Up
			Remove-SPManagedPath -Identity $relativeURL -HostHeader -confirm:$false
		}
	}

	Context "When creating a wildcard managed path for no Web Application" {
		It "Should create a wildcard managed path for all host header site collections" {
			# Arrange
			$relativeURL = "DSPTestPath"
			
			# Act
			New-DSPManagedPath -RelativeURL $relativeURL -Wildcard

			# Assert
			$existingManagedPath = Get-SPManagedPath -HostHeader | Where {$_.Name -eq $relativeURL}

			$existingManagedPath | Should Not BeNullOrEmpty
			$existingManagedPath.Type | Should Be "WildcardInclusion"

			# Clean Up
			Remove-SPManagedPath -Identity $relativeURL -HostHeader -confirm:$false
		}
	}

	Context "When the managed path already exists" {
		It "Should not throw and error" {
			# Arrange
			$relativeURL = "DSPTestPath"
			
			# Act
			New-DSPManagedPath -RelativeURL $relativeURL -WebApplication $webApplicationUrl

			# Assert
			{ New-DSPManagedPath -RelativeURL $relativeURL -WebApplication $webApplicationUrl } | Should Not Throw

			# Clean Up
			Remove-SPManagedPath -Identity $relativeURL -WebApplication $webApplicationUrl -confirm:$false
		}
	}
}