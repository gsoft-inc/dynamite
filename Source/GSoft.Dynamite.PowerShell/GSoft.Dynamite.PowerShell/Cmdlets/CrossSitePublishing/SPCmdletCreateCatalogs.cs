using System;
using System.Linq;
using System.Management.Automation;
using System.Xml.Linq;
using GSoft.Dynamite.PowerShell.Extensions;
using GSoft.Dynamite.PowerShell.PipeBindsObjects;
using GSoft.Dynamite.PowerShell.Unity;
using GSoft.Dynamite.Utils;
using Microsoft.Practices.Unity;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.PowerShell.Cmdlets.CrossSitePublishing
{
    /// <summary>
    /// Cmdlet for catalogs creation
    /// </summary>
    [Cmdlet("Create", "SPCatalogs")]
    public class SPCatalog : Cmdlet
    {
        /// <summary>
        /// Dynamite Helpers
        /// </summary>
        private ListHelper _listHelper;
        private CatalogHelper _catalogHelper;
        private ContentTypeHelper _contentTypeHelper;

        private XDocument _configurationFile = null;

        [Parameter(Mandatory = true,
            ValueFromPipeline = true,
            HelpMessage = "The path to the file containing the terms to import or an XmlDocument object or XML string.",
            Position = 1)]
        [Alias("Xml")]
        public XmlDocumentPipeBind InputFile { get; set; }

        protected override void EndProcessing()
        {
            this.ResolveDependencies();

            var xml = InputFile.Read();
            _configurationFile = xml.ToXDocument();

            // Get all webs nodes
            var webNodes = from webNode in _configurationFile.Descendants("Web")
                       select (webNode);

            foreach (var webNode in webNodes)
            {
                var webUrl = webNode.Attribute("Url").Value;

                using (var spSite = new SPSite(webUrl))
                {
                    var spWeb = spSite.OpenWeb();

                    // Get all catalogs nodes
                    var catalogNodes = from catalogNode in webNode.Descendants("Catalog")
                                       select (catalogNode);

                    foreach (var catalogNode in catalogNodes)
                    {
                        var catalogName = catalogNode.Attribute("Name").Value;
                        var catalogDescription = catalogNode.Attribute("Description").Value;
                        var listTemplate = spWeb.ListTemplates[catalogNode.Attribute("ListTemplate").Value];
                        var taxonomyFieldMap = catalogNode.Attribute("TaxonomyFieldMap").Value;

                        // Get content types
                        var contentTypes = from contentType in catalogNode.Descendants("ContentTypes").Descendants("ContentType")
                                           select contentType.Attribute("Name").Value;

                         // Get availables fields
                        var availableFields = from contentType in catalogNode.Descendants("AvailableFields").Descendants("Field")
                                              select contentType.Attribute("InternalName").Value;


                        // Create the list if doesn't exists
                        var list = this._listHelper.EnsureList(spWeb, catalogName, catalogDescription, listTemplate);

                        // Add content type to the list if doesn't exist
                        foreach (var contentType in contentTypes)
                        {
                            var ct = spWeb.AvailableContentTypes[contentType];

                            if (ct != null)
                            {
                                if (list.ContentTypes[contentType] == null)
                                {
                                    list.ContentTypes.Add(ct);
                                }
                            }
                        }

                        list.Update();

                        if (String.IsNullOrEmpty(taxonomyFieldMap))
                        {
                            // Set the list as catalog without navigation
                            this._catalogHelper.SetListAsCatalog(list, availableFields);
                       }                    
                    }   
                }
            }

            base.EndProcessing();
        }

        /// <summary>
        /// Resolve Dependencies for helpers
        /// </summary>
        private void ResolveDependencies()
        {
            this._listHelper = PowerShellContainer.Current.Resolve<ListHelper>();
            this._catalogHelper = PowerShellContainer.Current.Resolve<CatalogHelper>();
            this._contentTypeHelper = PowerShellContainer.Current.Resolve<ContentTypeHelper>();
        }
    }

}
