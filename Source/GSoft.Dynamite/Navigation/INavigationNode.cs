using System;
using System.Collections.Generic;
using Microsoft.SharePoint.Publishing.Navigation;

namespace GSoft.Dynamite.Navigation
{
    /// <summary>
    /// Navigation Node class interface.
    /// </summary>
    public interface INavigationNode
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        string Title { get; set; }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        string Url { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [is current node].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is current node]; otherwise, <c>false</c>.
        /// </value>
        bool IsCurrentNode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [is node in current branch].
        /// </summary>
        /// <value>
        /// <c>true</c> if [is node in current branch]; otherwise, <c>false</c>.
        /// </value>
        bool IsNodeInCurrentBranch { get; set; }

        /// <summary>
        /// Gets or sets the parent node ID.
        /// </summary>
        /// <value>
        /// The parent node ID.
        /// </value>
        Guid ParentNodeId { get; set; }

        /// <summary>
        /// Gets or sets the child nodes.
        /// </summary>
        /// <value>
        /// The child nodes.
        /// </value>
        IEnumerable<INavigationNode> ChildNodes { get; set; }

        /// <summary>
        /// Sets the current branch properties for the node.
        /// </summary>
        /// <param name="currentTerm">The current term.</param>
        /// <param name="currentBranchTerms">The current branch terms.</param>
        void SetCurrentBranchProperties(NavigationTerm currentTerm, IEnumerable<NavigationTerm> currentBranchTerms);
    }
}
