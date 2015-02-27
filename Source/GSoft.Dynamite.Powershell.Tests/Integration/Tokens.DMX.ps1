# ******************************************
# PowerShell Files Tokens
# ******************************************

# --------------- Web.ps1  -----------------

# ******************************************
# Export-DSPWeb Tokens 
# ******************************************
$DSP_WebApplicationUrl = "http://dmx/ "
$DSP_XmlSchema = ".\TestWebSchema.xsd"
$DSP_OutputFileName = ".\ExportWebTest_PROD.xml"
$DSP_TempSiteCollection = "sites/test"
$DSP_CurrentAccount = [System.Security.Principal.WindowsIdentity]::GetCurrent().Name
$DSP_VariationsConfigFile = "./TestVariationsSettings.xml"