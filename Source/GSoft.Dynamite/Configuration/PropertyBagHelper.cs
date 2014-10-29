using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Logging;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace GSoft.Dynamite.Configuration
{
    /// <summary>
    /// Helper class to interact with the Property bags
    /// </summary>
    public class PropertyBagHelper : IPropertyBagHelper
    {
        private ILogger logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">A logger</param>
        public PropertyBagHelper(ILogger logger)
        {
            this.logger = logger;
        }

        #region Most Nested Scope
        /// <summary>
        /// Method to get the most nested value for the key
        /// </summary>
        /// <param name="web">The web to start looking for the key</param>
        /// <param name="key">The key</param>
        /// <returns>A serialized version of the value or null if not found</returns>
        public string GetMostNestedValue(SPWeb web, string key)
        {
            string value = this.GetWebValue(web, key);

            if (value == null)
            {
                value = this.GetSiteValue(web.Site, key);

                if (value == null)
                {
                    value = this.GetWebApplicationValue(web.Site.WebApplication, key);
                }
            }

            return value;
        }
        #endregion Most Nested Scope

        #region Web scope
        /// <summary>
        /// Method to Set a value in a Web scoped property bag
        /// </summary>
        /// <param name="webUrl">The Url of the web</param>
        /// <param name="propertyBagValues">The property bag values</param>
        public void SetWebValues(string webUrl, IList<PropertyBagValue> propertyBagValues)
        {
            using (var site = new SPSite(webUrl))
            {
                using (var web = site.OpenWeb())
                {
                    this.Set(web.AllProperties, web.IndexedPropertyKeys, propertyBagValues, webUrl);
                    web.Update();
                }
            }
        }

        /// <summary>
        /// Method to Set a value in a Web scoped property bag
        /// </summary>
        /// <param name="web">The web</param>
        /// <param name="propertyBagValues">The property bag values</param>
        public void SetWebValues(SPWeb web, IList<PropertyBagValue> propertyBagValues)
        {
            this.Set(web.AllProperties, web.IndexedPropertyKeys, propertyBagValues, web.Url);
            web.Update();
        }

        /// <summary>
        /// Method to Get a value in the Web scoped property bag
        /// </summary>
        /// <param name="web">The web to get the value from</param>
        /// <param name="key">The key</param>
        /// <returns>A serialized version of the object stored or null if no key found</returns>
        public string GetWebValue(SPWeb web, string key)
        {
            if (web == null)
            {
                throw new ArgumentNullException("web");
            }

            return this.Get(web.AllProperties, key);
        }

        #endregion Web scope

        #region Site scope

        /// <summary>
        /// Method to Get a value in the Site scoped property bag
        /// </summary>
        /// <param name="site">The Site to get the value from</param>
        /// <param name="key">The key</param>
        /// <returns>A serialized version of the object stored or null if no key found</returns>
        public string GetSiteValue(SPSite site, string key)
        {
            if (site == null || site.RootWeb == null)
            {
                throw new ArgumentNullException("site");
            }

            return this.Get(site.RootWeb.AllProperties, key);
        }

        #endregion Site scope

        #region Web Application scope
        /// <summary>
        /// Method to get the Web Application value for the key
        /// </summary>
        /// <param name="webApplication">The WebApplication</param>
        /// <param name="key">The Key</param>
        /// <returns>A serialized version of the value or null if not found</returns>
        public string GetWebApplicationValue(SPWebApplication webApplication, string key)
        {
            if (webApplication == null)
            {
                throw new ArgumentNullException("webApplication");
            }

            return this.Get(webApplication.Properties, key);
        }

        /// <summary>
        /// Method to set a web application value
        /// </summary>
        /// <param name="webApplicationUrl">The url of the Web Application</param>
        /// <param name="propertyBagValues">The property bag values to insert</param>
        public void SetWebApplicationValue(string webApplicationUrl, IList<PropertyBagValue> propertyBagValues)
        {
            var webApplication = SPWebApplication.Lookup(new Uri(webApplicationUrl));
            this.Set(webApplication.Properties, null, propertyBagValues, webApplicationUrl);
        }
        #endregion Web Application scope

        private string Get(Hashtable bag, string key)
        {
            object property = null;

            if (bag != null && bag.Contains(key))
            {
                property = bag[key];
            }

            return property != null ? property.ToString() : null;
        }

        private void Set(Hashtable bag, ISet<string> indexedBag, IList<PropertyBagValue> values, string url)
        {
            foreach (var propertyBagValue in values)
            {
                // Check if the key exist and if we override it.
                if (bag.ContainsKey(propertyBagValue.Key) && propertyBagValue.Overwrite)
                {
                    this.logger.Warn(string.Format(CultureInfo.InvariantCulture, "Overwriting property bag '{0}' with value '{1}' to url '{2}'", propertyBagValue.Key, propertyBagValue.Value, url));
                    bag[propertyBagValue.Key] = propertyBagValue.Value;
                }
                else if (!bag.ContainsKey(propertyBagValue.Key))
                {
                    // Add value to root web property bag
                    this.logger.Info(string.Format(CultureInfo.InvariantCulture, "Adding property bag '{0}' with value '{1}' to url '{2}'", propertyBagValue.Key, propertyBagValue.Value, url));
                    bag.Add(propertyBagValue.Key, propertyBagValue.Value);
                }

                // Add property bag key to indexed property keys
                if (indexedBag != null && !indexedBag.Contains(propertyBagValue.Key) && propertyBagValue.Indexed)
                {
                    this.logger.Info(string.Format(CultureInfo.InvariantCulture, "Setting property bag '{0}' to be indexable by search on url '{1}'", propertyBagValue.Key, url));
                    indexedBag.Add(propertyBagValue.Key);
                }
            }
        }
    }
}
