using System.Collections.Generic;
using Microsoft.Office.Server.Search.Administration;
using Microsoft.Office.Server.Search.Query;
using Microsoft.SqlServer.Server;

namespace GSoft.Dynamite.Definitions
{
    /// <summary>
    /// Definition of a search result source
    /// </summary>
    public class ResultSourceInfo
    {
        private string _searchProvider;

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
        public IDictionary<string, SortDirection> SortSettings { get; set; }

        /// <summary>
        /// If true, overwrite the result source if existing
        /// </summary>
        public bool Overwrite { get; set; }

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
