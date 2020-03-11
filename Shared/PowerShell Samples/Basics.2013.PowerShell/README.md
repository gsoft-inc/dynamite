# Basic Powershell Script Structure #

### Summary ###
In this sample we demonstrait the basic file structure for PowerShell solution install scripts using the Dynamite SharePoint PowerShell Toolkit.

### Applies to ###
-  SharePoint 2013 on-premises

### Prerequisites ###
- DSP toolkit needs to be installed. 

### Solution ###
Solution | Author(s)
---------|----------
Basics.2013.PowerShell | Edouard Shaar

### Version history ###
Version  | Date | Comments
---------| -----| --------
1.0  | December 31th 2015 | Initial release

### Disclaimer ###
**THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.**

----------
## Running this Sample ##
1. Build and Package the WSP Files using Visual Studio.
2. Create your own Tokens file in the Scripts project tokens folder. The file should have the name ``Tokens.{ComputerName}.ps1`` and have the same variables (not values) as the eddysp tokens file.
2. Open a PowerShell Console in the *Basics.2013.PowerShell.Scripts* folder
3. Run ``Publish-DeploymentFolder.ps1``
4. Run ``.\DSP\Install-DSPModule.ps1``
5. Start a New PowerShell Console in the *DeployPkg* folder
6. Run ``Deploy-Solutions.ps1``
7. Start a New PowerShell Console in the *DeployPkg* folder
8. Run ``Install-All.ps1``

## About the Solution ##
This SharePoint on-premises solution deploys a css file (sp.custom.css) to the site collection Style Library. The solution is installed and configured 100% by using PowerShell Scripts.

### The WSP Solution ###
A single feature called "StyleLibraryModule" is used at a site collection level in order to deploy the sp.custon.css file to the root web Style Library.

### The Scripts Project ###
<pre>
Basics.2013.PowerShell.Scripts (Root)
|   Basics.2013.PowerShell.Scripts.csproj
|   Deploy-Solutions.ps1
|   Install-All.ps1
|   Publish-DeploymentFolder.ps1
|
+---Configuration
|       Enable-SiteFeatures.template.ps1
|       New-SiteStructure.ps1
|       SiteStructure-input.template.xml
|
+---Solutions
|       Solutions.template.xml
|
\---Tokens
        Tokens.eddysp.ps1
</pre>

- **The Root Folder**<br/>This folder contains the files executed by a user durring an installation.
- **Configuration**<br/>This is the main script folder where all the actual work is done. It is reconmended to have sub folders in here like "Search" or "User Profile".
- **Solutions**<br/>All the WSP files will be found in this folder when the deployment package is published. Its best just to keep things clean.
- **Tokens**<br/>All the token configuration files.