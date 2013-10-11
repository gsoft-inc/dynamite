using System;
using System.Diagnostics.CodeAnalysis;

namespace GSoft.Dynamite.Utils
{  
    /// <summary>
    /// Search service utilities
    /// </summary>
    public class SearchHelper
    {
        /// <summary>
        /// Creates a site search scope if it doesn't exist yet
        /// </summary>
        /// <param name="site">The site collection</param>
        /// <param name="scopeName">The name of the search scope</param>
        /// <param name="displayGroupName">The scope's display group</param>
        /// <param name="searchPagePath">The scope's custom search page url (cannot be empty)</param>
        /// <returns>The search scope</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public Scope EnsureSiteScope(SPSite site, string scopeName, string displayGroupName, string searchPagePath)
        {
            // remotescopes class retrieves information via search web service so we run this as the search service account
            RemoteScopes remoteScopes = new RemoteScopes(SPServiceContext.GetContext(site));

            // see if there is an existing scope
            Scope scope = remoteScopes.GetScopesForSite(new Uri(site.Url)).Cast<Scope>().FirstOrDefault(s => s.Name == scopeName);
            
            // only add if the scope doesn't exist already
            if (scope == null)
            {
                scope = remoteScopes.AllScopes.Create(scopeName, string.Empty, new Uri(site.Url), true, searchPagePath, ScopeCompilationType.AlwaysCompile);
            }

            // see if there is an existing display group         
            ScopeDisplayGroup displayGroup = remoteScopes.GetDisplayGroupsForSite(new Uri(site.Url)).Cast<ScopeDisplayGroup>().FirstOrDefault(d => d.Name == displayGroupName);

            // add if the display group doesn't exist
            if (displayGroup == null)
            {
                displayGroup = remoteScopes.AllDisplayGroups.Create(displayGroupName, string.Empty, new Uri(site.Url), true);
            }

            // add scope to display group if not already added
            if (!displayGroup.Contains(scope))
            {
                displayGroup.Add(scope);
                displayGroup.Update();
            }

            // optionally force a scope compilation so this is available immediately
            remoteScopes.StartCompilation();

            return scope;
        }

        /// <summary>
        /// Creates a farm-wide shared search scope
        /// </summary>
        /// <param name="site">The site collection of the context</param>
        /// <param name="scopeName">The name of the shared scope to create</param>
        /// <param name="displayGroupName">The search scope display group name</param>
        /// <param name="searchPagePath">Path to scope-specific search page</param>
        /// <returns>The newly created scope</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public Scope EnsureSharedScope(SPSite site, string scopeName, string displayGroupName, string searchPagePath)
        {
            // remotescopes class retrieves information via search web service so we run this as the search service account
            RemoteScopes remoteScopes = new RemoteScopes(SPServiceContext.GetContext(site));

            // see if there is an existing scope
            Scope scope = remoteScopes.GetSharedScopes().Cast<Scope>().FirstOrDefault(s => s.Name == scopeName);

            // only add if the scope doesn't exist already
            if (scope == null)
            {
                scope = remoteScopes.AllScopes.Create(scopeName, string.Empty, null, true, searchPagePath, ScopeCompilationType.AlwaysCompile);
            }

            // see if there is an existing display group         
            ScopeDisplayGroup displayGroup = remoteScopes.GetDisplayGroupsForSite(new Uri(site.Url)).Cast<ScopeDisplayGroup>().FirstOrDefault(d => d.Name == displayGroupName);

            // add if the display group doesn't exist
            if (displayGroup == null)
            {
                displayGroup = remoteScopes.AllDisplayGroups.Create(displayGroupName, string.Empty, new Uri(site.Url), true);
            }

            // add scope to display group if not already added
            if (!displayGroup.Contains(scope))
            {
                displayGroup.Add(scope);
                displayGroup.Update();
            }

            // optionally force a scope compilation so this is available immediately
            remoteScopes.StartCompilation();

            return scope;
        }
    }
}
