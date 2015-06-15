using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GSoft.Dynamite.Search.Enums;
using Microsoft.Office.Server.Search.Administration;
using Microsoft.Office.Server.Search.Query;

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
            this.UpdateMode = ResultSourceUpdateBehavior.NoChangesIfAlreadyExists;
            this.SortSettings = new Dictionary<string, SortDirection>();
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
        /// The sorting setting by field. The Key corresponds to the field name.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Allow overwrite of backing store to enable easier initialization of object.")]
        public IDictionary<string, SortDirection> SortSettings { get; set; }

        /// <summary>
        /// Specifies the Ranking Model Id to be used (only taken into account if "Rank" is specified in the SortSettings)
        /// </summary>
        public Guid RankingModelId { get; set; }

        /// <summary>
        /// Set the update behavior for the result source
        /// </summary>
        public ResultSourceUpdateBehavior UpdateMode { get; set; }

        /// <summary>
        /// The KQL Query
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// Whether this result source should be flagged as default result source
        /// when registered on a particular owner (site or search service app).
        /// </summary>
        public bool IsDefaultResultSourceForOwner { get; set; }

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
