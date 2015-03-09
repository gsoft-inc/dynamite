using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Office.Server.Search.Administration;

namespace GSoft.Dynamite.Search
{
    /// <summary>
    /// Definition for a managed property
    /// </summary>
    public class ManagedPropertyInfo
    {
        /// <summary>
        /// Default constructor for serialization purposes
        /// </summary>
        public ManagedPropertyInfo()
        {
        }

        /// <summary>
        /// Initializes a new ManagedPropertyInfo
        /// </summary>
        /// <param name="name">The name of the managed property</param>
        /// <param name="type">The type of the managed property</param>
        public ManagedPropertyInfo(string name, ManagedDataType type) : this(name)
        {
            this.DataType = type;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="name">Name of the managed property</param>
        public ManagedPropertyInfo(string name)
        {
            this.Name = name;
            this.CrawledProperties = new Dictionary<string, int>();

            // Default configuration
            this.Sortable = false;
            this.Queryable = true;
            this.Searchable = true;
            this.Refinable = true;
            this.RespectPriority = false;
            this.HasMultipleValues = false;
            this.FullTextIndex = "Default";
            this.SafeForAnonymous = true;
            this.Context = 2;
            this.OverwriteIfAlreadyExists = true;
        }

        /// <summary>
        /// Name of the managed property
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the context group this managed property should be ranked in.
        /// </summary>
        public short Context { get; set; }

        /// <summary>
        /// Gets or sets whether this managed property can be queried with a scoped query.
        /// </summary>
        public bool Queryable { get; set; }

        /// <summary>
        /// Gets or sets whether this managed property should end up in the full text index.
        /// </summary>
        public bool Searchable { get; set; }

        /// <summary>
        /// Gets or sets a Boolean value indicating whether a managed property value is retrievable.
        /// </summary>
        public bool Retrievable { get; set; }

        /// <summary>
        /// Gets or sets whether this managed property can be sorted.
        /// </summary>
        public bool Sortable { get; set; }

        /// <summary>
        /// Gets or sets the type of sort info created for this managed property.
        /// </summary>
        public SortableType SortableType { get; set; }

        /// <summary>
        /// Gets or sets whether this managed property has refiners enabled.
        /// </summary>
        public bool Refinable { get; set; }

        /// <summary>
        /// Gets the data type of a managed property.
        /// </summary>
        public ManagedDataType DataType { get; set; }

        /// <summary>
        /// Mapped crawled properties names
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Allow overwrite of backing store to enable easier initialization of object.")]
        public IDictionary<string, int> CrawledProperties { get; set; }

        /// <summary>
        /// Gets whether this managed property will only be mapped from the single crawled property with the lowest mapping order, or from all mapped crawled properties.
        /// </summary>
        public bool RespectPriority { get; set; }

        /// <summary>
        /// Gets a Boolean value indicating whether a managed property contains multiple values.
        /// </summary>
        public bool HasMultipleValues { get; set; }

        /// <summary>
        /// Gets or sets the name of the full-text index catalog this managed property is indexed in.
        /// </summary>
        public string FullTextIndex { get; set; }

        /// <summary>
        /// Gets or sets whether this managed property should be returned for queries executed by anonymous users.
        /// </summary>
        public bool SafeForAnonymous { get; set; }

        /// <summary>
        /// Gets or sets the upgrade behavior of this managed property definition.
        /// If true, any existing managed property will be deleted, then re-created
        /// when re-ensured through ISearchHelper.EnsureManagedProperty.
        /// </summary>
        public bool OverwriteIfAlreadyExists { get; set; }
    }
}
