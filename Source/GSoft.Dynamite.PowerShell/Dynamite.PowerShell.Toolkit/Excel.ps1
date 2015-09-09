# Utility functions
function Get-ColumnName {
    Param
    (
        [Parameter(Mandatory=$true)]
        [int]$ColumnNumber	
    )
    
    $dividend = $ColumnNumber;
    $columnName = [string]::Empty;

    while ($dividend -gt 0)
    {
        $modulo = ($dividend - 1) % 26;
        $columnName = [Convert]::ToChar(65 + $modulo).ToString() + $columnName;
        $dividend = [int](($dividend - $modulo) / 26);
    } 

    return $columnName;
}

function Get-ColumnNumber {
    Param
    (
        [Parameter(Mandatory=$true)]
        [string]$ColumnName	
    )

    if ([string]::IsNullOrEmpty($ColumnName))
    {
        throw "Invalid column name parameter"
    }

    $ColumnName = $ColumnName.ToUpperInvariant()

    [int]$sum = 0;

    [char]$ch = $null
    
    for ($i = 0; $i -lt $columnName.Length; $i++)
    {
        $ch = $columnName[$i]

        if ([char]::IsDigit($ch))
        {
            throw "Invalid column name parameter on character " + $ch
        }
        $sum *= 26;
        $sum += ([int][char]$ch - [int][char]'A'  + 1)
    }

    return $sum
}

function Get-ExcelCellValue {

    Param
    (
        [ValidateNotNullOrEmpty()]
        [Parameter(Mandatory=$true, Position=0)]
        $WorkbookPart,
        
        [ValidateNotNullOrEmpty()]
        [Parameter(Mandatory=$true, Position=1)]
        $Cell
    )

    $CellDataType = $Cell.DataType.Value

    # These formats are known to be date formats built into Excel
    $StyleSheetsIndexForDateFormats = @(14,15,16,17,18,19,20,21,22,27,28,29,30,31,32,33,34,35,36,45,46,47,50,51,52,53,54,55,56,57,58,71,72,73,74,75,76,77,78,79,80,81)

    switch ($CellDataType)
    {
        "SharedString" {

            $SharedStringTable = Invoke-GenericMethod $WorkbookPart "GetPartsOfType" "DocumentFormat.OpenXml.Packaging.SharedStringTablePart" @()
        
            $SharedStringIndex = $null
            [void][int32]::TryParse($Cell.CellValue.InnerText, [ref]$SharedStringIndex)
    
            $Elements = Invoke-GenericMethod $SharedStringTable.SharedStringTable "Elements" "DocumentFormat.OpenXml.Spreadsheet.SharedStringItem" @() 
            $CellValue = $Elements.Get($SharedStringIndex).InnerText	
        }

        { ($_ -eq" String") -or ($_ -eq "Inline String") -or ($_ -eq" Error") } {

            $CellValue = $Cell.CellValue.InnerText
        }

        # Dates
        { ($_ -eq "Number") -or ($_ -eq $null) } {
            
            $WorkbookStylesPart = Invoke-GenericMethod $WorkbookPart "GetPartsOfType" "DocumentFormat.OpenXml.Packaging.WorkbookStylesPart" @()

            $Elements = Invoke-GenericMethod $WorkbookStylesPart.StyleSheet  "Elements" "DocumentFormat.OpenXml.Spreadsheet.CellFormats" @()
                
            # Get cell formats
            $CellFormats = Invoke-GenericMethod $Elements  "Elements" "DocumentFormat.OpenXml.Spreadsheet.CellFormat" @() 

            $numFmtId = $CellFormats.Get($Cell.StyleIndex.InnerText).NumberFormatId.Value

            # Check the style sheet index for dates
            if ($StyleSheetsIndexForDateFormats -contains $numFmtId)
            {
                $CellValue = [DateTime]::FromOADate([double]::Parse($Cell.CellValue.InnerText)).ToUniversalTime()
            }
            else
            {
                $CellValue = $Cell.CellValue.InnerText
            }
        }

        "Boolean" {

            switch ($Cell.CellValue.InnerText)
            {
                0 { $CellValue  = "FALSE" }
    
                1 { $CellValue  = "TRUE" }
            }
        }
    }
        
    return $CellValue
}


<#
    .SYNOPSIS
        Open an Excel file
    
    .DESCRIPTION
        Open the Excel file specified as paramater. The assembly "DocumnetFormat.OpenXml" is needed to open Excel files.
    --------------------------------------------------------------------------------------
    Module 'Dynamite.PowerShell.Toolkit'
    by: GSoft, Team Dynamite.
    > GSoft & Dynamite : http://www.gsoft.com
    > Dynamite Github : https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit
    > Documentation : https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit/wiki
    --------------------------------------------------------------------------------------
        
    .PARAMETER Path
        [REQUIRED] The Excel file path

    .EXAMPLE
            
            $ExcelFile = Open-DSPExcelFile -Path "C:\Excel.xslx"

    .LINK
    GSoft, Team Dynamite on Github
    > https://github.com/GSoft-SharePoint
    
    Dynamite PowerShell Toolkit on Github
    > https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit
    
    Documentation
    > https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit/wiki
    
#>
function Open-DSPExcelFile {

    [CmdletBinding()]
    Param
    (
        [ValidateScript({Test-Path $_ -PathType 'Leaf'})]
        [Parameter(Mandatory=$true, Position=0)]
        $Path
    )

    # Load OpenXML assembly from SDK if installed
    $assembly = [Reflection.Assembly]::LoadWithPartialName("DocumentFormat.OpenXml")

    if ($assembly -eq $null)
    {
        # Try to get the dll from Sharegate installation
        $assembly = [Reflection.Assembly]::LoadFile("C:\Program Files (x86)\Sharegate\DocumentFormat.OpenXml.dll")

        if ($assembly -eq $null)
        {
            Write-Warning "Unable to load assembly 'DocumentFormat.OpenXml.dll'. Make sure Open XML SDK or Sharegate are installed on this machine"
        }
    }
    
    if($assembly)
    {
        Try {

            $ExcelFile = [DocumentFormat.OpenXml.Packaging.SpreadsheetDocument]::Open($Path, $true)
            
            return $ExcelFile
        }
        Catch 
        {
            if ($ExcelFile)
            {
                $ExcelFile.Dispose()
            }

            $ErrorMessage = $_.Exception.Message
            Throw $ErrorMessage		 
        }
    }
}

<#
    .SYNOPSIS
        Merges columns in a Excel file
    
    .DESCRIPTION
        Merge the content of multiple columns into a single one
    --------------------------------------------------------------------------------------
    Module 'Dynamite.PowerShell.Toolkit'
    by: GSoft, Team Dynamite.
    > GSoft & Dynamite : http://www.gsoft.com
    > Dynamite Github : https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit
    > Documentation : https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit/wiki
    --------------------------------------------------------------------------------------
        
    .PARAMETER ExcelFile
        [REQUIRED] The Excel file instance

    .PARAMETER TargetColumn
        [REQUIRED] The column name to merge to

    .PARAMETER SourceColumns
        [REQUIRED] List of columns to get the content to merge

    .PARAMETER WorksheetName
        [OPTIONAL] The Excel worksheet name to work in

    .PARAMETER NoDispose
        [OPTIONAL] If this parameter is specified, the cmdlet doesn't release the Excel File oject after execution. Use this option to process multiple operations on the same file instance.

    .EXAMPLE
            $ExcelFile = Open-DSPExcelFile -Path "C:\Excel.xslx"

            # Merge Columns
            $ExcelFile | Merge-Columns -TargetColumn "Col1" -SourceColumns "Col2","Col3","Col4" -WorksheetName "Sheet1" -NoDispose

    .LINK
    GSoft, Team Dynamite on Github
    > https://github.com/GSoft-SharePoint
    
    Dynamite PowerShell Toolkit on Github
    > https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit
    
    Documentation
    > https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit/wiki 
#>
function Merge-DSPExcelColumns {

    [CmdletBinding()]
    Param
    (
        [ValidateNotNullOrEmpty()]
        [Parameter(Mandatory=$true, Position=0, ValueFromPipeline=$true)]
        $ExcelFile,
    
        [Parameter(Mandatory=$true, Position=1)]
        [string]$TargetColumn,
        
        [Parameter(Mandatory=$true, Position=2)]
        [array]$SourceColumns,
        
        [Parameter(Mandatory=$false)]
        [string]$WorksheetName,

        [Parameter(Mandatory=$false)]
        [switch]$NoDispose
    )
    
    Try
    {
        $workbookPart = $ExcelFile.WorkbookPart
        $workbook = $workbookPart.Workbook

        if ([string]::IsNullOrEmpty($WorksheetName) -ne $true)
        {
            $Sheet = $workbook.Descendants() | Where-Object { $_.Name -like $WorksheetName -and $_.LocalName -eq "sheet" }
            if ($Sheet -eq $null)
            {
                Throw "Workheet '$WorksheetName' not found in the file"
            }
        }
        else
        {
            $Sheet = $workbook.Descendants() | Where-Object { $_.LocalName -eq "sheet" } | Select-Object -First 1
            $SheetName = $Sheet.Name
            $DebugMsg = "No worksheet name specified. Using first sheet '$SheetName'"
            Write-Debug $DebugMsg 
        }
    
        $SheetId = ($Sheet.Id | Select-Object -Property Value -First 1).Value
        $WorksheetPart = $workbookPart.GetPartById($SheetId)
    
        $SourceColumnsIndex = @()
        $Rows = Invoke-GenericMethod $WorksheetPart.WorkSheet "Descendants" "DocumentFormat.OpenXml.Spreadsheet.Row" @()
    
        $Rows | ForEach-Object {
            $CurrentRow = $_
            $CurrentRowIndex = $_.RowIndex.Value
        
            $Cells = Invoke-GenericMethod $CurrentRow "Descendants" "DocumentFormat.OpenXml.Spreadsheet.Cell" @()

            # First Row
            if ($CurrentRowIndex -eq 1)
            {			
                # Get the target column index				

                $ExactMatchPattern = "\b" + $TargetColumn + "\b"
                $TargetHeaderCell = $Cells | Where-Object { ((Get-ExcelCellValue -WorkbookPart $workbookPart -Cell $_) | Select-String -Pattern $TargetColumn) -ne $null }				
                $TargetHeaderCellIndex = $TargetHeaderCell.CellReference.Value -replace "\d",[string]::Empty
            
                # Get source columns index 
                $Cells | Where-Object { ((Get-ExcelCellValue -WorkbookPart $workbookPart -Cell $_) | Select-String -Pattern $SourceColumns) -ne $null } | ForEach-Object {		
                    $SourceColumnsIndex +=  $_.CellReference.Value -replace "\d",[string]::Empty
                }
            }
            else #Other rows
            {				
                if ($TargetHeaderCellIndex -ne $null)
                {
                    # Get the target cell
                    $TargetCell  =  $Cells | Where-Object { [regex]::Match($_.CellReference.Value, "^" + $TargetHeaderCellIndex +"\d").Success -eq $true }
               
                    # Get the source cells
                    $SourceColumnsIndex | Foreach-Object {

                        $Token = $_
                        # Match cell on the correct column index
                        $SourceCell =  $Cells | Where-Object { [regex]::Match($_.CellReference.Value, "^" + $Token +"\d").Success -eq $true }
                    
                        # Merge values
                        $NewValue = (Get-ExcelCellValue -WorkbookPart $workbookPart -Cell $TargetCell) + (Get-ExcelCellValue -WorkbookPart $workbookPart -Cell $SourceCell)
                        $TargetCell.CellValue = New-Object DocumentFormat.OpenXml.Spreadsheet.CellValue($NewValue)
                        $TargetCell.DataType = [DocumentFormat.OpenXml.Spreadsheet.CellValues]::String    
                    }
                }
            }
        }
        
        $WorksheetPart.WorkSheet.Save()

        if ($NoDispose.IsPresent -eq $false)
        {
            $DebugMsg = "'-NoDispose' parameter was not specified. Disposing the Excel file object..."
            Write-Debug $DebugMsg 
            $ExcelFile.Dispose()
        }
    }
    Catch
    {
        if ($ExcelFile)
        {
            $ExcelFile.Dispose()
        }

        $ErrorMessage = $_.Exception.Message
        Throw $ErrorMessage	
    }
}

<#
    .SYNOPSIS
        Removes a column in a Excel file
    
    .DESCRIPTION
        Removes a column in a Excel file using its name
    --------------------------------------------------------------------------------------
    Module 'Dynamite.PowerShell.Toolkit'
    by: GSoft, Team Dynamite.
    > GSoft & Dynamite : http://www.gsoft.com
    > Dynamite Github : https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit
    > Documentation : https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit/wiki
    --------------------------------------------------------------------------------------
        
    .PARAMETER ExcelFile
        [REQUIRED] The Excel file instance

    .PARAMETER Columns
        [REQUIRED] The columns to delete

    .PARAMETER WorksheetName
        [OPTIONAL] The Excel worksheet name to work in

    .PARAMETER NoDispose
        [OPTIONAL] If this parameter is specified, the cmdlet doesn't release the Excel File oject after execution. Use this option to process multiple operations on the same file instance.

    .EXAMPLE
            $ExcelFile = Open-DSPExcelFile -Path "C:\Excel.xslx"

            # Remove Columns
            $ExcelFile | Remove-Column -Column "Col1" -WorksheetName "Sheet1"

    .LINK
    GSoft, Team Dynamite on Github
    > https://github.com/GSoft-SharePoint
    
    Dynamite PowerShell Toolkit on Github
    > https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit
    
    Documentation
    > https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit/wiki
    
#>
function Remove-DSPExcelColumn {

    [CmdletBinding()]
    Param
    (
        [ValidateNotNullOrEmpty()]
        [Parameter(Mandatory=$true, Position=0, ValueFromPipeline=$true)]
         $ExcelFile,
        
        [Parameter(Mandatory=$true, Position=1)]
        [string]$ColumnName,
        
        [Parameter(Mandatory=$false, Position=2)]
        [string]$WorksheetName,

        [Parameter(Mandatory=$false)]
        [switch]$NoDispose
    )

    Try 
    {
        
        $workbookPart = $ExcelFile.WorkbookPart
        $workbook = $workbookPart.Workbook

        if ([string]::IsNullOrEmpty($WorksheetName) -ne $true)
        {
            $Sheet = $workbook.Descendants() | Where-Object { $_.Name -like $WorksheetName -and $_.LocalName -eq "sheet" }
            if ($Sheet -eq $null)
            {
                $ExcelFile.Dispose()
                Throw "Workheet '$WorksheetName' not found in the file"
            }
        }
        else
        {
            $Sheet = $workbook.Descendants() | Where-Object { $_.LocalName -eq "sheet" } | Select-Object -First 1
            $SheetName = $Sheet.Name

            $DebugMsg = "No worksheet name specified. Using first sheet '$SheetName'"
            Write-Debug $DebugMsg
        }
    
        $SheetId = ($Sheet.Id | Select-Object -Property Value -First 1).Value
        $WorksheetPart = $workbookPart.GetPartById($SheetId)
    
        $ColumnToDeleteIndex = $null
    
        $Rows = Invoke-GenericMethod $WorksheetPart.WorkSheet "Descendants" "DocumentFormat.OpenXml.Spreadsheet.Row" @()
    
        $Rows | ForEach-Object {

            $CurrentRow = $_
            $CurrentRowIndex = $CurrentRow.RowIndex.Value

            $Cells = Invoke-GenericMethod $CurrentRow "Descendants" "DocumentFormat.OpenXml.Spreadsheet.Cell" @()
    
            # First Row
            if ($CurrentRowIndex -eq 1)
            {
                # Get the index of the column to delete			
                $ExactMatchPattern = "^\b" + $ColumnName + "\b$"
                $HeaderCell = $Cells | Where-Object { ((Get-ExcelCellValue -WorkbookPart $workbookPart -Cell $_) | Select-String -Pattern $ExactMatchPattern) -ne $null }	
                
                if ($HeaderCell -ne $null)
                {
                    $ColumnToDeleteIndex = $HeaderCell.CellReference.Value -replace "\d",[string]::Empty
                }
                else
                {
                    Write-Warning "Column with name '$ColumnName' not found in the Excel file $ExcelFilePath"
                }
            }
            if ($ColumnToDeleteIndex -ne $null)
            {
                $DebugMsg = "Deleting column '$ColumnName' on row $CurrentRowIndex..."
                Write-Debug $DebugMsg
            
                # Match cell on the correct column index on the current row
                $CellToDelete =  $CurrentRow | Where-Object { [regex]::Match($_.CellReference.Value, "^" + $ColumnToDeleteIndex +"\d").Success -eq $true }
                #if ($CellToDelete -ne $null)
                #{
                    $CurrentCellIndex = $ColumnToDeleteIndex
                    
                    # Update index of next cells
                    $CellToDelete.ElementsAfter()  | Foreach-Object {
                    
                        $ColumnIndex = $_.CellReference.Value -replace "^[A-Z]*",$CurrentCellIndex
                        
                        # Get the index for the next cell
                        $CurrentCellIndex = [regex]::Match($_.CellReference.Value, "^[A-Z]*").Captures[0].Value
                        
                        # Update the index
                        $_.CellReference = $ColumnIndex
                    }
                    
                    # Remove the cell
                    $CellToDelete.Remove()
                #}
            }	
        
        }
            
        $WorksheetPart.WorkSheet.Save()

        if ($NoDispose.IsPresent -eq $false)
        {
            $DebugMsg = "'-NoDispose' parameter was not specified. Disposing the Excel file object..."
            Write-Debug $DebugMsg
            $ExcelFile.Dispose()
        }
    }
    Catch
    {
        if ($ExcelFile)
        {
            $ExcelFile.Dispose()
        }

        $ErrorMessage = $_.Exception.Message
        Throw $ErrorMessage	
    }
}

<#
    .SYNOPSIS
        Copies the content of a column to another
    
    .DESCRIPTION
        Copies the content of a column to another (excluding the top row header)
    --------------------------------------------------------------------------------------
    Module 'Dynamite.PowerShell.Toolkit'
    by: GSoft, Team Dynamite.
    > GSoft & Dynamite : http://www.gsoft.com
    > Dynamite Github : https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit
    > Documentation : https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit/wiki
    --------------------------------------------------------------------------------------
        
    .PARAMETER ExcelFile
        [REQUIRED] The Excel file instance

    .PARAMETER SourceColumn
        [REQUIRED] The source column to get the content from

    .PARAMETER TargetColumn
        [REQUIRED] The target column to copy the content to

    .PARAMETER WorksheetName
        [OPTIONAL] The Excel worksheet name to work in

    .PARAMETER NoDispose
        [OPTIONAL] If this parameter is specified, the cmdlet doesn't release the Excel File oject after execution. Use this option to process multiple operations on the same file instance.

    .EXAMPLE
            $ExcelFile = Open-DSPExcelFile -Path "C:\Excel.xslx"

            # Copy Column Content
            $ExcelFile | Copy-Column -SourceColumn "Col1" -TargetColumn "Col2" -WorksheetName "Sheet1"

    .LINK
    GSoft, Team Dynamite on Github
    > https://github.com/GSoft-SharePoint
    
    Dynamite PowerShell Toolkit on Github
    > https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit
    
    Documentation
    > https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit/wiki
    
#>
function Copy-DSPExcelColumn {

    [CmdletBinding()]
    Param
    (
        [ValidateNotNullOrEmpty()]
        [Parameter(Mandatory=$true, Position=0, ValueFromPipeline=$true)]
         $ExcelFile,
        
        [Parameter(Mandatory=$true)]
        [string]$SourceColumn,
        
        [Parameter(Mandatory=$true)]
        [string]$TargetColumn,
        
        [Parameter(Mandatory=$false)]
        [string]$WorksheetName,

        [Parameter(Mandatory=$false)]
        [switch]$NoDispose
    )
    
    Try
    {

        $workbookPart = $ExcelFile.WorkbookPart
        $workbook = $workbookPart.Workbook

        if ([string]::IsNullOrEmpty($WorksheetName) -ne $true)
        {
            $Sheet = $workbook.Descendants() | Where-Object { $_.Name -like $WorksheetName -and $_.LocalName -eq "sheet" }
            if ($Sheet -eq $null)
            {
                Throw "Workheet '$WorksheetName' not found in the file"
            }
        }
        else
        {
            $Sheet = $workbook.Descendants() | Where-Object { $_.LocalName -eq "sheet" } | Select-Object -First 1
            $SheetName = $Sheet.Name

            $DebugMsg = "No worksheet name specified. Using first sheet '$SheetName'"   
            Write-Debug $DebugMsg
        }
    
        $SheetId = ($Sheet.Id | Select-Object -Property Value -First 1).Value
        $WorksheetPart = $workbookPart.GetPartById($SheetId)

        $Rows = Invoke-GenericMethod $WorksheetPart.WorkSheet "Descendants" "DocumentFormat.OpenXml.Spreadsheet.Row" @()
    
        $Rows | ForEach-Object {

            $CurrentRow = $_
            $CurrentRowIndex = $CurrentRow.RowIndex.Value

            $Cells = Invoke-GenericMethod $CurrentRow "Descendants" "DocumentFormat.OpenXml.Spreadsheet.Cell" @()
        
            # First Row
            if ($CurrentRowIndex -eq 1)
            {
                # Get the index of the column to delete
                $SourceHeaderCell = $Cells | Where-Object { ((Get-ExcelCellValue -WorkbookPart $workbookPart -Cell $_) | Select-String -Pattern ("^\b" + $SourceColumn + "\b$")) -ne $null }
                $TargetHeaderCell = $Cells | Where-Object { ((Get-ExcelCellValue -WorkbookPart $workbookPart -Cell $_) | Select-String -Pattern ("^\b" + $TargetColumn + "\b$")) -ne $null }
            
                if ($SourceHeaderCell -ne $null -and $TargetHeaderCell -ne $null )
                {
                    $ColumnToCopyFromIndex = $SourceHeaderCell.CellReference.Value -replace "\d",[string]::Empty
                    $ColumnToCopyToIndex = $TargetHeaderCell.CellReference.Value -replace "\d",[string]::Empty
                }
                else
                {
                    Write-Warning "One or more columns was not found!"
                }
            }
            else #Next rows
            {
                if ($ColumnToCopyFromIndex -ne $null -and $ColumnToCopyToIndex -ne $null)
                {			
                    # Match cell on the correct column index on the current row
                    $CellToCopyFrom =  $CurrentRow | Where-Object { [regex]::Match($_.CellReference.Value, "^" + $ColumnToCopyFromIndex +"\d").Success -eq $true }
                    $CellToCopyTo =  $CurrentRow | Where-Object { [regex]::Match($_.CellReference.Value, "^" + $ColumnToCopyToIndex +"\d").Success -eq $true }
                
                    if ($CellToCopyTo -ne $null -and $CellToCopyFrom -ne $null)
                    {				
                        $CellToCopyTo.CellValue = New-Object DocumentFormat.OpenXml.Spreadsheet.CellValue((Get-ExcelCellValue -WorkbookPart $workbookPart -Cell $CellToCopyFrom) )
                        $CellToCopyTo.DataType = [DocumentFormat.OpenXml.Spreadsheet.CellValues]::String    
                    }
                }
            }
        }
            
        $WorksheetPart.WorkSheet.Save()

        if ($NoDispose.IsPresent -eq $false)
        {
            $DebugMsg = "'-NoDispose' parameter was not specified. Disposing the Excel file object..."
            Write-Debug $DebugMsg
            $ExcelFile.Dispose()
        }
    }
    Catch
    {
        if ($ExcelFile)
        {
            $ExcelFile.Dispose()
        }

        $ErrorMessage = $_.Exception.Message
        Throw $ErrorMessage
    }
}

<#
    .SYNOPSIS
        Adds a column to an Excel sheet
    
    .DESCRIPTION
        Adds a column to an Excel sheet
    --------------------------------------------------------------------------------------
    Module 'Dynamite.PowerShell.Toolkit'
    by: GSoft, Team Dynamite.
    > GSoft & Dynamite : http://www.gsoft.com
    > Dynamite Github : https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit
    > Documentation : https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit/wiki
    --------------------------------------------------------------------------------------
        
    .PARAMETER ExcelFile
        [REQUIRED] The Excel file instance

    .PARAMETER ColumnName
        [REQUIRED] The column name to add

    .PARAMETER Position
        [OPTIONAL] The position to insert the new column. If no position is specified, the column is inserted at first position

    .PARAMETER WorksheetName
        [OPTIONAL] The Excel worksheet name to work in

    .PARAMETER NoDispose
        [OPTIONAL] If this parameter is specified, the cmdlet doesn't release the Excel File oject after execution. Use this option to process multiple operations on the same file instance.

    .EXAMPLE
            $ExcelFile = Open-DSPExcelFile -Path "C:\Excel.xslx"

            # Add Column
            $ExcelFile | Add-Column -ColumnName "Col1"  -Position 3

    .LINK
    GSoft, Team Dynamite on Github
    > https://github.com/GSoft-SharePoint
    
    Dynamite PowerShell Toolkit on Github
    > https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit
    
    Documentation
    > https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit/wiki
    
#>
function Add-DSPExcelColumn {

    [CmdletBinding()]
    Param
    (
        [ValidateNotNullOrEmpty()]
        [Parameter(Mandatory=$true, Position=0, ValueFromPipeline=$true)]
        $ExcelFile,
        
        [Parameter(Mandatory=$true)]
        [string]$ColumnName,
        
        [Parameter(Mandatory=$false)]
        [int]$Position,
        
        [Parameter(Mandatory=$false)]
        [string]$WorksheetName,

        [Parameter(Mandatory=$false)]
        [switch]$NoDispose
    )
    
    Try
    {
        $workbookPart = $ExcelFile.WorkbookPart
        $workbook = $workbookPart.Workbook

        if ([string]::IsNullOrEmpty($WorksheetName) -ne $true)
        {
            $Sheet = $workbook.Descendants() | Where-Object { $_.Name -like $WorksheetName -and $_.LocalName -eq "sheet" }
            if ($Sheet -eq $null)
            {
                Throw "Workheet '$WorksheetName' not found in the file"
            }
        }
        else
        {
            $Sheet = $workbook.Descendants() | Where-Object { $_.LocalName -eq "sheet" } | Select-Object -First 1
            $SheetName = $Sheet.Name

            $DebugMsg = "No worksheet name specified. Using first sheet '$SheetName'"
            Write-Debug $DebugMsg
        }
    
        $SheetId = ($Sheet.Id | Select-Object -Property Value -First 1).Value
        $WorksheetPart = $workbookPart.GetPartById($SheetId)
    
        $Rows = Invoke-GenericMethod $WorksheetPart.WorkSheet "Descendants" "DocumentFormat.OpenXml.Spreadsheet.Row" @()
        $HeaderCell = ($Rows | Select-Object -First 1) | Where-Object {$_.Text -eq $ColumnName}

        if ($HeaderCell -ne $null)
        {
            Write-Warning "Column with name '$ColumnName' already exists in the document. Skipping..."
        }
        else
        {
            $Rows | ForEach-Object {

                $CurrentRow = $_
                $CurrentRowIndex = $CurrentRow.RowIndex.Value
            
                $NewCell = New-Object DocumentFormat.OpenXml.Spreadsheet.Cell
                $NewCell.DataType = [DocumentFormat.OpenXml.Spreadsheet.CellValues]::String   
            
                # First Row
                if ($CurrentRowIndex -eq 1)
                {		
                    $NewCell.CellValue = New-Object DocumentFormat.OpenXml.Spreadsheet.CellValue($ColumnName)
                }
                else #Next rows
                {
                    $NewCell.CellValue = New-Object DocumentFormat.OpenXml.Spreadsheet.CellValue([string]::Empty)
                }
            
                if ($Position -eq 0)
                {
                    Write-Warning "'Position' was not specified. Colmun '$ColumnName' will be inserted at position 1"
                    $Position = 1
                }
        
                # Get the next column after desired position. By this way, it is possible to determine the correct cell reference for the new cell
                $CellToInsertBefore = $CurrentRow | Select-Object -First $Position | Select-Object -Last 1	
            
                # To set a cell reference for a new value, we can't use $NewCell.CellReference.Value = <value> directly. We must use a string value format
                $StringValue = New-Object  DocumentFormat.OpenXml.StringValue($CellToInsertBefore.CellReference.Value)
                $NewCell.CellReference = $StringValue
                $InsertedCell = $CellToInsertBefore.InsertBeforeSelf($NewCell)
                        
                # Update manually the cell reference for the next cells
                $InsertedCell.ElementsAfter()  | Foreach-Object {
            
                    $CurrentCellIndexLetter = $_.CellReference.Value -replace "\d",[string]::Empty
                    $CurrentColumnNumber = Get-ColumnNumber $CurrentCellIndexLetter
                
                    $NextCellIndexLetter = Get-ColumnName ($CurrentColumnNumber+1)
                    $NextCellIndex =  $NextCellIndexLetter + $CurrentRowIndex

                    $_.CellReference.Value = $NextCellIndex
                }
            }
        
            
        }

        $WorksheetPart.WorkSheet.Save()

        if ($NoDispose.IsPresent -eq $false)
        {
            $DebugMsg = "'-NoDispose' parameter was not specified. Disposing the Excel file object..."
            Write-Debug $DebugMsg
            $ExcelFile.Dispose()
        }
    }
    Catch
    {
        if ($ExcelFile)
        {
            $ExcelFile.Dispose()
        }

        $ErrorMessage = $_.Exception.Message
        Throw $ErrorMessage
    }
}

<#
    .SYNOPSIS
        Replaces a string in a Excel Sheet
    
    .DESCRIPTION
         Replaces a string matching a regular rexpression in a Excel Sheet. The replacement can be restricted to a single column
    --------------------------------------------------------------------------------------
    Module 'Dynamite.PowerShell.Toolkit'
    by: GSoft, Team Dynamite.
    > GSoft & Dynamite : http://www.gsoft.com
    > Dynamite Github : https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit
    > Documentation : https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit/wiki
    --------------------------------------------------------------------------------------
        
    .PARAMETER ExcelFile
        [REQUIRED] The Excel file instance

    .PARAMETER Pattern
        [REQUIRED] The regex pattern to search for

    .PARAMETER Value
        [REQUIRED] The replacement value. Note that you can pass a scriptblock instead. In this case, it is executed if a cell content match your pattern. Use the paramater name $CurrentCellValue to manipulate the current cell content in you scriptblock
                   The return value of your script block will be the new content of the cell.

    .PARAMETER Escape
        [OPTIONAL] If the regex pattern contains special characters to escape, use this option. (e.g .DOMAIN\user)

    .PARAMETER Column
        [OPTIONAL] The operation is scoped only on this column

    .PARAMETER AsIdentifier
        [OPTIONAL] If this option is enabled, treats the column as an identifier. Write 1,2,3,.. corresponding to the row index

    .PARAMETER WorksheetName
        [OPTIONAL] The Excel worksheet name to work in

    .PARAMETER NoDispose
        [OPTIONAL] If this parameter is specified, the cmdlet doesn't release the Excel File oject after execution. Use this option to process multiple operations on the same file instance.

    .EXAMPLE
            $ExcelFile = Open-DSPExcelFile -Path "C:\Excel.xslx"

            # Replace value
            $ExcelFile | Edit-ColumnValue -Pattern "OLDDOMAIN\user" -Value "NEWDOMAIN\user" -Escape -Column "Col1" -WorksheetName "Sheet1"

            # Replace value with a script block
            $ExcelFile | Edit-ColumnValue -Pattern "OLDDOMAIN\user" -Value { Param($CurrentCellValue) return "My new value" }

            $ExcelFile | Edit-ColumnValue -Column "Col1" -AsIdentifier

    .LINK
    GSoft, Team Dynamite on Github
    > https://github.com/GSoft-SharePoint
    
    Dynamite PowerShell Toolkit on Github
    > https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit
    
    Documentation
    > https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit/wiki
    
#>
function Edit-DSPExcelColumnValue {

    [CmdletBinding()]
    Param
    (
        [ValidateNotNullOrEmpty()]
        [Parameter(Mandatory=$true, Position=0, ValueFromPipeline=$true)]
        $ExcelFile,
        
        [Parameter(Mandatory=$false)]
        [Parameter(ParameterSetName ="Default")]
        [string]$Pattern,
        
        [Parameter(Mandatory=$false)]
        [Parameter(ParameterSetName ="Default")]
        $Value,
        
        [Parameter(Mandatory=$false)]
        [Parameter(ParameterSetName ="Default")]
        [switch]$Escape,		
        
        [Parameter(Mandatory=$false)]
        [Parameter(ParameterSetName ="AutoIncrement")]
        [Parameter(ParameterSetName ="Default")]
        [string]$Column,

        [Parameter(Mandatory=$false)]
        [Parameter(ParameterSetName ="AutoIncrement")]
        [switch]$AsIdentifier,
        
        [Parameter(Mandatory=$false)]
        [string]$WorksheetName,

        [Parameter(Mandatory=$false)]
        [switch]$NoDispose
    )
    
    Try
    {
        $workbookPart = $ExcelFile.WorkbookPart
        $workbook = $workbookPart.Workbook

        if ([string]::IsNullOrEmpty($WorksheetName) -ne $true)
        {
            $Sheet = $workbook.Descendants() | Where-Object { $_.Name -like $WorksheetName -and $_.LocalName -eq "sheet" }
            if ($Sheet -eq $null)
            {
                $ExcelFile.Dispose()
                Throw "Workheet '$WorksheetName' not found in the file"
            }
        }
        else
        {
            $Sheet = $workbook.Descendants() | Where-Object { $_.LocalName -eq "sheet" } | Select-Object -First 1
            $SheetName = $Sheet.Name

            $DebugMsg = "No worksheet name specified. Using first sheet '$SheetName'"
            Write-Debug $DebugMsg
        }
    
        $SheetId = ($Sheet.Id | Select-Object -Property Value -First 1).Value
        $WorksheetPart = $workbookPart.GetPartById($SheetId)
    
        $Rows = Invoke-GenericMethod $WorksheetPart.WorkSheet "Descendants" "DocumentFormat.OpenXml.Spreadsheet.Row" @()

        $ColumnToReplaceOnIndex = $null
    
        if ($Escape.IsPresent)
        {
            $Pattern = [Regex]::Escape($Pattern)
        }
    
        $Rows | ForEach-Object {
    
            $CurrentRow = $_
            $CurrentRowIndex = $CurrentRow.RowIndex.Value

            $Cells = Invoke-GenericMethod $CurrentRow "Descendants" "DocumentFormat.OpenXml.Spreadsheet.Cell" @()
        
            # First Row
            if ($CurrentRowIndex -eq 1)
            {
                # If the replace is on a specific column, we get the index of it
                if ([string]::IsNullOrEmpty($Column) -eq $false)
                {
                    $SourceHeaderCell = $Cells | Where-Object { ((Get-ExcelCellValue -WorkbookPart $workbookPart -Cell $_) | Select-String -Pattern ("^\b" + $Column + "\b$")) -ne $null }
                    $ColumnToReplaceOnIndex = $SourceHeaderCell.CellReference.Value -replace "\d",[string]::Empty
                }
                else			
                {
                    $DebugMsg = "No column was specified. Replacement will apply on the whole document"
                    Write-Debug $DebugMsg
                }
            }
            else
            {
                # Process only the matching columns
                if ($ColumnToReplaceOnIndex -ne $null)
                {
                    $ReplacedCell = $CurrentRow | Where-Object { ($_.CellReference.Value -replace "\d",[string]::Empty) -eq  $ColumnToReplaceOnIndex }  | Select-Object -First 1

                    if($AsIdentifier.IsPresent)
                    {
                        $ReplacedCell.CellValue = New-Object DocumentFormat.OpenXml.Spreadsheet.CellValue($CurrentRowIndex -1)
                        $ReplacedCell.DataType = [DocumentFormat.OpenXml.Spreadsheet.CellValues]::Number
                    }
                    else
                    {    
                        $CurrentCellValue = Get-ExcelCellValue -WorkbookPart $workbookPart -Cell $ReplacedCell
                        if ($Value -is [scriptblock] -and ($CurrentCellValue -match $Pattern))
                        {
                            $CalculatedValue = &$Value -CurrentCellValue $CurrentCellValue

                            # Replacement is done on the whole cell content
                            $ReplacedCell.CellValue = New-Object DocumentFormat.OpenXml.Spreadsheet.CellValue($CalculatedValue)

                        }
                        else
                        {
                            $ReplacedCell.CellValue = New-Object DocumentFormat.OpenXml.Spreadsheet.CellValue($CurrentCellValue -replace $Pattern, $Value)						    
                        }   

                        $ReplacedCell.DataType = [DocumentFormat.OpenXml.Spreadsheet.CellValues]::String  
                    }				
                }
                else
                {
                    # Process all columns
                    $Cells | ForEach-Object {	
    
                        $CurrentCell = $_	   
                        $CurrentCellValue = Get-ExcelCellValue -WorkbookPart $workbookPart -Cell $CurrentCell

                        if ($Value -is [scriptblock] -and ($CurrentCellValue -match $Pattern))
                        {
                            $CalculatedValue = &$Value -CurrentCellValue $CurrentCellValue

                            # Replacement is done on the whole cell content according to the script block result
                            $CurrentCell.CellValue = New-Object DocumentFormat.OpenXml.Spreadsheet.CellValue($CalculatedValue)
                        }
                        else
                        {
                            $CurrentCell.CellValue = New-Object DocumentFormat.OpenXml.Spreadsheet.CellValue($CurrentCellValue -replace $Pattern, $Value)					    
                        }

                        $CurrentCell.DataType = [DocumentFormat.OpenXml.Spreadsheet.CellValues]::String
                    }	
                }
            }	
        }
    
        $WorksheetPart.WorkSheet.Save()

        if ($NoDispose.IsPresent -eq $false)
        {
            $DebugMsg = "'-NoDispose' parameter was not specified. Disposing the Excel file object..."     
            Write-Debug $DebugMsg
            $ExcelFile.Dispose()
        }
    }
    Catch
    {
        if ($ExcelFile)
        {
            $ExcelFile.Dispose()
        }

        $ErrorMessage = $_.Exception.Message
        Throw $ErrorMessage
    }
}

<#
    .SYNOPSIS
        Get the content of the file
    
    .DESCRIPTION
         Get the rows content for the specified columns in the file
    --------------------------------------------------------------------------------------
    Module 'Dynamite.PowerShell.Toolkit'
    by: GSoft, Team Dynamite.
    > GSoft & Dynamite : http://www.gsoft.com
    > Dynamite Github : https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit
    > Documentation : https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit/wiki
    --------------------------------------------------------------------------------------
        
    .PARAMETER ExcelFile
        [REQUIRED] The Excel file instance

    .PARAMETER Columns
        [OPTIONAL] Excel columns to get the content from

    .PARAMETER TrimDuplicates
        [OPTIONAL] Trim duplicates rows on the output

    .PARAMETER NoDispose
        [OPTIONAL] If this parameter is specified, the cmdlet doesn't release the Excel File oject after execution. Use this option to process multiple operations on the same file instance.

    .EXAMPLE
            $ExcelFile = Open-DSPExcelFile -Path "C:\Excel.xslx"

            $ExcelFile | Get-FileContent -Columns "Reusable HTML","Modified By" -TrimDuplicates

    .LINK
    GSoft, Team Dynamite on Github
    > https://github.com/GSoft-SharePoint
    
    Dynamite PowerShell Toolkit on Github
    > https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit
    
    Documentation
    > https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit/wiki
    
#>
function Get-DSPExcelFileContent {

    [CmdletBinding()]
    Param
    (
        [ValidateNotNullOrEmpty()]
        [Parameter(Mandatory=$true, Position=0, ValueFromPipeline=$true)]
        $ExcelFile,
        
        [ValidateNotNullOrEmpty()]
        [Parameter(Mandatory=$true)]
        [Parameter(ParameterSetName ="Default")]
        [array]$Columns,

        [Parameter(Mandatory=$false)]
        [Parameter(ParameterSetName ="Default")]
        [switch]$TrimDuplicates,
            
        [Parameter(Mandatory=$false)]
        [string]$WorksheetName,

        [Parameter(Mandatory=$false)]
        [switch]$NoDispose
    )

    $OutputContent = @()
    $SourceColumnsIndex = @{}

    Try
    {
        $workbookPart = $ExcelFile.WorkbookPart
        $workbook = $workbookPart.Workbook

        # Determine the correct Excel worksheet
        if ([string]::IsNullOrEmpty($WorksheetName) -ne $true)
        {
            $Sheet = $workbook.Descendants() | Where-Object { $_.Name -like $WorksheetName -and $_.LocalName -eq "sheet" }
            if ($Sheet -eq $null)
            {
                Throw "Workheet '$WorksheetName' not found in the file"
            }
        }
        else
        {
            $Sheet = $workbook.Descendants() | Where-Object { $_.LocalName -eq "sheet" } | Select-Object -First 1
            $SheetName = $Sheet.Name

            $DebugMsg = "No worksheet name specified. Using first sheet '$SheetName'"
            Write-Debug $DebugMsg
        }
    
        $SheetId = ($Sheet.Id | Select-Object -Property Value -First 1).Value
        $WorksheetPart = $workbookPart.GetPartById($SheetId)
    
        $Rows = Invoke-GenericMethod $WorksheetPart.WorkSheet "Descendants" "DocumentFormat.OpenXml.Spreadsheet.Row" @()

        $Rows | ForEach-Object {
        
            $RowObject = New-Object -TypeName PSObject
            $CurrentRow = $_
            $CurrentRowIndex = $_.RowIndex.Value
        
            $Cells = Invoke-GenericMethod $CurrentRow "Descendants" "DocumentFormat.OpenXml.Spreadsheet.Cell" @()

            # First Row
            if ($CurrentRowIndex -eq 1)
            {		
                # Get the exact match for column names
                $Columns | ForEach-Object { 

                    $IsFound = $false
                    
                    $ExactMatchPattern = "\b" + $_ + "\b"
                    
                    $Cells | ForEach-Object {
                    
                        # Get the cell value
                        $CellValue = Get-ExcelCellValue -WorkbookPart $workbookPart -Cell $_
                    
                        if ($CellValue)
                        {
                            if (($CellValue | Select-String -Pattern $ExactMatchPattern) -ne $null)
                            {
                                    $IsFound = $true
                                    $ColumnName = $CellValue	
                                    $ColumnReference = $_.CellReference.Value -replace "\d",[string]::Empty
                                    $SourceColumnsIndex.Add($ColumnReference, $ColumnName)
                            }
                        }
                    }

                    if ($IsFound -eq $false)
                    {		
                        Write-Warning "Column with name $_ not found in the file"
                    }
                }
            }
            else #Other rows
            {		
                if ($SourceColumnsIndex.Count -gt 0)
                {		       
                    $SourceColumnsIndex.Keys | Foreach-Object {

                        $Index = $_

                        # Match cell on the correct column index
                        $SourceCell =  $Cells | Where-Object { [regex]::Match($_.CellReference.Value, "^" + $Index +"\d").Success -eq $true }
                    
                        $SourceCellValue = Get-ExcelCellValue -WorkbookPart $workbookPart -Cell $SourceCell

                        $RowObject | Add-Member -Type NoteProperty -Name $SourceColumnsIndex.Get_Item($_) -Value $SourceCellValue
                    }	
  
                    $OutputContent += $RowObject	 
                }
            }
        }
        
        $WorksheetPart.WorkSheet.Save()

        if ($NoDispose.IsPresent -eq $false)
        {
            $DebugMsg = "'-NoDispose' parameter was not specified. Disposing the Excel file object..."
            Write-Debug $DebugMsg
            $ExcelFile.Dispose()
        }

        # Return the content of the selected columns in the file

        if ($TrimDuplicates.IsPresent)
        {
            $OutputContent =  $OutputContent | Select-object $Columns -Unique
        }

        return $OutputContent
    }
    Catch
    {
        if ($ExcelFile)
        {
            $ExcelFile.Dispose()
        }

        $ErrorMessage = $_.Exception.Message
        Throw $ErrorMessage
    }
}

<#
    .SYNOPSIS
        Find specfific tokens in *.xlsx files in a directory
    
    .DESCRIPTION
         Get all the *.xlsx files and matching lines that contains one ore more occurences of tokens passed as parameters for specified columns
    --------------------------------------------------------------------------------------
    Module 'Dynamite.PowerShell.Toolkit'
    by: GSoft, Team Dynamite.
    > GSoft & Dynamite : http://www.gsoft.com
    > Dynamite Github : https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit
    > Documentation : https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit/wiki
    --------------------------------------------------------------------------------------
        
    .PARAMETER Folder
        [REQUIRED] A valid folder to look in

    .PARAMETER Columns
        [OPTIONAL] Excel columns to search from

    .PARAMETER Patterns
        [OPTIONAL] Array of regex patterns to search for

    .PARAMETER WorksheetName
        [OPTIONAL] The Excel worksheet name to work in

    .EXAMPLE
            
             Find-DSPExcelFiles -Folder "C:\Logs" -Columns "Column1","Domain" -Patterns "Value","dev\\myuser"

    .LINK
    GSoft, Team Dynamite on Github
    > https://github.com/GSoft-SharePoint
    
    Dynamite PowerShell Toolkit on Github
    > https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit
    
    Documentation
    > https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit/wiki
    
#>
function Find-DSPExcelFiles {

    [CmdletBinding()]
    Param
    (
        [ValidateScript({Test-Path $_ -PathType 'Container'})]
        [Parameter(Mandatory=$true, Position=0, ValueFromPipeline=$true)]
        $Folder,
        
        [ValidateNotNullOrEmpty()]
        [Parameter(Mandatory=$false)]
        [Parameter(ParameterSetName ="Default")]
        [array]$Columns,

        [ValidateNotNullOrEmpty()]
        [Parameter(Mandatory=$true)]
        [array]$Patterns,

        [Parameter(Mandatory=$false)]
        [string]$WorksheetName
    )

    $ExcelFilesMatched = @()

    $ExcelFiles = Get-ChildItem $Folder -Recurse | Where-Object { ($_.PSIsContainer -eq $false) -and ($_.Extension -eq ".xlsx") }

    $ExcelFiles| ForEach-Object {

        $ExcelFileOutputObject = New-Object -TypeName PSObject
        $NumberOfLinesFound = 0

        # Open the Excel file
        $CurrentExcelFile = Open-DSPExcelFile -Path $_.FullName

        $DebugMsg = "Processing file '" + $_.FullName + "'..."
        Write-Debug $DebugMsg

        if ($CurrentExcelFile -ne $null)
        {
            $SourceColumnsIndex = @{}

            $workbookPart = $CurrentExcelFile.WorkbookPart
            $workbook = $workbookPart.Workbook

            if ([string]::IsNullOrEmpty($WorksheetName) -ne $true)
            {
                $Sheet = $workbook.Descendants() | Where-Object { $_.Name -like $WorksheetName -and $_.LocalName -eq "sheet" }
                if ($Sheet -eq $null)
                {
                    $CurrentExcelFile.Dispose()
                    Throw "Workheet '$WorksheetName' not found in the file"
                }
            }
            else
            {
                $Sheet = $workbook.Descendants() | Where-Object { $_.LocalName -eq "sheet" } | Select-Object -First 1
                $SheetName = $Sheet.Name

                $DebugMsg = "No worksheet name specified. Using first sheet '$SheetName'"
                Write-Debug $DebugMsg
            }
    
            $SheetId = ($Sheet.Id | Select-Object -Property Value -First 1).Value
            $WorksheetPart = $workbookPart.GetPartById($SheetId)
    
            $Rows = Invoke-GenericMethod $WorksheetPart.WorkSheet "Descendants" "DocumentFormat.OpenXml.Spreadsheet.Row" @()
    
            $Rows | ForEach-Object {
        
                $RowMatched = $false
                $CurrentRow = $_
                $CurrentRowIndex = $_.RowIndex.Value
                $Cells = Invoke-GenericMethod $CurrentRow "Descendants" "DocumentFormat.OpenXml.Spreadsheet.Cell" @()
        
                # First Row
                if ($CurrentRowIndex -eq 1)
                {		
                    # Get the exact match for column names
                    $Columns | ForEach-Object { 

                        $IsFound = $false
                        $ExactMatchPattern = "\b" + $_ + "\b"
                    
                        $Cells | ForEach-Object {
                    
                            # Get the cell value
                            $CellValue = Get-ExcelCellValue -WorkbookPart $workbookPart -Cell $_
                    
                            if ($CellValue)
                            {
                                if (($CellValue | Select-String -Pattern $ExactMatchPattern) -ne $null)
                                {
                                        $IsFound = $true
                                        $ColumnName = $CellValue	
                                        $ColumnReference = $_.CellReference.Value -replace "\d",[string]::Empty
                                        $SourceColumnsIndex.Add($ColumnReference, $ColumnName)
                                }
                            }
                        }

                        if ($IsFound -eq $false)
                        {		
                            Write-Warning "Column with name $_ not found in the file"
                        }
                    }
                }
                else #Other rows
                {		
                    if ($SourceColumnsIndex.Count -gt 0)
                    {		       
                        $SourceColumnsIndex.Keys | Foreach-Object {

                            $Index = $_

                            # Match cell on the correct column index
                            $SourceCell =  $Cells | Where-Object { [regex]::Match($_.CellReference.Value, "^" + $Index +"\d").Success -eq $true }
                    
                            if ($RowMatched -eq $false)
                            {
                                # If the cell value contains any of elements in patterns, add the 
                                if (((Get-ExcelCellValue -WorkbookPart $workbookPart -Cell $SourceCell) | Select-String -Pattern $Patterns) -ne $null )
                                {
                                    $RowMatched = $true
                                }	
                            }		
                        }	
                    }
                }

                if ($RowMatched)
                {	
                    $NumberOfLinesFound++
                }
            }

            if ($NumberOfLinesFound -gt 0)
            {
                $ExcelFileOutputObject | Add-Member -Type NoteProperty -Name "File Path" -Value $_.FullName
                $ExcelFileOutputObject | Add-Member -Type NoteProperty -Name "Matching lines count" -Value $NumberOfLinesFound

                $ExcelFilesMatched += $ExcelFileOutputObject			
            }

            $CurrentExcelFile.Dispose()
        }
    }

    return $ExcelFilesMatched
}

<#
    .SYNOPSIS
        Renames a column of an Excel sheet
    
    .DESCRIPTION
        Renames a column of an Excel sheet
    --------------------------------------------------------------------------------------
    Module 'Dynamite.PowerShell.Toolkit'
    by: GSoft, Team Dynamite.
    > GSoft & Dynamite : http://www.gsoft.com
    > Dynamite Github : https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit
    > Documentation : https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit/wiki
    --------------------------------------------------------------------------------------
        
    .PARAMETER ExcelFile
        [REQUIRED] The Excel file instance

    .PARAMETER OldColumnName
        [REQUIRED] The name of the column to be renamed

    .PARAMETER NewColumnName
        [REQUIRED] The new name of the column

    .PARAMETER Position
        [OPTIONAL] The position to insert the new column. If no position is specified, the column is inserted at first position

    .PARAMETER WorksheetName
        [OPTIONAL] The Excel worksheet name to work in

    .PARAMETER NoDispose
        [OPTIONAL] If this parameter is specified, the cmdlet doesn't release the Excel File oject after execution. Use this option to process multiple operations on the same file instance.

    .EXAMPLE
            $ExcelFile = Open-DSPExcelFile -Path "C:\Excel.xslx"

            # Rename Column
            $ExcelFile | Rename-DSPExcelColumn -OldColumnName "OldCol"  -NewColumnName "NewCol"

    .LINK
    GSoft, Team Dynamite on Github
    > https://github.com/GSoft-SharePoint
    
    Dynamite PowerShell Toolkit on Github
    > https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit
    
    Documentation
    > https://github.com/GSoft-SharePoint/Dynamite-PowerShell-Toolkit/wiki
    
#>
function Rename-DSPExcelColumn {
    [CmdletBinding()]
    Param
    (
        [ValidateNotNullOrEmpty()]
        [Parameter(Mandatory=$true, Position=0, ValueFromPipeline=$true)]
        $ExcelFile,
        
        [Parameter(Mandatory=$true)]
        [string]$OldColumnName,
        
        [Parameter(Mandatory=$true)]
        [string]$NewColumnName,
        
        [Parameter(Mandatory=$false)]
        [int]$Position,
        
        [Parameter(Mandatory=$false)]
        [string]$WorksheetName,

        [Parameter(Mandatory=$false)]
        [switch]$NoDispose
    )
    
    Try
    {
        $workbookPart = $ExcelFile.WorkbookPart
        $workbook = $workbookPart.Workbook

        if ([string]::IsNullOrEmpty($WorksheetName) -ne $true)
        {
            $Sheet = $workbook.Descendants() | Where-Object { $_.Name -like $WorksheetName -and $_.LocalName -eq "sheet" }
            if ($Sheet -eq $null)
            {
                Throw "Workheet '$WorksheetName' not found in the file"
            }
        }
        else
        {
            $Sheet = $workbook.Descendants() | Where-Object { $_.LocalName -eq "sheet" } | Select-Object -First 1
            $SheetName = $Sheet.Name

            $DebugMsg = "No worksheet name specified. Using first sheet '$SheetName'"
            Write-Debug $DebugMsg
        }
    
        $SheetId = ($Sheet.Id | Select-Object -Property Value -First 1).Value
        $WorksheetPart = $workbookPart.GetPartById($SheetId)
    
        $Rows = Invoke-GenericMethod $WorksheetPart.WorkSheet "Descendants" "DocumentFormat.OpenXml.Spreadsheet.Row" @()
        $HeaderCell = ($Rows | Select-Object -First 1) | Where-Object {$_.Text -eq $OldColumnName}

        if ($HeaderCell -ne $null)
        {
            if([string]::IsNullOrEmpty($NewColumnName))
            {
                Throw "No new column name specified"
            }
            else
            {
                # Old column is found and new column name is specified, proceed with renaming
                $HeaderCell.CellValue = $NewColumnName
            }
        }
        else
        {
            Throw "Column '$OldColumnName' not found in the worksheet"
        }

        $WorksheetPart.WorkSheet.Save()

        if ($NoDispose.IsPresent -eq $false)
        {
            $DebugMsg = "'-NoDispose' parameter was not specified. Disposing the Excel file object..."     
            Write-Debug $DebugMsg
            $ExcelFile.Dispose()
        }
    }
    Catch
    {
        if ($ExcelFile)
        {
            $ExcelFile.Dispose()
        }

        $ErrorMessage = $_.Exception.Message
        Throw $ErrorMessage
    }
}



