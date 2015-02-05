using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using GSoft.Dynamite.Binding;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.Globalization;
using GSoft.Dynamite.Globalization.Variations;
using GSoft.Dynamite.Logging;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing;

namespace GSoft.Dynamite.ContentTypes
{
    /// <summary>
    /// Helper class for managing content types.
    /// </summary>
    public class ContentTypeHelper : IContentTypeHelper
    {
        private readonly IVariationHelper variationHelper;
        private readonly IFieldHelper fieldHelper;
        private readonly IResourceLocator resourceLocator;
        private readonly ILogger log;

        /// <summary>
        /// Initializes a new <see cref="ContentTypeHelper"/> instance
        /// </summary>
        /// <param name="variationHelper">Variations helper</param>
        /// <param name="fieldHelper">Field helper</param>
        /// <param name="resourceLocator">The resource locator</param>
        /// <param name="log">Logging utility</param>
        public ContentTypeHelper(IVariationHelper variationHelper, IFieldHelper fieldHelper, IResourceLocator resourceLocator, ILogger log)
        {
            this.variationHelper = variationHelper;
            this.fieldHelper = fieldHelper;
            this.resourceLocator = resourceLocator;
            this.log = log;
        }

        /// <summary>
        /// Ensure the content type based on its content type info. 
        /// Sets the description and Groups resource, adds the fields and calls update.
        /// </summary>
        /// <param name="contentTypeCollection">The content type collection.</param>
        /// <param name="contentTypeInfo">The content type information.</param>
        /// <returns>
        /// The created and configured content type.
        /// </returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public SPContentType EnsureContentType(SPContentTypeCollection contentTypeCollection, ContentTypeInfo contentTypeInfo)
        {
            var contentType = this.InnerEnsureContentType(contentTypeCollection, contentTypeInfo);

            SPList list;
            if (!TryGetListFromContentTypeCollection(contentTypeCollection, out list))
            {
                // Set the content type title, description, and group information for each language.
                // Only do this when not on a web because the SPContentType Title property does not support resource values at this level.
                // The content type for a list is created at the root web level, then added to the list.
                this.SetTitleDescriptionAndGroupValues(contentTypeInfo, contentType);
            }

            return contentType;
        }

        /// <summary>
        /// Ensure a list of content type
        /// </summary>
        /// <param name="contentTypeCollection">The content type collection</param>
        /// <param name="contentTypeInfos">The content types information</param>
        /// <returns>The content types list</returns>
        public IEnumerable<SPContentType> EnsureContentType(SPContentTypeCollection contentTypeCollection, ICollection<ContentTypeInfo> contentTypeInfos)
        {
            var contentTypes = new List<SPContentType>();

            foreach (ContentTypeInfo contentType in contentTypeInfos)
            {
                contentTypes.Add(this.EnsureContentType(contentTypeCollection, contentType));
            }

            return contentTypes;
        }

        /// <summary>
        /// Deletes the content type if it has no SPContentTypeUsages.
        /// If it does, the content type will be deleted from the usages that are lists where it has no items.
        /// </summary>
        /// <param name="contentType">The content type.</param>
        /// <exception cref="System.ArgumentNullException">For any null parameter.</exception>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public void DeleteContentTypeIfNotUsed(SPContentType contentType)
        {
            // Find where the content type is being used.
            ICollection<SPContentTypeUsage> usages = SPContentTypeUsage.GetUsages(contentType);
            if (usages.Count <= 0)
            {
                // Delete unused content type.
                contentType.ParentWeb.ContentTypes.Delete(contentType.Id);
            }
            else
            {
                // Prepare the query to get all items in a list that uses the content type.
                SPQuery query = new SPQuery()
                {
                    Query = string.Concat(
                            "<Where><Eq>",
                                "<FieldRef Name='ContentTypeId'/>",
                                string.Format(CultureInfo.InvariantCulture, "<Value Type='Text'>{0}</Value>", contentType.Id),
                            "</Eq></Where>")
                };

                // Get the usages that are in a list.
                List<SPContentTypeUsage> listUsages = (from u in usages where u.IsUrlToList select u).ToList();
                foreach (SPContentTypeUsage usage in listUsages)
                {
                    // For a list usage, we get all the items in the list that use the content type.
                    SPList list = contentType.ParentWeb.GetList(usage.Url);
                    SPListItemCollection listItems = list.GetItems(query);

                    // if no items are found...
                    if (listItems.Count <= 0)
                    {
                        // Delete unused content type.
                        list.ContentTypes.Delete(contentType.Id);
                    }
                }
            }
        }

        /// <summary>
        /// Reorders fields in the content type according to index position.
        /// </summary>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="orderedFields">A collection of indexes (0 based) and their corresponding field information.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public void ReorderFieldsInContentType(SPContentType contentType, ICollection<IFieldInfo> orderedFields)
        {
            var fieldInternalNames = contentType.FieldLinks.Cast<SPFieldLink>().Where(x => !x.Hidden).Select(x => x.Name).ToList();

            foreach (var orderedField in orderedFields)
            {
                fieldInternalNames.Remove(orderedField.InternalName);
            }

            var orderedFieldsArray = orderedFields.ToArray();
            for (var i = 0; i < orderedFieldsArray.Length; i++)
            {
                fieldInternalNames.Insert(i, orderedFieldsArray[i].InternalName);
            }

            contentType.FieldLinks.Reorder(fieldInternalNames.ToArray());
            contentType.Update();
        }

        #region Private methods

        private SPContentType InnerEnsureContentType(SPContentTypeCollection contentTypeCollection, ContentTypeInfo contentTypeInfo)
        {
            if (contentTypeCollection == null)
            {
                throw new ArgumentNullException("contentTypeCollection");
            }

            SPContentTypeId contentTypeId = new SPContentTypeId(contentTypeInfo.ContentTypeId);
            SPList list = null;

            var contentTypeResourceTitle = this.resourceLocator.GetResourceString(contentTypeInfo.ResourceFileName, contentTypeInfo.DisplayNameResourceKey);

            if (TryGetListFromContentTypeCollection(contentTypeCollection, out list))
            {
                // Make sure its not already in the list.
                var contentTypeInList = list.ContentTypes.Cast<SPContentType>().FirstOrDefault(ct => ct.Id == contentTypeId || ct.Parent.Id == contentTypeId);
                if (contentTypeInList == null)
                {
                    // Can we add the content type to the list?
                    if (list.IsContentTypeAllowed(contentTypeId))
                    {
                        // Enable content types if not yet done.
                        if (!list.ContentTypesEnabled)
                        {
                            list.ContentTypesEnabled = true;
                            list.Update(true);
                        }

                        // Try to use the list's web's content type if it already exists
                        var contentTypeInWeb = list.ParentWeb.Site.RootWeb.AvailableContentTypes[contentTypeId];

                        if (contentTypeInWeb == null)
                        {
                            // By convention, content types should always exist on root web as site-collection-wide
                            // content types before they get linked on a specific list.
                            var rootWebContentTypeCollection = list.ParentWeb.Site.RootWeb.ContentTypes;
                            contentTypeInWeb = this.EnsureContentType(rootWebContentTypeCollection, contentTypeInfo);

                            this.log.Warn(
                                "EnsureContentType - Forced the creation of Content Type (name={0} ctid={1}) on the root web (url=) instead of adding the CT directly on the list (id={2} title={3}). By convention, all CTs should be provisonned on RootWeb before being re-used in lists.",
                                contentTypeInWeb.Name,
                                contentTypeInWeb.Id.ToString(),
                                list.ID,
                                list.Title);
                        }

                        // Add the web content type to the collection.
                        return list.ContentTypes.Add(contentTypeInWeb);
                    }
                }
                else
                {
                    this.InnerEnsureFieldInContentType(contentTypeInList, contentTypeInfo.Fields);

                    return contentTypeInList;
                }
            }
            else
            {
                SPWeb web = null;
                if (TryGetWebFromContentTypeCollection(contentTypeCollection, out web))
                {
                    // Make sure its not already in ther web.
                    var contentTypeInWeb = web.ContentTypes[contentTypeId];
                    if (contentTypeInWeb == null)
                    {
                        SPContentTypeCollection rootWebContentTypeCollection = null;

                        if (web.ID == web.Site.RootWeb.ID)
                        {
                            rootWebContentTypeCollection = contentTypeCollection;
                        }
                        else
                        {
                            rootWebContentTypeCollection = web.Site.RootWeb.ContentTypes;

                            this.log.Warn(
                                "EnsureContentType - Will force creation of content type (id={0} name={1}) on root web instead of on specified sub-web. This is to enforce the following convention: all CTs should be provisioned at root of site collection, to ease maintenance. Ensure your content types on the root web's SPContentTypeCollection to avoid this warning.",
                                contentTypeId.ToString(),
                                contentTypeInfo.DisplayNameResourceKey);
                        }

                        var contentTypeInRootWeb = rootWebContentTypeCollection[contentTypeId];

                        if (contentTypeInRootWeb == null)
                        {
                            // Add the content type to the Root Web collection. By convention, we avoid provisioning
                            // CTs directly on sub-webs to make CT management easier (i.e. all of your site collection's
                            // content types should be configured at the root of the site collection).
                            var newWebContentType = new SPContentType(contentTypeId, rootWebContentTypeCollection, contentTypeResourceTitle);
                            contentTypeInRootWeb = rootWebContentTypeCollection.Add(newWebContentType);
                        }

                        this.InnerEnsureFieldInContentType(contentTypeInRootWeb, contentTypeInfo.Fields);

                        return contentTypeInRootWeb;
                    }
                    else
                    {
                        this.InnerEnsureFieldInContentType(contentTypeInWeb, contentTypeInfo.Fields);
                        return contentTypeInWeb;
                    }
                }

                // Case if there is no Content Types in the Web (e.g single SPWeb)
                var returnedContentType = this.EnsureContentType(contentTypeCollection, contentTypeInfo);
                return returnedContentType;
            }

            return null;
        }

        /// <summary>
        /// Ensures the SPFields are in the content type. If not, they will be added and the content type updated.
        /// </summary>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="fieldInfos">The field information.</param>
        /// <returns>IEnumerable of SPFields that where found.</returns>
        private IEnumerable<SPField> InnerEnsureFieldInContentType(SPContentType contentType, ICollection<IFieldInfo> fieldInfos)
        {
            bool fieldWasAdded = false;
            List<SPField> fields = new List<SPField>();

            // For each field we want to add.
            foreach (IFieldInfo fieldInfo in fieldInfos)
            {
                // We get the field from AvailableFields because we don't need to modify the field.
                SPField field = contentType.ParentWeb.AvailableFields.Cast<SPField>().SingleOrDefault(f => f.Id == fieldInfo.Id);
                if (field == null)
                {
                    // Site column not provisionned yet, gotta add that column to root web first (by convention)
                    field = this.fieldHelper.EnsureField(contentType.ParentWeb.Site.RootWeb.Fields, fieldInfo);

                    this.log.Warn(
                        "EnsureContentType - Forced creation of missing site column (fieldId={0}, fieldName={1}) on root web of the site collection, by convention. To avoid this warning, first provision your site columns with FieldHelper, then secondly use those site columns in your content type.",
                        fieldInfo.Id,
                        fieldInfo.InternalName);
                }

                // We add it to the list of fields we got.
                fields.Add(field);

                // Then we add it to the content type without updating the content type.
                if (AddFieldToContentType(contentType, field, false, fieldInfo.Required))
                {
                    fieldWasAdded = true;
                }
            }

            if (fieldWasAdded)
            {
                // When One or more fields are added to the content type, we update the content type.
                try
                {
                    // Gotta make sure the update goes through to children CTs.
                    // However, if not child CT exists, this will throw a SPException.
                    contentType.Update(true);
                }
                catch (SPException maybeNoChildrenException)
                {
                    if (maybeNoChildrenException.Message.Contains("The content type has no children"))
                    {
                        // attempt a single no-children update instead
                        contentType.Update();
                    }
                    else
                    {
                        // not the SPException we're familiar with - better let it bubble up
                        throw;
                    }
                }
            }

            return fields;
        }

        private static bool AddFieldToContentType(SPContentType contentType, SPField field, bool updateContentType, RequiredType isRequired)
        {
            // Create the field ref.
            SPFieldLink fieldOneLink = new SPFieldLink(field);
            if (contentType.FieldLinks[fieldOneLink.Id] == null)
            {
                // Set the RequiredType value on the Content Type
                switch (isRequired)
                {
                    case RequiredType.Required:
                        fieldOneLink.Required = true;
                        break;
                    case RequiredType.NotRequired:
                        fieldOneLink.Required = false;
                        break;
                    case RequiredType.Inherit:
                    default:
                        // Do nothing, it will inherit from the Field definition
                        break;
                }

                // Field is not in the content type so we add it.
                contentType.FieldLinks.Add(fieldOneLink);

                // Update the content type.
                if (updateContentType)
                {
                    contentType.Update(true);
                }

                return true;
            }

            return false;
        }

        private void SetTitleDescriptionAndGroupValues(ContentTypeInfo contentTypeInfo, SPContentType contentType)
        {
            //// Get a list of the available languages and end with the main language
            var web = contentType.ParentWeb;
            var availableLanguages = web.SupportedUICultures.Reverse().ToList();

            // If it's a publishing web, add the variation labels as available languages
            if (PublishingWeb.IsPublishingWeb(web) && this.variationHelper.IsVariationsEnabled(web.Site))
            {
                var labels = this.variationHelper.GetVariationLabels(web.Site);
                if (labels.Count > 0)
                {
                    // Predicate to check if the web contains the label language in it's available languages
                    Func<VariationLabel, bool> notAvailableWebLanguageFunc = (label) =>
                        !availableLanguages.Any(lang => lang.Name.Equals(label.Language, StringComparison.InvariantCultureIgnoreCase));

                    // Get the label languages that aren't already in the web's available languages
                    var labelLanguages = labels
                        .Where(notAvailableWebLanguageFunc)
                        .Select(label => new CultureInfo(label.Language));

                    availableLanguages.AddRange(labelLanguages);
                }
            }

            // If multiple languages are enabled, since we have a full ContentTypeInfo object, we want to populate 
            // all alternate language labels for the Content Type
            foreach (CultureInfo availableLanguage in availableLanguages)
            {
                var previousUiCulture = Thread.CurrentThread.CurrentUICulture;
                Thread.CurrentThread.CurrentUICulture = availableLanguage;

                contentType.Name = this.resourceLocator.GetResourceString(contentTypeInfo.ResourceFileName, contentTypeInfo.DisplayNameResourceKey);

                contentType.Description = this.resourceLocator.GetResourceString(contentTypeInfo.ResourceFileName, contentTypeInfo.DescriptionResourceKey);
                contentType.Description = this.resourceLocator.Find(contentTypeInfo.ResourceFileName, contentTypeInfo.DescriptionResourceKey, availableLanguage.LCID);

                contentType.Group = this.resourceLocator.GetResourceString(contentTypeInfo.ResourceFileName, contentTypeInfo.GroupResourceKey);

                Thread.CurrentThread.CurrentUICulture = previousUiCulture;
            }

            contentType.Update();
        }

        private static bool TryGetListFromContentTypeCollection(SPContentTypeCollection collection, out SPList list)
        {
            if (collection.Count > 0)
            {
                SPContentType first = collection[0];
                if (first != null)
                {
                    if (first.ParentList != null)
                    {
                        list = first.ParentList;
                        return true;
                    }
                }
            }

            list = null;
            return false;
        }

        private static bool TryGetWebFromContentTypeCollection(SPContentTypeCollection collection, out SPWeb web)
        {
            if (collection.Count > 0)
            {
                SPContentType first = collection[0];
                if (first != null)
                {
                    if (first.ParentWeb != null)
                    {
                        web = first.ParentWeb;
                        return true;
                    }
                }
            }

            web = null;
            return false;
        }
        #endregion
    }
}
