namespace GSoft.Dynamite.Lists
{
    using System.Diagnostics.CodeAnalysis;

    using Microsoft.SharePoint;

    public interface IListSecurityHelper
    {
        /// <summary>
        /// Method to remove the collaboration rights to all members excepts administrator
        /// </summary>
        /// <param name="list">The list to affect the change</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics in public facing members is discouraged for more consistency with dependency injection.")]
        void SetListToReadOnlyExceptAdmin(SPSecurableObject list);

        /// <summary>
        /// Method to remove the collaboration rights to all members excepts administrator
        /// </summary>
        /// <param name="list">The list to affect the change</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics in public facing members is discouraged for more consistency with dependency injection.")]
        void SetListHiddenExceptAdmin(SPSecurableObject list);
    }
}