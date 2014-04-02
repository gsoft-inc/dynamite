namespace GSoft.Dynamite.WebParts
{
    /// <summary>
    /// The DefaultPageWebPartIndex interface.
    /// </summary>
    public interface IDefaultPageWebPartIndex
    {
        /// <summary>
        /// Gets the default web parts that should be inserted on the page at the specified URL
        /// </summary>
        /// <param name="path">The publishing page URL path</param>
        /// <returns>The default web parts information</returns>
        IDefaultPageWebParts GetDefaultWebPartsForPageUrl(string path);
    }
}
