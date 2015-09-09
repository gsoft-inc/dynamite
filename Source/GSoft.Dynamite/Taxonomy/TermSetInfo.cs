using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Taxonomy;

namespace GSoft.Dynamite.Taxonomy
{
    /// <summary>
    /// Encapsulates taxonomy Term Set properties
    /// </summary>
    public class TermSetInfo
    {
        /// <summary>
        /// Default constructor for TermSetInfo for serialization purposes
        /// </summary>
        public TermSetInfo()
        {
            this.Labels = new Dictionary<CultureInfo, string>();     
        }

        /// <summary>
        /// Constructor for single language (CurrentUICulture) TermSetInfo belonging to default site collection term group
        /// </summary>
        /// <param name="id">The term set's ID</param>
        /// <param name="label">The term set's default name</param>
        public TermSetInfo(Guid id, string label)
            : this()
        {
            this.Id = id;
            this.Label = label;
            this.Group = null;      // should assume site-collection specific term group
        }

        /// <summary>
        /// Constructor for single language (CurrentUICulture) TermSetInfo belonging to specific farm-wide term group
        /// </summary>
        /// <param name="id">The term set's ID</param>
        /// <param name="label">The term set's default name</param>
        /// <param name="termGroup">The parent term group</param>
        public TermSetInfo(Guid id, string label, TermGroupInfo termGroup)
            : this(id, label)
        {
            this.Group = termGroup;     // global farm term group
        }

        /// <summary>
        /// Constructor for fully translated TermSetInfo belonging to default site collection term group
        /// </summary>
        /// <param name="id">The term set's ID</param>
        /// <param name="labels">The term set's default labels</param>
        public TermSetInfo(Guid id, IDictionary<CultureInfo, string> labels) 
            : this()
        {
            this.Id = id;
            this.Labels = labels;
            this.Group = null;      // should assume site-collection specific term group
        }

        /// <summary>
        /// Constructor for fully translated TermSetInfo belonging to specific farm-wide term group
        /// </summary>
        /// <param name="id">The term set's ID</param>
        /// <param name="labels">The term set's default labels</param>
        /// <param name="termGroup">The parent term group</param>
        public TermSetInfo(Guid id, IDictionary<CultureInfo, string> labels, TermGroupInfo termGroup)
            : this(id, labels)
        {
            this.Group = termGroup;     // global farm term group
        }

        /// <summary>
        /// Convenience constructor to create TermSetInfo instances from SharePoint
        /// term set objects
        /// </summary>
        /// <param name="sharePointTermSet">The SharePoint taxonomy term set</param>
        public TermSetInfo(TermSet sharePointTermSet)
            : this(sharePointTermSet.Id, sharePointTermSet.Name, new TermGroupInfo(sharePointTermSet.Group))
        {
        }

        /// <summary>
        /// Id of the term set
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Term set label in the current MUI language
        /// </summary>
        public string Label
        {
            get
            {
                // get the label for the current UI thread culture
                return this.Labels.ContainsKey(CultureInfo.CurrentUICulture) ?
                    this.Labels[CultureInfo.CurrentUICulture] : string.Empty;
            }

            set
            {
                // set the label for the current UI thread culture
                this.Labels[CultureInfo.CurrentUICulture] = value;
            }
        }

        /// <summary>
        /// Labels by languages (LCID) for the Term Set
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Allow overwrite of backing store to enable easier initialization of object.")]
        public IDictionary<CultureInfo, string> Labels { get; set; }

        /// <summary>
        /// Parent Term Group definition. If this value is null, assume 
        /// default site collection term group and default farm term store.
        /// </summary>
        public TermGroupInfo Group { get; set; }

        /// <summary>
        /// Given a site collection and a taxonomy session, fetches the parent term group
        /// for the current term set.
        /// </summary>
        /// <param name="currentSession">The current taxonomy session</param>
        /// <param name="currentSite">
        /// The current site collection - used to resolve the site-collection-specific term group whenever the parent TermGroupInfo on the current TermSetInfo is null.
        /// </param>
        /// <returns>The SharePoint taxonomy term group</returns>
        public Group ResolveParentGroup(TaxonomySession currentSession, SPSite currentSite)
        {
            TermStore currentStore = this.ResolveParentTermStore(currentSession);

            if (this.Group == null && currentSite == null)
            {
                string missingSiteErrorMsg = string.Format(
                    CultureInfo.InvariantCulture,
                    "Error while resolving parent term store group of term set ID={0} Name=. Both the TermGroupInfo and currentSite specified were null. Either initialize your TermSetInfo object with a valid TermGroupInfo or specify a currentSite SPSite instance so the SiteCollectionGroup can be resolved.",
                    this.Id,
                    this.Label);

                throw new NotSupportedException(missingSiteErrorMsg);
            }

            if (this.Group == null)
            {
                // Whenever the parent TermGroupInfo is null, by convention, we assume we're dealing with
                // the default site collection term store.
                if (currentStore == null)
                {
                    // If the TermGroupInfo is null, then ResolveParentTermStore should have returned us the DefaultSiteCollectionTermStore.
                    string missingGroupErrorMsg = string.Format(
                        CultureInfo.InvariantCulture,
                        "Error while resolving parent term store group of term set ID={0} Name=. Since the parent TermGroupInfo is null, we assume we're dealing with the DefaultSiteCollectionTermStore. However this default term store appears to not be configured. Please configure your managed metadata service as the 'Default location for site columns' or specify a parent TermGroupInfo on your TermSetInfo.",
                        this.Id,
                        this.Label);

                    throw new NotSupportedException(missingGroupErrorMsg);
                }

                return currentStore.GetSiteCollectionGroup(currentSite);
            }
            else if (currentStore != null)
            {
                return currentStore.Groups[this.Group.Name];
            }
            else
            {
                string missingStoreErrorMsg = string.Format(
                       CultureInfo.InvariantCulture,
                       "Error while resolving parent term store of term set ID={0} Name=. Please configure your managed metadata service as the 'Default location for site columns' or specify a parent TermGroupInfo and a grandparent TermStoreInfo on your TermSetInfo.",
                       this.Id,
                       this.Label);

                throw new NotSupportedException(missingStoreErrorMsg);
            }
        }

        /// <summary>
        /// Given a taxonomy session, fetches the parent term store for the current term set.
        /// </summary>
        /// <param name="currentSession">The current taxonomy session</param>
        /// <returns>The SharePoint managed metadata service instance</returns>
        public TermStore ResolveParentTermStore(TaxonomySession currentSession)
        {
            if (this.Group == null || (this.Group != null && this.Group.TermStore == null))
            {
                // Whenever the parent TermGroupInfo is absent or the grandparent TermStoreInfo
                // is missing, by convention, we assume we're working with the default site collection
                // term store.
                return currentSession.DefaultSiteCollectionTermStore;
            } 
            else
            {
                // Grandparent TermStoreInfo isnt't null so we can fetch the specific store 
                return currentSession.TermStores[this.Group.TermStore.Name];
            }
        }
    }
}
