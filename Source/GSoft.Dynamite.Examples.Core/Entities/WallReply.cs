using System.Diagnostics.CodeAnalysis;
using GSoft.Dynamite.Binding;
using GSoft.Dynamite.Examples.Constants;
using GSoft.Dynamite.ValueTypes;

namespace GSoft.Dynamite.Examples.Entities
{
    /// <summary>
    /// Entity for replies to wall posts
    /// </summary>
    public class WallReply : BaseEntity
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public WallReply() : base()
        {
            this.Tags = new TaxonomyValueCollection();
        }

        /// <summary>
        /// Text content of reply
        /// </summary>
        [Property(WallFields.TextContentName)]
        public string Text { get; set; }

        /// <summary>
        /// Reply managed metadata
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Entity binder need to replace collection.")]
        [Property(WallFields.TagsName)]
        public TaxonomyValueCollection Tags { get; set; }

        /// <summary>
        /// Author of reply
        /// </summary>
        [Property(WallFields.AuthorName)]
        public UserValue Author { get; set; }

        /// <summary>
        /// Parent wall post
        /// </summary>
        [Property(WallFields.PostLookupName)]
        public LookupValue WallPost { get; set; }

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
