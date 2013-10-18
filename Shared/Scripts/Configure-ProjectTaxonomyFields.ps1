# Reference script to initialize the term sets in the managed metadata service application.
function global:Configure-ProjectTaxonomyFields() {

	$CAProperty = Get-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Shared Tools\Web Server Extensions\14.0\WSS\" -Name CentralAdministrationURL
	$centralAdminURL = $CAProperty.CentralAdministrationURL
	$taxonomyService = "Managed Metadata Service"
	$siteUrl = "http://spdev-luji/sites/client"

	Write-Host "Connecting to term store..."

	$centralAdmin = Get-SPSite $centralAdminURL
	$taxonomySession = Get-SPTaxonomySession -Site $centralAdmin
	$termStore = $taxonomySession.TermStores[$taxonomyService]

	$projectGroup = $termStore.Groups["GSoft.Dynamite.Examples"]
	$termSet = $projectGroup.TermSets["Wall"]

	$site = Get-SPSite $siteUrl

	[Microsoft.SharePoint.Taxonomy.TaxonomyField]$field = $site.RootWeb.Fields | Where-Object { $_.InternalName -eq "WallTags" }

	if ($field) {
		$field.SspId = $termStore.Id;
		$field.TermSetId = $termSet.Id;
		$field.AnchorId = [System.GUID]::Empty;
		$field.TargetTemplate = [System.string]::Empty;
		$field.Update();
	}

	$site.Dispose();    

	Write-Host "Done."
}