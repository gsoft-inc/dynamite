using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using GSoft.Dynamite.ContentTypes;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.Globalization;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.Taxonomy;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Navigation;
using Microsoft.SharePoint.Taxonomy;

namespace GSoft.Dynamite.Lists
{
    /// <summary>
    /// Helper class to manage lists.
    /// </summary>
    public class ListHelper : IListHelper
    {
        private const string PagesLibraryRootFolder = "Pages";

        private readonly IContentTypeHelper contentTypeBuilder;
        private readonly IResourceLocator resourceLocator;
        private readonly IFieldLocator fieldLocator;
        private readonly IFieldHelper fieldHelper;
        private readonly ILogger logger;
        private readonly IListLocator listLocator;

        /// <summary>Creates a list helper</summary>
        /// <param name="contentTypeBuilder">The content Type Builder.</param>
        /// <param name="fieldLocator">The field locator.</param>
        /// <param name="fieldHelper">Field creator utility</param>
        /// <param name="resourceLocator">The resource locator</param>
        /// <param name="logger">The logger</param>
        /// <param name="listLocator">List locator</param>
        public ListHelper(
            IContentTypeHelper contentTypeBuilder, 
            IFieldLocator fieldLocator,
            IFieldHelper fieldHelper,
            IResourceLocator resourceLocator, 
            ILogger logger, 
            IListLocator listLocator)
        {
            this.contentTypeBuilder = contentTypeBuilder;
            this.fieldLocator = fieldLocator;
            this.fieldHelper = fieldHelper;
            this.resourceLocator = resourceLocator;
            this.logger = logger;
            this.listLocator = listLocator;
        }

        /// <summary>
        /// Finds the list template corresponding to the specified name
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If the template does not exist</exception>
        /// <param name="web">The current web</param>
        /// <param name="templateName">The list template name</param>
        /// <returns>The list template</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public SPListTemplate GetListTemplate(SPWeb web, string templateName)
        {
            var template = web.ListTemplates.Cast<SPListTemplate>().FirstOrDefault(i => i.Name == templateName);
            if (template == null)
            {
                throw new ArgumentOutOfRangeException(string.Format(CultureInfo.InvariantCulture, "List template {0} is not available in the web.", templateName));
            }

            return template;
        }

        /// <summary>
        /// Creates the list or returns the existing one.
        /// </summary>
        /// <remarks>The list name and description will not be translated</remarks>
        /// <exception cref="SPException">If the list already exists but doesn't have the specified list template.</exception>
        /// <param name="web">The current web</param>
        /// <param name="name">The name of the list</param>
        /// <param name="description">The description of the list</param>
        /// <param name="listTemplate">The desired list template to use to instantiate the list</param>
        /// <returns>The new list or the existing list</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public SPList EnsureList(SPWeb web, string name, string description, SPListTemplate listTemplate)
        {
            var list = this.listLocator.TryGetList(web, name);

            if (list != null)
            {
                // List already exists, check for correct template
                if (list.BaseTemplate != listTemplate.Type)
                {
                    throw new SPException(string.Format(CultureInfo.InvariantCulture, "List {0} has list template type {1} but should have list template type {2}.", name, list.BaseTemplate, listTemplate.Type));
                }
            }
            else
            {
                // Create new list
                var id = web.Lists.Add(name, description, listTemplate);

                list = web.Lists[id];
            }

            return list;
        }

        /// <summary>
        /// Creates the list or returns the existing one.
        /// </summary>
        /// <remarks>The list name and description will not be translated</remarks>
        /// <exception cref="SPException">If the list already exists but doesn't have the specified list template.</exception>
        /// <param name="web">The current web</param>
        /// <param name="name">The name of the list</param>
        /// <param name="description">The description of the list</param>
        /// <param name="templateType">The desired list template type to use to instantiate the list</param>
        /// <returns>The new list or the existing list</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public SPList EnsureList(SPWeb web, string name, string description, SPListTemplateType templateType)
        {
            var list = this.listLocator.TryGetList(web, name);

            if (list != null)
            {
                // List already exists, check for correct template
                if (list.BaseTemplate != templateType)
                {
                    throw new SPException(string.Format(CultureInfo.InvariantCulture, "List {0} has list template type {1} but should have list template type {2}.", name, list.BaseTemplate, templateType));
                }
            }
            else
            {
                // Create new list
                var id = web.Lists.Add(name, description, templateType);

                list = web.Lists[id];
            }

            return list;
        }

        /// <summary>
        /// Ensure the list in the web
        /// </summary>
        /// <param name="web">The web</param>
        /// <param name="webRelativePathToList">The web-relative path to the list's root folder.</param>
        /// <param name="titleResourceKey">Titles' resource key</param>
        /// <param name="descriptionResourceKey">Descriptions' resource key</param>
        /// <param name="templateType">The template type of the list</param>
        /// <returns>The list object</returns>
        public SPList EnsureList(SPWeb web, string webRelativePathToList, string titleResourceKey, string descriptionResourceKey, SPListTemplateType templateType)
        {
            var list = this.listLocator.TryGetList(web, webRelativePathToList);

            if (list != null)
            {
                // List already exists, check for correct template
                if (list.BaseTemplate != templateType)
                {
                    throw new SPException(string.Format(CultureInfo.InvariantCulture, "List with root folder url {0} has list template type {1} but should have list template type {2}.", webRelativePathToList, list.BaseTemplate, templateType));
                }
            }
            else
            {
                // Create new list
                var id = web.Lists.Add(webRelativePathToList, string.Empty, templateType);
                list = web.Lists[id];
            }

            // Update title and description
            // Note that the variations synchronization process for a list doesn't copy the resources settings in the target sites
            var availableLanguages = web.SupportedUICultures.Reverse();   // end with the main language
            foreach (var availableLanguage in availableLanguages)
            {
                var title = this.resourceLocator.Find(titleResourceKey, availableLanguage.LCID);
                var description = this.resourceLocator.Find(descriptionResourceKey, availableLanguage.LCID);

                list.TitleResource.SetValueForUICulture(availableLanguage, title);
                list.DescriptionResource.SetValueForUICulture(availableLanguage, description);
            }

            list.Update();

            return list;
        }

        /// <summary>
        /// Creates the list or returns the existing one.
        /// </summary>
        /// <remarks>The list name and description will not be translated</remarks>
        /// <exception cref="SPException">If the list already exists but doesn't have the specified list template.</exception>
        /// <param name="web">The current web</param>
        /// <param name="listInfo">The list to create</param>
        /// <returns>The new list or the existing list</returns>
        public SPList EnsureList(SPWeb web, ListInfo listInfo)
        {
            var list = this.listLocator.TryGetList(web, listInfo.WebRelativeUrl.ToString());

            // Ensure the list
            if (list == null)
            {
                list = this.EnsureList(web, listInfo.WebRelativeUrl.ToString(), listInfo.DisplayNameResourceKey, listInfo.DescriptionResourceKey, listInfo.ListTemplate);
            }
            else
            {
                // If it isn't the pages library
                if (string.CompareOrdinal(list.RootFolder.Name, PagesLibraryRootFolder) != 0)
                {                            
                    this.logger.Info("List " + listInfo.WebRelativeUrl.ToString() + " already exists");

                    // If the Overwrite parameter is set to true, celete and recreate the catalog
                    if (listInfo.Overwrite)
                    {
                        this.logger.Info("Overwrite is set to true, recreating the list " + listInfo.WebRelativeUrl.ToString());

                        list.Delete();

                        // TODO: review ListInfo.WebRelativeUrl. RootFolderUrl used to be its name. Is it assumed that all lists live under /Lists/? Is it assumed that 
                        // _no ListInfo EVER_ will ever ever be created with a WebRelativeUrl value of /Lists/SomeListName ??? This needs more review...
                        list = this.EnsureList(web, listInfo.WebRelativeUrl.ToString(), listInfo.DisplayNameResourceKey, listInfo.DescriptionResourceKey, listInfo.ListTemplate);
                    }
                    else
                    {
                        // Get the existing list
                        list = this.EnsureList(web, listInfo.WebRelativeUrl.ToString(), listInfo.DisplayNameResourceKey, listInfo.DescriptionResourceKey, listInfo.ListTemplate);
                    }
                }
            }

            // Remove Item Content Type
            if (listInfo.RemoveDefaultContentType)
            {
                this.logger.Info("Removing the default Item Content Type");

                // If content type is direct child of item, remove it
                this.RemoveItemContentType(list);
            }

            // Add All Content Types
            this.contentTypeBuilder.EnsureContentType(list.ContentTypes, listInfo.ContentTypes);

            // Draft VisibilityType
            if (listInfo.HasDraftVisibilityType)
            {
                list.EnableModeration = true;
                list.DraftVersionVisibility = listInfo.DraftVisibilityType;

                list.Update();
            }

            // Ratings
            this.SetRatings(list, listInfo.RatingType, listInfo.EnableRatings);

            // Set list Write Security
            this.SetWriteSecurity(list, listInfo.WriteSecurity);

            // Quick Launch Navigation
            if (listInfo.AddToQuickLaunch)
            {
                this.AddtoQuickLaunch(list);
            }

            // Attachements
            if (!listInfo.EnableAttachements)
            {
                list.EnableAttachments = listInfo.EnableAttachements;
                list.Update();
            }

            // Get the updated list object because we have to reference previous added fields that the old list object didn't have (cause NullReferenceException).    
            list = this.listLocator.TryGetList(web, listInfo.WebRelativeUrl.ToString());

            // Default View Fields
            this.AddFieldsToDefaultView(list, listInfo.DefaultViewFields);

            // Ensure the field definitions to make sure that all fields are present and to override/apply column default Values
            this.fieldHelper.EnsureField(list.Fields, listInfo.FieldDefinitions);

            return list;
        }

        /// <summary>
        /// Ensure a list of lists in the web
        /// </summary>
        /// <param name="web">The web</param>
        /// <param name="listInfos">The list information</param>
        /// <returns>List of lists</returns>
        public IEnumerable<SPList> EnsureList(SPWeb web, ICollection<ListInfo> listInfos)
        {
            var lists = new List<SPList>();

            foreach (ListInfo list in listInfos)
            {
                lists.Add(this.EnsureList(web, list));
            }

            return lists;
        }

        /// <summary>
        /// Enable or disable ratings on a SPList
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="ratingType">The rating type. Can be "Likes" or "Ratings" </param>
        /// <param name="ratingStatus">True to enable. False to disable.</param>
        public void SetRatings(SPList list, string ratingType, bool ratingStatus)
        {
            // Retrieve assembly from a puplib class
            Assembly assembly = typeof(Microsoft.SharePoint.Portal.RatingsSettingsPage).Assembly;

            // Get ReputationHelper type
            Type reputationHelper = assembly.GetType("Microsoft.SharePoint.Portal.ReputationHelper");

            MethodInfo enableMethod = reputationHelper.GetMethod("EnableReputation", BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo disableMethod = reputationHelper.GetMethod("DisableReputation", BindingFlags.Static | BindingFlags.NonPublic);

            if (ratingStatus && !string.IsNullOrEmpty(ratingType))
            {
                enableMethod.Invoke(null, new object[] { list, ratingType, false });
            }
            else
            {
                disableMethod.Invoke(null, new object[] { list });
            }

            list.Update();
        }

        /// <summary>
        /// Add the list to the quick launch bar
        /// </summary>
        /// <param name="list">The list</param>
        public void AddtoQuickLaunch(SPList list)
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

        /// <summary>
        ///  Set WriteSecurity on a SPList
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="writeSecurityOptions">The Write Security option</param>
        public void SetWriteSecurity(SPList list, WriteSecurityOptions writeSecurityOptions)
        {
            list.WriteSecurity = (int)writeSecurityOptions;
            list.Update();
        }

        /// <summary>
        /// Enforce the unique value(s) for a list field. In case the field is reused in the site collection, we can make that change on the list scope.
        /// </summary>
        /// <param name="list">The list who owns the field</param>
        /// <param name="field">The field to enforce</param>
        public void EnforceUniqueValuesToField(SPList list, IFieldInfo field)
        {
            if (list != null && field != null)
            {
                var listField = this.fieldLocator.GetFieldById(list.Fields, field.Id);

                if (listField != null)
                {
                    listField.EnforceUniqueValues = true;
                    listField.Indexed = true;
                    listField.Update();
                }
            }
        }

        /// <summary>
        /// Method to remove the Item Content Type from the List
        /// </summary>
        /// <param name="list">The current List</param>
        public void RemoveItemContentType(SPList list)
        {
            // If content type is direct child of item, remove it
            var bestMatchItem = list.ContentTypes.BestMatch(SPBuiltInContentTypeId.Item);
            if (bestMatchItem.Parent == SPBuiltInContentTypeId.Item)
            {
                list.ContentTypes.Delete(bestMatchItem);
            }
        }

        #region List View

        /// <summary>
        /// Add fields in the default view of the list
        /// </summary>
        /// <param name="list">the current list</param>
        /// <param name="fields">the collection of fields</param>
        public void AddFieldsToDefaultView(SPList list, ICollection<IFieldInfo> fields)
        {
            this.AddFieldsToDefaultView(list, fields, false);
        }

        /// <summary>
        /// Add fields in the default view of the list
        /// </summary>
        /// <param name="list">the current list</param>
        /// <param name="fields">the collection of fields</param>
        /// <param name="removeExistingViewFields">if set to <c>true</c> [remove existing view fields].</param>
        public void AddFieldsToDefaultView(SPList list, ICollection<IFieldInfo> fields, bool removeExistingViewFields)
        {
            var defaultView = list.DefaultView;
            var fieldCollection = defaultView.ViewFields;

            // Remove default view fields
            if (removeExistingViewFields)
            {
                fieldCollection.DeleteAll();
            }

            foreach (IFieldInfo field in fields)
            {
                if (list.Fields.Contains(field.Id))
                {
                    this.EnsureFieldInView(fieldCollection, list.Fields[field.Id].InternalName);
                }
                else if (list.Fields.ContainsFieldWithStaticName(field.InternalName))
                {
                    this.EnsureFieldInView(fieldCollection, list.Fields.GetFieldByInternalName(field.InternalName).InternalName);
                }
                else
                {
                    if (list.Fields.Contains(field.Id))
                    {
                        this.EnsureFieldInView(fieldCollection, list.Fields[field.Id].InternalName);
                    }
                    else
                    {
                        this.logger.Warn("Field with ID {0} was not found in list '{1}' fields", field.Id, list.Title);
                    }
                }

                defaultView.Update();
            }
        }

        /// <summary>
        /// Ensure the field in the view
        /// </summary>
        /// <param name="fieldCollection">the collection of fields</param>
        /// <param name="fieldInternalName">the current field internal name</param>
        public void EnsureFieldInView(SPViewFieldCollection fieldCollection, string fieldInternalName)
        {
            if (!string.IsNullOrEmpty(fieldInternalName))
            {
                if (!fieldCollection.Exists(fieldInternalName))
                {
                    fieldCollection.Add(fieldInternalName);
                }
            }
        }
        #endregion
    }
}
