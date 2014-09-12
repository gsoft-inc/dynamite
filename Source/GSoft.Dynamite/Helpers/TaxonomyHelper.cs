using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using GSoft.Dynamite.Definitions;
using GSoft.Dynamite.Definitions.Values;
using GSoft.Dynamite.Taxonomy;
using GSoft.Dynamite.Utils;
using GSoft.Dynamite.ValueTypes;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Taxonomy;

namespace GSoft.Dynamite.Helpers
{
    /// <summary>
    /// Helper class for managing Taxonomy.
    /// </summary>
    public class TaxonomyHelper
    {
        private const string AssemblyFullName = "Microsoft.SharePoint.Taxonomy, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
        private const string ClassFullName = "Microsoft.SharePoint.Taxonomy.TaxonomyItemEventReceiver";

        private readonly EventReceiverHelper eventReceiverHelper;
        private readonly ITaxonomyService taxonomyService;

        /// <summary>
        /// Creates a taxonomy helper.
        /// </summary>
        /// <param name="eventReceiverHelper">An event receiver helper.</param>
        /// <param name="taxonomyService">The taxonomy service.</param>
        public TaxonomyHelper(EventReceiverHelper eventReceiverHelper, ITaxonomyService taxonomyService)
        {
            this.eventReceiverHelper = eventReceiverHelper;
            this.taxonomyService = taxonomyService;
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
                TaxonomyField field = (TaxonomyField)web.Site.RootWeb.Fields[fieldId];
                InternalAssignTermSetToTaxonomyField(termStore, field, termStoreGroupName, termSetName, termSubsetName);
                AssignTermSetToAllListUsagesOfSiteColumn(web.Site, termStore, fieldId, termStoreGroupName, termSetName, termSubsetName);
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
                TaxonomyField field = (TaxonomyField)web.Site.RootWeb.Fields[fieldId];
                InternalAssignTermSetToTaxonomyField(termStore, field, termStoreGroupName, termSetName, termSubsetName);
                AssignTermSetToAllListUsagesOfSiteColumn(web.Site, termStore, fieldId, termStoreGroupName, termSetName, termSubsetName);
            }
        }

        /// <summary>
        /// Assigns a term set to a site column in the default term store from the site collection's reserved group
        /// term store.
        /// </summary>
        /// <param name="web">The web containing the field.</param>
        /// <param name="fieldId">The field to associate with the term set.</param>
        /// <param name="termSetName">The name of the term set to assign to the column.</param>
        /// <param name="termSubsetName">The name of the term sub set the term is attached to. This parameter can be null.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public void AssignTermSetToSiteColumn(SPWeb web, Guid fieldId, string termSetName, string termSubsetName)
        {
            if (web.Fields.Contains(fieldId))
            {
                TaxonomySession session = new TaxonomySession(web.Site);
                TermStore termStore = session.DefaultSiteCollectionTermStore;
                Group siteCollectionGroup = termStore.GetSiteCollectionGroup(web.Site);
                TaxonomyField field = (TaxonomyField)web.Site.RootWeb.Fields[fieldId];
                InternalAssignTermSetToTaxonomyField(termStore, field, siteCollectionGroup.Name, termSetName, termSubsetName);
                AssignTermSetToAllListUsagesOfSiteColumn(web.Site, termStore, fieldId, siteCollectionGroup.Name, termSetName, termSubsetName);
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
        /// <param name="termSubsetId">The ID of the term sub set the term is attached to.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public void AssignTermSetToSiteColumn(SPWeb web, Guid fieldId, string termStoreGroupName, string termSetName, Guid termSubsetId)
        {
            if (web.Fields.Contains(fieldId))
            {
                var session = new TaxonomySession(web.Site);
                var termStore = session.DefaultSiteCollectionTermStore;
                var field = (TaxonomyField)web.Fields[fieldId];
                InternalAssignTermSetToTaxonomyField(termStore, field, termStoreGroupName, termSetName, termSubsetId);
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
                InternalAssignTermSetToTaxonomyField(termStore, field, termStoreGroupName, termSetName, termSubsetName);
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
        /// <param name="termSubsetId">The ID of the term sub set the term is attached to.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public void AssignTermSetToListColumn(SPList list, Guid fieldId, string termStoreGroupName, string termSetName, Guid termSubsetId)
        {
            if (list.Fields.Contains(fieldId))
            {
                var session = new TaxonomySession(list.ParentWeb.Site);
                var termStore = session.DefaultSiteCollectionTermStore;
                var field = (TaxonomyField)list.Fields[fieldId];
                InternalAssignTermSetToTaxonomyField(termStore, field, termStoreGroupName, termSetName, termSubsetId);
            }
        }

        /// <summary>
        /// Assigns a term set to a site column in the default site collection
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
                InternalAssignTermSetToTaxonomyField(termStore, field, termStoreGroupName, termSetName, termSubsetName);
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
            bool hasItemAdding = this.eventReceiverHelper.EventReceiverDefinitionExist(eventReceivers, SPEventReceiverType.ItemAdding, AssemblyFullName, ClassFullName);
            if (!hasItemAdding)
            {
                // Add the ItemAdding event receiver.
                eventReceivers.Add(SPEventReceiverType.ItemAdding, AssemblyFullName, ClassFullName);
            }

            // Check if the ItemUpdating exists in the collection.
            bool hasItemUpdating = this.eventReceiverHelper.EventReceiverDefinitionExist(eventReceivers, SPEventReceiverType.ItemUpdating, AssemblyFullName, ClassFullName);
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
            Assembly taxonomyAssembly = Assembly.Load("Microsoft.SharePoint.Taxonomy, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c");

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
        /// Get the validated string for a Taxonomy Field
        /// </summary>
        /// <param name="web">Web to look for</param>
        /// <param name="fieldName">Field to search</param>
        /// <param name="termGroup">The term group</param>
        /// <param name="termSet">The term set</param>
        /// <param name="termLabel">The term label</param>
        /// <returns>The validated string.</returns>
        public string GetTaxonomyFieldValueValidatedString(SPWeb web, string fieldName, string termGroup, string termSet, string termLabel)
        {
            SPField field = web.Fields.GetFieldByInternalName(fieldName);

            TaxonomyValue term = this.taxonomyService.GetTaxonomyValueForLabel(web.Site, termGroup, termSet, termLabel);

            if (term != null)
            {
                // Must be exist in the Taxonomy Hidden List
                var taxonomyFieldValue = new TaxonomyFieldValue(field);
                taxonomyFieldValue.PopulateFromLabelGuidPair(TaxonomyItem.NormalizeName(term.Label) + "|" + term.Id);

                return taxonomyFieldValue.ValidatedString;
            }

            return string.Empty;
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
            var taxonomySession = new TaxonomySession(web.Site);
            TermStore termStore = taxonomySession.DefaultSiteCollectionTermStore;

            var termGroup = termStore.Groups[termGroupName];
            var termSet = termGroup.TermSets[termSetName];

            var term = this.taxonomyService.GetTaxonomyValueForLabel(web.Site, termGroup.Name, termSet.Name, termLabel);

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
        /// Set a Taxonomy Field value
        /// </summary>
        /// <param name="web">The web</param>
        /// <param name="field">The field</param>
        /// <param name="defaultValue">The taxonomy default value</param>
        public void SetDefaultTaxonomyValue(
            SPWeb web, TaxonomyField field, TaxonomyFieldInfoValue defaultValue)
        {
            var termGroupName = defaultValue.TermGroup.Name;
            var termSetName = defaultValue.TermSet.Labels[new CultureInfo((int)web.Language)];

            if (defaultValue.Values != null)
            {
                if (defaultValue.Values.Length > 1)
                {
                    if (field.AllowMultipleValues)
                    {
                        this.SetDefaultTaxonomyMultiValue(web, field, termGroupName, termSetName, defaultValue.Values.Select(x => x.Name).ToArray());
                    }
                }
                else
                {
                    var firstOrDefault = defaultValue.Values.FirstOrDefault();
                    if (firstOrDefault != null)
                    {
                        this.SetDefaultTaxonomyValue(web, field, termGroupName, termSetName, firstOrDefault.Name);
                    }
                }
            }     
        }

        /// <summary>
        /// Set default value for a multi valued taxonomy site column
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="field">The field.</param>
        /// <param name="termGroupName">Term group name</param>
        /// <param name="termSetName">Term set name</param>
        /// <param name="termLabels">Term label</param>
        public void SetDefaultTaxonomyMultiValue(
            SPWeb web, SPField field, string termGroupName, string termSetName, string[] termLabels)
        {
            var taxonomySession = new TaxonomySession(web.Site);
            TermStore termStore = taxonomySession.DefaultSiteCollectionTermStore;

            var multipleterms = new List<string>();

            var termGroup = termStore.Groups[termGroupName];
            var termSet = termGroup.TermSets[termSetName];

            foreach (var label in termLabels)
            {
                var term = this.taxonomyService.GetTaxonomyValueForLabel(web.Site, termGroup.Name, termSet.Name, label);

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

        /// <summary>
        /// Set a taxonomy value for a SPListItem
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="item">The SPListItem.</param>
        /// <param name="fieldName">Field name to update.</param>
        /// <param name="termGroupName">Term group name.</param>
        /// <param name="termSetName">Term Set Name.</param>
        /// <param name="termLabel">Term Label.</param>
        public void SetTaxonomyFieldValue(SPWeb web, SPListItem item, string fieldName, string termGroupName, string termSetName, string termLabel)
        {
            var term = this.taxonomyService.GetTaxonomyValueForLabel(web.Site, termGroupName, termSetName, termLabel);

            var taxonomySession = new TaxonomySession(web.Site);
            TermStore termStore = taxonomySession.DefaultSiteCollectionTermStore;

            var termGroup = termStore.Groups[termGroupName];
            var termSet = termGroup.TermSets[termSetName];

            var taxField = item.Fields.GetFieldByInternalName(fieldName);

            if (term != null)
            {
                var taxonomyFieldValue = new TaxonomyFieldValue(taxField);
                string path = TaxonomyItem.NormalizeName(term.Label) + TaxonomyField.TaxonomyGuidLabelDelimiter
                              + term.Id.ToString();

                taxonomyFieldValue.PopulateFromLabelGuidPair(path);

                int[] ids = TaxonomyField.GetWssIdsOfTerm(web.Site, termStore.Id, termSet.Id, term.Id, true, 1);

                if (ids.Length == 0)
                {
                    taxonomyFieldValue.WssId = -1;
                }

                ((TaxonomyField)taxField).SetFieldValue(item, taxonomyFieldValue);

                item.Update();
            }
        }

        /// <summary>
        /// Get the default language of the term store
        /// </summary>
        /// <param name="site">The site</param>
        /// <returns>The LCID of the default language</returns>
        public int GetTermStoreDefaultLanguage(SPSite site)
        {
            var taxonomySession = new TaxonomySession(site);
            var termStore = taxonomySession.DefaultSiteCollectionTermStore;

            return termStore.DefaultLanguage;
        }

        #region Private Methods
        private static void AssignTermSetToAllListUsagesOfSiteColumn(SPSite site, TermStore termStore, Guid fieldId, string termStoreGroupName, string termSetName, string termSubsetName)
        {
            var listFieldsToUpdate = new List<TaxonomyField>();

            foreach (SPWeb oneWeb in site.AllWebs)
            {
                foreach (SPList oneList in oneWeb.Lists)
                {
                    foreach (SPField oneField in oneList.Fields)
                    {
                        if (oneField.Id == fieldId)
                        {
                            if (oneField is TaxonomyField)
                            {
                                listFieldsToUpdate.Add((TaxonomyField)oneField);
                            }
                        }
                    }
                }
            }

            // Can't update the fields while iterating over their parent collection, so gotta do it after
            foreach (TaxonomyField taxFieldToReconnect in listFieldsToUpdate)
            {
                InternalAssignTermSetToTaxonomyField(termStore, taxFieldToReconnect, termStoreGroupName, termSetName, termSubsetName);
            }
        }

        private static void InternalAssignTermSetToTaxonomyField(TermStore termStore, TaxonomyField field, string termStoreGroupName, string termSetName, string termSubsetName)
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
                Term term = termSet.GetTerms(termSubsetName, true)[0];
                field.AnchorId = term.Id;
            }
            else
            {
                field.AnchorId = Guid.Empty;
            }

            field.Update();

            termStore.WorkingLanguage = originalWorkingLanguage;
        }

        private static void InternalAssignTermSetToTaxonomyField(TermStore termStore, TaxonomyField field, string termStoreGroupName, string termSetName, Guid termSubsetId)
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
            field.AnchorId = Guid.Empty != termSubsetId ? termSubsetId : Guid.Empty;
            field.Update();

            termStore.WorkingLanguage = originalWorkingLanguage;
        }
        #endregion
    }
}
