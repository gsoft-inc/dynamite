$here = Split-Path -Parent $MyInvocation.MyCommand.Path

# Script under test (sut)
$sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path).Replace(".Tests.", ".")
$sutPath = "$here\..\..\GSoft.Dynamite.PowerShell\$sut"

# ----------------------
# Tests configuration
# ----------------------

$ExcelValidFilePath = Join-Path -Path "$here" -ChildPath ".\ExportSharegate_Valid.xlsx"

Describe "Open-DSPExcelFile" {

	Context "Valid Excel file exported by Sharegate" {

		It "should open the file" {
			$ExcelFile = Open-DSPExcelFile -Path $ExcelValidFilePath

			$ExcelFile | Should Not Be Null
		}		
	}
}

Describe "Get-DSPExcelFileContent" {

		Context "Valid Excel file exported by Sharegate" 	{

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
		}
}

Describe "Merge-DSPExcelColumns" {

	Context "Valid Excel file exported by Sharegate" {

		It "should merge the content of the specified columns into the target columns" {

			# Create a copy of the file
			$TempFolder = New-Item -ItemType 'Directory' -Path $here -Name "Temp" -Force
			$CopiedItem = Copy-Item -Path $ExcelValidFilePath -Destination $TempFolder -PassThru
			$ExcelFile = Open-DSPExcelFile -Path $CopiedItem.FullName

			$ExcelFile | Merge-DSPExcelColumns -TargetColumn "Column1" -SourceColumns @("Column2","Column3")

			$FileContent =  $ExcelFile | Get-DSPExcelFileContent -Columns @("Column1")

			# Test values
			$FileContent[0].Column1 | Should Be "Value1Value2Value3"
			$FileContent[1].Column1 | Should Be "Value11Value22Value33"
			$FileContent[2].Column1 | Should Be "Value111Value222Value333"
			
			# Test teardown
			Remove-Item $TempFolder -Recurse
		}		
	}
}

Describe "Add-DSPExcelColumn" {

	Context "Valid Excel file exported by Sharegate" {

		It "should add the column in the file" {
		
			# Create a copy of the file
			$TempFolder = New-Item -ItemType 'Directory' -Path $here -Name "Temp" -Force
			$CopiedItem = Copy-Item -Path $ExcelValidFilePath -Destination $TempFolder -PassThru
			$ExcelFile = Open-DSPExcelFile -Path $CopiedItem.FullName

			$ExcelFile | Add-DSPExcelColumn -ColumnName "NewColumn"

			$FileContent =  $ExcelFile | Get-DSPExcelFileContent -Columns @("NewColumn")
		
			# Test values
			$FileContent[0].NewColumn | Should Not Be Null

			# Test teardown
			Remove-Item $TempFolder -Recurse
		}		
	}
}

Describe "Remove-DSPExcelColumn" {

	Context "Valid Excel file exported by Sharegate" {

		It "should remove the column in the file" {
		
			# Create a copy of the file
			$TempFolder = New-Item -ItemType 'Directory' -Path $here -Name "Temp" -Force
			$CopiedItem = Copy-Item -Path $ExcelValidFilePath -Destination $TempFolder -PassThru
			$ExcelFile = Open-DSPExcelFile -Path $CopiedItem.FullName

			$ExcelFile | Remove-DSPExcelColumn -ColumnName "Column1"

			$FileContent =  $ExcelFile | Get-DSPExcelFileContent -Columns @("Column1","Column2")
			
			# Test values
			$FileContent[0].Column1 | Should Throw
			$FileContent[0].Column2 | Should Not Be Null

			# Test teardown
			Remove-Item $TempFolder -Recurse
		}		
	}
}

Describe "Copy-DSPExcelColumn" {

	Context "Valid Excel file exported by Sharegate" {

		It "should copy the content between a source column and a target column for each row in the file" {
		
			# Create a copy of the file
			$TempFolder = New-Item -ItemType 'Directory' -Path $here -Name "Temp" -Force
			$CopiedItem = Copy-Item -Path $ExcelValidFilePath -Destination $TempFolder -PassThru
			$ExcelFile = Open-DSPExcelFile -Path $CopiedItem.FullName

			$ExcelFile | Copy-DSPExcelColumn -SourceColumn "Column2" -TargetColumn "Column1"

			$FileContent =  $ExcelFile | Get-DSPExcelFileContent -Columns @("Column1","Column2")
		
			# Test values
			$FileContent[0].Column1 | Should Be "Value2"
			$FileContent[0].Column2 | Should Be "Value2"

			$FileContent[1].Column1 | Should Be "Value22"
			$FileContent[1].Column2 | Should Be "Value22"

			$FileContent[2].Column1 | Should Be "Value222"
			$FileContent[2].Column2 | Should Be "Value222"

			# Test teardown
			Remove-Item $TempFolder -Recurse
		}		
	}
}

Describe "Edit-DSPExcelColumn" {

	Context "Valid Excel file exported by Sharegate" {

		It "should replace the value in the whole file if no column is specified" {
		
			# Create a copy of the file
			$TempFolder = New-Item -ItemType 'Directory' -Path $here -Name "Temp" -Force
			$CopiedItem = Copy-Item -Path $ExcelValidFilePath -Destination $TempFolder -PassThru
			$ExcelFile = Open-DSPExcelFile -Path $CopiedItem.FullName

			$ExcelFile | Edit-DSPExcelColumnValue -Pattern "Value" -Value "Replaced"

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
			Remove-Item $TempFolder -Recurse
		}		

		It "should replace all values matching the regex token" {

			# Create a copy of the file
			$TempFolder = New-Item -ItemType 'Directory' -Path $here -Name "Temp" -Force
			$CopiedItem = Copy-Item -Path $ExcelValidFilePath -Destination $TempFolder -PassThru
			$ExcelFile = Open-DSPExcelFile -Path $CopiedItem.FullName

			$ExcelFile | Edit-DSPExcelColumnValue -Pattern "DEV\\\w+" -Value "DEV\john.doe"

			$FileContent =  $ExcelFile | Get-DSPExcelFileContent -Columns @("User")
		
			# Test values
			$FileContent.Count | Should Be 3
			$FileContent[0].User | Should Be "DEV\john.doe"
				
			$FileContent[1].User | Should Be "DEV\john.doe"
	
			$FileContent[2].User | Should Be "DEV\john.doe"

			# Test teardown
			Remove-Item $TempFolder -Recurse
	
		}

		It "should replace values only in the specified column" {

			# Create a copy of the file
			$TempFolder = New-Item -ItemType 'Directory' -Path $here -Name "Temp" -Force
			$CopiedItem = Copy-Item -Path $ExcelValidFilePath -Destination $TempFolder -PassThru
			$ExcelFile = Open-DSPExcelFile -Path $CopiedItem.FullName

			$ExcelFile | Edit-DSPExcelColumnValue -Pattern "Value" -Value "Replaced" -Column "Column1"

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
			Remove-Item $TempFolder -Recurse
		}
	}
}