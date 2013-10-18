using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GSoft.Dynamite.Binding;
using GSoft.Dynamite.Examples.Core.Constants;
using GSoft.Dynamite.ValueTypes;

namespace GSoft.Dynamite.Examples.Core.Entities
{
    /// <summary>
    /// Entity for wall post
    /// </summary>
    public class WallPost : BaseEntity
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public WallPost() : base()
        {
            this.Tags = new TaxonomyValueCollection();
            this.Replies = new List<WallReply>();
        }

        /// <summary>
        /// Text content of post
        /// </summary>
        [Property(WallFields.TextContentName)]
        public string Text { get; set; }

        /// <summary>
        /// Post managed metadata
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Entity binder need to replace collection.")]
        [Property(WallFields.TagsName)]        
        public TaxonomyValueCollection Tags { get; set; }

        /// <summary>
        /// Author of post
        /// </summary>
        [Property(WallFields.AuthorName)]
        public UserValue Author { get; set; }

        /// <summary>
        /// The comments that have this post as parent
        /// </summary>
        public IEnumerable<WallReply> Replies { get; set; }

        /// <summary>
        /// Safe shortcut to Author display name
        /// </summary>
        public string AuthorName
        {
            get
            {
                return this.Author != null ? this.Author.DisplayName : string.Empty;
            }
        }
    }
}
