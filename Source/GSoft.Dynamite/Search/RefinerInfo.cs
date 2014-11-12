using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using GSoft.Dynamite.Search.Enums;

namespace GSoft.Dynamite.Search
{
    /// <summary>
    /// Defines a refiner for a faceted search configuration
    /// </summary>
    public class RefinerInfo
    {
        /// <summary>
        /// Creates a new refiner info
        /// </summary>
        /// <param name="managedPropertyName">The managed property name</param>
        /// <param name="type">The type of the refiner</param>
        /// <param name="displayTemplateJsFile">The display template url (relative site collection JavaScript file)</param>
        /// <param name="displayName">The display name which will appear in the refinement panel web part</param>
        /// <param name="maxNumberRefinementOptions">The maximum number of refinements</param>
        /// <param name="sortBy">The sort by</param>
        /// <param name="sortOrder">The sort order</param>
        /// <param name="intervals">The intervals (only if it is a DateTime)</param>
        /// <param name="useDefaultDateIntervals">Specifies if the refiner must use the default intervals (only in the case of DateTime)</param>
        public RefinerInfo(
            string managedPropertyName,
            RefinerType type, 
            string displayTemplateJsFile,
            string displayName, 
            int maxNumberRefinementOptions, 
            RefinerSortBy sortBy,
            RefinerSortOrder sortOrder, 
            string intervals, 
            bool useDefaultDateIntervals)
        {
            this.ManagedPropertyName = managedPropertyName;
            this.RefinerType = type;
            this.DisplayTemplateJsLocation = displayTemplateJsFile;
            this.DisplayName = displayName;
            this.MaxNumberRefinementOptions = maxNumberRefinementOptions;
            this.SortBy = sortBy;
            this.SortOrder = sortOrder;
            this.Intervals = intervals;
            this.UseDefaultDateIntervals = useDefaultDateIntervals;
        }

        /// <summary>
        /// Creates a new refiner info
        /// </summary>
        /// <param name="managedPropertyName">The managed property name</param>
        /// <param name="type">The type of the refiner</param>
        /// <param name="isMultivalue">Specifies the default display template to use (simple or multi value)</param>
        public RefinerInfo(string managedPropertyName, RefinerType type, bool isMultivalue)
        {
            this.ManagedPropertyName = managedPropertyName;
            this.RefinerType = type;
            this.DisplayTemplateJsLocation = isMultivalue ? "~sitecollection/_catalogs/masterpage/Display Templates/Filters/Filter_MultiValue.js" : "~sitecollection/_catalogs/masterpage/Display Templates/Filters/Filter_Default.js";
            
            // Default values
            this.SortBy = RefinerSortBy.Count;
            this.SortOrder = RefinerSortOrder.Descending;
            this.MaxNumberRefinementOptions = 15;
            this.RefinerSpecStringOverride = string.Empty;
            this.DisplayName = string.Empty;
            this.Intervals = "null";
        }
     
        /// <summary>
        /// The sort order. Possible values: "ascending", "descending"
        /// </summary>
        public RefinerSortOrder SortOrder { get; set; }

        /// <summary>
        /// The maximum number or refinements that appear in the refinement panel web part
        /// </summary>
        public int MaxNumberRefinementOptions { get; set; }

        /// <summary>
        /// Specifies the search managed property name which the refiner belongs to
        /// </summary>
        public string ManagedPropertyName { get; set; }

        /// <summary>
        /// If no display template is chosen, specifies the default display template to use (simple or multi value)
        /// </summary>
        public bool IsMultivalue { get; set; }

        /// <summary>
        /// The refiner type (Text or DateTime)
        /// </summary>
        public RefinerType RefinerType { get; set; }

        /// <summary>
        /// The sort by option (By name, By count or By number)
        /// </summary>
        public RefinerSortBy SortBy { get; set; }

        /// <summary>
        /// Relative site collection (with tilde) display template JavaScript file url
        /// </summary>
        public string DisplayTemplateJsLocation { get; set; }

        /// <summary>
        /// The display name which will appear in the refinement panel web part
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Specifies if the refiner must use the default intervals (only in the case of DateTime)
        /// </summary>
        public bool UseDefaultDateIntervals { get; set; }

        /// <summary>
        /// Specifies an override of the refiner specification
        /// </summary>
        public string RefinerSpecStringOverride { get; set; }

        /// <summary>
        /// The intervals (only if it is a DateTime)
        /// </summary>
        public string Intervals { get; set; }

        /// <summary>
        /// The auto calculated refinement string for the term custom property
        /// </summary>
        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "We want lowercase here.")]
        public string RefinementString
        {
            get
            {
                var refinementString = string.Empty;
                var fp = CultureInfo.InvariantCulture;

                if (this.RefinerType.Equals(RefinerType.DateTime))
                {
                    var tommorow = DateTime.Now.ToUniversalTime().AddDays(1).ToString("yyyy-MM-ddThh:mm:ssZ", fp);
                    var today = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddThh:mm:ssZ", fp);
                    var lastYear = DateTime.Now.ToUniversalTime().AddMonths(-12).ToString("yyyy-MM-ddThh:mm:ssZ", fp);
                    var lastWeek = DateTime.Now.ToUniversalTime().AddDays(-7).ToString("yyyy-MM-ddThh:mm:ssZ", fp);

                    refinementString = this.ManagedPropertyName + "(discretize=manual/" + lastYear + "/" +
                                               tommorow + "/" + lastWeek + "/" + today + ")";
                }

                if (this.RefinerType.Equals(RefinerType.Text))
                {
                    refinementString = this.ManagedPropertyName + "(sort=" + this.SortBy.ToString().ToLowerInvariant() +
                                           "/" + this.SortOrder.ToString().ToLowerInvariant() + ",filter=" +
                                           this.MaxNumberRefinementOptions + "/0/*)";
                }

                return refinementString;
            }
        }

        /// <summary>
        /// The alias of the refiner
        /// </summary>
        public string Alias
        {
            get
            {
                var alias = string.Empty;

                if (this.RefinerType.Equals(RefinerType.DateTime))
                {
                    alias = "[\"DocCreatedTm\",\"urn:schemas-microsoft-com:office:office#Created\",\"DAV:creationdate\"]";
                }

                if (this.RefinerType.Equals(RefinerType.Text))
                {
                    alias = "null";
                }

                return alias;
            }
        }
    }
}
