using System;
using System.Globalization;
﻿using System.Linq;
using System.Web;
using GSoft.Dynamite.Repositories.Entities;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;

namespace GSoft.Dynamite.Extensions
{
    using System.IO;

    /// <summary>
    /// Extensions for the SPWeb type.
    /// </summary>
    public static class WebExtensions
    {
        /// <summary>
        /// Gets the pages library.
        /// </summary>
        /// <param name="web">The web to get the Pages library from.</param>
        /// <exception cref="System.ArgumentException">No Pages library was found for this web.</exception>
        /// <returns>The Pages library.</returns>
        public static SPList GetPagesLibrary(this SPWeb web)
        {
            try
            {
                var list = web.GetList(SPUtility.ConcatUrls(web.ServerRelativeUrl, SPUtility.GetLocalizedString("$Resources:List_Pages_UrlName", "osrvcore", web.Language)));
                return list;
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        /// <summary>
        /// Applies a composed look to a web.
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="composedLook">The name of the composed look.</param>
        public static void ApplyComposedLook(this SPWeb web, ComposedLook composedLook)
        {
            using (var elevatedWeb = web.Site.OpenWeb(web.ID))
            {
                // Set Master Page
                elevatedWeb.CustomMasterUrl = HttpUtility.UrlDecode(new Uri(composedLook.MasterPagePath.Url).AbsolutePath);
                elevatedWeb.MasterUrl = HttpUtility.UrlDecode(new Uri(composedLook.MasterPagePath.Url).AbsolutePath);
                elevatedWeb.Properties["DesignPreviewLayoutUrl"] = HttpUtility.UrlDecode(new Uri(composedLook.MasterPagePath.Url).AbsolutePath);
                elevatedWeb.Update();

                // Set theme
                elevatedWeb.ApplyTheme(
                    new Uri(composedLook.ThemePath.Url).AbsolutePath,
                    composedLook.FontSchemePath == null ? string.Empty : HttpUtility.UrlDecode(new Uri(composedLook.FontSchemePath.Url).AbsolutePath),
                    composedLook.ImagePath == null ? string.Empty : HttpUtility.UrlDecode(new Uri(composedLook.ImagePath.Url).AbsolutePath),
                    false);
                elevatedWeb.Update();

                // Update current selected composed look
                if (web.IsRootWeb)
                {
                    UpdateCurrentComposedLookItem(web, composedLook); 
                }
            }
        }
        
		/// Gets the custom list template with the specified name.
        /// </summary>
        /// <param name="web">The SharePoint web.</param>
        /// <param name="name">The list template name.</param>
        /// <returns>An SPListTemplate or null if nothing is found.</returns>
        public static SPListTemplate GetCustomListTemplate(this SPWeb web, string name)
        {
            var listTemplates = web.Site.GetCustomListTemplates(web);
            var listTemplate = (from SPListTemplate template in listTemplates where template.Name == name select template).FirstOrDefault();
            return listTemplate;
        }
		
        private static void UpdateCurrentComposedLookItem(SPWeb web, ComposedLook composedLook)
        {
            var catalog = web.GetCatalog(SPListTemplateType.DesignCatalog);
            var items = catalog.GetItems(new SPQuery
            {
                RowLimit = 1u,
                Query = "<Where><Eq><FieldRef Name='DisplayOrder'/><Value Type='Number'>0</Value></Eq></Where>",
                ViewFields = "<FieldRef Name='DisplayOrder'/>",
                ViewFieldsOnly = true
            });

            // Delete current composed look item
            if (items.Count == 1)
            {
                items[0].Delete();
            }

            // Create the new composed look item
            var item = catalog.AddItem();
            item[BuiltInFields.DisplayOrderName] = 0;
            item[SPBuiltInFieldId.Name] = SPResource.GetString(web.UICulture, "DesignGalleryCurrentItemName");
            item[SPBuiltInFieldId.Title] = SPResource.GetString(web.UICulture, "DesignGalleryCurrentItemName");
            item[BuiltInFields.MasterPageUrlName] = HttpUtility.UrlDecode(new Uri(composedLook.MasterPagePath.Url).AbsolutePath);
            item[BuiltInFields.ThemeUrlName] = HttpUtility.UrlDecode(new Uri(composedLook.ThemePath.Url).AbsolutePath);
            item[BuiltInFields.ImageUrlName] = composedLook.ImagePath == null ? string.Empty : HttpUtility.UrlDecode(new Uri(composedLook.ImagePath.Url).AbsolutePath);
            item[BuiltInFields.FontSchemeUrlName] = HttpUtility.UrlDecode(new Uri(composedLook.FontSchemePath.Url).AbsolutePath);
            item.Update();
		}
    }
}
