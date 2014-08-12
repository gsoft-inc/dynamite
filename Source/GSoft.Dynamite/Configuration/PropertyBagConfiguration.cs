using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Configuration
{
    /// <summary>
    /// Implementation of the IConfiguration interface using the PropertyBag to get values
    /// </summary>
    public class PropertyBagConfiguration : IConfiguration
    {
        private readonly string ErrorEmailKey = "GSOFT_DYNAMITE_ERROR_EMAIL";
        private readonly string GoogleAnalyticsIdKey = "GSOFT_DYNAMITE_GOOGLE_ANALYTICS_TRACKING_ID";

        private PropertyBagHelper propertyBagHelper;

        public PropertyBagConfiguration(PropertyBagHelper propertyBagHelper)
        {
            this.propertyBagHelper = propertyBagHelper;
        }

        /// <summary>
        /// Method to get a configuration value with a specific Key in the property bags
        /// </summary>
        /// <param name="web">The current web</param>
        /// <param name="key">A key to retrieve the value</param>
        /// <returns>A serialized version of the value</returns>
        /// <remarks>
        /// The implementation of this method should check on the most nested scope first than fallback on the next.
        /// Web > Site > WebApplication > Farm
        /// </remarks>
        public string GetByKeyByMostNestedScope(SPWeb web, string key)
        {
            return this.propertyBagHelper.GetMostNestedValue(web, key);
        }

        /// <summary>
        /// Method to get the Mail to send exception and errors in the property bags
        /// </summary>
        /// <param name="web">The current web</param>
        /// <returns>Comma seperated emails</returns>
        /// <remarks>
        /// The implementation of this method should check on the most nested scope first than fallback on the next.
        /// Web > Site > WebApplication > Farm
        /// </remarks>
        public string GetErrorEmailByMostNestedScope(SPWeb web)
        {
            return this.GetByKeyByMostNestedScope(web, this.ErrorEmailKey);
        }

        /// <summary>
        /// Method to get the google analytics Id
        /// </summary>
        /// <param name="web">The current web</param>
        /// <returns>the google analytics id</returns>
        /// <remarks>
        /// The implementation of this method should check on the most nested scope first than fallback on the next.
        /// Web > Site > WebApplication > Farm
        /// </remarks>
        public string GetGoogleAnalyticsIdByMostNestedScope(SPWeb web)
        {
            return this.GetByKeyByMostNestedScope(web, this.GoogleAnalyticsIdKey);
        }
    }
}
