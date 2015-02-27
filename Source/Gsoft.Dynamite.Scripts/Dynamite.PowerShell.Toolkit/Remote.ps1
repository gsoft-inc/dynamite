function Get-DSPTrustedHosts()
{
	(get-item wsman:\localhost\Client\TrustedHosts).value
}

function Add-DSPTrustedHosts()
{
	[CmdletBinding()]
	param
	(
		[Parameter(Mandatory=$true, Position=1)]
		[string]$proposedHost
	)

    if (Test-DSPIsAdmin)
    {
        $trustedHosts = Get-DSPTrustedHosts

        if (!$trustedHosts.split(",").Contains($proposedHost))
        {
            Write-Host "Adding $proposedHost to Trusted Hosts : $trustedHosts ... " -nonewline -foregroundcolor yellow
            
            if ( [string]::IsNullOrEmpty($trustedHosts) )
            {
                $pattern = $proposedHost
            }
            else
            {
                $pattern = "$trustedHosts,$proposedHost"
            }
                        
            Set-Item wsman:\localhost\Client\TrustedHosts -value $pattern -Force
            Write-Host "DONE! " -nonewline -foregroundcolor green
            Write-Host "Restarting WinRM service... " -nonewline -foregroundcolor yellow
            
            Restart-Service WinRM
            Write-Host "DONE!" -foregroundcolor green
        }
    }
    else 
    {
        Write-Host "YOU ARE NOT AN ADMINISTRATOR. YOU CAN'T UPDATE THE TRUSTED HOSTS"
    }
}

function Initialize-DSPRemotePowerShell()
{
	[CmdletBinding()]
	param
	(
		[Parameter(Mandatory=$true, Position=1)]
		[string]$proposedHost
	)

    # Enable PowerShell Remoting
    Enable-PSRemoting -Force

    # Add Hostname to Trusted Hostname
    Add-DSPTrustedHosts $proposedHost
}

function Enter-DSPRemoteSession()
{
	[CmdletBinding()]
	param
	(
		[Parameter(Mandatory=$true, Position=0)]
		[string]$Username,

        [Parameter(Mandatory=$true, Position=1)]
		[string]$Password,

        [Parameter(Mandatory=$true, Position=2)]
		[string]$ComputerName 
	)

    $securePassword = ConvertTo-SecureString $Password -AsPlainText -Force
    $cred = New-Object System.Management.Automation.PSCredential ($Username, $securePassword )

    Enter-PSSession –ComputerName $ComputerName –Credential $cred -Authentication Credssp
}