$here = Split-Path -Parent $MyInvocation.MyCommand.Path

# Script under test (sut)
$sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path).Replace(".Tests.", ".")
$sutPath = "$here\..\..\GSoft.Dynamite.Scripts\$sut"
$folderPath = (Get-Location).Path + "\Logs"
 
Describe "Logging.ps1" {

	# Test utility functions

	function New-LogsFolder {
		if ((Test-Path $folderPath) -ne $true) {
            New-Item -Path $folderPath -ItemType directory
		}
	}
	
	function Remove-LogsFolder {
		if (Test-Path $folderPath) {
			Get-ChildItem -Path $folderPath -Recurse | Remove-Item -Force -Recurse
			Remove-Item $folderPath
		}
	}

	Context "when deployment folder has not yet been created" {
	
		BeforeEach {	
			# Pre-condition: make sure nothing exists under $folderPath
			Write-Host "     --Test Setup--"
			Remove-LogsFolder
		}

		AfterEach {
			# Post-condition: make sure nothing exists under $folderPath
			Remove-LogsFolder
			Write-Host "     --Test Teardown--"
		}

		It "should create a logging folder at specified location" {
            if ($host.name -eq 'ConsoleHost') {

                # run the script
                $folderPath = (Get-Location).Path + "\Logs"
			    Start-DSPLogging -commandName "Test-DSPLogging" -folder $folderPath
                Stop-DSPLogging -commandName "Test-DSPLogging" -folder $folderPath
			    Test-Path $folderPath | Should Be $true
            }
            else {
                Write-Warning "Cannot test 'Start-Transcript' in anything else than the console host."
            }
		}

		It "should create a logging folder with a single *.log file at specified location" {
            if ($host.name -eq 'ConsoleHost') {
			    # run the script
                $folderPath = (Get-Location).Path + "\Logs"
			    Start-DSPLogging -commandName "Test-DSPLogging" -folder $folderPath
                Stop-DSPLogging -commandName "Test-DSPLogging" -folder $folderPath

                $logFile = Get-ChildItem $folderPath *.log
			    $logFile.Count -eq 1 | Should Be $true
			    $logFile.Length -gt 1 | Should Be $true
            }
            else {
                Write-Warning "Cannot test 'Start-Transcript' in anything else than the console host."
            }
		}
	}

	Context "when deployment folder has been created" {
	
		BeforeEach {	
			# Pre-condition: make sure nothing exists under $folderPath
			Write-Host "     --Test Setup--"
			New-LogsFolder
		}

		AfterEach {
			# Post-condition: make sure nothing exists under $folderPath
			Remove-LogsFolder
			Write-Host "     --Test Teardown--"
		}

		It "should create a logging folder with a single *.log file at specified location" {
            if ($host.name -eq 'ConsoleHost') {
			    # run the script
                $folderPath = (Get-Location).Path + "\Logs"
			    Start-DSPLogging -commandName "Test-DSPLogging" -folder $folderPath
                Stop-DSPLogging -commandName "Test-DSPLogging" -folder $folderPath
			
                $logFile = Get-ChildItem $folderPath *.log
			    $logFile.Count -eq 1 | Should Be $true
			    $logFile.Length -gt 1 | Should Be $true
            }
            else {
                Write-Warning "Cannot test 'Start-Transcript' in anything else than the console host."
            }
		}
	}

	Context "when transcript is already started" {
	
		BeforeEach {	
			# Pre-condition: make sure nothing exists under $folderPath
			Write-Host "     --Test Setup--"
			Remove-LogsFolder
            if ($host.name -eq 'ConsoleHost') {
			    Start-Transcript
            }
		}

		AfterEach {
			# Post-condition: make sure nothing exists under $folderPath
			Remove-LogsFolder
			Write-Host "     --Test Teardown--"
		}

		It "should stop the current transcript and log normally" {
            if ($host.name -eq 'ConsoleHost') {
			    # run the script
                $folderPath = (Get-Location).Path + "\Logs"
			    Start-DSPLogging -commandName "Test-DSPLogging" -folder $folderPath
                Stop-DSPLogging -commandName "Test-DSPLogging" -folder $folderPath
			
                $logFile = Get-ChildItem $folderPath *.log
			    $logFile.Count -eq 1 | Should Be $true
			    $logFile.Length -gt 1 | Should Be $true
            }
            else {
                Write-Warning "Cannot test 'Start-Transcript' in anything else than the console host."
            }
		}
	}
}