namespace GSoft.Dynamite.Setup
{
    using Microsoft.SharePoint;

    /// <summary>
    /// List view definition information.
    /// </summary>
    public class ViewInfo
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the view fields.
        /// </summary>
        /// <value>
        /// The view fields.
        /// </value>
        public string[] ViewFields { get; set; }

        /// <summary>
        /// Gets or sets the query.
        /// </summary>
        /// <value>
        /// The query.
        /// </value>
        public string Query { get; set; }

        /// <summary>
        /// Gets or sets the joins.
        /// </summary>
        /// <value>
        /// The joins.
        /// </value>
        public string Joins { get; set; }

        /// <summary>
        /// Gets or sets the projected fields.
        /// </summary>
        /// <value>
        /// The projected fields.
        /// </value>
        public string ProjectedFields { get; set; }

        /// <summary>
        /// Gets or sets the row limit.
        /// </summary>
        /// <value>
        /// The row limit.
        /// </value>
        public uint RowLimit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is paged.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is paged; otherwise, <c>false</c>.
        /// </value>
        public bool IsPaged { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is default view.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is default view; otherwise, <c>false</c>.
        /// </value>
        public bool IsDefaultView { get; set; }

        /// <summary>
        /// Gets or sets the type of the view.
        /// </summary>
        /// <value>
        /// The type of the view.
        /// </value>
        public SPViewCollection.SPViewType ViewType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is personal view.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is personal view; otherwise, <c>false</c>.
        /// </value>
        public bool IsPersonalView { get; set; }
    }
}
