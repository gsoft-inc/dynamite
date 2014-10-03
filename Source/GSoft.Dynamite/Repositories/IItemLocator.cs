namespace GSoft.Dynamite.Repositories
{
    using System.Diagnostics.CodeAnalysis;

    using Microsoft.SharePoint;

    /// <summary>
    /// The ItemLocator interface.
    /// </summary>
    public interface IItemLocator
    {
        /// <summary>
        /// Get the list item corresponding to the given title 
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="listUrl">The list path to reach the list.</param>
        /// <param name="itemTitle">The title of the list item.</param>
        /// <returns>
        /// The <see cref="SPSecurableObject"/>.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", Justification = "Risky business but URL strings are more convenient here")]
        SPSecurableObject GetByTitle(SPWeb web, string listUrl, string itemTitle);
    }
}
