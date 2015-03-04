$here = Split-Path -Path $MyInvocation.MyCommand.Path -Parent 

# Script under test (sut)
$sut = (Split-Path -Path $MyInvocation.MyCommand.Path -Leaf).Replace(".Tests.", ".")
$sutPath = "$here\..\..\GSoft.Dynamite.PowerShell\$sut"

function Get-CurrentDomain {
	try {
		# Return this version if wmi is installed
		return [string](Get-WmiObject -Class Win32_NTDomain).DomainName.Trim()
	} catch {
		# Fall back on this version in case of error
		return $env:USERDOMAIN
	}
}

# Global test parameters
$tokenValue = "`"tokenValue`""
$templateFileName = "TemplateFile"
$domainNameTokenFile = $here + "\Tokens." + (Get-CurrentDomain) + ".ps1"

 
Describe "Token.ps1" {

	# Test utility functions
	# Set working directory to the current test folder
	Set-Location $here

	function New-HostTokenFile {
        $tokenFile = (Get-Location).Path + "\Tokens." + [System.Net.Dns]::GetHostName() + ".ps1"
        New-Item -Path $tokenFile -ItemType file -Value "`$DSP_TestTokenValue = $tokenValue" -Force
	}

	function New-DomainTokenFile {
        $tokenFile = (Get-Location).Path + "\Tokens." + (Get-CurrentDomain) + ".ps1"
        New-Item -Path $tokenFile -ItemType file -Value "`$DSP_TestTokenValue = $tokenValue" -Force
	}
	
	function Remove-TokenFiles {
		Get-ChildItem -Filter Tokens.*.ps1 | Remove-Item -Force -Recurse
	}

    function New-TemplateFileIntellisenseFormat {
        $templateFile = (Get-Location).Path + "\$templateFileName.template.ps1"
        New-Item -Path $templateFile -ItemType file -Value "`"[[DSP_TestTokenValue]]`"" -Force
    }

    function New-TemplateFileLegacyFormat {
        $templateFile = (Get-Location).Path + "\$templateFileName.ps1.template"
        New-Item -Path $templateFile -ItemType file -Value "`"[[DSP_TestTokenValue]]`"" -Force
    }

    function Remove-TemplateFiles {
		Get-ChildItem -Path $here -Include ("*.template", "*.template.*", "$templateFileName.ps1") -Recurse | Remove-Item -Force -Recurse
	}

	Context "Replacing tokens with HOST tokens file" {
	
		BeforeEach {	
			# Pre-condition: make sure nothing exists under $folderPath
			Write-Host -Object "     --Test Setup--"
			New-HostTokenFile
		}

		AfterEach {
			# Post-condition: make sure nothing exists under $folderPath
			Remove-TokenFiles
            Remove-TemplateFiles
			Write-Host -Object "     --Test Teardown--"
		}

		It "should replace the token in the legacy template format (*.template)" {
			# run the script
            New-TemplateFileLegacyFormat
            Update-DSPTokens

            $updatedTemplateFile = $here | Get-ChildItem -Filter "$templateFileName.ps1"
			($updatedTemplateFile).Count -eq 1 | Should Be $true
			$updatedTemplateFile.FullName | Should ContainExactly $tokenValue
		}

		It "should replace the token in the intellisense template format (*.template.*)" {
			# run the script
            New-TemplateFileIntellisenseFormat
            Update-DSPTokens

            $updatedTemplateFile = $here | Get-ChildItem -Filter "$templateFileName.ps1"
			($updatedTemplateFile).Count -eq 1 | Should Be $true
			$updatedTemplateFile.FullName | Should ContainExactly $tokenValue
		}
	}

	Context "Replacing tokens with DOMAIN tokens file" {
	
		BeforeEach {	
			# Pre-condition: make sure nothing exists under $folderPath
			Write-Host -Object "     --Test Setup--"
            New-DomainTokenFile
		}

		AfterEach {
			# Post-condition: make sure nothing exists under $folderPath
			Remove-TokenFiles
            Remove-TemplateFiles
			Write-Host -Object "     --Test Teardown--"
		}

		It "should replace the token in the legacy template format (*.template)" {
			# run the script
            New-TemplateFileLegacyFormat
            Update-DSPTokens -UseDomain

            $updatedTemplateFile = Get-ChildItem -Filter "$templateFileName.ps1"
			($updatedTemplateFile).Count -eq 1 | Should Be $true
			$updatedTemplateFile.FullName | Should ContainExactly $tokenValue
		}

		It "should replace the token in the intellisense template format (*.template.*)" {
			# run the script
            New-TemplateFileIntellisenseFormat
            Update-DSPTokens -UseDomain

            $updatedTemplateFile = Get-ChildItem -Filter "$templateFileName.ps1"
			($updatedTemplateFile).Count -eq 1 | Should Be $true
			$updatedTemplateFile.FullName | Should ContainExactly $tokenValue
		}
	}

	Context "No token file" {
	
		BeforeEach {	
			# Pre-condition: make sure nothing exists under $folderPath
			Write-Host -Object "     --Test Setup--"
			Remove-TokenFiles
            Remove-TemplateFiles
		}

		AfterEach {
			# Post-condition: make sure nothing exists under $folderPath
			Remove-TokenFiles
            Remove-TemplateFiles
			Write-Host -Object "     --Test Teardown--"
		}

		It "should throw file not found exception" {
			{ Update-DSPTokens } | Should Throw
		}
	}

	Context "No template files" {
	
		BeforeEach {	
			# Pre-condition: make sure nothing exists under $folderPath
			Write-Host -Object "     --Test Setup--"
			New-HostTokenFile
            Remove-TemplateFiles
		}

		AfterEach {
			# Post-condition: make sure nothing exists under $folderPath
			Remove-TokenFiles
            Remove-TemplateFiles
			Write-Host -Object "     --Test Teardown--"
		}

		It "should not throw exception and template file shouldn't exist" {
            Update-DSPTokens			
			Get-ChildItem -Filter "$templateFileName.ps1"| Should BeNullOrEmpty
		}
	}

	# Reset working directory
	Set-Location (Split-Path -Path (Get-Location) -Parent)
}