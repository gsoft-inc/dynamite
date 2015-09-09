namespace GSoft.Dynamite.Configuration
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Microsoft.SharePoint.Administration;

    /// <summary>
    /// Helper to add, clean, remove WebConfig modifications programmatically
    /// </summary>
    public interface IWebConfigModificationHelper
    {
        /// <summary>
        /// Method to add one or multiple WebConfig modifications
        /// NOTE: There should not have 2 modifications with the same Owner.
        /// </summary>
        /// <param name="webApp">The current Web Application</param>
        /// <param name="webConfigModificationCollection">The collection of WebConfig modifications to remove-and-add</param>
        /// <remarks>All SPWebConfigModification Owner should be UNIQUE !</remarks>
        void AddAndCleanWebConfigModification(SPWebApplication webApp, Collection<SPWebConfigModification> webConfigModificationCollection);

        /// <summary>
        /// Method to remove all existing WebConfig Modifications by the same owner.
        /// By Design, owner should be unique so we can remove duplicates.
        /// </summary>
        /// <param name="webApplication">The current Web Application</param>
        /// <param name="owner">The Owner key. Only one modification should have that owner</param>
        /// <remarks>All SPWebConfigModification Owner should be UNIQUE !</remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of public static members discouraged in favor of dependency injection.")]
        void RemoveExistingModificationsFromOwner(SPWebApplication webApplication, string owner);

        /// <summary>
        /// Method to remove all existing WebConfig Modifications for the listed owners.
        /// By Design, owner should be unique per WebConfig modification so we can remove duplicates.
        /// </summary>
        /// <param name="webApplication">The current Web Application</param>
        /// <param name="owners">A list of owners for which we want to remove modifications.</param>
        /// <remarks>All SPWebConfigModification Owner should be UNIQUE !</remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of public static members discouraged in favor of dependency injection.")]
        void RemoveExistingModificationsFromOwner(SPWebApplication webApplication, IList<string> owners);
    }
}