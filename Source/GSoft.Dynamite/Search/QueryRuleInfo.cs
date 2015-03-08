using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSoft.Dynamite.Search
{
    public class QueryRuleInfo
    {
        /// <summary>
        /// Creates a new query rule definition
        /// </summary>
        /// <param name="displayName">Display name</param>
        /// <param name="isActive"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        public QueryRuleInfo(string displayName)
        {
            this.DisplayName = displayName;
            this.IsActive = true;
            this.StartDate = null;
            this.EndDate = null;
            this.OverwriteIfAlreadyExists = true;
        }

        /// <summary>
        /// Display name of query rule
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Should the rule be active upon creation?
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Optional start date for query rule schedule
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Optional end date for query rule schedule
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Defines the upgrade behavior of this query rule definition
        /// </summary>
        public bool OverwriteIfAlreadyExists { get; set; }
    }
}
