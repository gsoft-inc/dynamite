using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Microsoft.SharePoint;
using System.IO;
using Microsoft.SharePoint.Utilities;
using GSoft.Dynamite.Definitions;
using GSoft.Dynamite.Globalization;

namespace GSoft.Dynamite.Lists
{
    /// <summary>
    /// Helper class to manage lists.
    /// </summary>
    public class ListHelper
    {
        private ContentTypeBuilder contentTypeBuilder;
        private IResourceLocator resourceLocator;

        /// <summary>
        /// Creates a list helper
        /// </summary>
        /// <param name="contentTypeBuilder">A content type helper</param>
        public ListHelper(ContentTypeBuilder contentTypeBuilder, IResourceLocator resourceLocator)
        {
            this.contentTypeBuilder = contentTypeBuilder;
            this.resourceLocator = resourceLocator;
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
        /// <param name="template">The desired list template to use to instantiate the list</param>
        /// <returns>The new list or the existing list</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public SPList EnsureList(SPWeb web, string name, string description, SPListTemplate template)
        {
            var list = this.TryGetList(web, name);

            if (list != null)
            {
                // List already exists, check for correct template
                if (list.BaseTemplate != template.Type)
                {
                    throw new SPException(string.Format(CultureInfo.InvariantCulture, "List {0} has list template type {1} but should have list template type {2}.", name, list.BaseTemplate, template.Type));
                }
            }
            else
            {
                // Create new list
                var id = web.Lists.Add(name, description, template);

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
        /// <param name="template">The desired list template type to use to instantiate the list</param>
        /// <returns>The new list or the existing list</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public SPList EnsureList(SPWeb web, string name, string description, SPListTemplateType templateType)
        {
            var list = this.TryGetList(web, name);

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
        /// Adds the content type id.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="contentTypeId">The content type id.</param>
        /// <exception cref="System.ArgumentNullException">Any null parameters.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">contentTypeId;Content Type not available in the lists parent web.</exception>
        public void AddContentType(SPList list, SPContentTypeId contentTypeId)
        {
            if (list == null)
            {
                throw new ArgumentNullException("list");
            }

            if (contentTypeId == null)
            {
                throw new ArgumentNullException("contentTypeId");
            }

            SPContentType contentType = list.ParentWeb.AvailableContentTypes[contentTypeId];

            if (contentType != null)
            {
                this.AddContentType(list, contentType);
            }
            else
            {
                throw new ArgumentOutOfRangeException("contentTypeId", "Content Type not available in the lists parent web.");
            }
        }

        /// <summary>
        /// Adds the content type.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <exception cref="System.ArgumentNullException">Any null parameter.</exception>
        public void AddContentType(SPList list, SPContentType contentType)
        {
            if (list == null)
            {
                throw new ArgumentNullException("list");
            }

            if (contentType == null)
            {
                throw new ArgumentNullException("contentType");
            }

            // Enable content types if not yet done.
            if (!list.ContentTypesEnabled)
            {
                list.ContentTypesEnabled = true;
                list.Update(true);
            }

            this.contentTypeBuilder.EnsureContentType(list.ContentTypes, contentType.Id, contentType.Name);
            list.Update(true);
        }

        /// <summary>
        /// Get the list bu root folder url
        /// </summary>
        /// <param name="web">
        /// The web.
        /// </param>
        /// <param name="listRootFolderUrl">
        /// The list Root Folder Url.
        /// </param>
        /// <returns>
        /// The list
        /// </returns>
        public SPList GetListByRootFolderUrl(SPWeb web, string listRootFolderUrl)
        {
            return

                (from SPList list in web.Lists
                 where list.RootFolder.Name.Equals(listRootFolderUrl, StringComparison.Ordinal)
                 select list).FirstOrDefault();
        }

        private SPList TryGetList(SPWeb web, string titleOrUrlOrResourceString)
        {
            // first try finding the list by name, simple
            var list = web.Lists.TryGetList(titleOrUrlOrResourceString);

            if (list == null)
            {
                try
                {
                    // second, try to find the list by its web-relative URL
                    list = web.GetList(SPUtility.ConcatUrls(web.ServerRelativeUrl, titleOrUrlOrResourceString));
                }
                catch (FileNotFoundException)
                {
                    // ignore exception, we need to try a third attempt that assumes the string parameter represents a resource string
                }

                if (list == null)
                {
                    // finally, try to handle the name as a resource key string
                    string[] resourceStringSplit = titleOrUrlOrResourceString.Split(',');
                    string nameFromResourceString = string.Empty;

                    if (resourceStringSplit.Length > 1)
                    {
                        // We're dealing with a resource string which looks like this: $Resources:Some.Namespace,Resource_Key
                        string resourceFileName = resourceStringSplit[0].Replace("$Resources:", string.Empty);
                        nameFromResourceString = this.resourceLocator.Find(resourceFileName, resourceStringSplit[1], web.UICulture.LCID);
                    }
                    else
                    {
                        // let's try to find a resource with that string directly as key
                        nameFromResourceString = this.resourceLocator.Find(titleOrUrlOrResourceString, web.UICulture.LCID);
                    }

                    if (!string.IsNullOrEmpty(nameFromResourceString))
                    {
                        list = web.Lists.TryGetList(nameFromResourceString);
                    }
                }
            }

            return list;
        }
    }
}
