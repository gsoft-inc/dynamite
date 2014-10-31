using System.Globalization;
using GSoft.Dynamite.Taxonomy;

namespace GSoft.Dynamite.Navigation
{
    public class ManagedNavigationInfo
    {
        public CultureInfo AssociatedLanguage { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether to [add new pages to navigation].
        /// </summary>
        /// <value>
        /// <c>true</c> if [add new pages to navigation]; otherwise, <c>false</c>.
        /// </value>
        public bool AddNewPagesToNavigation { get; private set; }

        public bool CreateFriendlyUrlsForNewsPages { get; private set; }

        /// <summary>
        /// Gets or sets the name of the term set.
        /// </summary>
        /// <value>
        /// The name of the term set.
        /// </value>
        public TermSetInfo TermSet { get; private set; }

        /// <summary>
        /// Gets or sets the name of the term group.
        /// </summary>
        /// <value>
        /// The name of the term group.
        /// </value>
        public TermGroupInfo TermGroup { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [preserve tagging on term set].
        /// </summary>
        /// <value>
        /// <c>true</c> if [preserve tagging on term set]; otherwise, <c>false</c>.
        /// </value>
        public bool PreserveTaggingOnTermSet;

        public ManagedNavigationInfo(TermSetInfo termSet, CultureInfo language)
            : this(termSet, termSet.Group, language,false,false,true)
        {
        }

        public ManagedNavigationInfo(TermSetInfo termSet, TermGroupInfo termGroup, CultureInfo language) : this(termSet, termGroup, language, false, false, true)
        {           
        }

        public ManagedNavigationInfo(TermSetInfo termSet, TermGroupInfo termGroup, CultureInfo language, bool addNewPagesToNavigation,
            bool createFriendlyUrlsForNewsPages, bool preserveTaggingOnTermSet)
        {
            this.PreserveTaggingOnTermSet = preserveTaggingOnTermSet;
            this.TermGroup = termGroup;
            this.AssociatedLanguage = language;
            this.TermSet = termSet;
            this.CreateFriendlyUrlsForNewsPages = createFriendlyUrlsForNewsPages;
            this.AddNewPagesToNavigation = addNewPagesToNavigation;
        }
    }
}
