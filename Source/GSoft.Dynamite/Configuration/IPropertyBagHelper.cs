namespace GSoft.Dynamite.Configuration
{
    using System;
    using System.Collections.Generic;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Administration;

    /// <summary>
    /// Helper to interact with the Property bags
    /// </summary>
    public interface IPropertyBagHelper
    {
        /// <summary>
        /// Method to get the most nested value for the key
        /// </summary>
        /// <param name="web">The web to start looking for the key</param>
        /// <param name="key">The key</param>
        /// <returns>A serialized version of the value or null if not found</returns>
        string GetMostNestedValue(SPWeb web, string key);

        /// <summary>
        /// Method to Set a value in a Web scoped property bag
        /// </summary>
        /// <param name="webUrl">The Url of the web</param>
        /// <param name="propertyBagValues">The property bag values</param>
        void SetWebValues(Uri webUrl, IList<PropertyBagValue> propertyBagValues);

        /// <summary>
        /// Method to Set a value in a Web scoped property bag
        /// </summary>
        /// <param name="web">The web</param>
        /// <param name="propertyBagValues">The property bag values</param>
        void SetWebValues(SPWeb web, IList<PropertyBagValue> propertyBagValues);

        /// <summary>
        /// Method to Get a value in the Web scoped property bag
        /// </summary>
        /// <param name="web">The web to get the value from</param>
        /// <param name="key">The key</param>
        /// <returns>A serialized version of the object stored or null if no key found</returns>
        string GetWebValue(SPWeb web, string key);

        /// <summary>
        /// Method to Get a value in the Site scoped property bag
        /// </summary>
        /// <param name="site">The Site to get the value from</param>
        /// <param name="key">The key</param>
        /// <returns>A serialized version of the object stored or null if no key found</returns>
        string GetSiteValue(SPSite site, string key);

        /// <summary>
        /// Method to get the Web Application value for the key
        /// </summary>
        /// <param name="webApplication">The WebApplication</param>
        /// <param name="key">The Key</param>
        /// <returns>A serialized version of the value or null if not found</returns>
        string GetWebApplicationValue(SPWebApplication webApplication, string key);

        /// <summary>
        /// Method to set a web application value
        /// </summary>
        /// <param name="webApplicationUrl">The url of the Web Application</param>
        /// <param name="propertyBagValues">The property bag values to insert</param>
        void SetWebApplicationValue(Uri webApplicationUrl, IList<PropertyBagValue> propertyBagValues);
    }
}