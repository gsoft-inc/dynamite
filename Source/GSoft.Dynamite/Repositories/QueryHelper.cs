using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using GSoft.Dynamite.Binding;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.Taxonomy;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Taxonomy;
using Microsoft.SharePoint.Utilities;

namespace GSoft.Dynamite.Repositories
{
    /// <summary>
    /// Utility to help build SPQuery filter strings
    /// </summary>
    public class QueryHelper : IQueryHelper
    {
        private ITaxonomyService taxonomyService;
        private ILogger log;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryHelper"/> class.
        /// </summary>
        /// <param name="taxonomyService">The taxonomy service.</param>
        /// <param name="logger">The logger.</param>
        public QueryHelper(ITaxonomyService taxonomyService, ILogger logger)
        {
            this.taxonomyService = taxonomyService;
            this.log = logger;
        }

        /// <summary>
        /// Gets the now in CAML.
        /// </summary>
        /// <value>The now in CAML.</value>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of static members discouraged in favor of non-static public member for more consistency with dependency injection")]
        public string NowInCAML
        {
            get
            {
                return "<Value Type=\"DateTime\" IncludeTimeValue=\"TRUE\">" + SPUtility.CreateISO8601DateTimeFromSystemDateTime(DateTime.Now) + "</Value>";
            }
        }

        /// <summary>
        /// Returns a string corresponding the the ViewFields attribute of a SPQuery
        /// with all the properties of a particular Entity
        /// </summary>
        /// <param name="entityType">The type of the entity</param>
        /// <returns>A string representing the list of view fields</returns>
        public string ViewFieldsForEntityType(Type entityType)
        {
            string viewFieldsString = string.Empty;
            PropertyInfo[] propertyInfos = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo info in propertyInfos)
            {
                var customAttributes = info.GetCustomAttributes(typeof(PropertyAttribute), false);
                var propertyDetails = customAttributes.OfType<PropertyAttribute>().FirstOrDefault();

                if (propertyDetails != null)
                {
                    var fieldInternalName = !string.IsNullOrEmpty(propertyDetails.PropertyName) ? propertyDetails.PropertyName : info.Name;
                    viewFieldsString += string.Format(CultureInfo.InvariantCulture, "<FieldRef Name='{0}' />", fieldInternalName);
                }
            }

            return viewFieldsString;
        }

        /// <summary>
        /// Trimming utility for rich text content returned from SPQueries
        /// </summary>
        /// <param name="toTrim">The content to trim</param>
        /// <returns>The trimmed string</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of static members discouraged in favor of non-static public member for more consistency with dependency injection")]
        public string TrimIfNotNullOrEmpty(string toTrim)
        {
            string trimmed = string.Empty;
            if (!string.IsNullOrEmpty(toTrim))
            {
                // trim zero-width-space characters (HTML: &#8203; or &#x200b;) that SharePoint likes to insert automatically
                trimmed = toTrim.Trim('\u200B');
            }

            return trimmed;
        }

        /// <summary>
        /// Generates a SPQuery filter for Taxonomy Term
        /// </summary>
        /// <param name="list">The list over which the query will be done</param>
        /// <param name="taxonomyFieldInternalName">The name of the site column associated with the term set</param>
        /// <param name="term">Term to match for</param>
        /// <param name="includeDescendants">Whether the Term's child terms should be query hits as well</param>
        /// <returns>The SPQuery filter</returns>
        public string TermFilter(SPList list, string taxonomyFieldInternalName, Term term, bool includeDescendants)
        {
            return this.TermFilter(list, taxonomyFieldInternalName, new List<Term>() { term }, includeDescendants);
        }

        /// <summary>
        /// Generates a SPQuery filter for Taxonomy Term
        /// </summary>
        /// <param name="list">The list over which the query will be done</param>
        /// <param name="taxonomyFieldInternalName">The name of the site column associated with the term set</param>
        /// <param name="terms">List of terms for why we want to match in an OR fashion</param>
        /// <param name="includeDescendants">Whether the Term's child terms should be query hits as well</param>
        /// <returns>The SPQuery filter</returns>
        public string TermFilter(SPList list, string taxonomyFieldInternalName, IList<Term> terms, bool includeDescendants)
        {
            string values = string.Empty;

            foreach (var term in terms)
            {
                try
                {
                    values += this.GetAllWssIdByTerm(list, term, includeDescendants);
                }
                catch (ArgumentException)
                {
                    // ignore the not-found labels
                }
            }

            // Filter over the taxonomy field with the proper SID lookup id to the taxonomy hidden list
            if (!string.IsNullOrEmpty(values))
            {
                var query = string.Format(
                    CultureInfo.InvariantCulture,
                    "<In><FieldRef Name='{0}' LookupId='TRUE'/><Values>{1}</Values></In>",
                    taxonomyFieldInternalName,
                    values);

                return query;
            }

            return string.Empty;
        }

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
        public string TermFilter(SPList list, string taxonomyFieldInternalName, string termSetName, string termLabel, bool includeDescendants)
        {
            var taxonomyTerm = this.taxonomyService.GetTermForLabel(list.ParentWeb.Site, termSetName, termLabel);

            if (taxonomyTerm == null)
            {
                string msg = string.Format(CultureInfo.InvariantCulture, "Unable to find term with label '{0}' in site '{1}' while creating query filter.", termLabel, list.ParentWeb.Site);
                throw new ArgumentException(msg);
            }

            return this.TermFilter(list, taxonomyFieldInternalName, taxonomyTerm, includeDescendants);
        }

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
        public string TermFilter(SPList list, string taxonomyFieldInternalName, string termStoreGroupName, string termSetName, string termLabel, bool includeDescendants)
        {
            var taxonomyTerm = this.taxonomyService.GetTermForLabel(list.ParentWeb.Site, termStoreGroupName, termSetName, termLabel);

            if (taxonomyTerm == null)
            {
                string msg = string.Format(CultureInfo.InvariantCulture, "Unable to find term with label '{0}' in site '{1}' while creating query filter.", termLabel, list.ParentWeb.Site);
                throw new ArgumentException(msg);
            }

            return this.TermFilter(list, taxonomyFieldInternalName, new List<Term>() { taxonomyTerm }, includeDescendants);
        }

        private string GetAllWssIdByTerm(SPList list, Term term, bool includeDescendants)
        {
            if (term != null)
            {
                // Get the lookup Ids of all taxonomy field values that point to this term or its decendants in the taxonomy hidden list
                int[] wssIds = TaxonomyField.GetWssIdsOfTerm(list.ParentWeb.Site, term.TermStore.Id, term.TermSet.Id, term.Id, includeDescendants, int.MaxValue);
                if (wssIds.Count() > 0)
                {
                    // Filter over the taxonomy field with the proper SID lookup id to the taxonomy hidden list
                    return string.Join(string.Empty, wssIds.Select(wssId => "<Value Type=\"Integer\">" + wssId + "</Value>").ToArray());
                }
                else
                {
                    this.log.Warn("Failed to find any item in the site collection that matches the term '" + term.Name + "'");
                    throw new ArgumentException("No usage found for term with id " + term.Id);
                }
            }
            else
            {
                throw new ArgumentNullException("term");
            }
        }
    }
}
