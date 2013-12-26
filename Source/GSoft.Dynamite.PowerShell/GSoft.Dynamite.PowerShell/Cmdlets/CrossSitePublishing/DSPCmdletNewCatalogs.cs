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
using Microsoft.SharePoint.PowerShell;
using System.Collections.Generic;
using GSoft.Dynamite.Taxonomy;
using System.Globalization;
using System.Threading;
using Microsoft.SharePoint.Taxonomy;
using System.Collections.ObjectModel;


namespace GSoft.Dynamite.PowerShell.Cmdlets.CrossSitePublishing
{
    /// <summary>
    /// Cmdlet for catalogs creation
    /// </summary>
    [Cmdlet(VerbsCommon.New, "DSPCatalogs")]
    public class DSPCmdletNewCatalogs : Cmdlet
    {
        /// <summary>
        /// Dynamite Helpers
        /// </summary>
        private ListHelper _listHelper;
        private CatalogHelper _catalogHelper;
        private TaxonomyHelper _taxonomyHelper;

        private XDocument _configurationFile = null;

        [Parameter(Mandatory = true,
            ValueFromPipeline = true,
            HelpMessage = "The path to the file containing the terms to import or an XmlDocument object or XML string.",
            Position = 1)]
        [Alias("Xml")]
        public XmlDocumentPipeBind InputFile { get; set; }
   
        public bool _delete;

        [Parameter(HelpMessage = "Delete catalog configuration",
            Position = 2)]
        public SwitchParameter Delete
        {
            get { return _delete; }
            set { _delete = value; }
        }

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
                        var catalogUrl = catalogNode.Attribute("RootFolderUrl").Value;
                        var catalogName = catalogNode.Attribute("DisplayName").Value;
                        var catalogDescription = catalogNode.Attribute("Description").Value;
                        var listTemplate = spWeb.ListTemplates[catalogNode.Attribute("ListTemplate").Value];
                        var taxonomyFieldMap = catalogNode.Attribute("TaxonomyFieldMap").Value;
                        var overwrite = Boolean.Parse(catalogNode.Attribute("Overwrite").Value);

                        // Get content types
                        var contentTypes = from contentType in catalogNode.Descendants("ContentTypes").Descendants("ContentType")
                                           select contentType;

                        // Get availables fields
                        var availableFields = from contentType in catalogNode.Descendants("ManagedProperties").Descendants("Property")
                                              select contentType.Attribute("Name").Value;

                        // Get segments
                        var segments = from segment in catalogNode.Descendants("Segments").Descendants("Segment")
                                       select segment;

                        // Get defaults for taxonomy Fields
                        var defaultsTaxFields = from defaultValue in catalogNode.Descendants("Defaults").Descendants("TaxonomyField")
                                       select defaultValue;

                        // Set current culture to be able to set the "Title" of the list
                        CultureInfo originalUICulture = Thread.CurrentThread.CurrentUICulture;
                        Thread.CurrentThread.CurrentUICulture =
                            new CultureInfo((int)spWeb.Language);

                        // Create the list if doesn't exists
                        var list = this._listHelper.GetListByRootFolderUrl(spWeb, catalogUrl);

                        if(this._delete)
                        {
                            if(list !=null)
                            {
                                WriteWarning("Delete the list " + catalogName);

                                // Delete the list
                                list.Delete();
                            }
                            else
                            {
                                WriteWarning("No list with the name " + catalogName);
                            }
                        }
                        else
                        {
                            if(list == null)
                            {
                                list = CreateList(spWeb, catalogUrl, catalogName, catalogDescription, listTemplate);
                            }
                            else
                            {
                                WriteWarning("Catalog " + catalogName + " is already exists");

                                // If the Overwrite paramter is set to true, celete and recreate the catalog
                                if(overwrite)
                                {
                                    WriteWarning("Overwrite is set to true, recreating the list " + catalogName);

                                    list.Delete();
                                    list = CreateList(spWeb, catalogUrl, catalogName, catalogDescription, listTemplate);                               
                                }
                            }

                            // Add content types to the list
                            CreateContentTypes(contentTypes, list);

                            // Add Segments
                            CreateSegments(segments, list);

                            // Set default values
                            SetTaxonomyDefaults(defaultsTaxFields, list);

                            if (String.IsNullOrEmpty(taxonomyFieldMap))
                            {
                                // Set the list as catalog without navigation
                                this._catalogHelper.SetListAsCatalog(list, availableFields);
                            }
                            else
                            {
                                // Set the list as catalog with navigation term
                                this._catalogHelper.SetListAsCatalog(list, availableFields, taxonomyFieldMap);
                            }
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
            this._taxonomyHelper = PowerShellContainer.Current.Resolve<TaxonomyHelper>();
        }

        /// <summary>
        /// Create the list
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="listUrl">The list url.</param>
        /// <param name="displayName">The display name.</param>
        /// <param name="listDescription">The list description.</param>
        /// <param name="listTemplate">The list template.</param>
        /// <returns></returns>
        private SPList CreateList(SPWeb web, string listUrl, string displayName, string listDescription, SPListTemplate listTemplate)
        {
            var list = this._listHelper.EnsureList(web, listUrl, listDescription, listTemplate);
            list.Title = displayName;
            list.ContentTypesEnabled = true;
            list.Update(true);

            return list;
        }

        /// <summary>
        /// Create Content Types
        /// </summary>
        /// <param name="contentTypesCollection">Content Types collection.</param>
        /// <param name="list">The list to configure.</param>
        private void CreateContentTypes(IEnumerable<XElement> contentTypesCollection, SPList list)
        {
            // Add content type to the list if doesn't exist
            foreach (XElement contentType in contentTypesCollection)
            {
                var contentTypeId = new SPContentTypeId(contentType.Attribute("ID").Value);

                var ct = list.ParentWeb.AvailableContentTypes[contentTypeId];

                if (ct == null)
                {
                    WriteWarning("Content type " + contentType + " doesn't exists");
                }

                if (ct != null)
                {
                    try
                    {
                        list.ContentTypes.Add(ct);
                    }
                    catch(SPException ex)
                    {
                        WriteWarning(ex.Message);
                    }
                }
            }

            list.Update();
        }

        /// <summary>
        /// Create segements
        /// </summary>
        /// <param name="segmentsCollection">The segments collection.</param>
        /// <param name="list">List to confgiure.</param>
        private void CreateSegments(IEnumerable<XElement> segmentsCollection, SPList list)
        {
            // Add segments to the list
            foreach (XElement segment in segmentsCollection)
            {
                var internalName = segment.Attribute("InternalName").Value;
                var displayName = segment.Attribute("DisplayName").Value;
                var description = segment.Attribute("Description").Value;
                var group = segment.Attribute("Group").Value;
                var isMultiple = Boolean.Parse(segment.Attribute("IsMultiple").Value);
                var isOpen = Boolean.Parse(segment.Attribute("IsOpen").Value);
                var termSetGroupName = segment.Attribute("TermSetGroupName").Value;
                var termSetName = segment.Attribute("TermSetName").Value;
             
                // Create the column in the list
                var taxonomyField = this._taxonomyHelper.CreateListTaxonomyField(list, internalName, displayName, description, group, isMultiple, isOpen);

                // Assign the termSet to the field
                this._taxonomyHelper.AssignTermSetToListColumn(list, taxonomyField.Id, termSetGroupName, termSetName, string.Empty);

                WriteVerbose("Segment " + internalName + " successfully created!");
            }
        }

        /// <summary>
        /// Set default values for taxonomy fields
        /// </summary>
        /// <param name="defaultsCollection">Defaults values.</param>
        /// <param name="list">The list to configure.</param>
        private void SetTaxonomyDefaults(IEnumerable<XElement> defaultsCollection, SPList list)
        {
            // Add segments to the list
            foreach (XElement defaultValue in defaultsCollection)
            {
                var internalName = defaultValue.Attribute("InternalName").Value;
                var termGroup = defaultValue.Attribute("TermSetGroupName").Value;
                var termSet = defaultValue.Attribute("TermSetName").Value;

                var field = list.Fields.GetFieldByInternalName(internalName);

                if(field.GetType() == typeof(TaxonomyField))
                {
                    var terms = new Collection<string>();

                    // Get terms
                    foreach(var term in defaultValue.Descendants("Term"))
                    {
                        terms.Add(term.Value);
                    }

                    if(((TaxonomyField)field).AllowMultipleValues)
                    {
                        this._taxonomyHelper.SetDefaultTaxonomyMultiValue(list.ParentWeb, field, termGroup, termSet, terms.ToArray<string>());
                    }
                    else
                    {
                        this._taxonomyHelper.SetDefaultTaxonomyValue(list.ParentWeb, field, termGroup, termSet, terms.ToArray<string>().First());
                    }
                }
                else
                {
                    WriteWarning("Field " + internalName + " is not a TaxonomyField");
                }
            }
        }
    }
}
