using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using GSoft.Dynamite.Binding;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.Globalization.Variations;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing;

namespace GSoft.Dynamite.ContentTypes
{
    /// <summary>
    /// Helper class for managing content types.
    /// </summary>
    public class ContentTypeHelper : IContentTypeHelper
    {
        private readonly IVariationHelper _variationHelper;

        /// <summary>
        /// Initializes a new <see cref="ContentTypeHelper"/> instance
        /// </summary>
        /// <param name="variationHelper">Variations helper</param>
        public ContentTypeHelper(IVariationHelper variationHelper)
        {
            this._variationHelper = variationHelper;
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
            SPContentType contentType = this.EnsureContentType(
                contentTypeCollection,
                new SPContentTypeId(contentTypeInfo.ContentTypeId),
                contentTypeInfo.DisplayName);

            this.EnsureFieldInContentType(contentType, contentTypeInfo.Fields);

            var web = contentType.ParentWeb;

            var availableLanguages = new List<CultureInfo>();

            var pubWeb = PublishingWeb.GetPublishingWeb(web);

            if (pubWeb != null)
            {
                var labels = this._variationHelper.GetVariationLabels(pubWeb.Web.Site);
                availableLanguages.AddRange(labels.Select(label => new CultureInfo(label.Language)));

                if (availableLanguages.Count == 0)
                {
                    availableLanguages = pubWeb.Web.SupportedUICultures.Reverse().ToList();
                }
            }
            else
            {
                availableLanguages = web.SupportedUICultures.Reverse().ToList();   // end with the main language
            }

            foreach (var availableLanguage in availableLanguages)
            {
                var currentCulture = CultureInfo.CurrentUICulture;

                // make sure the ResourceLocator will fetch the correct culture's DisplayName value
                Thread.CurrentThread.CurrentUICulture = availableLanguage;
                contentType.Name = contentTypeInfo.DisplayName;
                contentType.Description = contentTypeInfo.Description;
                contentType.Group = contentTypeInfo.Group;

                // restore the MUI culture to the old value
                Thread.CurrentThread.CurrentUICulture = currentCulture;
            }

            contentType.Update();

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
        /// Ensures the SPContentType is in the collection. If not, it will be created and added.
        /// </summary>
        /// <param name="contentTypeCollection">The content type collection.</param>
        /// <param name="contentTypeId">The content type id.</param>
        /// <param name="contentTypeName">Name of the content type.</param>
        /// <returns><c>True</c> if it was added, else <c>False</c>.</returns>
        /// <exception cref="System.ArgumentNullException">For any null parameter.</exception>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public SPContentType EnsureContentType(SPContentTypeCollection contentTypeCollection, SPContentTypeId contentTypeId, string contentTypeName)
        {
            if (contentTypeCollection == null)
            {
                throw new ArgumentNullException("contentTypeCollection");
            }

            if (contentTypeId == null)
            {
                throw new ArgumentNullException("contentTypeId");
            }

            if (string.IsNullOrEmpty(contentTypeName))
            {
                throw new ArgumentNullException("contentTypeName");
            }

            SPList list = null;

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
                        var contentTypeInWeb = list.ParentWeb.AvailableContentTypes[contentTypeId];

                        if (contentTypeInWeb != null)
                        {
                            // Add the web content type to the collection.
                            return list.ContentTypes.Add(contentTypeInWeb);
                        }
                        else
                        {
                            // Create the content type directly on the list
                            var newListContentType = new SPContentType(contentTypeId, contentTypeCollection, contentTypeName);
                            var returnedListContentType = list.ContentTypes.Add(newListContentType);
                            return returnedListContentType;
                        }
                    }
                }
                else
                {
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
                        // Add the content type to the collection.
                        var newWebContentType = new SPContentType(contentTypeId, contentTypeCollection, contentTypeName);
                        var returnedWebContentType = contentTypeCollection.Add(newWebContentType);
                        return returnedWebContentType;
                    }
                    else
                    {
                        return contentTypeInWeb;
                    }
                }

                // Case if there is no Content Types in the Web (e.g single SPWeb)
                var newContentType = new SPContentType(contentTypeId, contentTypeCollection, contentTypeName);
                var returnedContentType = contentTypeCollection.Add(newContentType);
                return returnedContentType;
            }

            return null;
        }

        /// <summary>
        /// Ensure a single content in a collection
        /// </summary>
        /// <param name="collection">The content type collection</param>
        /// <param name="contentType">The content type info</param>
        /// <returns>The content type object</returns>
        public SPContentType EnsureContentType(SPContentTypeCollection collection, SPContentType contentType)
        {
            return this.EnsureContentType(collection, contentType.Id, contentType.Name);
        }

        /// <summary>
        /// Deletes the content type if not used.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="contentTypeId">The content type id.</param>
        /// <exception cref="System.ArgumentNullException">For any null parameter.</exception>
        /// <exception cref="Microsoft.SharePoint.SPContentTypeReadOnlyException">If the contentype is readonly.</exception>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public void DeleteContentTypeIfNotUsed(SPContentTypeCollection collection, SPContentTypeId contentTypeId)
        {
            if (contentTypeId == null)
            {
                throw new ArgumentNullException("contentTypeId");
            }

            if (contentTypeId == null)
            {
                throw new ArgumentNullException("contentTypeId");
            }

            // Get the content type from the web.
            SPContentType contentType = collection[collection.BestMatch(contentTypeId)];

            // return false if the content type does not exist.
            if (contentType != null)
            {
                // Delete the content type if not used.
                this.DeleteContentTypeIfNotUsed(contentType);
            }
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
        /// Ensures the SPField is in the content type. If not, it will be added and the content type updated.
        /// </summary>
        /// <param name="contentType">Type content type.</param>
        /// <param name="fieldInfo">The field info.</param>
        /// <returns>Null if the field does not exist, else the field is returned.</returns>
        /// <exception cref="System.ArgumentNullException">For any null parameter.</exception>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public SPField EnsureFieldInContentType(SPContentType contentType, IFieldInfo fieldInfo)
        {
            if (contentType == null)
            {
                throw new ArgumentNullException("contentType");
            }

            if (fieldInfo == null)
            {
                throw new ArgumentNullException("fieldInfo");
            }

            // Get the SPWeb from the contentType
            SPWeb web = contentType.ParentWeb;

            // We get from AvailableFields because we don't need to modify the field.
            SPField field = web.AvailableFields[fieldInfo.Id];

            if (field != null)
            {
                // Add the field to the content type and its children.
                AddFieldToContentType(contentType, field, true, fieldInfo.Required);
            }

            return field;
        }

        /// <summary>
        /// Ensures the SPFields are in the content type. If not, they will be added and the content type updated.
        /// </summary>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="fieldInfos">The field information.</param>
        /// <returns>IEnumerable of SPFields that where found.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public IEnumerable<SPField> EnsureFieldInContentType(SPContentType contentType, ICollection<IFieldInfo> fieldInfos)
        {
            bool fieldWasAdded = false;
            List<SPField> fields = new List<SPField>();

            // For each field we want to add.
            foreach (IFieldInfo fieldInfo in fieldInfos)
            {
                // We get the field from AvailableFields because we don't need to modify the field.
                SPField field = contentType.ParentWeb.AvailableFields[fieldInfo.Id];
                if (field != null)
                {
                    // We add it to the list of fields we got.
                    fields.Add(field);

                    // Then we add it to the content type without updating the content type.
                    if (AddFieldToContentType(contentType, field, false, fieldInfo.Required))
                    {
                        fieldWasAdded = true;
                    }
                }
            }

            if (fieldWasAdded)
            {
                // When One or more fields are added to the content type, we update the content type.
                contentType.Update(true);
            }

            return fields;
        }

        /// <summary>
        /// Adds the event receiver definition to the content type.
        /// </summary>
        /// <param name="contentType">The content type.</param>
        /// <param name="type">The receiver type.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="className">Name of the class.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public void AddEventReceiverDefinition(SPContentType contentType, SPEventReceiverType type, string assemblyName, string className)
        {
            var classType = Type.GetType(string.Format(CultureInfo.InvariantCulture, "{0}, {1}", className, assemblyName));
            if (classType != null)
            {
                var assembly = Assembly.GetAssembly(classType);
                this.AddEventReceiverDefinition(contentType, type, assembly, className);
            }
        }

        /// <summary>
        /// Adds the event receiver definition to the content type.
        /// </summary>
        /// <param name="contentType">The content type.</param>
        /// <param name="type">The receiver type.</param>
        /// <param name="assembly">The assembly.</param>
        /// <param name="className">Name of the class.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public void AddEventReceiverDefinition(SPContentType contentType, SPEventReceiverType type, Assembly assembly, string className)
        {
            var isAlreadyDefined = contentType.EventReceivers.Cast<SPEventReceiverDefinition>()
                .Any(x => (x.Class == className) && (x.Type == type));

            // If definition isn't already defined, add it to the content type
            if (!isAlreadyDefined)
            {
                var eventReceiverDefinition = contentType.EventReceivers.Add();
                eventReceiverDefinition.Type = type;
                eventReceiverDefinition.Assembly = assembly.FullName;
                eventReceiverDefinition.Class = className;
                eventReceiverDefinition.Update();
                contentType.Update(true);
            }
        }

        /// <summary>
        /// Remove the event receiver definition for the content type.
        /// </summary>
        /// <param name="contentType">The content type.</param>
        /// <param name="type">The receiver type.</param>
        /// <param name="className">Name of the class.</param>
        public void DeleteEventReceiverDefinition(SPContentType contentType, SPEventReceiverType type, string className)
        {
            var eventReceiverDefinition = contentType.EventReceivers.Cast<SPEventReceiverDefinition>().FirstOrDefault(x => (x.Class == className) && (x.Type == type));

            // If definition isn't already defined, add it to the content type
            if (eventReceiverDefinition != null)
            {
                var eventToDelete = contentType.EventReceivers.Cast<SPEventReceiverDefinition>().Where(eventReceiver => eventReceiver.Type == eventReceiverDefinition.Type).ToList();

                eventToDelete.ForEach(c => c.Delete());
                
                contentType.Update(true);
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
        private static bool AddFieldToContentType(SPContentType contentType, SPField field, bool updateContentType, RequiredTypes isRequired)
        {
            // Create the field ref.
            SPFieldLink fieldOneLink = new SPFieldLink(field);
            if (contentType.FieldLinks[fieldOneLink.Id] == null)
            {
                // Set the RequiredType value on the Content Type
                switch (isRequired)
                {
                    case RequiredTypes.Required:
                        fieldOneLink.Required = true;
                        break;
                    case RequiredTypes.NotRequired:
                        fieldOneLink.Required = false;
                        break;
                    case RequiredTypes.Inherit:
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
