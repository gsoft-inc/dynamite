using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.SharePoint.Publishing.Navigation;

namespace GSoft.Dynamite.Navigation
{
    /// <summary>
    /// Navigation Node class.
    /// </summary>
    public class NavigationNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationNode"/> class.
        /// </summary>
        public NavigationNode()
        {
            this.ChildNodes = new List<NavigationNode>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationNode"/> class using a navigation term.
        /// </summary>
        /// <param name="term">The navigation term.</param>
        public NavigationNode(NavigationTerm term)
            : this()
        {
            this.Id = term.Id;
            this.ParentNodeId = (term.Parent != null) ? term.Parent.Id : Guid.Empty;
            this.Title = term.Title.Value;
            this.Url = new Uri(term.GetResolvedDisplayUrl(string.Empty), UriKind.RelativeOrAbsolute);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationNode"/> class.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <param name="currentTerm">The current term.</param>
        /// <param name="currentBranchTerms">The terms in the current branch.</param>
        public NavigationNode(NavigationTerm term, NavigationTerm currentTerm, IEnumerable<NavigationTerm> currentBranchTerms)
            : this(term)
        {
            this.SetCurrentBranchProperties(currentTerm, currentBranchTerms);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationNode"/> class.
        /// </summary>
        /// <param name="row">The data row from a SharePoint search results table.</param>
        /// <param name="navigationManagedProperty">The navigation managed property.</param>
        public NavigationNode(DataRow row, string navigationManagedProperty)
            : this()
        {
            this.Title = row["Title"].ToString();
            this.ParentNodeId = ExtractNavigationTermGuid(row[navigationManagedProperty].ToString());

            // TODO: this assumes that Path returned by search result is NOT relative (i.e. absolute). 
            // If the path is relative, we need to forward UriKind.Relative and figure out what the impact 
            // of relative Uris is down the road.
            this.Url = new Uri(row["Path"].ToString(), UriKind.RelativeOrAbsolute);
        }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        public Uri Url { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [is current node].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is current node]; otherwise, <c>false</c>.
        /// </value>
        public bool IsCurrentNode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [is node in current branch].
        /// </summary>
        /// <value>
        /// <c>true</c> if [is node in current branch]; otherwise, <c>false</c>.
        /// </value>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "InCurrent", Justification = "The meaning of this method is 'Is Node In Current Branch', nothing about 'Incurrent' here...")]
        public bool IsNodeInCurrentBranch { get; set; }

        /// <summary>
        /// Gets or sets the parent node ID.
        /// </summary>
        /// <value>
        /// The parent node ID.
        /// </value>
        public Guid ParentNodeId { get; set; }

        /// <summary>
        /// Gets or sets the child nodes.
        /// </summary>
        /// <value>
        /// The child nodes.
        /// </value>
        public IEnumerable<NavigationNode> ChildNodes { get; set; }

        /// <summary>
        /// Sets the current branch properties for the node.
        /// </summary>
        /// <param name="currentTerm">The current term.</param>
        /// <param name="currentBranchTerms">The current branch terms.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "We want to keep the full NavigationTerm information in case we need it in the future.")]
        public void SetCurrentBranchProperties(NavigationTerm currentTerm, IEnumerable<NavigationTerm> currentBranchTerms)
        {
            this.IsCurrentNode = currentTerm != null && currentTerm.Id.Equals(this.Id);
            this.IsNodeInCurrentBranch = currentBranchTerms.Any(y => y.Id.Equals(this.Id));
        }

        private static Guid ExtractNavigationTermGuid(string navigationManagedPropertyValue)
        {
            var match = Regex.Match(navigationManagedPropertyValue, @"(?<=GP0\|#)\b[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}\b", RegexOptions.IgnoreCase);
            return match.Success ? new Guid(match.ToString()) : Guid.Empty;
        }
    }
}