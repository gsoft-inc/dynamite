function Start-DSPLogging {
  <#
    .SYNOPSIS
    Starts a transcript log of the command.
    .DESCRIPTION
    Safely starts transcript logging for the command in the specified folder.  Default logging format is 'CommandName_MM-dd-yyyy_hh-mm-ss.log'.
    .EXAMPLE
    Start-DSPLogging -commandName "Test-DSPLogging" -folder ((Get-Location).Path + "\Logs")
    .EXAMPLE
    Start-DSPLogging -commandName "Test-DSPLogging" -folder ((Get-Location).Path + "\Logs") -time (Get-Date -Format "MM-dd-yyy")
    .EXAMPLE
    Start-DSPLogging -commandName "Test-DSPLogging" -folder ((Get-Location).Path + "\Logs") -file ".\logs\my_logs.log"
    .PARAMETER commandName
    The name of the command you are logging the transcript.
    .PARAMETER folder
    The folder path in which to create the log file.
    .PARAMETER time
    The string which will be appended to the file name as the timestamp. Default is 'MM-dd-yyyy_hh-mm-ss'.
    .PARAMETER file
    The file path of the log file. Default is 'current_location\logs'.
  #>
  [CmdletBinding()]
  param
  (
    [Parameter(
        Mandatory=$True,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='What is the name of the command for which to wish to log the transcript?')]
    [string]$commandName,

    [Parameter(
        Mandatory=$True,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='In what folder do you wish to create the log file?')]
    [string]$folder,

    [Parameter(
        Mandatory=$False, 
        ValueFromPipeline=$True)]
    [string]$time = (Get-Date -Format "MM-dd-yyyy_hh-mm-ss"),

    [Parameter(
        Mandatory=$False, 
        ValueFromPipeline=$True)]
    [string]$file = ($folder + "\" + [System.IO.Path]::GetFileNameWithoutExtension($commandName) + $time +".log")
  )

    begin {
        Write-Verbose "Beginning log for command: '$commandName' in file '$file'"
    }

    process {
        Write-Verbose "Stopping transcript if it's already started"
        
		# Only try to log when in a console host
		if ($host.name -eq 'ConsoleHost') {
			try {
				Stop-Transcript | out-null
			}
			catch [System.InvalidOperationException]{}

			if(!(Test-Path -Path $folder)) {
				Write-Verbose "Creating folder '$folder'"
				New-Item -ItemType directory -Path $folder
			}
			else {
				# Reset the log folder
				Get-ChildItem $folder | Foreach-Object { Remove-Item  $_.FullName -Force }
			}

			# Stat log transcript
			Start-Transcript -Path $file | Write-Output
		} else {
			Write-Warning -Message "Unable to start transcript on host '$($host.name)'.  Please use the console host."
		}
    }
}

function Stop-DSPLogging {
  <#
  .SYNOPSIS
  Stops a transcript log.
  .DESCRIPTION
  Safely stops the transcript logging.
  .EXAMPLE
  Stop-DSPLogging
  #>

    process {
        try {
			# Only try to stop log when in a console host
			if ($host.name -eq 'ConsoleHost') {
				Stop-Transcript | Write-Output
				Write-Verbose "Stopped transcript"
			} else {
				Write-Warning -Message "Unable to stop transcript on host '$($host.name)'.  Please use the console host."
			}
        }
        catch [System.InvalidOperationException]{
            Write-Warning "Tryed to stop transcript when it wasn't started."
        }
    }
}