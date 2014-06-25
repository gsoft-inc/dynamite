namespace GSoft.Dynamite.Lists
{
    using System.Diagnostics.CodeAnalysis;

    using Microsoft.SharePoint;

    public interface IListLocator
    {
        /// <summary>
        /// Find a list by its web-relative url
        /// </summary>
        /// <param name="web">The context's web</param>
        /// <param name="listUrl">The web-relative path to the list</param>
        /// <returns>The list</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Statics to be avoided in favor consistency with use of constructor injection for class collaborators.")]
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", Justification = "List urls are available as strings through the ListUrls utility.")]
        SPList GetByUrl(SPWeb web, string listUrl);

        /// <summary>
        /// Find a list by its name's resource key
        /// </summary>
        /// <param name="web">The context's web</param>
        /// <param name="listNameResourceKey">The web-relative path to the list</param>
        /// <returns>The list</returns>
        SPList GetByNameResourceKey(SPWeb web, string listNameResourceKey);
    }
}
