using System.Globalization;
using GSoft.Dynamite.Taxonomy;

namespace GSoft.Dynamite.Navigation
{
    /// <summary>
    /// Metadata for taxonomy navigation configuration
    /// </summary>
    public class ManagedNavigationInfo
    {
        /// <summary>
        /// Default constructor for serialization purposes
        /// </summary>
        public ManagedNavigationInfo()
        {            
        }

        /// <summary>
        /// Initializes a new <see cref="ManagedNavigationInfo"/> instance
        /// </summary>
        /// <param name="termSet">Metadata about the term set driving navigation</param>
        /// <param name="language">The current language being configured</param>
        public ManagedNavigationInfo(TermSetInfo termSet, CultureInfo language)
            : this(termSet, language, false, false, true)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="ManagedNavigationInfo"/> instance
        /// </summary>
        /// <param name="termSet">Metadata about the term set driving navigation</param>
        /// <param name="language">The current language being configured</param>
        /// <param name="addNewPagesToNavigation">Whether new pages should be added to navigation term set automatically</param>
        /// <param name="createFriendlyUrlsForNewsPages">Whether catalog-type pages (such as news items) should use friendly URLs</param>
        /// <param name="preserveTaggingOnTermSet">Whether tagging with the term set should still be allowed</param>
        public ManagedNavigationInfo(
            TermSetInfo termSet,
            CultureInfo language, 
            bool addNewPagesToNavigation,
            bool createFriendlyUrlsForNewsPages, 
            bool preserveTaggingOnTermSet)
        {
            this.PreserveTaggingOnTermSet = preserveTaggingOnTermSet;
            this.AssociatedLanguage = language;
            this.TermSet = termSet;
            this.CreateFriendlyUrlsForNewsPages = createFriendlyUrlsForNewsPages;
            this.AddNewPagesToNavigation = addNewPagesToNavigation;
        }

        /// <summary>
        /// Link to navigation's language
        /// </summary>
        public CultureInfo AssociatedLanguage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to [add new pages to navigation].
        /// </summary>
        /// <value>
        /// <c>true</c> if [add new pages to navigation]; otherwise, <c>false</c>.
        /// </value>
        public bool AddNewPagesToNavigation { get; set; }

        /// <summary>
        /// Whether friendly URLs should be created for Catalog-type pages
        /// </summary>
        /// <remarks>
        /// TODO: Change this from News to something more general
        /// </remarks>
        public bool CreateFriendlyUrlsForNewsPages { get; set; }

        /// <summary>
        /// Gets or sets the name of the term set.
        /// </summary>
        /// <value>
        /// The name of the term set.
        /// </value>
        public TermSetInfo TermSet { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [preserve tagging on term set].
        /// </summary>
        /// <value>
        /// <c>true</c> if [preserve tagging on term set]; otherwise, <c>false</c>.
        /// </value>
        public bool PreserveTaggingOnTermSet { get; set; }
    }
}
