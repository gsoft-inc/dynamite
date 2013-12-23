using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using GSoft.Dynamite.Utils;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Taxonomy;

namespace GSoft.Dynamite.Taxonomy
{
    /// <summary>
    /// Helper class for managing Taxonomy.
    /// </summary>
    public class TaxonomyHelper
    {
        private const string ASSEMBLYFULLNAME = "Microsoft.SharePoint.Taxonomy, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
        private const string CLASSFULLNAME = "Microsoft.SharePoint.Taxonomy.TaxonomyItemEventReceiver";
        
        private EventReceiverHelper _eventReceiverHelper;

        /// <summary>
        /// Creates a taxonomy helper
        /// </summary>
        /// <param name="eventReceiverHelper">An event receiver helper</param>
        public TaxonomyHelper(EventReceiverHelper eventReceiverHelper)
        {
            this._eventReceiverHelper = eventReceiverHelper;
        }

        /// <summary>
        /// Assigns a term set to a site column.
        /// </summary>
        /// <param name="web">The web containing the field.</param>
        /// <param name="fieldId">The field to associate with the term set.</param>
        /// <param name="termStoreName">The name of the term store.</param>
        /// <param name="termStoreGroupName">The name of the term store group.</param>
        /// <param name="termSetName">The name of the term set to assign to the column.</param>
        /// <param name="termSubsetName">The name of the term sub set the term is attached to. This parameter can be null.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public void AssignTermSetToSiteColumn(SPWeb web, Guid fieldId, string termStoreName, string termStoreGroupName, string termSetName, string termSubsetName)
        {
            if (web.Fields.Contains(fieldId))
            {
                TaxonomySession session = new TaxonomySession(web.Site);
                TermStore termStore = session.TermStores[termStoreName];
                TaxonomyField field = (TaxonomyField)web.Fields[fieldId];
                AssignTermSetToSiteColumn(termStore, field, termStoreGroupName, termSetName, termSubsetName);
            }
        }

        /// <summary>
        /// Assigns a term set to a site column in the default site collection
        /// term store.
        /// </summary>
        /// <param name="web">The web containing the field.</param>
        /// <param name="fieldId">The field to associate with the term set.</param>
        /// <param name="termStoreGroupName">The name of the term store group.</param>
        /// <param name="termSetName">The name of the term set to assign to the column.</param>
        /// <param name="termSubsetName">The name of the term sub set the term is attached to. This parameter can be null.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public void AssignTermSetToSiteColumn(SPWeb web, Guid fieldId, string termStoreGroupName, string termSetName, string termSubsetName)
        {
            if (web.Fields.Contains(fieldId))
            {
                TaxonomySession session = new TaxonomySession(web.Site);
                TermStore termStore = session.DefaultSiteCollectionTermStore;
                TaxonomyField field = (TaxonomyField)web.Fields[fieldId];
                AssignTermSetToSiteColumn(termStore, field, termStoreGroupName, termSetName, termSubsetName);
            }
        }

        /// <summary>
        /// Ensures the taxonomy event receivers.
        /// </summary>
        /// <param name="eventReceivers">The event receivers definition collection.</param>
        /// <exception cref="System.ArgumentNullException">All null parameters.</exception>
        public void EnsureTaxonomyEventReceivers(SPEventReceiverDefinitionCollection eventReceivers)
        {
            if (eventReceivers == null)
            {
                throw new ArgumentNullException("eventReceivers");
            }

            // Check if the ItemAdding exists in the collection.
            bool hasItemAdding = this._eventReceiverHelper.EventReceiverDefinitionExist(eventReceivers, SPEventReceiverType.ItemAdding, ASSEMBLYFULLNAME, CLASSFULLNAME);
            if (!hasItemAdding)
            {
                // Add the ItemAdding event receiver.
                eventReceivers.Add(SPEventReceiverType.ItemAdding, ASSEMBLYFULLNAME, CLASSFULLNAME);
            }

            // Check if the ItemUpdating exists in the collection.
            bool hasItemUpdating = this._eventReceiverHelper.EventReceiverDefinitionExist(eventReceivers, SPEventReceiverType.ItemUpdating, ASSEMBLYFULLNAME, CLASSFULLNAME);
            if (!hasItemUpdating)
            {
                // Add the ItemUpdating event receiver.
                eventReceivers.Add(SPEventReceiverType.ItemUpdating, ASSEMBLYFULLNAME, CLASSFULLNAME);
            }
        }

        /// <summary>
        /// Changes the Enterprise Keywords setting on a list
        /// </summary>
        /// <remarks>To disable Enterprise Keywords, delete the field from the list manually.</remarks>
        /// <param name="list">The list</param>
        /// <param name="keywordsAsSocialTags">Whether the list's keywords should be used as MySite social tags</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public void EnableListEnterpriseKeywordsSetting(SPList list, bool keywordsAsSocialTags)
        {
            Assembly taxonomyAssembly = Assembly.Load("Microsoft.SharePoint.Taxonomy, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c");

            // Get an instance of internal class for the keyword association
            Type listFieldSettings = taxonomyAssembly.GetType("Microsoft.SharePoint.Taxonomy.MetadataListFieldSettings");

            // Pass the list to the internal class's constructor
            object listSettings = listFieldSettings.GetConstructor(new Type[] { typeof(SPList) }).Invoke(new object[] { list });

            // Get an instance of keyword property and set the boolean
            listFieldSettings.GetProperty("EnableKeywordsField", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(listSettings, true, null);
            listFieldSettings.GetProperty("EnableMetadataPromotion", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(listSettings, keywordsAsSocialTags, null);

            // Update the list
            listFieldSettings.GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(listSettings, null);
        }

        /// <summary>
        /// Gets the term group by name.
        /// </summary>
        /// <param name="termStore">The term store.</param>
        /// <param name="groupName">Name of the group.</param>
        /// <returns>The term group.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public Group GetTermGroupByName(TermStore termStore, string groupName)
        {
            var originalWorkingLanguage = termStore.WorkingLanguage;
            termStore.WorkingLanguage = Language.English.Culture.LCID;
            var group = termStore.Groups[groupName];
            termStore.WorkingLanguage = originalWorkingLanguage;

            return group;
        }

        /// <summary>
        /// Gets the term set by name.
        /// </summary>
        /// <param name="termStore">The term store.</param>
        /// <param name="group">The term group.</param>
        /// <param name="termSetName">Name of the term set.</param>
        /// <returns>The term set.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public TermSet GetTermSetByName(TermStore termStore, Group group, string termSetName)
        {
            var originalWorkingLanguage = termStore.WorkingLanguage;
            termStore.WorkingLanguage = Language.English.Culture.LCID;
            var termSet = group.TermSets[termSetName];
            termStore.WorkingLanguage = originalWorkingLanguage;

            return termSet;
        }

        #region Private Methods
        private static void AssignTermSetToSiteColumn(TermStore termStore, TaxonomyField field, string termStoreGroupName, string termSetName, string termSubsetName)
        {
            int originalWorkingLanguage = termStore.WorkingLanguage;
            termStore.WorkingLanguage = Language.English.Culture.LCID;

            Group group = termStore.Groups[termStoreGroupName];
            TermSet termSet = group.TermSets[termSetName];

            // Connect to MMS
            field.SspId = termSet.TermStore.Id;
            field.TermSetId = termSet.Id;
            field.TargetTemplate = string.Empty;

            // Select a sub node of the termset to limit selection
            if (!string.IsNullOrEmpty(termSubsetName))
            {
                Term term = termStore.GetTerms(termSubsetName, true)[0];
                field.AnchorId = term.Id;
            }
            else
            {
                field.AnchorId = Guid.Empty;
            }

            field.Update();

            termStore.WorkingLanguage = originalWorkingLanguage;
        }
        #endregion Private Methods
    }
}
