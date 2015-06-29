$here = Split-Path -Parent $MyInvocation.MyCommand.Path

# Script under test (sut)
$sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path).Replace(".Tests.", ".")
$sutPath = "$here\..\..\GSoft.Dynamite.PowerShell\$sut"
$sourceFolderPath = (Get-Location).Path + "\source"
$destFolderPath = (Get-Location).Path + "\destination"
 
Describe "Utilities.ps1" {

    #region Test utility functions
    function New-SourceFolder {
        if ((Test-Path $sourceFolderPath) -ne $true) {
            New-Item -Path $sourceFolderPath -ItemType directory
        }
    }

    function New-DestinationFolder {
        if ((Test-Path $destFolderPath) -ne $true) {
            New-Item -Path $destFolderPath -ItemType directory
        }
    }
    
    function Remove-Folders {
        if (Test-Path $sourceFolderPath) {
            Get-ChildItem -Path $sourceFolderPath -Recurse | Remove-Item -Force -Recurse
            Remove-Item $sourceFolderPath
        }

        if (Test-Path $destFolderPath) {
            Get-ChildItem -Path $destFolderPath -Recurse | Remove-Item -Force -Recurse
            Remove-Item $destFolderPath
        }
    }

    function New-PowerShellFiles {
        New-Item -Path ($sourceFolderPath + "\test1.ps1") -ItemType file -Force
        New-Item -Path ($sourceFolderPath + "\test2.ps1") -ItemType file -Force
        New-Item -Path ($sourceFolderPath + "\test3.ps1") -ItemType file -Force
        New-Item -Path ($sourceFolderPath + "\test4.ps1") -ItemType file -Force
    }

    function New-TextFiles {
        New-Item -Path ($sourceFolderPath + "\test1.txt") -ItemType file -Force
        New-Item -Path ($sourceFolderPath + "\test2.txt") -ItemType file -Force
        New-Item -Path ($sourceFolderPath + "\test3.txt") -ItemType file -Force
        New-Item -Path ($sourceFolderPath + "\test4.txt") -ItemType file -Force
    }

    function New-SolutionFiles {
        New-Item -Path ($sourceFolderPath + "\test1.wsp") -ItemType file -Force
        New-Item -Path ($sourceFolderPath + "\test2.wsp") -ItemType file -Force
        New-Item -Path ($sourceFolderPath + "\test3.wsp") -ItemType file -Force
        New-Item -Path ($sourceFolderPath + "\test4.wsp") -ItemType file -Force
    }
    #endregion

    Context "when copying files with wildcard match specified" {
    
        BeforeEach {	
            Write-Host "     --Test Setup--"
            New-SourceFolder
            New-DestinationFolder
        }

        AfterEach {
            Write-Host "     --Test Teardown--"
            Remove-Folders
        }

        It "should copy all the files to the destination folder" {
            # Arrange
            New-PowerShellFiles
            New-TextFiles

            # Act
            Copy-DSPFiles -Path $sourceFolderPath -DestinationPath $destFolderPath -Match *.*

            # Assert
            Test-Path $destFolderPath | Should Be $true
            (Get-ChildItem $destFolderPath -Include @("*.ps1") -Recurse).Count -gt 0 | Should Be $true
            (Get-ChildItem $destFolderPath -Include @("*.txt") -Recurse).Count -gt 0 | Should Be $true
        }
    }

    Context "when copying files with match specified" {
    
        BeforeEach {	
            Write-Host "     --Test Setup--"
            New-SourceFolder
            New-DestinationFolder
        }

        AfterEach {
            Write-Host "     --Test Teardown--"
            Remove-Folders
        }

        It "should copy only files in the specified match to the destination folder" {
            # Arrange
            New-PowerShellFiles
            New-TextFiles

            # Act
            Copy-DSPFiles -Path $sourceFolderPath -DestinationPath $destFolderPath -Match @("*.ps1")

            # Assert
            Test-Path $destFolderPath | Should Be $true
            (Get-ChildItem $destFolderPath -Include @("*.ps1") -Recurse).Count -gt 0 | Should Be $true
            (Get-ChildItem $destFolderPath -Include @("*.txt") -Recurse).Count -gt 0 | Should Be $false
        }
    }

    Context "when copying files with exclude specified" {
    
        BeforeEach {	
            Write-Host "     --Test Setup--"
            New-SourceFolder
            New-DestinationFolder
        }

        AfterEach {
            Write-Host "     --Test Teardown--"
            Remove-Folders
        }

        It "should not copy files in the specified exclude to the destination folder" {
            # Arrange
            New-PowerShellFiles
            New-TextFiles

            # Act
            Copy-DSPFiles -Path $sourceFolderPath -DestinationPath $destFolderPath -Match *.* -Exclude *.txt

            # Assert
            Test-Path $destFolderPath | Should Be $true
            (Get-ChildItem $destFolderPath -Include @("*.ps1") -Recurse).Count -gt 0 | Should Be $true
            (Get-ChildItem $destFolderPath -Include @("*.txt") -Recurse).Count -gt 0 | Should Be $false
        }
    }

    Context "when copying solution files" {
    
        BeforeEach {	
            Write-Host "     --Test Setup--"
            New-SourceFolder
            New-DestinationFolder
        }

        AfterEach {
            Write-Host "     --Test Teardown--"
            Remove-Folders
        }

        It "should only copy files with *.wsp extension to the destination folder" {
            # Arrange
            New-SolutionFiles

            # Act
            Copy-DSPSolutions -Path $sourceFolderPath -DestinationPath $destFolderPath

            # Assert
            Test-Path $destFolderPath | Should Be $true
            (Get-ChildItem $destFolderPath -Include @("*.wsp") -Recurse).Count -gt 0 | Should Be $true
            (Get-ChildItem $destFolderPath -Exclude @("*.wsp") -Recurse).Count -gt 0 | Should Be $false
        }
    }
}