using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using GSoft.Dynamite.Events;
using GSoft.Dynamite.Taxonomy;
using GSoft.Dynamite.Utils;
using GSoft.Dynamite.ValueTypes;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Taxonomy;

namespace GSoft.Dynamite.Taxonomy
{
    /// <summary>
    /// Helper class for managing Taxonomy.
    /// </summary>
    public class TaxonomyHelper : ITaxonomyHelper
    {
        private const string AssemblyFullName = "Microsoft.SharePoint.Taxonomy, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
        private const string ClassFullName = "Microsoft.SharePoint.Taxonomy.TaxonomyItemEventReceiver";

        private readonly IEventReceiverHelper eventReceiverHelper;
        private readonly ITaxonomyService taxonomyService;

        /// <summary>
        /// Creates a taxonomy helper.
        /// </summary>
        /// <param name="eventReceiverHelper">An event receiver helper.</param>
        /// <param name="taxonomyService">The taxonomy service.</param>
        public TaxonomyHelper(IEventReceiverHelper eventReceiverHelper, ITaxonomyService taxonomyService)
        {
            this.eventReceiverHelper = eventReceiverHelper;
            this.taxonomyService = taxonomyService;
        }

        /// <summary>
        /// Applies a term store mapping to a SharePoint field
        /// </summary>
        /// <param name="site">The current site collection</param>
        /// <param name="field">The site or list column to map to the term store</param>
        /// <param name="columnTermStoreMapping">
        /// The term set or sub-term-specific anchor which will determine what's available in the field's taxonomy picker
        /// </param>
        public void AssignTermStoreMappingToField(SPSite site, SPField field, TaxonomyContext columnTermStoreMapping)
        {
            TaxonomySession session = new TaxonomySession(site);

            TermStore store = null;
            if (columnTermStoreMapping.TermStore == null)
            {
                store = session.DefaultSiteCollectionTermStore;
            }
            else
            {
                store = session.TermStores[columnTermStoreMapping.TermStore.Name];
            }

            Group termStoreGroup = null;
            if (columnTermStoreMapping.Group == null)
            {
                termStoreGroup = store.GetSiteCollectionGroup(site);
            }
            else
            {
                termStoreGroup = store.Groups[columnTermStoreMapping.Group.Name];
            }

            TaxonomyField taxoField = (TaxonomyField)field;

            if (columnTermStoreMapping.TermSubset != null)
            {
                InternalAssignTermSetToTaxonomyField(store, taxoField, termStoreGroup.Id, columnTermStoreMapping.TermSet.Id, columnTermStoreMapping.TermSubset.Id);
            }
            else
            {
                InternalAssignTermSetToTaxonomyField(store, taxoField, termStoreGroup.Id, columnTermStoreMapping.TermSet.Id, Guid.Empty);
            }
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
        /// <param name="termStoreGroupId">The term store group identifier.</param>
        /// <param name="termSetId">The term set identifier.</param>
        /// <param name="termSubsetId">The ID of the term sub set the term is attached to.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public void AssignTermSetToSiteColumn(SPWeb web, Guid fieldId, Guid termStoreGroupId, Guid termSetId, Guid termSubsetId)
        {
            if (web.Fields.Contains(fieldId))
            {
                var session = new TaxonomySession(web.Site);
                var termStore = session.DefaultSiteCollectionTermStore;
                var field = (TaxonomyField)web.Fields[fieldId];
                InternalAssignTermSetToTaxonomyField(termStore, field, termStoreGroupId, termSetId, termSubsetId);
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
        /// <param name="termStoreGroupId">The term store group identifier.</param>
        /// <param name="termSetId">The term set identifier.</param>
        /// <param name="termSubsetId">The ID of the term sub set the term is attached to.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public void AssignTermSetToListColumn(SPList list, Guid fieldId, Guid termStoreGroupId, Guid termSetId, Guid termSubsetId)
        {
            if (list.Fields.Contains(fieldId))
            {
                var session = new TaxonomySession(list.ParentWeb.Site);
                var termStore = session.DefaultSiteCollectionTermStore;
                var field = (TaxonomyField)list.Fields[fieldId];
                InternalAssignTermSetToTaxonomyField(termStore, field, termStoreGroupId, termSetId, termSubsetId);
            }
        }

        /// <summary>
        /// Assigns a global farm-wide term set to a list column
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
        /// Assigns a local site-collection-specific term set to a list column
        /// term store.
        /// </summary>
        /// <param name="list">The list containing the field.</param>
        /// <param name="fieldId">The field to associate with the term set.</param>
        /// <param name="termSetName">The name of the term set to assign to the column.</param>
        /// <param name="termSubsetName">The name of the term sub set the term is attached to. This parameter can be null.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public void AssignTermSetToListColumn(SPList list, Guid fieldId, string termSetName, string termSubsetName)
        {
            if (list.Fields.Contains(fieldId))
            {
                TaxonomySession session = new TaxonomySession(list.ParentWeb.Site);
                TermStore termStore = session.DefaultSiteCollectionTermStore;
                TaxonomyField field = (TaxonomyField)list.Fields[fieldId];
                Group siteCollectionGroup = termStore.GetSiteCollectionGroup(list.ParentWeb.Site);
                InternalAssignTermSetToTaxonomyField(termStore, field, siteCollectionGroup.Name, termSetName, termSubsetName);
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
        public void SetDefaultTaxonomyFieldValue(SPWeb web, SPField field, string termGroupName, string termSetName, string termLabel)
        {
            var taxonomySession = new TaxonomySession(web.Site);
            TermStore termStore = taxonomySession.DefaultSiteCollectionTermStore;

            Group termGroup = null;

            if (!string.IsNullOrEmpty(termGroupName))
            {
                termGroup = termStore.Groups[termGroupName];
            }
            else
            {
                // we're dealing with a site-collection-specifc term group
                termGroup = termStore.GetSiteCollectionGroup(web.Site);
            }

            var termSet = termGroup.TermSets[termSetName];

            // TODO: rework this to work with termIds instead of termLabels, because this logic DOES NOT support duplicate labels (i.e. two separate
            // terms CANNOT have a similar label with this logic)
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
        public void SetDefaultTaxonomyFieldValue(
            SPWeb web, TaxonomyField field, TaxonomyFullValue defaultValue)
        {
            var termGroupName = string.Empty;
            if (defaultValue.Context.Group != null)
            {
                // null Group means we're dealing with a site-collection-specific term group
                termGroupName = defaultValue.Context.Group.Name;
            }

            var defaultLanguage = new CultureInfo(this.GetTermStoreDefaultLanguage(web.Site));

            // Get the term set name according to the default term store language
            var termSetName = defaultValue.Context.TermSet.Labels[defaultLanguage];

            var term = defaultValue.Term;

            if (term != null)
            {
                string label;

               // May arrive if the term label haven't been updated correctly on the source object for the current language
                if (string.IsNullOrEmpty(term.Label) && term.Labels.Count > 0)
                {
                    // Trying to get the default language label
                    label = term.Labels[defaultLanguage];
                }
                else
                {
                    label = term.Label;
                }

                this.SetDefaultTaxonomyFieldValue(web, field, termGroupName, termSetName, label);
            }     
        }

        /// <summary>
        /// Set default value for a multi valued taxonomy site column
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="field">The field.</param>
        /// <param name="termGroupName">Term group name</param>
        /// <param name="termSetName">Term set name</param>
        /// <param name="termLabels">Term labels</param>
        public void SetDefaultTaxonomyFieldMultiValue(SPWeb web, SPField field, string termGroupName, string termSetName, string[] termLabels)
        {
            var taxonomySession = new TaxonomySession(web.Site);
            TermStore termStore = taxonomySession.DefaultSiteCollectionTermStore;

            var labelGuidPairs = new List<string>();

            Group termGroup = null;

            if (!string.IsNullOrEmpty(termGroupName))
            {
                termGroup = termStore.Groups[termGroupName];
            }
            else
            {
                // we're dealing with a site-collection-specifc term group
                termGroup = termStore.GetSiteCollectionGroup(web.Site);
            }

            var termSet = termGroup.TermSets[termSetName];

            foreach (var label in termLabels)
            {
                // TODO: rework this to work with termIds instead of termLabels, because this logic DOES NOT support duplicate labels (i.e. two separate
                // terms CANNOT have a similar label with this logic)
                var term = this.taxonomyService.GetTaxonomyValueForLabel(web.Site, termGroup.Name, termSet.Name, label);

                if (term != null)
                {
                    string labelGuidPair = TaxonomyItem.NormalizeName(term.Label) + TaxonomyField.TaxonomyGuidLabelDelimiter
                                 + term.Id.ToString();

                    labelGuidPairs.Add(labelGuidPair);
                }
            }

            if (labelGuidPairs.Count >= 1)
            {
                var taxonomyFieldValueCollection = new TaxonomyFieldValueCollection(field);

                labelGuidPairs.ForEach(labelGuidPair =>
                    {
                        TaxonomyFieldValue taxoFieldValue = new TaxonomyFieldValue(field);
                        taxoFieldValue.PopulateFromLabelGuidPair(labelGuidPair);

                        taxonomyFieldValueCollection.Add(taxoFieldValue);
                    });

                string collectionValidatedString = field.GetValidatedString(taxonomyFieldValueCollection);

                var lookup = (SPFieldLookup)field;
                lookup.DefaultValue = collectionValidatedString;
                lookup.Update();
            }
        }

        /// <summary>
        /// Set default value for a multi valued taxonomy site column
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="field">The field.</param>
        /// <param name="defaultValueCollection">The default value collection.</param>
        public void SetDefaultTaxonomyFieldMultiValue(SPWeb web, TaxonomyField field, TaxonomyFullValueCollection defaultValueCollection)
        {
            if (defaultValueCollection.Count > 0)
            {
                TaxonomyFullValue firstDefaultValue = defaultValueCollection[0];

                var termGroupName = string.Empty;
                if (firstDefaultValue.Context.Group != null)
                {
                    // null Group means we're dealing with a site-collection-specific term group
                    termGroupName = firstDefaultValue.Context.Group.Name;
                }

                var defaultLanguage = new CultureInfo(this.GetTermStoreDefaultLanguage(web.Site));

                // Get the term set name according to the default term store language
                var termSetName = firstDefaultValue.Context.TermSet.Labels[defaultLanguage];

                string[] labels = defaultValueCollection.Select(fullValue =>
                    {
                        string label;
                        TermInfo fullValueTermInfo = fullValue.Term;

                        // May arrive if the term label haven't been updated correctly on the source object for the current language
                        if (string.IsNullOrEmpty(fullValueTermInfo.Label) && fullValueTermInfo.Labels.Count > 0)
                        {
                            // Trying to get the default language label
                            label = fullValueTermInfo.Labels[defaultLanguage];
                        }
                        else
                        {
                            label = fullValueTermInfo.Label;
                        }

                        return label;
                    }).ToArray();

                this.SetDefaultTaxonomyFieldMultiValue(web, field, termGroupName, termSetName, labels);
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
        /// Get a taxonomy term group by its id
        /// </summary>
        /// <param name="termStore">the term store</param>
        /// <param name="id">The id of the group</param>
        /// <returns>The taxonomy group</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public Group GetTermGroupById(TermStore termStore, Guid id)
        {
            var originalWorkingLanguage = termStore.WorkingLanguage;
            termStore.WorkingLanguage = Language.English.Culture.LCID;
            var group = termStore.GetGroup(id);
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
        /// Get a taxonomy term set bu its id
        /// </summary>
        /// <param name="termStore">The term store</param>
        /// <param name="group">The taxonomy term group</param>
        /// <param name="id">The term set id</param>
        /// <returns>The taxonomy term set</returns>
        public TermSet GetTermSetById(TermStore termStore, Group group, Guid id)
        {
            var originalWorkingLanguage = termStore.WorkingLanguage;
            termStore.WorkingLanguage = Language.English.Culture.LCID;
            var termSet = group.TermSets[id];
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
                            var oneTaxoField = oneField as TaxonomyField;
                            if (oneTaxoField != null)
                            {
                                listFieldsToUpdate.Add(oneTaxoField);
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
        }

        private static void InternalAssignTermSetToTaxonomyField(TermStore termStore, TaxonomyField field, Guid termStoreGroupId, Guid termSetId, Guid termSubsetId)
        {
            Group group = termStore.Groups[termStoreGroupId];
            TermSet termSet = group.TermSets[termSetId];

            // Connect to MMS
            field.SspId = termSet.TermStore.Id;
            field.TermSetId = termSet.Id;
            field.TargetTemplate = string.Empty;

            // Select a sub node of the termset to limit selection
            field.AnchorId = Guid.Empty != termSubsetId ? termSubsetId : Guid.Empty;
            field.Update();
        }
        #endregion
    }
}
