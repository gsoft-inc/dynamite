using System;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using System.Xml.Linq;
using System.Xml.Serialization;
using GSoft.Dynamite.PowerShell.Cmdlets.CrossSitePublishing.Entities;
using GSoft.Dynamite.PowerShell.Extensions;
using GSoft.Dynamite.PowerShell.PipeBindsObjects;
using GSoft.Dynamite.PowerShell.Unity;
using GSoft.Dynamite.Taxonomy;
using GSoft.Dynamite.Utils;
using Microsoft.Practices.Unity;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.PowerShell.Cmdlets.CrossSitePublishing
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Cmdlet for catalogs creation
    /// </summary>
    [Cmdlet(VerbsCommon.New, "DSPCatalogs")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    // ReSharper disable once InconsistentNaming
    public class DSPCmdletNewCatalogs : Cmdlet
    {
        /// <summary>
        /// Dynamite Helpers
        /// </summary>
        private ListHelper _listHelper;
        private CatalogHelper _catalogHelper;
        private TaxonomyHelper _taxonomyHelper;
        private XmlSerializer _serializer;

        /// <summary>
        /// Gets or sets the input file.
        /// </summary>
        [Parameter(Mandatory = true, 
            ValueFromPipeline = true, 
            HelpMessage = "The path to the file containing the terms to import or an XmlDocument object or XML string.", 
            Position = 1)]
        [Alias("Xml")]
        public XmlDocumentPipeBind InputFile { get; set; }

        /// <summary>
        /// The end processing.
        /// </summary>
        protected override void EndProcessing()
        {
            // Resolve Unity dependencies
            this.ResolveDependencies();

            // Initialize XML serializer
            this._serializer = new XmlSerializer(typeof(Catalog));

            // Process XML
            var xml = this.InputFile.Read();
            var configFile = xml.ToXDocument();
            this.ProcessCatalogs(configFile);
  
            // End cmdlet processing
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
                    var web = site.OpenWeb();

                    // Get all catalogs configurations
                    var catalogConfigurations = from catalogNode in webNode.Descendants("Catalog") 
                                                select (Catalog)this._serializer.Deserialize(catalogNode.CreateReader());

                    foreach (var catalogConfiguration in catalogConfigurations)
                    {                        
                        // Set current culture to be able to set the "Title" of the list
                        Thread.CurrentThread.CurrentUICulture = new CultureInfo((int)web.Language);

                        // Create the list if doesn't exists
                        var list = this.EnsureCatalogList(web, catalogConfiguration);

                        // Add content types to the list
                        this.CreateContentTypes(list, catalogConfiguration);

                        // Add Fields Segments
                        this.CreateSegments(list, catalogConfiguration);

                        // Set default values for Fields
                        this.SetDefaultValues(list, catalogConfiguration);

                        // Set Display Settings
                        this.SetDisplaySettings(list, catalogConfiguration);

                        // Set versioning settings
                        if (catalogConfiguration.HasDraftVisibilityType)
                        {
                            list.EnableModeration = true;
                            list.DraftVersionVisibility = (DraftVisibilityType)Enum.Parse(
                                typeof(DraftVisibilityType),
                                catalogConfiguration.DraftVisibilityType);

                            list.Update();
                        }

                        if (string.IsNullOrEmpty(catalogConfiguration.TaxonomyFieldMap))
                        {
                            // Set the list as catalog without navigation
                            this._catalogHelper.SetListAsCatalog(list, catalogConfiguration.ManagedProperties.Select(x => x.Name));
                        }
                        else
                        {
                            // Set the list as catalog with navigation term
                            this._catalogHelper.SetListAsCatalog(list, catalogConfiguration.ManagedProperties.Select(x => x.Name), catalogConfiguration.TaxonomyFieldMap);
                        }

                        if (catalogConfiguration.EnableRatings)
                        {
                            // Enable ratings
                            this.WriteWarning("Set '" + catalogConfiguration.RatingType + "' ratings for " + catalogConfiguration.DisplayName + " to " + true);
                            this._listHelper.SetRatings(list, catalogConfiguration.RatingType, true); 
                        }
                        else
                        {
                            // Disable ratings
                            this.WriteWarning("Set ratings for " + catalogConfiguration.DisplayName + " to " + false);
                            this._listHelper.SetRatings(list, catalogConfiguration.RatingType, false); 
                        }

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

        private SPList EnsureCatalogList(SPWeb web, Catalog catalogConfiguration)
        {
            var list = this._listHelper.GetListByRootFolderUrl(web, catalogConfiguration.RootFolderUrl);

            if (list == null)
            {
                list = this.EnsureList(web, catalogConfiguration);
            }
            else
            {
                this.WriteWarning("Catalog " + catalogConfiguration.DisplayName + " already exists");

                // If the Overwrite paramter is set to true, celete and recreate the catalog
                if (catalogConfiguration.Overwrite)
                {
                    this.WriteWarning("Overwrite is set to true, recreating the list " + catalogConfiguration.DisplayName);

                    list.Delete();
                    list = this.EnsureList(web, catalogConfiguration);
                }
                else
                {
                    // Get the existing list
                    list = this.EnsureList(web, catalogConfiguration);
                }
            }

            return list;
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Private method.")]
        private void SetDisplaySettings(SPList list, Catalog catalogConfiguration)
        {
            if (catalogConfiguration.FieldDisplaySettings != null)
            {
                // Add segments to the list
                foreach (var field in catalogConfiguration.FieldDisplaySettings)
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

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Private method.")]
        private SPList EnsureList(SPWeb web, Catalog catalogConfiguration)
        {
            var list = this._listHelper.GetListByRootFolderUrl(web, catalogConfiguration.RootFolderUrl);
                
            if (list == null)
            {
                // Create new list
                var listTemplate = web.ListTemplates.Cast<SPListTemplate>().Single(x => x.Type == (SPListTemplateType)catalogConfiguration.ListTemplateId);
                var id = web.Lists.Add(catalogConfiguration.RootFolderUrl, catalogConfiguration.Description, listTemplate);
                list = web.Lists[id];
            }

            list.Title = catalogConfiguration.DisplayName;
            list.ContentTypesEnabled = true;
            list.Update(true);

            return list;
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Private method.")]
        private void CreateContentTypes(SPList list, Catalog catalogConfiguration)
        {
            if (catalogConfiguration.RemoveDefaultContentType)
            {
                // If content type is direct child of item, remove it
                var itemContentTypeId = list.ContentTypes.BestMatch(SPBuiltInContentTypeId.Item);
                if (itemContentTypeId.Parent == SPBuiltInContentTypeId.Item)
                {
                    list.ContentTypes.Delete(itemContentTypeId);
                }
            }

            // Add content type to the list if doesn't exist
            foreach (var contentType in catalogConfiguration.ContentTypes)
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

        private void CreateSegments(SPList list, Catalog catalogConfiguration)
        {
            // Add segments to the list
            foreach (var segment in catalogConfiguration.Segments)
            {
                if (segment is TaxonomyField)
                {
                    var taxonomySegment = segment as TaxonomyField;

                    // Create the column in the list
                    var taxonomyField = this._listHelper.CreateListTaxonomyField(list, taxonomySegment.InternalName, taxonomySegment.DisplayName, taxonomySegment.Description, segment.Group, taxonomySegment.IsMultiple, taxonomySegment.IsOpen);

                    // Assign the termSet to the field with an anchor term if specified
                    this._taxonomyHelper.AssignTermSetToListColumn(list, taxonomyField.Id, taxonomySegment.TermSetGroupName, taxonomySegment.TermSetName, taxonomySegment.TermSubsetName);
                    this.WriteVerbose("TaxonomyField " + segment.InternalName + " successfully created!"); 
                }
                else if (segment is TextField)
                {
                    var textSegment = segment as TextField;

                    // Create the column in the list
                    this._listHelper.CreateTextField(list, segment.InternalName, segment.DisplayName, segment.Description, segment.Group, textSegment.IsMultiline);
                    this.WriteVerbose("TextField " + segment.InternalName + " successfully created!");
                }
            }
        }

        private void SetDefaultValues(SPList list, Catalog catalogConfiguration)
        {
            // Add segments to the list
            foreach (var defaultValue in catalogConfiguration.Defaults)
            {
                var field = list.Fields.GetFieldByInternalName(defaultValue.InternalName);
                if (field.GetType() == typeof(Microsoft.SharePoint.Taxonomy.TaxonomyField) && (defaultValue is TaxonomyField))
                {
                    var taxonomyDefaultValue = defaultValue as TaxonomyField;
                    if (((Microsoft.SharePoint.Taxonomy.TaxonomyField)field).AllowMultipleValues)
                    {
                        this._taxonomyHelper.SetDefaultTaxonomyMultiValue(list.ParentWeb, field, taxonomyDefaultValue.TermSetGroupName, taxonomyDefaultValue.TermSetName, defaultValue.Values);
                    }
                    else
                    {
                        this._taxonomyHelper.SetDefaultTaxonomyValue(list.ParentWeb, field, taxonomyDefaultValue.TermSetGroupName, taxonomyDefaultValue.TermSetName, defaultValue.Values.First());
                    }
                }
                else
                {
                    this.WriteWarning("Field " + defaultValue.InternalName + " is not a TaxonomyField");
                }

                if (field.GetType() == typeof(SPFieldText))
                {
                    field.DefaultValue = defaultValue.Values.FirstOrDefault();
                    field.Update();
                }
                else
                {
                    this.WriteWarning("Field " + defaultValue.InternalName + " is not a SPField");
                }
            }
        }
    }
}
