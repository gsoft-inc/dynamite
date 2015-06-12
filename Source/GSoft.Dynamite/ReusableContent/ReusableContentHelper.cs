using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
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
        private readonly string ReusableContentListName = "ReusableContent";

        private ILogger logger;
        private IListLocator listLocator;
        private ICamlBuilder camlBuilder;

        /// <summary>
        /// Helper class constructor to work with Reusable Content.
        /// </summary>
        /// <param name="logger">The logger to log info and errors</param>
        /// <param name="listLocator">List locator to find the ReusableContentList</param>
        /// <param name="camlBuilder">Caml Builder for the query</param>
        public ReusableContentHelper(ILogger logger, IListLocator listLocator, ICamlBuilder camlBuilder)
        {
            this.logger = logger;
            this.listLocator = listLocator;
            this.camlBuilder = camlBuilder;
        }

        /// <summary>
        /// Method to ensure (create if not exist) and update a reusable content in a specific site.
        /// </summary>
        /// <param name="site">The Site Collection to ensure the reusablec content</param>
        /// <param name="reusableContents">The information on the reusable contents to ensure</param>
        /// <returns>The reusable content (with the content of the html file)</returns>
        public void EnsureReusableContent(SPSite site, IList<ReusableContentInfo> reusableContents)
        {
            var list = this.listLocator.GetByUrl(site.RootWeb, this.ReusableContentListName);

            // Load the HTML Content first
            foreach (var reusableContent in reusableContents)
            {
                // If the HTML Content load was successful, ensure the list item
                if (this.LoadContentFile(reusableContent))
                {
                    var query = new SPQuery();

                    query.Query = this.camlBuilder.Where(this.camlBuilder.Equal(this.camlBuilder.FieldRef(BuiltInFields.Title.InternalName), this.camlBuilder.Value(reusableContent.Title)));
                    var itemCollection = list.GetItems(query);

                    SPListItem item;

                    if (itemCollection.Count > 0)
                    {
                        // The Reusable Content Exist, let's ensure it's properties.
                        item = itemCollection[0];
                    }
                    else
                    {
                        // The Reusable Content does not exists, let's create it.
                        item = list.Items.Add();
                        item[BuiltInFields.Title.InternalName] = reusableContent.Title;
                    }

                    item[PublishingFields.AutomaticUpdate.InternalName] = reusableContent.IsAutomaticUpdate.ToString();
                    item[PublishingFields.ShowInRibbon.InternalName] = reusableContent.IsShowInRibbon.ToString();
                    item[PublishingFields.ReusableHtml.InternalName] = reusableContent.Content.ToString();
                    item[PublishingFields.ContentCategory.InternalName] = reusableContent.Category.ToString();

                    item.Update();

                    this.logger.Info("Reusable Content with title '{0}' was successfully ensured in site '{1}'.", reusableContent.Title, site.Url);
                }
            }
        }

        private bool LoadContentFile(ReusableContentInfo reusableContentInfo)
        {
            var isSuccess = false;

            // Validate the Filename
            if (!reusableContentInfo.Filename.EndsWith(".html", true, CultureInfo.InstalledUICulture))
            {
                this.logger.Error("ReusableContentHelper.ParseReusableContent: Invalid filename for the HTML File, it does not ends with .html. (value : {0}", reusableContentInfo.Filename);
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
    }
}
