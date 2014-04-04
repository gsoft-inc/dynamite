using System;
using System.Collections.Generic;
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
        private const string AssemblyFullName = "Microsoft.SharePoint.Taxonomy, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
        private const string ClassFullName = "Microsoft.SharePoint.Taxonomy.TaxonomyItemEventReceiver";
        
        private readonly EventReceiverHelper _eventReceiverHelper;
        private readonly ITaxonomyService _taxonomyService;

        /// <summary>
        /// Creates a taxonomy helper.
        /// </summary>
        /// <param name="eventReceiverHelper">An event receiver helper.</param>
        /// <param name="taxonomyService">The taxonomy service.</param>
        public TaxonomyHelper(EventReceiverHelper eventReceiverHelper, ITaxonomyService taxonomyService)
        {
            this._eventReceiverHelper = eventReceiverHelper;
            this._taxonomyService = taxonomyService;
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
        /// Assigns a term set to a list column.
        /// </summary>
        /// <param name="list">The list containing the field.</param>
        /// <param name="fieldId">The field to associate with the term set.</param>
        /// <param name="termStoreName">The name of the term store.</param>
        /// <param name="termStoreGroupName">The name of the term store group.</param>
        /// <param name="termSetName">The name of the term set to assign to the column.</param>
        /// <param name="termSubsetName">The name of the term sub set the term is attached to. This parameter can be null.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public void AssignTermSetToListColumn(SPList list, Guid fieldId, string termStoreName, string termStoreGroupName, string termSetName, string termSubsetName)
        {
            if (list.Fields.Contains(fieldId))
            {
                TaxonomySession session = new TaxonomySession(list.ParentWeb.Site);
                TermStore termStore = session.TermStores[termStoreName];
                TaxonomyField field = (TaxonomyField)list.Fields[fieldId];
                AssignTermSetToSiteColumn(termStore, field, termStoreGroupName, termSetName, termSubsetName);
            }
        }

        /// <summary>
        /// Assigns a term set to a list column in the default site collection
        /// term store.
        /// </summary>
        /// <param name="list">The list containing the field.</param>
        /// <param name="fieldId">The field to associate with the term set.</param>
        /// <param name="termStoreGroupName">The name of the term store group.</param>
        /// <param name="termSetName">The name of the term set to assign to the column.</param>
        /// <param name="termSubsetName">The name of the term sub set the term is attached to. This parameter can be null.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public void AssignTermSetToListColumn(SPList list, Guid fieldId, string termStoreGroupName, string termSetName, string termSubsetName)
        {
            if (list.Fields.Contains(fieldId))
            {
                TaxonomySession session = new TaxonomySession(list.ParentWeb.Site);
                TermStore termStore = session.DefaultSiteCollectionTermStore;
                TaxonomyField field = (TaxonomyField)list.Fields[fieldId];
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
            bool hasItemAdding = this._eventReceiverHelper.EventReceiverDefinitionExist(eventReceivers, SPEventReceiverType.ItemAdding, AssemblyFullName, ClassFullName);
            if (!hasItemAdding)
            {
                // Add the ItemAdding event receiver.
                eventReceivers.Add(SPEventReceiverType.ItemAdding, AssemblyFullName, ClassFullName);
            }

            // Check if the ItemUpdating exists in the collection.
            bool hasItemUpdating = this._eventReceiverHelper.EventReceiverDefinitionExist(eventReceivers, SPEventReceiverType.ItemUpdating, AssemblyFullName, ClassFullName);
            if (!hasItemUpdating)
            {
                // Add the ItemUpdating event receiver.
                eventReceivers.Add(SPEventReceiverType.ItemUpdating, AssemblyFullName, ClassFullName);
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
        /// Set default value for a taxonomy site column
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="field">The field.</param>
        /// <param name="termGroupName">The term group name.</param>
        /// <param name="termSetName">the term set name.</param>
        /// <param name="termLabel">The term label.</param>
        public void SetDefaultTaxonomyValue(SPWeb web, SPField field, string termGroupName, string termSetName, string termLabel)
        {
            var term = this._taxonomyService.GetTaxonomyValueForLabel(web.Site, termGroupName, termSetName, termLabel);

            var taxonomySession = new TaxonomySession(web.Site);
            TermStore termStore = taxonomySession.DefaultSiteCollectionTermStore;

            var termGroup = termStore.Groups[termGroupName];
            var termSet = termGroup.TermSets[termSetName];

            if (term != null)
            {
                var statusTaxonomyFieldDefaultValue = new TaxonomyFieldValue(field);
                string path = TaxonomyItem.NormalizeName(term.Label) + TaxonomyField.TaxonomyGuidLabelDelimiter
                              + term.Id.ToString();
                statusTaxonomyFieldDefaultValue.PopulateFromLabelGuidPair(path);

                int[] ids = TaxonomyField.GetWssIdsOfTerm(web.Site, termStore.Id, termSet.Id, term.Id, true, 1);

                if (ids.Length == 0)
                {
                    statusTaxonomyFieldDefaultValue.WssId = -1;
                }

                statusTaxonomyFieldDefaultValue.TermGuid = statusTaxonomyFieldDefaultValue.TermGuid.ToUpperInvariant();
                field.DefaultValue = statusTaxonomyFieldDefaultValue.ValidatedString;
                field.Update();
            }
        }

        /// <summary>
        /// Set default value for a multi valued taxonomy site column
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="field">The field.</param>
        /// <param name="termGroupName">Term group name</param>
        /// <param name="termSetName">Term set name</param>
        /// <param name="terms">Term label</param>
        public void SetDefaultTaxonomyMultiValue(
            SPWeb web, SPField field, string termGroupName, string termSetName, string[] terms)
        {
            var taxonomySession = new TaxonomySession(web.Site);
            TermStore termStore = taxonomySession.DefaultSiteCollectionTermStore;

            var multipleterms = new List<string>();

            var termGroup = termStore.Groups[termGroupName];
            var termSet = termGroup.TermSets[termSetName];

            foreach (var label in terms)
            {
                var term = this._taxonomyService.GetTaxonomyValueForLabel(web.Site, termGroupName, termSetName, label);

                if (term != null)
                {
                    int[] ids = TaxonomyField.GetWssIdsOfTerm(web.Site, termStore.Id, termSet.Id, term.Id, true, 1);

                    int wssId = -1;

                    if (ids.Length >= 1)
                    {
                        wssId = ids[0];
                    }

                    string path = TaxonomyItem.NormalizeName(term.Label) + TaxonomyField.TaxonomyGuidLabelDelimiter
                                 + term.Id.ToString();

                    multipleterms.Add(wssId + ";#" + path);
                }
            }

            if (multipleterms.Count >= 1)
            {
                string allvalues = string.Join(";#", multipleterms.ToArray());

                var lookup = (SPFieldLookup)field;
                lookup.DefaultValue = allvalues;
                lookup.Update();
            }
        }

        /// <summary>
        /// Gets the term group by name.
        /// </summary>
        /// <param name="termStore">The term store.</param>
        /// <param name="groupName">Name of the group.</param>
        /// <returns>
        /// The term group.
        /// </returns>
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

        #endregion
    }
}
