namespace GSoft.Dynamite.Catalogs
{
    using System.Collections.Generic;

    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Publishing;

    //public interface ICatalogBuilder
    //{
    //    /// <summary>
    //    /// Set a SharePoint as a product catalog without navigation term associated
    //    /// Note: For more information, see PublishingCatalogUtility in Microsoft.SharePoint.Publishing
    //    /// </summary>
    //    /// <param name="list">The SharePoint list.</param>
    //    /// <param name="availableFields">List of internal field names that are available through the catalog.</param>
    //    /// <returns>The SharePoint list configured as a catalog.</returns>
    //    SPList SetListAsCatalog(SPList list, IEnumerable<string> availableFields);

    //    /// <summary>
    //    /// Set a SharePoint as a product catalog with a taxonomy term for navigation.
    //    /// </summary>
    //    /// <param name="list">The SharePoint list.</param>
    //    /// <param name="availableFields">List of internal field names that are available through the catalog.</param>
    //    /// <param name="taxonomyFieldMap">The taxonomy field that will be used for navigation.</param>
    //    /// <returns>The SharePoint list configured as a catalog.</returns>
    //    SPList SetListAsCatalog(SPList list, IEnumerable<string> availableFields, string taxonomyFieldMap);

    //    /// <summary>
    //    /// Method to process a Catalog Object and configures it accordingly
    //    /// </summary>
    //    /// <param name="web">The current web</param>
    //    /// <param name="catalog">The catalog definition</param>
    //    /// <returns>The newly created list</returns>
    //    SPList ProcessCatalog(SPWeb web, Catalog catalog);

    //    /// <summary>
    //    /// Method to get a CatalogConnectionSettings from the site
    //    /// </summary>
    //    /// <param name="site">The SPSite to get the connection from</param>
    //    /// <param name="serverRelativeUrl">The server relative url where the catalog belong</param>
    //    /// <param name="catalogRootUrl">The root url of the catalog.</param>
    //    /// <returns>A catalogConnectionSettings object</returns>
    //    CatalogConnectionSettings GetCatalogConnectionSettings(SPSite site, string serverRelativeUrl, string catalogRootUrl);

    //    /// <summary>
    //    /// Method to create a catalog connection
    //    /// </summary>
    //    /// <param name="site">The site where to create the connection</param>
    //    /// <param name="catalogConnectionSettings">The catalog connection settings to create</param>
    //    /// <param name="overwriteIfExist">if true and existing, the connection will be deleted then recreated</param>
    //    void CreateCatalogConnection(SPSite site, CatalogConnectionSettings catalogConnectionSettings, bool overwriteIfExist);
    //}
}