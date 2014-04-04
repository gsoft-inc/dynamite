using System.Globalization;
using GSoft.Dynamite.Structures;

namespace GSoft.Dynamite.Globalization
{
    /// <summary>
    /// Abstraction for localization service
    /// </summary>
    public interface IResourceLocator
    {
        /// <summary>
        /// Retrieves the resource object specified by the key
        /// </summary>
        /// <param name="resourceKey">The resource key</param>
        /// <returns>The resource in the current UI language</returns>
        string Find(string resourceKey);

        /// <summary>
        /// Finds the specified resource.
        /// </summary>
        /// <param name="resource">The resource value configuration.</param>
        /// <returns>The resource value in the current UI language.</returns>
        string Find(ResourceValue resource);

        /// <summary>
        /// Retrieves the resource object specified by the key and language
        /// </summary>
        /// <param name="resourceKey">The resource key</param>
        /// <param name="lcid">The LCID of the desired culture</param>
        /// <returns>The resource in the specified language</returns>
        string Find(string resourceKey, int lcid);

        /// <summary>
        /// Finds the specified resource.
        /// </summary>
        /// <param name="resource">The resource value configuration.</param>
        /// <param name="lcid">The LCID.</param>
        /// <returns>The resource in the specified language.</returns>
        string Find(ResourceValue resource, int lcid);

        /// <summary>
        /// Retrieves the resource object specified by the key and language
        /// </summary>
        /// <param name="resourceFileName">The name of to the resource file where the resource is located</param>
        /// <param name="resourceKey">The resource key</param>
        /// <returns>The resource in the specified language</returns>
        string Find(string resourceFileName, string resourceKey);

        /// <summary>
        /// Retrieves the resource object specified by the key and language
        /// </summary>
        /// <param name="resourceFileName">The name of to the resource file where the resource is located</param>
        /// <param name="resourceKey">The resource key</param>
        /// <param name="lcid">The LCID of the desired culture</param>
        /// <returns>The resource in the specified language</returns>
        string Find(string resourceFileName, string resourceKey, int lcid);

        /// <summary>
        /// Retrieves the resource object specified by the key and culture
        /// </summary>
        /// <param name="resourceFileName">The name of to the resource file where the resource is located</param>
        /// <param name="resourceKey">The resource key</param>
        /// <param name="culture">The desired culture</param>
        /// <returns>The resource in the specified language</returns>
        string Find(string resourceFileName, string resourceKey, CultureInfo culture);
    }
}
