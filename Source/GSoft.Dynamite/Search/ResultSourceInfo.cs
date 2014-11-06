using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Office.Server.Search.Administration;
using Microsoft.Office.Server.Search.Query;
using Microsoft.SharePoint.JSGrid;
using Microsoft.SqlServer.Server;

namespace GSoft.Dynamite.Search
{
    /// <summary>
    /// Definition of a search result source
    /// </summary>
    public class ResultSourceInfo
    {
        private string _searchProvider;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ResultSourceInfo()
        {
            this.UpdateMode = UpdateBehavior.NoChangesIfAlreadyExists;
        }

        /// <summary>
        /// The update mode for the result source
        /// </summary>
        public enum UpdateBehavior
        {
            /// <summary>
            /// Delete and recreate the result source if already exists
            /// </summary>
            OverwriteResultSource,

            /// <summary>
            /// Overwrite only the query string of the result source
            /// </summary>
            OverwriteQuery,

            /// <summary>
            /// Append string to the existing query
            /// </summary>
            AppendToQuery,

            /// <summary>
            /// Rollback the query to its previous state
            /// </summary>
            RevertQuery,

            /// <summary>
            /// Don't make any changes on the result source if already exists
            /// </summary>
            NoChangesIfAlreadyExists
        }

        /// <summary>
        /// Name of the result source
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Level of the result source
        /// </summary>
        public SearchObjectLevel Level { get; set; }

        /// <summary>
        /// The sorting setting by field. The Key corresponds the field name.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Allow overwrite of backing store to enable easier initialization of object.")]
        public IDictionary<string, SortDirection> SortSettings { get; set; }

        /// <summary>
        /// Set the update behavior for the result source
        /// </summary>
        public UpdateBehavior UpdateMode { get; set; }

        /// <summary>
        /// The KQL Query
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// The Search Provider
        /// </summary>
        public string SearchProvider
        {
            get { return this._searchProvider ?? (this._searchProvider = "Local SharePoint Provider"); }
            set { this._searchProvider = value; }
        }
    }
}
