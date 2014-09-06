using System.Diagnostics.CodeAnalysis;

namespace GSoft.Dynamite.Caml
{
    /// <summary>
    /// Enumerables used in CAML queries.
    /// </summary>
    [SuppressMessage(
        "StyleCop.CSharp.DocumentationRules", 
        "SA1650:ElementDocumentationMustBeSpelledCorrectly", 
        Justification = "Enumerables is spelled correctly.  Get your shit together StyleCop!.")]
    public class CamlEnums
    {
        /// <summary>
        /// Sort type.
        /// </summary>
        public enum SortType
        {
            /// <summary>
            /// The ascending sort type
            /// </summary>
            Ascending,

            /// <summary>
            /// The descending sort type
            /// </summary>
            Descending
        }

        /// <summary>
        /// Membership type.
        /// </summary>
        public enum MembershipType
        {
            /// <summary>
            /// The SharePoint web all users membership type
            /// </summary>
            SPWebAllUsers,

            /// <summary>
            /// The SharePoint group membership type
            /// </summary>
            SPGroup,

            /// <summary>
            /// The SharePoint web groups membership type
            /// </summary>
            SPWebGroups,

            /// <summary>
            /// The SharePoint current user groups membership type
            /// </summary>
            CurrentUserGroups,

            /// <summary>
            /// The SharePoint web users membership type
            /// </summary>
            SPWebUsers
        }

        /// <summary>
        /// Base type.
        /// </summary>
        public enum BaseType
        {
            /// <summary>
            /// The generic list base type
            /// </summary>
            GenericList,

            /// <summary>
            /// The document library base type
            /// </summary>
            DocumentLibrary,

            /// <summary>
            /// The discussion forum base type
            /// </summary>
            DiscussionForum,

            /// <summary>
            /// The vote or survey base type
            /// </summary>
            VoteOrSurvey,

            /// <summary>
            /// The issues list base type
            /// </summary>
            IssuesList
        }

        /// <summary>
        /// Query scope.
        /// </summary>
        public enum QueryScope
        {
            /// <summary>
            /// The web only query scope
            /// </summary>
            WebOnly,

            /// <summary>
            /// The recursive query scope
            /// </summary>
            Recursive,

            /// <summary>
            /// The site collection query scope
            /// </summary>
            SiteCollection
        }

        /// <summary>
        /// Auto hyperlink type.
        /// </summary>
        public enum AutoHyperlinkType
        {
            /// <summary>
            /// The none auto hyperlink type
            /// </summary>
            None,

            /// <summary>
            /// The plain  auto hyperlink type
            /// </summary>
            Plain,

            /// <summary>
            /// The HTML encoded  auto hyperlink type
            /// </summary>
            HTMLEncoded
        }

        /// <summary>
        /// URL encoding type.
        /// </summary>
        public enum UrlEncodingType
        {
            /// <summary>
            /// The none URL encoding type
            /// </summary>
            None,

            /// <summary>
            /// The standard URL encoding type
            /// </summary>
            Standard,

            /// <summary>
            /// The encode as URL URL encoding type
            /// </summary>
            EncodeAsUrl
        }
    }
}
