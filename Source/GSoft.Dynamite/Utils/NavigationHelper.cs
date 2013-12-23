using GSoft.Dynamite.Navigation;
using GSoft.Dynamite.Taxonomy;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing.Navigation;
using Microsoft.SharePoint.Taxonomy;

namespace GSoft.Dynamite.Utils
{
    /// <summary>
    /// Navigation configuration helper.
    /// </summary>
    public class NavigationHelper
    {
        private readonly TaxonomyHelper _taxonomyHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationHelper" /> class.
        /// </summary>
        /// <param name="taxonomyHelper">The taxonomy helper.</param>
        public NavigationHelper(TaxonomyHelper taxonomyHelper)
        {
            _taxonomyHelper = taxonomyHelper;
        }

        /// <summary>
        /// Sets the web navigation settings.
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="settings">The settings.</param>
        public void SetWebNavigationSettings(SPWeb web, ManagedNavigationSettings settings)
        {
            var taxonomySession = new TaxonomySession(web.Site);
            if (taxonomySession.TermStores.Count > 0)
            {
                var termStore = taxonomySession.TermStores[settings.TermStoreName];
                var group = _taxonomyHelper.GetTermGroupByName(termStore, settings.TermGroupName);
                var termSet = _taxonomyHelper.GetTermSetByName(termStore, group, settings.TermSetName);
                var navigationSettings = new WebNavigationSettings(web);
                
                navigationSettings.GlobalNavigation.TermStoreId = termStore.Id;
                navigationSettings.GlobalNavigation.TermSetId = termSet.Id;
                navigationSettings.Update(taxonomySession);

                if (settings.PreserveTaggingOnTermSet)
                {
                    termSet.IsAvailableForTagging = true;
                    termStore.CommitAll(); 
                }
            }
        }
    }
}
