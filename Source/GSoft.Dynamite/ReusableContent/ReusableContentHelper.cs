using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Binding;
using GSoft.Dynamite.Caml;
using GSoft.Dynamite.Fields.Constants;
using GSoft.Dynamite.Lists;
using GSoft.Dynamite.Logging;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;

namespace GSoft.Dynamite.ReusableContent
{
    /// <summary>
    /// Helper class to work with Reusable Content.
    /// </summary>
    public class ReusableContentHelper : IReusableContentHelper
    {
        private const string ReusableContentListName = "ReusableContent";

        private ILogger logger;
        private IListLocator listLocator;
        private ICamlBuilder camlBuilder;
        private ISharePointEntityBinder binder;

        /// <summary>
        /// Helper class constructor to work with Reusable Content.
        /// </summary>
        /// <param name="logger">The logger to log info and errors</param>
        /// <param name="listLocator">List locator to find the ReusableContentList</param>
        /// <param name="camlBuilder">Caml Builder for the query</param>
        /// <param name="binder">The entity binder</param>
        public ReusableContentHelper(ILogger logger, IListLocator listLocator, ICamlBuilder camlBuilder, ISharePointEntityBinder binder)
        {
            this.logger = logger;
            this.listLocator = listLocator;
            this.camlBuilder = camlBuilder;
            this.binder = binder;
        }

        /// <summary>
        /// Gets the reusable content by title.
        /// </summary>
        /// <param name="site">The Site Collection.</param>
        /// <param name="reusableContentTitle">The reusable content title.</param>
        /// <returns>The reusable content</returns>
        public ReusableContentInfo GetByTitle(SPSite site, string reusableContentTitle)
        {
            var list = this.listLocator.GetByUrl(site.RootWeb, new Uri(ReusableContentListName, UriKind.Relative));

            var cultureSuffix = CultureInfo.CurrentUICulture.LCID == Language.English.Culture.LCID ? "_EN" : "_FR";
            var listItem = this.GetListItemByTitle(list, reusableContentTitle);

            if (listItem == null)
            {
                listItem = this.GetListItemByTitle(list, reusableContentTitle + cultureSuffix);
            }

            if (listItem != null)
            {
                var entity = new ReusableContentInfo();
                this.binder.ToEntity<ReusableContentInfo>(entity, listItem);
                return entity;
            }

            return null;
        }

        /// <summary>
        /// Method to get all available Reusable Content Titles
        /// </summary>
        /// <param name="site">The current Site collection context</param>
        /// <returns>A list of string (reusable content title) or null.</returns>
        public IList<string> GetAllReusableContentTitles(SPSite site)
        {
            var list = this.listLocator.GetByUrl(site.RootWeb, new Uri(ReusableContentListName, UriKind.Relative));

            var itemCollection = list.Items;

            if (itemCollection.Count > 0)
            {
                return itemCollection.Cast<SPListItem>().Select(item => item.Title).ToList();
            }

            return null;
        }

        /// <summary>
        /// Method to ensure (create if not exist) and update a reusable content in a specific site.
        /// </summary>
        /// <remarks>
        /// Reusable Content exist in the same name list in the RootWeb of the Site Collection.
        /// </remarks>
        /// <param name="site">The Site Collection to ensure the reusable content</param>
        /// <param name="reusableContents">The information on the reusable contents to ensure</param>
        public void EnsureReusableContent(SPSite site, IList<ReusableContentInfo> reusableContents)
        {
            var list = this.listLocator.GetByUrl(site.RootWeb, new Uri(ReusableContentListName, UriKind.Relative));

            // Load the HTML Content first
            foreach (var reusableContent in reusableContents)
            {
                // If the HTML Content load was successful, ensure the list item
                if (this.LoadContentFile(reusableContent))
                {
                    SPListItem item = this.GetListItemByTitle(list, reusableContent.Title);

                    if (item == null)
                    {
                        // The Reusable Content does not exists, let's create it.
                        item = list.Items.Add();
                    }

                    // Ensure the Category
                    this.EnsureContentCategory(list, reusableContent.Category);

                    // Bind the entity to the list item
                    this.binder.FromEntity<ReusableContentInfo>(reusableContent, item);
                    item.Update();

                    this.logger.Info("Reusable Content with title '{0}' was successfully ensured in site '{1}'.", reusableContent.Title, site.Url);
                }
            }
        }

        private SPListItem GetListItemByTitle(SPList reusableContentList, string reusableContentTitle)
        {
            var query = new SPQuery();

            query.Query = this.camlBuilder.Where(this.camlBuilder.Equal(this.camlBuilder.FieldRef(BuiltInFields.TitleName), this.camlBuilder.Value(reusableContentTitle)));
            var itemCollection = reusableContentList.GetItems(query);

            SPListItem item = null;
            if (itemCollection.Count > 0)
            {
                item = itemCollection[0];
            }

            return item;
        }

        private bool LoadContentFile(ReusableContentInfo reusableContentInfo)
        {
            var isSuccess = false;

            // Validate the Filename
            if (!reusableContentInfo.FileName.EndsWith(".html", true, CultureInfo.InstalledUICulture))
            {
                this.logger.Error("ReusableContentHelper.ParseReusableContent: Invalid filename for the HTML File, it does not ends with .html. (value : {0}", reusableContentInfo.FileName);
            }

            // HTML Content of the file
            var htmlContent = string.Empty;

            string pathLayout = reusableContentInfo.HTMLFilePath;

            // File validation
            if (string.IsNullOrEmpty(pathLayout) || !File.Exists(pathLayout))
            {
                this.logger.Error("Unable to locate file at path '{0}' for reusable content.", pathLayout);
            }
            else
            {
                try
                {
                    // Load the HTML file content
                    using (var fileStream = new FileStream(pathLayout, FileMode.Open, FileAccess.Read))
                    {
                        using (var streamReader = new StreamReader(fileStream))
                        {
                            htmlContent = streamReader.ReadToEnd();
                            isSuccess = true;
                        }
                    }
                }
                catch (Exception exception)
                {
                    if (exception is FileNotFoundException || exception is SecurityException || exception is DirectoryNotFoundException)
                    {
                        this.logger.Error("An exception occured while tryin to read the file content at path '{0}' for reusable content.", pathLayout);
                        this.logger.Exception(exception);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            reusableContentInfo.Content = htmlContent;

            return isSuccess;
        }

        private void EnsureContentCategory(SPList reusableContentList, string category)
        {
            SPFieldChoice categoryField = null;
            try
            {
                categoryField = reusableContentList.Fields.GetFieldByInternalName(PublishingFields.ContentCategory.InternalName) as SPFieldChoice;
            }
            catch (ArgumentException ex)
            {
                this.logger.Error("Unable to find the field with internal name '{0}' on list '{1}'.", PublishingFields.ContentCategory.InternalName, reusableContentList.RootFolder.Url);
                this.logger.Exception(ex);
            }

            if (categoryField != null)
            {
                if (!categoryField.Choices.Contains(category))
                {
                    categoryField.Choices.Add(category);
                    categoryField.Update();
                }
            }
        }
    }
}
