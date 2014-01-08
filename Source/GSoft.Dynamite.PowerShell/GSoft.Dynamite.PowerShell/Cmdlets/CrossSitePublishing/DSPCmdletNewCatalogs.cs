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
using GSoft.Dynamite.Schemas;


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
                        var listTemplateId = int.Parse(catalogNode.Attribute("ListTemplateId").Value);

                        DraftVisibilityType draftVisibilityType;

                        var listTemplate = spWeb.ListTemplates.Cast<SPListTemplate>().Single(x => x.Type == (SPListTemplateType) listTemplateId);
                        var taxonomyFieldMap = catalogNode.Attribute("TaxonomyFieldMap").Value;
                        var overwrite = Boolean.Parse(catalogNode.Attribute("Overwrite").Value);
                        var removeDefaultContentType = Boolean.Parse(catalogNode.Attribute("RemoveDefaultContentType").Value);

                        // Get content types
                        var contentTypes = from contentType in catalogNode.Descendants("ContentTypes").Descendants("ContentType")
                                           select contentType;

                        // Get availables fields
                        var availableFields = from contentType in catalogNode.Descendants("ManagedProperties").Descendants("Property")
                                              select contentType.Attribute("Name").Value;

                        // Get TaxonomyFields segments
                        var taxonomyFieldSegments = from segment in catalogNode.Descendants("Segments").Descendants("TaxonomyField")
                                       select segment;

                        // Get TextFields segments
                        var textFieldSegments = from segment in catalogNode.Descendants("Segments").Descendants("TextField")
                                               select segment;

                        // Get defaults for taxonomy Fields
                        var defaultsTaxonomyFields = from defaultValue in catalogNode.Descendants("Defaults").Descendants("TaxonomyField")
                                       select defaultValue;

                        // Get defaults for text fields
                        var defaultsTextFields = from defaultValue in catalogNode.Descendants("Defaults").Descendants("TextField")
                                                select defaultValue;

                        // Set current culture to be able to set the "Title" of the list
                        CultureInfo originalUICulture = Thread.CurrentThread.CurrentUICulture;
                        Thread.CurrentThread.CurrentUICulture =
                            new CultureInfo((int)spWeb.Language);

                        // Create the list if doesn't exists
                        var list = this._listHelper.GetListByRootFolderUrl(spWeb, catalogUrl);
                       
                        if(list == null)
                        {
                            list = EnsureList(spWeb, catalogUrl, catalogName, catalogDescription, listTemplate);
                        }
                        else
                        {
                            WriteWarning("Catalog " + catalogName + " is already exists");

                            // If the Overwrite paramter is set to true, celete and recreate the catalog
                            if(overwrite)
                            {
                                WriteWarning("Overwrite is set to true, recreating the list " + catalogName);

                                list.Delete();
                                list = EnsureList(spWeb, catalogUrl, catalogName, catalogDescription, listTemplate);                               
                            }
                            else
                            {
                                // Get the existing list
                                list = EnsureList(spWeb, catalogUrl, catalogName, catalogDescription, listTemplate);     
                            }                              
                        }

                        // Create return object
                        var catalog = new Catalog() {Name = list.Title, Id = list.ID, ParentWebUrl = list.ParentWebUrl, RootFolder = list.ParentWebUrl + "/" + list.RootFolder };

                        // Add content types to the list
                        CreateContentTypes(contentTypes, list, removeDefaultContentType);

                        // Add Taxonomy Fields Segments
                        CreateTaxonomyFieldSegments(taxonomyFieldSegments, list);

                        // Add Text Fields Segments
                        CreateTextFieldSegments(textFieldSegments, list);

                        // Set default values for Taxonomy Fields
                        SetTaxonomyDefaults(defaultsTaxonomyFields, list);

                        // Set default values for Text Fields
                        SetTextFieldDefaults(defaultsTextFields, list);

                        // Set versioning settings
                        if (!String.IsNullOrEmpty(catalogNode.Attribute("DraftVisibilityType").Value))
                        {
                            draftVisibilityType = (DraftVisibilityType)Enum.Parse(typeof(DraftVisibilityType), catalogNode.Attribute("DraftVisibilityType").Value, true);
                            list.EnableModeration = true;
                            list.DraftVersionVisibility = draftVisibilityType;
                            list.Update();
                        }

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

                        // Write object to the pipeline
                        WriteObject(catalog, true);
                                                
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
        private SPList EnsureList(SPWeb web, string listUrl, string displayName, string listDescription, SPListTemplate listTemplate)
        {
            var list = this._listHelper.GetListByRootFolderUrl(web, listUrl);
                
            if(list == null)
            {
                // Create new list
                var id = web.Lists.Add(listUrl, listDescription, listTemplate);
                list = web.Lists[id];
            }

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
        private void CreateContentTypes(IEnumerable<XElement> contentTypesCollection, SPList list, bool removeDefaultContentType)
        {
            if(removeDefaultContentType)
            {
                // If content type is direct child of item, remove it
                var itemContentTypeId = list.ContentTypes.BestMatch(SPBuiltInContentTypeId.Item);
                if (itemContentTypeId.Parent == SPBuiltInContentTypeId.Item)
                {
                    list.ContentTypes.Delete(itemContentTypeId);
                }
            }

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
        /// Create TaxonomyFields segments
        /// </summary>
        /// <param name="segmentsCollection">The segments collection.</param>
        /// <param name="list">List to configure.</param>
        private void CreateTaxonomyFieldSegments(IEnumerable<XElement> segmentsCollection, SPList list)
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
                var taxonomyField = this._listHelper.CreateListTaxonomyField(list, internalName, displayName, description, group, isMultiple, isOpen);

                // Assign the termSet to the field
                this._taxonomyHelper.AssignTermSetToListColumn(list, taxonomyField.Id, termSetGroupName, termSetName, string.Empty);
                                       
                WriteVerbose("TaxonomyField " + internalName + " successfully created!");
            }
        }

        /// <summary>
        /// Create TextField segments
        /// </summary>
        /// <param name="segmentsCollection">The segments collection.</param>
        /// <param name="list">List to configure.</param>
        private void CreateTextFieldSegments(IEnumerable<XElement> segmentsCollection, SPList list)
        {
            // Add segments to the list
            foreach (XElement segment in segmentsCollection)
            {
                var internalName = segment.Attribute("InternalName").Value;
                var displayName = segment.Attribute("DisplayName").Value;
                var description = segment.Attribute("Description").Value;
                var group = segment.Attribute("Group").Value;
                var isMultiple = Boolean.Parse(segment.Attribute("IsMultiline").Value);
                
                // Create the column in the list
                var textField = this._listHelper.CreateTextField(list, internalName, displayName, description, group, isMultiple);
                     
                WriteVerbose("TextField " + internalName + " successfully created!");
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
                    foreach(var term in defaultValue.Descendants("Value"))
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

        /// <summary>
        /// Set default values for text fields
        /// </summary>
        /// <param name="defaultsCollection">Defaults values.</param>
        /// <param name="list">The list to configure.</param>
        private void SetTextFieldDefaults(IEnumerable<XElement> defaultsCollection, SPList list)
        {
            // Add segments to the list
            foreach (XElement defaultValue in defaultsCollection)
            {
                var internalName = defaultValue.Attribute("InternalName").Value;
                var field = list.Fields.GetFieldByInternalName(internalName);

                if (field.GetType() == typeof(SPFieldText))
                {
                    if(defaultValue.Descendants("Value").Count() > 1)
                    {
                        WriteWarning("There is more than one default value for " + internalName + " SPField. Please specify  an unique value.");
                    }
                    else
                    {
                        var val = defaultValue.Descendants("Value").Single().Value;
                        field.DefaultValue = val;
                        field.Update();
                    }                
                }
                else
                {
                    WriteWarning("Field " + internalName + " is not a SPField");
                }
            }
        }
    }

    /// <summary>
    /// Object to pass to the pipeline
    /// </summary>
    public class Catalog
    {
        public string Name { get; set; }
        public Guid Id { get; set; }
        public string ParentWebUrl { get; set; }
        public string RootFolder { get; set; }
    }
}
