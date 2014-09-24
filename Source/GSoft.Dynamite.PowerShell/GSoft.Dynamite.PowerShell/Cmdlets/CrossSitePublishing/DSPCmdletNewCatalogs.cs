using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using System.Xml.Linq;
using System.Xml.Serialization;
using Autofac;
using GSoft.Dynamite.Helpers;
using GSoft.Dynamite.Lists;
using GSoft.Dynamite.PowerShell.Cmdlets.CrossSitePublishing.Entities;
using GSoft.Dynamite.PowerShell.Extensions;
using GSoft.Dynamite.PowerShell.PipeBindsObjects;
using GSoft.Dynamite.PowerShell.Unity;
using GSoft.Dynamite.Taxonomy;
using GSoft.Dynamite.Utils;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Navigation;
using Microsoft.SharePoint.PowerShell;
using Microsoft.SharePoint.Utilities;
using GSoft.Dynamite.Definitions;
using GSoft.Dynamite.FieldTypes;

namespace GSoft.Dynamite.PowerShell.Cmdlets.CrossSitePublishing
{
    /// <summary>
    /// Cmdlet for catalogs creation
    /// </summary>
    [Cmdlet(VerbsCommon.New, "DSPCatalogs")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    // ReSharper disable once InconsistentNaming
    public class DSPCmdletNewCatalogs : SPCmdlet
    {
        private XmlSerializer _serializer;

        /// <summary>
        /// Gets or sets the input file.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, HelpMessage = "The path to the file containing the terms to import or an XmlDocument object or XML string.", Position = 1)]
        [Alias("Xml")]
        public XmlDocumentPipeBind InputFile { get; set; }

        /// <summary>
        /// The end processing.
        /// </summary>
        protected override void InternalEndProcessing()
        {
            // Initialize XML serializer
            this._serializer = new XmlSerializer(typeof(Catalog));

            // Process XML
            var xml = this.InputFile.Read();
            var configFile = xml.ToXDocument();
            this.ProcessCatalogs(configFile);

            // End cmdlet processing
            base.InternalEndProcessing();
        }

        /// <summary>
        /// Catalog creation logic
        /// </summary>
        /// <param name="configFile">The configuration file.</param>
        private void ProcessCatalogs(XDocument configFile)
        {
            // Get all webs nodes
            var webNodes = from webNode in configFile.Descendants("Web") select webNode;
            foreach (var webNode in webNodes)
            {
                // For each web, create and configure the catalogs
                var webUrl = webNode.Attribute("Url").Value;
                using (var site = new SPSite(webUrl))
                {
                    using (var web = site.OpenWeb())
                    {
                        using (var childScope = PowerShellContainer.BeginLifetimeScope(web))
                        {
                            var listHelper = childScope.Resolve<ListHelper>();
                            var catalogHelper = childScope.Resolve<CatalogHelper>();

                            // Get all catalogs configurations
                            var catalogs = from catalogNode in webNode.Descendants("Catalog")
                                           select (Catalog)this._serializer.Deserialize(catalogNode.CreateReader());

                            foreach (var catalog in catalogs)
                            {
                                // Set current culture to be able to set the "Title" of the list
                                Thread.CurrentThread.CurrentUICulture = new CultureInfo((int)web.Language);

                                // Create the list if doesn't exists
                                var list = this.EnsureCatalogList(web, catalog);

                                // Add content types to the list
                                this.CreateContentTypes(list, catalog);

                                // Add Fields Segments
                                this.CreateSegments(list, catalog);

                                // Set default values for Fields
                                this.SetDefaultValues(list, catalog);

                                // Set Display Settings
                                this.SetDisplaySettings(list, catalog);

                                // Set the default view fields
                                if (catalog.DefaultViewFields != null)
                                {
                                    var fieldInfos = catalog.DefaultViewFields
                                        .Where(x => !string.IsNullOrEmpty(x.InternalName))
                                        .Select(x => new MinimalFieldInfo(x.InternalName, Guid.Empty))
                                        .ToArray();

                                    listHelper.AddFieldsToDefaultView(list.ParentWeb, list, fieldInfos, true); 
                                }

                                // Update the reference to the list object
                                list = listHelper.GetListByRootFolderUrl(web, catalog.RootFolderUrl);

                                // Set versioning settings
                                if (!string.IsNullOrEmpty(catalog.DraftVisibilityType))
                                {
                                    list.EnableModeration = true;
                                    list.DraftVersionVisibility = (DraftVisibilityType)Enum.Parse(
                                        typeof(DraftVisibilityType),
                                        catalog.DraftVisibilityType);

                                    list.Update();
                                }

                                if (string.IsNullOrEmpty(catalog.TaxonomyFieldMap))
                                {
                                    // Set the list as catalog without navigation
                                    if (catalog.EnableAnonymous)
                                    {
                                        catalogHelper.SetListAsCatalog(list, catalog.ManagedProperties.Select(x => x.Name), true);
                                    }
                                    else
                                    {
                                        catalogHelper.SetListAsCatalog(list, catalog.ManagedProperties.Select(x => x.Name));
                                    }
                                }
                                else
                                {
                                    // Set the list as catalog with navigation term
                                    if (catalog.EnableAnonymous)
                                    {
                                        catalogHelper.SetListAsCatalog(list, catalog.ManagedProperties.Select(x => x.Name), catalog.TaxonomyFieldMap, true); 
                                    }
                                    else
                                    {
                                        catalogHelper.SetListAsCatalog(list, catalog.ManagedProperties.Select(x => x.Name), catalog.TaxonomyFieldMap);
                                    }
                                }

                                if (catalog.EnableRatings)
                                {
                                    // Enable ratings
                                    this.WriteWarning("Set '" + catalog.RatingType + "' ratings for " + catalog.DisplayName + " to " + true);
                                    listHelper.SetRatings(list, catalog.RatingType, true);
                                }
                                else
                                {
                                    // Disable ratings
                                    this.WriteWarning("Set ratings for " + catalog.DisplayName + " to " + false);
                                    listHelper.SetRatings(list, catalog.RatingType, false);
                                }

                                // Set list Write Security
                                this.SetWriteSecurity(list, catalog);

                                // Set Navigation settings
                                this.SetNavigationSettings(list, catalog);

                                // Create return object
                                var catalogSettings = new CatalogSettings()
                                {
                                    Name = list.Title,
                                    Id = list.ID,
                                    ParentWebUrl = list.ParentWeb.Url,
                                    RootFolder = list.ParentWebUrl + "/" + list.RootFolder
                                };

                                this.WriteObject(catalogSettings, true);
                            }
                        }
                    }
                }
            }
        }

        private SPList EnsureCatalogList(SPWeb web, Catalog catalog)
        {
            SPList list = null;

            using (var childScope = PowerShellContainer.BeginLifetimeScope(web))
            {
                var listHelper = childScope.Resolve<ListHelper>();
                list = listHelper.GetListByRootFolderUrl(web, catalog.RootFolderUrl);

                if (list == null)
                {
                    list = this.EnsureList(web, catalog);
                }
                else
                {
                    this.WriteWarning("Catalog " + catalog.DisplayName + " already exists");

                    // If the Overwrite paramter is set to true, celete and recreate the catalog
                    if (catalog.Overwrite)
                    {
                        this.WriteWarning("Overwrite is set to true, recreating the list " + catalog.DisplayName);

                        list.Delete();
                        list = this.EnsureList(web, catalog);
                    }
                    else
                    {
                        // Get the existing list
                        list = this.EnsureList(web, catalog);
                    }
                }
            }

            return list;
        }

        private void SetDisplaySettings(SPList list, Catalog catalog)
        {
            if (catalog.FieldDisplaySettings != null)
            {
                // Add segments to the list
                foreach (var field in catalog.FieldDisplaySettings)
                {
                    var listfield = list.Fields.GetFieldByInternalName(field.InternalName);
                    if (listfield != null)
                    {
                        listfield.ShowInDisplayForm = field.ShowInDisplayForm;
                        listfield.ShowInEditForm = field.ShowInEditForm;
                        listfield.ShowInListSettings = field.ShowInListSettings;
                        listfield.ShowInNewForm = field.ShowInNewForm;
                        listfield.ShowInVersionHistory = field.ShowInVersionHistory;
                        listfield.ShowInViewForms = field.ShowInViewForm;

                        listfield.Update();
                    }
                }

                list.Update();
            }
        }

        private SPList EnsureList(SPWeb web, Catalog catalog)
        {
            SPList list = null;

            using (var childScope = PowerShellContainer.BeginLifetimeScope(web))
            {
                var listHelper = childScope.Resolve<ListHelper>();
                list = listHelper.GetListByRootFolderUrl(web, catalog.RootFolderUrl);
            }

            if (list == null)
            {
                // Create new list
                var listTemplate = web.ListTemplates.Cast<SPListTemplate>().Single(x => x.Type == (SPListTemplateType)catalog.ListTemplateId);
                var id = web.Lists.Add(catalog.RootFolderUrl, catalog.Description, listTemplate);
                list = web.Lists[id];
            }

            list.Title = catalog.DisplayName;
            list.ContentTypesEnabled = true;
            list.Update(true);

            return list;
        }

        private void CreateContentTypes(SPList list, Catalog catalog)
        {
            if (catalog.RemoveDefaultContentType)
            {
                // If content type is direct child of item, remove it
                var itemContentTypeId = list.ContentTypes.BestMatch(SPBuiltInContentTypeId.Item);
                if (itemContentTypeId.Parent == SPBuiltInContentTypeId.Item)
                {
                    list.ContentTypes.Delete(itemContentTypeId);
                }
            }

            // Add content type to the list if doesn't exist
            foreach (var contentType in catalog.ContentTypes)
            {
                var contentTypeId = new SPContentTypeId(contentType.Id);

                var ct = list.ParentWeb.AvailableContentTypes[contentTypeId];

                if (ct == null)
                {
                    this.WriteWarning("Content type " + contentType + " doesn't exists");
                }

                if (ct != null)
                {
                    try
                    {
                        list.ContentTypes.Add(ct);
                    }
                    catch (SPException ex)
                    {
                        this.WriteWarning(ex.Message);
                    }
                }
            }

            list.Update();
        }

        private void CreateSegments(SPList list, Catalog catalog)
        {
            using (var childScope = PowerShellContainer.BeginLifetimeScope(list.ParentWeb))
            {
                var listHelper = childScope.Resolve<ListHelper>();
                var taxonomyHelper = childScope.Resolve<TaxonomyHelper>();

                // Add segments to the list
                foreach (var segment in catalog.Segments)
                {
                    if (segment is TaxonomyField)
                    {
                        var taxonomySegment = segment as TaxonomyField;

                        // Create the column in the list
                        var taxonomyField = listHelper.CreateListTaxonomyField(list, taxonomySegment.InternalName, taxonomySegment.DisplayName, taxonomySegment.Description, segment.Group, taxonomySegment.IsMultiple, taxonomySegment.IsOpen);

                        // Set required if true
                        if (taxonomySegment.IsRequired)
                        {
                            taxonomyField.Required = true;
                            taxonomyField.Update();
                        }

                        // Assign the termSet to the field with an anchor term if specified
                        if (taxonomySegment.TermSubsetId != null)
                        {
                            var termId = new Guid(taxonomySegment.TermSubsetId);
                            taxonomyHelper.AssignTermSetToListColumn(list, taxonomyField.Id, taxonomySegment.TermSetGroupName, taxonomySegment.TermSetName, termId);
                        }
                        else
                        {
                            taxonomyHelper.AssignTermSetToListColumn(list, taxonomyField.Id, taxonomySegment.TermSetGroupName, taxonomySegment.TermSetName, taxonomySegment.TermSubsetName);
                        }

                        this.WriteVerbose("TaxonomyField " + segment.InternalName + " successfully created!"); 
                    }
                    else if (segment is TextField)
                    {
                        var textSegment = segment as TextField;

                        // Create the column in the list
                        var textField = listHelper.CreateTextField(list, segment.InternalName, segment.DisplayName, segment.Description, segment.Group, textSegment.IsMultiline);
                        
                        // Set required if true
                        if (textSegment.IsRequired)
                        {
                            textField.Required = true;
                            textField.Update();
                        }

                        this.WriteVerbose("TextField " + segment.InternalName + " successfully created!");
                    }
                }
            }
        }

        private void SetDefaultValues(SPList list, Catalog catalog)
        {
            using (var childScope = PowerShellContainer.BeginLifetimeScope(list.ParentWeb))
            {
                var taxonomyHelper = childScope.Resolve<TaxonomyHelper>();

                // Add segments to the list
                foreach (var defaultValue in catalog.Defaults)
                {
                    var field = list.Fields.GetFieldByInternalName(defaultValue.InternalName);
                    if (field.GetType() == typeof(Microsoft.SharePoint.Taxonomy.TaxonomyField) && (defaultValue is TaxonomyField))
                    {
                        var taxonomyDefaultValue = defaultValue as TaxonomyField;
                        if (((Microsoft.SharePoint.Taxonomy.TaxonomyField)field).AllowMultipleValues)
                        {
                            ////taxonomyHelper.SetDefaultTaxonomyMultiValue(list.ParentWeb, field, taxonomyDefaultValue.TermSetGroupName, taxonomyDefaultValue.TermSetName, defaultValue.Values);
                        }
                        else
                        {
                            ////taxonomyHelper.SetDefaultTaxonomyValue(list.ParentWeb, field, taxonomyDefaultValue.TTermSetGroupName, taxonomyDefaultValue.TermSetName, defaultValue.Values.First());
                        }
                    }
                    else if (field.GetType() == typeof(SPFieldText))
                    {
                        field.DefaultValue = defaultValue.Values.FirstOrDefault();
                        field.Update();
                    }
                    else
                    {
                        this.WriteWarning(string.Format(CultureInfo.InvariantCulture, "Field '{0}' of type '{1}' cannot be found.", defaultValue.InternalName, defaultValue.GetType().Name));
                    }
                }
            }
        }

        private void SetWriteSecurity(SPList list, Catalog catalog)
        {
            // Allowed values are 1, 2 or 4
            // http://msdn.microsoft.com/en-us/library/microsoft.sharepoint.splist.writesecurity(v=office.15).aspx
            if (catalog.WriteSecurity == 1 || catalog.WriteSecurity == 2 || catalog.WriteSecurity == 4)
            {
                list.WriteSecurity = catalog.WriteSecurity;
                list.Update();
            }
        }

        private void SetNavigationSettings(SPList list, Catalog catalog)
        {
            if (catalog.AddToQuickLaunch)
            {
                var web = list.ParentWeb;

                // Check for an existing link to the list.
                var listNode = web.Navigation.GetNodeByUrl(list.DefaultViewUrl);

                // No link, so create one.
                if (listNode == null)
                {
                    // Create the node.
                    listNode = new SPNavigationNode(list.Title, list.DefaultViewUrl);

                    // Add it to Quick Launch.
                    web.Navigation.AddToQuickLaunch(listNode, SPQuickLaunchHeading.Lists);
                }
            }
        }
    }
}
