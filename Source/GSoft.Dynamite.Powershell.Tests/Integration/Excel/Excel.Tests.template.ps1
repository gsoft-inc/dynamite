$here = Split-Path -Parent $MyInvocation.MyCommand.Path

# Script under test (sut)
$sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path).Replace(".Tests.", ".")
$sutPath = "$here\..\..\GSoft.Dynamite.PowerShell\$sut"

# ----------------------
# Tests configuration
# ----------------------

$ExcelValidFilePath = Join-Path -Path "$here" -ChildPath ".\ExportSharegate_Valid.xlsx"
$ExcelValidFilePathUserModified = Join-Path -Path "$here" -ChildPath ".\ExportSharegate_UserModified.xlsx"

Describe "Open-DSPExcelFile" {

	# ----------------------
	# Open-DSPExcelFile
	# ----------------------
	Context "Valid Excel file exported by Sharegate" {

		It "should open the file" {
			$ExcelFile = Open-DSPExcelFile -Path $ExcelValidFilePath
			$ExcelFile.Dispose()

			$ExcelFile | Should Not Be Null
		}		
	}
}

Describe "Get-DSPExcelFileContent" {

	# ----------------------
	# Get-DSPExcelFileContent
	# ----------------------
	Context "Valid Excel file exported by Sharegate" 	{

		It "should throw an error if the specified worksheet doesn't exist in the file" {
			
			# Create a copy of the file
			$TempFolder = New-Item -ItemType 'Directory' -Path $here -Name "Temp" -Force
			$CopiedItem = Copy-Item -Path $ExcelValidFilePath -Destination $TempFolder -PassThru -Force
			$ExcelFile = Open-DSPExcelFile -Path $CopiedItem.FullName

			{ $ExcelFile | Get-DSPExcelFileContent -Columns @("Column1","Column2","Column3") -WorksheetName "FakeWorksheet" } | Should Throw
						
			# Test teardown
			Remove-Item $TempFolder -Recurse -Confirm:$false -Force
		}
		
		It "should dispose the Excel file object after execution if '-NoDispose' parameter isn't present" {
		
			# Create a copy of the file
			$TempFolder = New-Item -ItemType 'Directory' -Path $here -Name "Temp" -Force
			$CopiedItem = Copy-Item -Path $ExcelValidFilePath -Destination $TempFolder -PassThru -Force
			$ExcelFile = Open-DSPExcelFile -Path $CopiedItem.FullName

			$ExcelFile | Get-DSPExcelFileContent -Columns @("Column1","Column2","Column3")

			# $Excel File is disposed at this time
			{ $ExcelFile | Get-DSPExcelFileContent -Columns @("Column1","Column2","Column3") } | Should Throw
			
			# Test teardown
			Remove-Item $TempFolder -Recurse -Confirm:$false -Force
		}

		It "should get the content of each row for specified columns" {
						 
				$ExcelFile = Open-DSPExcelFile -Path $ExcelValidFilePath
							
				$FileContent =  $ExcelFile | Get-DSPExcelFileContent -Columns @("Column1","Column2","Column3")
				
				# Test values
				$FileContent.Count | Should Be 3
				$FileContent[0].Column1 | Should Be "Value1"
				$FileContent[0].Column2 | Should Be "Value2"
				$FileContent[0].Column3 | Should Be "Value3"
				
				$FileContent[1].Column1 | Should Be "Value11"
				$FileContent[1].Column2 | Should Be "Value22"
				$FileContent[1].Column3 | Should Be "Value33"
				
				$FileContent[2].Column1 | Should Be "Value111"
				$FileContent[2].Column2 | Should Be "Value222"
				$FileContent[2].Column3 | Should Be "Value333"									
			}

		It "should get the content of the file even if it was modified by user in Excel" {
		
				$ExcelFile = Open-DSPExcelFile -Path $ExcelValidFilePathUserModified
							
				$FileContent =  $ExcelFile | Get-DSPExcelFileContent -Columns @("Column1","Column2","Column3")
				
				# Test values
				$FileContent.Count | Should Be 3
				$FileContent[0].Column1 | Should Be "Value1"
				$FileContent[0].Column2 | Should Be "Value2"
				$FileContent[0].Column3 | Should Be "Value3"
				
				$FileContent[1].Column1 | Should Be "Value11"
				$FileContent[1].Column2 | Should Be "Value22"
				$FileContent[1].Column3 | Should Be "Value33"
				
				$FileContent[2].Column1 | Should Be "Value111"
				$FileContent[2].Column2 | Should Be "Value222"
				$FileContent[2].Column3 | Should Be "Value333"	
		}
	}
}

Describe "Merge-DSPExcelColumns" {

	# ----------------------
	# Merge-DSPExcelColumns
	# ----------------------
	Context "Valid Excel file exported by Sharegate" {

		It "should throw an error if the specified worksheet doesn't exist in the file" {
			
			# Create a copy of the file
			$TempFolder = New-Item -ItemType 'Directory' -Path $here -Name "Temp" -Force
			$CopiedItem = Copy-Item -Path $ExcelValidFilePath -Destination $TempFolder -PassThru -Force
			$ExcelFile = Open-DSPExcelFile -Path $CopiedItem.FullName

			{ $ExcelFile | Merge-DSPExcelColumns -TargetColumn "Column1" -SourceColumns @("Column2","Column3") -WorksheetName "FakeWorksheet" } | Should Throw
			
			# Test teardown
			Remove-Item $TempFolder -Recurse -Confirm:$false -Force
		}
		
		It "should dispose the Excel file object after execution if '-NoDispose' parameter isn't present" {
		
			# Create a copy of the file
			$TempFolder = New-Item -ItemType 'Directory' -Path $here -Name "Temp" -Force
			$CopiedItem = Copy-Item -Path $ExcelValidFilePath -Destination $TempFolder -PassThru -Force
			$ExcelFile = Open-DSPExcelFile -Path $CopiedItem.FullName

			$ExcelFile | Merge-DSPExcelColumns -TargetColumn "Column1" -SourceColumns @("Column2","Column3") 

			# $Excel File is disposed at this time
			{ $ExcelFile | Merge-DSPExcelColumns -TargetColumn "Column1" -SourceColumns @("Column2","Column3") } | Should Throw
			
			# Test teardown
			Remove-Item $TempFolder -Recurse -Confirm:$false -Force
		}

		It "should merge the content of the specified columns into the target columns" {

			# Create a copy of the file
			$TempFolder = New-Item -ItemType 'Directory' -Path $here -Name "Temp" -Force
			$CopiedItem = Copy-Item -Path $ExcelValidFilePath -Destination $TempFolder -PassThru -Force
			$ExcelFile = Open-DSPExcelFile -Path $CopiedItem.FullName

			$ExcelFile | Merge-DSPExcelColumns -TargetColumn "Column1" -SourceColumns @("Column2","Column3") -NoDispose

			$FileContent =  $ExcelFile | Get-DSPExcelFileContent -Columns @("Column1")

			# Test values
			$FileContent[0].Column1 | Should Be "Value1Value2Value3"
			$FileContent[1].Column1 | Should Be "Value11Value22Value33"
			$FileContent[2].Column1 | Should Be "Value111Value222Value333"
			
			# Test teardown
			Remove-Item $TempFolder -Recurse -Confirm:$false -Force
		}	
	
		It "should merge the content of the specified columns into the target columns even if it was modified by user in Excel" {
		
			# Create a copy of the file
			$TempFolder = New-Item -ItemType 'Directory' -Path $here -Name "Temp" -Force
			$CopiedItem = Copy-Item -Path $ExcelValidFilePathUserModified -Destination $TempFolder -PassThru -Force
			$ExcelFile = Open-DSPExcelFile -Path $CopiedItem.FullName

			$ExcelFile | Merge-DSPExcelColumns -TargetColumn "Column1" -SourceColumns @("Column2","Column3") -NoDispose

			$FileContent =  $ExcelFile | Get-DSPExcelFileContent -Columns @("Column1")

			# Test values
			$FileContent[0].Column1 | Should Be "Value1Value2Value3"
			$FileContent[1].Column1 | Should Be "Value11Value22Value33"
			$FileContent[2].Column1 | Should Be "Value111Value222Value333"
			
			# Test teardown
			Remove-Item $TempFolder -Recurse -Confirm:$false -Force	
		}	
	}
}

Describe "Add-DSPExcelColumn" {

	# ----------------------
	# Add-DSPExcelColumn
	# ----------------------
	Context "Valid Excel file exported by Sharegate" {

		It "should throw an error if the specified worksheet doesn't exist in the file" {
			
			# Create a copy of the file
			$TempFolder = New-Item -ItemType 'Directory' -Path $here -Name "Temp" -Force
			$CopiedItem = Copy-Item -Path $ExcelValidFilePath -Destination $TempFolder -PassThru -Force
			$ExcelFile = Open-DSPExcelFile -Path $CopiedItem.FullName

			{ $ExcelFile | Add-DSPExcelColumn -ColumnName "NewColumn" -WorksheetName "FakeWorksheet" } | Should Throw
			
			# Test teardown
			Remove-Item $TempFolder -Recurse -Confirm:$false -Force
		}
		
		It "should dispose the Excel file object after execution if '-NoDispose' parameter isn't present" {
		
			# Create a copy of the file
			$TempFolder = New-Item -ItemType 'Directory' -Path $here -Name "Temp" -Force
			$CopiedItem = Copy-Item -Path $ExcelValidFilePath -Destination $TempFolder -PassThru -Force
			$ExcelFile = Open-DSPExcelFile -Path $CopiedItem.FullName

			$ExcelFile | Add-DSPExcelColumn -ColumnName "NewColumn"

			# $Excel File is disposed at this time
			{ $ExcelFile | Add-DSPExcelColumn -ColumnName "NewColumn" } | Should Throw
			
			# Test teardown
			Remove-Item $TempFolder -Recurse -Confirm:$false -Force
		}

		It "should add the column in the file" {
		
			# Create a copy of the file
			$TempFolder = New-Item -ItemType 'Directory' -Path $here -Name "Temp" -Force
			$CopiedItem = Copy-Item -Path $ExcelValidFilePath -Destination $TempFolder -PassThru -Force
			$ExcelFile = Open-DSPExcelFile -Path $CopiedItem.FullName

			$ExcelFile | Add-DSPExcelColumn -ColumnName "NewColumn" -NoDispose

			$FileContent =  $ExcelFile | Get-DSPExcelFileContent -Columns @("NewColumn") 
		
			# Test values
			$FileContent[0].NewColumn | Should Not Be Null

			# Test teardown
			Remove-Item $TempFolder -Recurse -Confirm:$false -Force
		}	
	
		It "should add the column in the file and set values as IDs for each row" {
		
			# Create a copy of the file
			$TempFolder = New-Item -ItemType 'Directory' -Path $here -Name "Temp" -Force
			$CopiedItem = Copy-Item -Path $ExcelValidFilePath -Destination $TempFolder -PassThru -Force
			$ExcelFile = Open-DSPExcelFile -Path $CopiedItem.FullName

			$ExcelFile | Add-DSPExcelColumn -ColumnName "NewColumn" -NoDispose

			$FileContent =  $ExcelFile | Get-DSPExcelFileContent -Columns @("NewColumn")
		
			# Test values
			$FileContent[0].NewColumn | Should Not Be Null

			# Test teardown
			Remove-Item $TempFolder -Recurse -Confirm:$false -Force
		}			
	}
}

Describe "Remove-DSPExcelColumn" {

	# ----------------------
	# Remove-DSPExcelColumn
	# ----------------------
	Context "Valid Excel file exported by Sharegate" {

		It "should throw an error if the specified worksheet doesn't exist in the file" {
			
			# Create a copy of the file
			$TempFolder = New-Item -ItemType 'Directory' -Path $here -Name "Temp" -Force
			$CopiedItem = Copy-Item -Path $ExcelValidFilePath -Destination $TempFolder -PassThru -Force
			$ExcelFile = Open-DSPExcelFile -Path $CopiedItem.FullName

			{ $ExcelFile | Remove-DSPExcelColumn -ColumnName "Column1" -WorksheetName "FakeWorksheet" } | Should Throw
			
			# Test teardown
			Remove-Item $TempFolder -Recurse -Confirm:$false -Force
		}
		
		It "should dispose the Excel file object after execution if '-NoDispose' parameter isn't present" {
		
			# Create a copy of the file
			$TempFolder = New-Item -ItemType 'Directory' -Path $here -Name "Temp" -Force
			$CopiedItem = Copy-Item -Path $ExcelValidFilePath -Destination $TempFolder -PassThru -Force
			$ExcelFile = Open-DSPExcelFile -Path $CopiedItem.FullName

			$ExcelFile | Remove-DSPExcelColumn -ColumnName "Column1"

			# $Excel File is disposed at this time
			{ $ExcelFile | Remove-DSPExcelColumn -ColumnName "Column1" } | Should Throw
			
			# Test teardown
			Remove-Item $TempFolder -Recurse -Confirm:$false -Force
		}

		It "should remove the column in the file" {
		
			# Create a copy of the file
			$TempFolder = New-Item -ItemType 'Directory' -Path $here -Name "Temp" -Force
			$CopiedItem = Copy-Item -Path $ExcelValidFilePath -Destination $TempFolder -PassThru -Force
			$ExcelFile = Open-DSPExcelFile -Path $CopiedItem.FullName

			$ExcelFile | Remove-DSPExcelColumn -ColumnName "Column1" -NoDispose

			$FileContent =  $ExcelFile | Get-DSPExcelFileContent -Columns @("Column1","Column2")
			
			# Test values
			$FileContent[0].Column1 | Should Throw
			$FileContent[0].Column2 | Should Not Be Null

			# Test teardown
			Remove-Item $TempFolder -Recurse -Confirm:$false -Force
		}		

		It "should remove the column in the file even if it was modified by user in Excel" {

			# Create a copy of the file
			$TempFolder = New-Item -ItemType 'Directory' -Path $here -Name "Temp" -Force
			$CopiedItem = Copy-Item -Path $ExcelValidFilePathUserModified -Destination $TempFolder -PassThru -Force
			$ExcelFile = Open-DSPExcelFile -Path $CopiedItem.FullName

			$ExcelFile | Remove-DSPExcelColumn -ColumnName "Column1" -NoDispose

			$FileContent =  $ExcelFile | Get-DSPExcelFileContent -Columns @("Column1","Column2")
			
			# Test values
			$FileContent[0].Column1 | Should Throw
			$FileContent[0].Column2 | Should Not Be Null

			# Test teardown
			Remove-Item $TempFolder -Recurse -Confirm:$false -Force
		}
	}
}

Describe "Copy-DSPExcelColumn" {

	# ----------------------
	# Copy-DSPExcelColumn
	# ----------------------
	Context "Valid Excel file exported by Sharegate" {
	
		It "should throw an error if the specified worksheet doesn't exist in the file" {
			
			# Create a copy of the file
			$TempFolder = New-Item -ItemType 'Directory' -Path $here -Name "Temp" -Force
			$CopiedItem = Copy-Item -Path $ExcelValidFilePath -Destination $TempFolder -PassThru -Force
			$ExcelFile = Open-DSPExcelFile -Path $CopiedItem.FullName

			{ $ExcelFile | Copy-DSPExcelColumn -SourceColumn "Column2" -TargetColumn "Column1" -WorksheetName "FakeWorksheet" }  | Should Throw
			
			# Test teardown
			Remove-Item $TempFolder -Recurse -Confirm:$false -Force
		}
		
		It "should dispose the Excel file object after execution if '-NoDispose' parameter isn't present" {
		
			# Create a copy of the file
			$TempFolder = New-Item -ItemType 'Directory' -Path $here -Name "Temp" -Force
			$CopiedItem = Copy-Item -Path $ExcelValidFilePath -Destination $TempFolder -PassThru -Force
			$ExcelFile = Open-DSPExcelFile -Path $CopiedItem.FullName

			$ExcelFile | Copy-DSPExcelColumn -SourceColumn "Column2" -TargetColumn "Column1"

			# $Excel File is disposed at this time
			{ $ExcelFile | Copy-DSPExcelColumn -SourceColumn "Column2" -TargetColumn "Column1" }  | Should Throw
			
			# Test teardown
			Remove-Item $TempFolder -Recurse -Confirm:$false -Force
		}

		It "should copy the content between a source column and a target column for each row in the file" {
		
			# Create a copy of the file
			$TempFolder = New-Item -ItemType 'Directory' -Path $here -Name "Temp" -Force
			$CopiedItem = Copy-Item -Path $ExcelValidFilePath -Destination $TempFolder -PassThru -Force
			$ExcelFile = Open-DSPExcelFile -Path $CopiedItem.FullName

			$ExcelFile | Copy-DSPExcelColumn -SourceColumn "Column2" -TargetColumn "Column1" -NoDispose

			$FileContent =  $ExcelFile | Get-DSPExcelFileContent -Columns @("Column1","Column2")
		
			# Test values
			$FileContent[0].Column1 | Should Be "Value2"
			$FileContent[0].Column2 | Should Be "Value2"

			$FileContent[1].Column1 | Should Be "Value22"
			$FileContent[1].Column2 | Should Be "Value22"

			$FileContent[2].Column1 | Should Be "Value222"
			$FileContent[2].Column2 | Should Be "Value222"

			# Test teardown
			Remove-Item $TempFolder -Recurse -Confirm:$false -Force
		}	
	
		It "should copy the content between a source column and a target column for each row in the file even if it was modified by user in Excel" {

			# Create a copy of the file
			$TempFolder = New-Item -ItemType 'Directory' -Path $here -Name "Temp" -Force
			$CopiedItem = Copy-Item -Path $ExcelValidFilePathUserModified -Destination $TempFolder -PassThru -Force
			$ExcelFile = Open-DSPExcelFile -Path $CopiedItem.FullName

			$ExcelFile | Copy-DSPExcelColumn -SourceColumn "Column2" -TargetColumn "Column1" -NoDispose

			$FileContent =  $ExcelFile | Get-DSPExcelFileContent -Columns @("Column1","Column2")
		
			# Test values
			$FileContent[0].Column1 | Should Be "Value2"
			$FileContent[0].Column2 | Should Be "Value2"

			$FileContent[1].Column1 | Should Be "Value22"
			$FileContent[1].Column2 | Should Be "Value22"

			$FileContent[2].Column1 | Should Be "Value222"
			$FileContent[2].Column2 | Should Be "Value222"

			# Test teardown
			Remove-Item $TempFolder -Recurse -Confirm:$false -Force
		}	
	}
}

Describe "Edit-DSPExcelColumn" {

	# ----------------------
	# Edit-DSPExcelColumn
	# ----------------------
	Context "Valid Excel file exported by Sharegate" {

		It "should throw an error if the specified worksheet doesn't exist in the file" {
			
			# Create a copy of the file
			$TempFolder = New-Item -ItemType 'Directory' -Path $here -Name "Temp" -Force
			$CopiedItem = Copy-Item -Path $ExcelValidFilePath -Destination $TempFolder -PassThru -Force
			$ExcelFile = Open-DSPExcelFile -Path $CopiedItem.FullName

			{ $ExcelFile | Edit-DSPExcelColumnValue -Pattern "Value" -Value "Replaced" -WorksheetName "FakeWorksheet" } | Should Throw
			
			# Test teardown
			Remove-Item $TempFolder -Recurse -Confirm:$false -Force
		}
		
		It "should dispose the Excel file object after execution if '-NoDispose' parameter isn't present" {
		
			# Create a copy of the file
			$TempFolder = New-Item -ItemType 'Directory' -Path $here -Name "Temp" -Force
			$CopiedItem = Copy-Item -Path $ExcelValidFilePath -Destination $TempFolder -PassThru -Force
			$ExcelFile = Open-DSPExcelFile -Path $CopiedItem.FullName

			$ExcelFile | Edit-DSPExcelColumnValue -Pattern "Value" -Value "Replaced"

			# $Excel File is disposed at this time
			{ $ExcelFile | Edit-DSPExcelColumnValue -Pattern "Value" -Value "Replaced" } | Should Throw
			
			# Test teardown
			Remove-Item $TempFolder -Recurse -Confirm:$false -Force
		}

		It "should replace the value in the whole file if no column is specified" {
		
			# Create a copy of the file
			$TempFolder = New-Item -ItemType 'Directory' -Path $here -Name "Temp" -Force
			$CopiedItem = Copy-Item -Path $ExcelValidFilePath -Destination $TempFolder -PassThru -Force
			$ExcelFile = Open-DSPExcelFile -Path $CopiedItem.FullName

			$ExcelFile | Edit-DSPExcelColumnValue -Pattern "Value" -Value "Replaced" -NoDispose

			$FileContent =  $ExcelFile | Get-DSPExcelFileContent -Columns @("Column1","Column2","Column3")
		
			# Test values
			$FileContent[0].Column1 | Should Be "Replaced1"
			$FileContent[0].Column2 | Should Be "Replaced2"
			$FileContent[0].Column3 | Should Be "Replaced3"
				
			$FileContent[1].Column1 | Should Be "Replaced11"
			$FileContent[1].Column2 | Should Be "Replaced22"
			$FileContent[1].Column3 | Should Be "Replaced33"
				
			$FileContent[2].Column1 | Should Be "Replaced111"
			$FileContent[2].Column2 | Should Be "Replaced222"
			$FileContent[2].Column3 | Should Be "Replaced333"		

			# Test teardown
			Remove-Item $TempFolder -Recurse -Confirm:$false -Force
		}		

		It "should replace all values matching the regex token" {

			# Create a copy of the file
			$TempFolder = New-Item -ItemType 'Directory' -Path $here -Name "Temp" -Force
			$CopiedItem = Copy-Item -Path $ExcelValidFilePath -Destination $TempFolder -PassThru -Force
			$ExcelFile = Open-DSPExcelFile -Path $CopiedItem.FullName

			$ExcelFile | Edit-DSPExcelColumnValue -Pattern "DEV\\\w+" -Value "NEWDOMAIN\john.doe" -NoDispose

			$FileContent =  $ExcelFile | Get-DSPExcelFileContent -Columns @("User")
		
			# Test values
			$FileContent.Count | Should Be 3
			$FileContent[0].User | Should Be "NEWDOMAIN\john.doe"
				
			$FileContent[1].User | Should Be "NEWDOMAIN\john.doe"
	
			$FileContent[2].User | Should Be "NEWDOMAIN\john.doe"

			# Test teardown
			Remove-Item $TempFolder -Recurse -Confirm:$false -Force	
		}

		It "should replace all values matching the regex token even if it was modified by user in Excel" {

			# Create a copy of the file
			$TempFolder = New-Item -ItemType 'Directory' -Path $here -Name "Temp" -Force
			$CopiedItem = Copy-Item -Path $ExcelValidFilePathUserModified -Destination $TempFolder -PassThru -Force
			$ExcelFile = Open-DSPExcelFile -Path $CopiedItem.FullName

			$ExcelFile | Edit-DSPExcelColumnValue -Pattern "DEV\\\w+" -Value "NEWDOMAIN\john.doe" -NoDispose

			$FileContent =  $ExcelFile | Get-DSPExcelFileContent -Columns @("User")
		
			# Test values
			$FileContent.Count | Should Be 3
			$FileContent[0].User | Should Be "NEWDOMAIN\john.doe"
				
			$FileContent[1].User | Should Be "NEWDOMAIN\john.doe"
	
			$FileContent[2].User | Should Be "NEWDOMAIN\john.doe"

			# Test teardown
			Remove-Item $TempFolder -Recurse -Confirm:$false -Force	

		}

		It "should replace values only in the specified column" {

			# Create a copy of the file
			$TempFolder = New-Item -ItemType 'Directory' -Path $here -Name "Temp" -Force
			$CopiedItem = Copy-Item -Path $ExcelValidFilePath -Destination $TempFolder -PassThru -Force
			$ExcelFile = Open-DSPExcelFile -Path $CopiedItem.FullName

			$ExcelFile | Edit-DSPExcelColumnValue -Pattern "Value" -Value "Replaced" -Column "Column1" -NoDispose

			$FileContent =  $ExcelFile | Get-DSPExcelFileContent -Columns @("Column1","Column2","Column3")
		
			# Test values
			$FileContent.Count | Should Be 3
			$FileContent[0].Column1 | Should Be "Replaced1"
			$FileContent[0].Column2 | Should Be "Value2"
			$FileContent[0].Column3 | Should Be "Value3"
				
			$FileContent[1].Column1 | Should Be "Replaced11"
			$FileContent[1].Column2 | Should Be "Value22"
			$FileContent[1].Column3 | Should Be "Value33"
				
			$FileContent[2].Column1 | Should Be "Replaced111"
			$FileContent[2].Column2 | Should Be "Value222"
			$FileContent[2].Column3 | Should Be "Value333"

			# Test teardown
			Remove-Item $TempFolder -Recurse -Confirm:$false -Force
		}

		It "should replace values only in the specified column even if it was modified by user in Excel" {

			# Create a copy of the file
			$TempFolder = New-Item -ItemType 'Directory' -Path $here -Name "Temp" -Force
			$CopiedItem = Copy-Item -Path $ExcelValidFilePathUserModified -Destination $TempFolder -PassThru -Force
			$ExcelFile = Open-DSPExcelFile -Path $CopiedItem.FullName

			$ExcelFile | Edit-DSPExcelColumnValue -Pattern "Value" -Value "Replaced" -Column "Column1" -NoDispose

			$FileContent =  $ExcelFile | Get-DSPExcelFileContent -Columns @("Column1","Column2","Column3")
		
			# Test values
			$FileContent.Count | Should Be 3
			$FileContent[0].Column1 | Should Be "Replaced1"
			$FileContent[0].Column2 | Should Be "Value2"
			$FileContent[0].Column3 | Should Be "Value3"
				
			$FileContent[1].Column1 | Should Be "Replaced11"
			$FileContent[1].Column2 | Should Be "Value22"
			$FileContent[1].Column3 | Should Be "Value33"
				
			$FileContent[2].Column1 | Should Be "Replaced111"
			$FileContent[2].Column2 | Should Be "Value222"
			$FileContent[2].Column3 | Should Be "Value333"

			# Test teardown
			Remove-Item $TempFolder -Recurse -Confirm:$false -Force
		}
		
		It "should generate IDs in the column if the -AsIdentifier parameter is specified" {
			
			# Create a copy of the file
			$TempFolder = New-Item -ItemType 'Directory' -Path $here -Name "Temp" -Force
			$CopiedItem = Copy-Item -Path $ExcelValidFilePath -Destination $TempFolder -PassThru -Force
			$ExcelFile = Open-DSPExcelFile -Path $CopiedItem.FullName

			$ExcelFile | Edit-DSPExcelColumnValue -Column "Column1" -AsIdentifier -NoDispose

			$FileContent =  $ExcelFile | Get-DSPExcelFileContent -Columns @("Column1")
			
			# Test values
			$FileContent.Count | Should Be 3
			$FileContent[0].Column1 | Should Be "1"
				
			$FileContent[1].Column1 | Should Be "2"
				
			$FileContent[2].Column1 | Should Be "3"

			# Test teardown
			Remove-Item $TempFolder -Recurse -Confirm:$false -Force			
		}
	}
}


Describe "Find-DSPExcelFiles" {

	It "should throw an error if the specified worksheet doesn't exist in the file" {
			
			# Create a copy of the file
			$TempFolder = New-Item -ItemType 'Directory' -Path $here -Name "Temp" -Force
			$CopiedItem = Copy-Item -Path $ExcelValidFilePath -Destination $TempFolder -PassThru -Force

			{ Find-DSPExcelFiles -Folder $TempFolder.FullName -Columns "Column1" -Patterns "Value" -WorksheetName "FakeWorksheet" } | Should Throw
			
			# Test teardown
			Remove-Item $TempFolder -Recurse -Confirm:$false -Force
	}

	It "should find files with specific tokens inside for both user modified Excel file and untouched Sharegate exported file in the folder" {

			# Create a copy of the files
			$TempFolder = New-Item -ItemType 'Directory' -Path $here -Name "Temp" -Force
			$CopiedItemSharegate = Copy-Item -Path $ExcelValidFilePath -Destination $TempFolder -PassThru -Force
			$CopiedItemUserModified = Copy-Item -Path $ExcelValidFilePathUserModified -Destination $TempFolder -PassThru -Force

			$result = Find-DSPExcelFiles -Folder $TempFolder.FullName -Columns "Column1","Column2","User" -Patterns "dev\\danb","Value"
			
			$result.Count | Should Be 2
			$result[0]."Matching lines count" | Should Be 3
			$result[1]."Matching lines count" | Should Be 3

			# Test teardown
			Remove-Item $TempFolder -Recurse -Confirm:$false -Force
	}

}