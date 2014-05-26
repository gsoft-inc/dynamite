using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Taxonomy;

namespace GSoft.Dynamite.Repositories
{
    /// <summary>
    /// Helps with building CAML queries
    /// </summary>
    public interface IQueryHelper
    {
        /// <summary>
        /// Gets the now in CAML.
        /// </summary>
        /// <value>The now in CAML.</value>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of static members discouraged in favor of non-static public member for more consistency with dependency injection")]
        string NowInCAML { get; }
        
        /// <summary>
        /// Returns a string corresponding the the ViewFields attribute of a SPQuery
        /// with all the properties of a particular Entity
        /// </summary>
        /// <param name="entityType">The type of the entity</param>
        /// <returns>A string representing the list of view fields</returns>
        string ViewFieldsForEntityType(Type entityType);

        /// <summary>
        /// Trimming utility for rich text content returned from SPQueries
        /// </summary>
        /// <param name="toTrim">The content to trim</param>
        /// <returns>The trimmed string</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of static members discouraged in favor of non-static public member for more consistency with dependency injection")]
        string TrimIfNotNullOrEmpty(string toTrim);

        /// <summary>
        /// Generates a SPQuery filter for Taxonomy Term
        /// </summary>
        /// <param name="list">The list over which the query will be done</param>
        /// <param name="taxonomyFieldInternalName">The name of the site column associated with the term set</param>
        /// <param name="term">Term to match for</param>
        /// <param name="includeDescendants">Whether the Term's child terms should be query hits as well</param>
        /// <returns>The SPQuery filter</returns>
        string TermFilter(SPList list, string taxonomyFieldInternalName, Term term, bool includeDescendants);

        /// <summary>
        /// Generates a SPQuery filter for Taxonomy Term
        /// </summary>
        /// <param name="list">The list over which the query will be done</param>
        /// <param name="taxonomyFieldInternalName">The name of the site column associated with the term set</param>
        /// <param name="terms">List of terms for why we want to match in an OR fashion</param>
        /// <param name="includeDescendants">Whether the Term's child terms should be query hits as well</param>
        /// <returns>The SPQuery filter</returns>
        string TermFilter(SPList list, string taxonomyFieldInternalName, IList<Term> terms, bool includeDescendants);

        /// <summary>
        /// Generates a SPQuery filter for Taxonomy Term from the site-collection-reserved term store group
        /// </summary>
        /// <param name="list">The list over which the query will be done</param>
        /// <param name="taxonomyFieldInternalName">The name of the site column associated with the term set</param>
        /// <param name="termSetName">Name of the term set</param>
        /// <param name="termLabel">Label by which to find the term (dupes not supported)</param>
        /// <param name="includeDescendants">Whether the Term's child terms should be query hits as well</param>
        /// <returns>
        /// The SPQuery filter
        /// </returns>
        string TermFilter(SPList list, string taxonomyFieldInternalName, string termSetName, string termLabel, bool includeDescendants);

        /// <summary>
        /// Generates a SPQuery filter for Taxonomy Term in a global farm term store group
        /// </summary>
        /// <param name="list">The list over which the query will be done</param>
        /// <param name="taxonomyFieldInternalName">The name of the site column associated with the term set</param>
        /// <param name="termStoreGroupName">Name of the global farm term store group</param>
        /// <param name="termSetName">Name of the term set</param>
        /// <param name="termLabel">Label by which to find the term (dupes not supported)</param>
        /// <param name="includeDescendants">Whether the Term's child terms should be query hits as well</param>
        /// <returns>
        /// The SPQuery filter
        /// </returns>
        string TermFilter(SPList list, string taxonomyFieldInternalName, string termStoreGroupName, string termSetName, string termLabel, bool includeDescendants);
    }
}