using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Utils
{
    /// <summary>
    /// Utilities to manipulate a web's UserCustomActions collection
    /// </summary>
    public class CustomActionHelper : ICustomActionHelper
    {
        /// <summary>
        /// Checks if the custom action is contained in the SPUserCustomActionCollection and if so, 
        /// returns the id in the customActionId parameter
        /// </summary>
        /// <param name="web">The SharePoint web</param>
        /// <param name="customActionName">Name property given to the CustomAction element in the definition</param>
        /// <returns>The custom action Id or an empty string if not found</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Dependency-injected classes should expose non-static members only for consistency.")]
        public string GetCustomActionIdForName(SPWeb web, string customActionName)
        {
            string customActionId = string.Empty;

            foreach (SPUserCustomAction customAction in web.UserCustomActions)
            {
                if (customAction.Name == customActionName)
                {
                    customActionId = customAction.Id.ToString();
                }
            }

            return customActionId;
        }

        /// <summary>
        /// Removes a custom action from a web
        /// </summary>
        /// <param name="web">The SharePoint web</param>
        /// <param name="actionName">The ID for the custom action</param>
        public void DeleteCustomAction(SPWeb web, string actionName)
        {
            var customActionId = this.GetCustomActionIdForName(web, actionName);
            if (!string.IsNullOrEmpty(customActionId))
            {
                var customAction = web.UserCustomActions[new Guid(customActionId)];
                customAction.Delete();
            }

            if (!string.IsNullOrEmpty(customActionId))
            {
                web.Update();
            }
        }
    }
}
