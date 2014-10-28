namespace GSoft.Dynamite.Utils
{
    using System.Diagnostics.CodeAnalysis;

    using Microsoft.SharePoint;

    public interface ICustomActionHelper
    {
        /// <summary>
        /// Checks if the custom action is contained in the SPUserCustomActionCollection and if so, 
        /// returns the id in the customActionId parameter
        /// </summary>
        /// <param name="web">The SharePoint web</param>
        /// <param name="customActionName">Name property given to the CustomAction element in the definition</param>
        /// <returns>The custom action Id or an empty string if not found</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Dependency-injected classes should expose non-static members only for consistency.")]
        string GetCustomActionIdForName(SPWeb web, string customActionName);

        /// <summary>
        /// Removes a custom action from a web
        /// </summary>
        /// <param name="web">The SharePoint web</param>
        /// <param name="actionName">The ID for the custom action</param>
        void DeleteCustomAction(SPWeb web, string actionName);
    }
}