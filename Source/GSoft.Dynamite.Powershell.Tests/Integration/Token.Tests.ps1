$here = Split-Path -Parent $MyInvocation.MyCommand.Path

# Script under test (sut)
$sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path).Replace(".Tests.", ".")
$sutPath = "$here\..\..\GSoft.Dynamite.Scripts\$sut"

function Get-CurrentDomain {
	try {
		# Return this version if wmi is installed
		return [string](gwmi Win32_NTDomain).DomainName.Trim()
	} catch {
		# Fall back on this version in case of error
		return $env:USERDOMAIN
	}
}

# Global test parameters
$tokenValue = "`"tokenValue`""
$templateFileName = "TemplateFile"
$domainNameTokenFile = (Get-Location).Path + "\Tokens." + (Get-CurrentDomain) + ".ps1"

 
Describe "Token.ps1" {

	# Test utility functions

	function New-HostTokenFile {
        $tokenFile = (Get-Location).Path + "\Tokens." + [System.Net.Dns]::GetHostName() + ".ps1"
        New-Item -Path $tokenFile -ItemType file -Value "`$DSP_TestTokenValue = $tokenValue" -Force
	}

	function New-DomainTokenFile {
        $tokenFile = (Get-Location).Path + "\Tokens." + (Get-CurrentDomain) + ".ps1"
        New-Item -Path $tokenFile -ItemType file -Value "`$DSP_TestTokenValue = $tokenValue" -Force
	}
	
	function Remove-TokenFiles {
		Get-ChildItem Tokens.*.ps1 | Remove-Item -Force -Recurse
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
		Get-ChildItem -Path $Path -Include ("*.template", "*.template.*", "$templateFileName.ps1") -Recurse | Remove-Item -Force -Recurse
	}

	Context "Replacing tokens with HOST tokens file" {
	
		BeforeEach {	
			# Pre-condition: make sure nothing exists under $folderPath
			Write-Host "     --Test Setup--"
			New-HostTokenFile
		}

		AfterEach {
			# Post-condition: make sure nothing exists under $folderPath
			Remove-TokenFiles
            Remove-TemplateFiles
			Write-Host "     --Test Teardown--"
		}

		It "should replace the token in the legacy template format (*.template)" {
			# run the script
            New-TemplateFileLegacyFormat
            Update-DSPTokens

            $updatedTemplateFile = Get-ChildItem "$templateFileName.ps1"
			($updatedTemplateFile).Count -eq 1 | Should Be $true
			$updatedTemplateFile.FullName | Should ContainExactly $tokenValue
		}

		It "should replace the token in the intellisense template format (*.template.*)" {
			# run the script
            New-TemplateFileIntellisenseFormat
            Update-DSPTokens

            $updatedTemplateFile = Get-ChildItem "$templateFileName.ps1"
			($updatedTemplateFile).Count -eq 1 | Should Be $true
			$updatedTemplateFile.FullName | Should ContainExactly $tokenValue
		}
	}

	Context "Replacing tokens with DOMAIN tokens file" {
	
		BeforeEach {	
			# Pre-condition: make sure nothing exists under $folderPath
			Write-Host "     --Test Setup--"
            New-DomainTokenFile
		}

		AfterEach {
			# Post-condition: make sure nothing exists under $folderPath
			Remove-TokenFiles
            Remove-TemplateFiles
			Write-Host "     --Test Teardown--"
		}

		It "should replace the token in the legacy template format (*.template)" {
			# run the script
            New-TemplateFileLegacyFormat
            Update-DSPTokens -UseDomain

            $updatedTemplateFile = Get-ChildItem "$templateFileName.ps1"
			($updatedTemplateFile).Count -eq 1 | Should Be $true
			$updatedTemplateFile.FullName | Should ContainExactly $tokenValue
		}

		It "should replace the token in the intellisense template format (*.template.*)" {
			# run the script
            New-TemplateFileIntellisenseFormat
            Update-DSPTokens -UseDomain

            $updatedTemplateFile = Get-ChildItem "$templateFileName.ps1"
			($updatedTemplateFile).Count -eq 1 | Should Be $true
			$updatedTemplateFile.FullName | Should ContainExactly $tokenValue
		}
	}
}