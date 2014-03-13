// -----------------------------------------------------------------------
// <copyright file="VariationExpert.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace GSoft.Dynamite.Variations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    using GSoft.Dynamite;

    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Publishing;

    /// <summary>
    /// The variation expert.
    /// </summary>
    public class VariationExpert : IVariationExpert
    {
        /// <summary>
        /// The get variation url.
        /// </summary>
        /// <param name="site">
        /// The site.
        /// </param>
        /// <returns>
        /// The <see cref="SPUrl"/>.
        /// </returns>
        public Uri GetVariationRootUri(SPSite site)
        {
            // Get Assembly hosting the hidden VariationSettings class 
            Assembly a = typeof(PublishingWeb).Assembly;

            // Find VariationSettings type 
            Type[] types = a.GetTypes();
            Type variationSettingsType = types.FirstOrDefault(t => t.FullName == "Microsoft.SharePoint.Publishing.Internal.VariationSettings");

            // Instantiate a VariationSettings object 
            ConstructorInfo ci = variationSettingsType.GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new Type[] { typeof(SPSite), typeof(bool) },
                null);
            var variationSettings = ci.Invoke(new object[] { site, false });

            // Retrieve the URL of the variation root
            PropertyInfo pi = variationSettings.GetType()
                .GetProperty("RootPublishingWebUrl", BindingFlags.NonPublic | BindingFlags.Instance);
            var url = pi.GetValue(variationSettings, null) as string;

            return new Uri(url);
        }
    }
}
