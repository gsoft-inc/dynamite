# Basic Powershell Script Structure #

### Summary ###
In this sample we demonstrait the basic file structure for PowerShell solution install scripts using [PnP](https://github.com/OfficeDev/PnP-PowerShell) for Office 365.

### Applies to ###
- Office 365 Multi Tenant (MT)
- Office 365 Dedicated (D)

### Prerequisites ###
- [PnP Cmdlet](https://github.com/OfficeDev/PnP-PowerShell/blob/master/Binaries/PnPPowerShellCommands16.msi?raw=true)

### Solution ###
Solution | Author(s)
---------|----------
Basics.O365.PowerShell | Edouard Shaar

### Version history ###
Version  | Date | Comments
---------| -----| --------
1.0  | January 8th 2016 | Initial release
1.1  | January 8th 2016 | Added documentation for the ps1 files.

### Disclaimer ###
**THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.**

----------
## Running this Sample ##
1. Create your own Tokens file in the Scripts project tokens folder. The file should have the name ``Tokens.{ComputerName}.xml`` and update the values based on the ones from the eddysp tokens file.
2. Open a PowerShell Console in the *Basics.O365.PowerShell.Scripts* folder
3. Run ``Install-All.ps1``

## About the Solution ##
This solution uses PowerShell combined with PnP in order to deploy a css file (sp.custom.css) to the site collection Style Library.

### The Scripts Project ###
<pre>
Basics.O365.PowerShell.Scripts (Root)
|   Basics.O365.PowerShell.Scripts.csproj
|   Install-All.ps1
|
+---Configuration
|       New-Site.ps1
|       Provision-Site.ps1
+---Files
|   \---Style Library
|           sp.custom.css
+---Templates
|       files.xml
\---Tokens
        Tokens.eshaarWin10Dev.xml
</pre>

- **The Root Folder**<br/>This folder contains the files executed by a user durring an installation.
- **Configuration**<br/>This is the main script folder where all the actual work is done. It is reconmended to have sub folders in here like "Search" or "User Profile".
- **Files**<br/>Here we find the files that will be deployed to SharePoint using the PnP Provisioning Engine. All subfolders are purely for keeping things clean.
- **Templates**<br/>This folder contains the xml configuration files for the PnP Provisioning Engine. Only [PnP tokens](https://github.com/OfficeDev/PnP-Sites-Core/blob/master/Core/ProvisioningEngineTokens.md) should be used in these files.
- **Tokens**<br/>All the token configuration files. These configuration files are passed into the ``Install-All.ps1`` file as a parameter.