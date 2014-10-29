namespace GSoft.Dynamite.Definitions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Xml.Linq;

    using GSoft.Dynamite.FieldTypes;

    using Microsoft.SharePoint;

    public interface IFieldHelper
    {
        /// <summary>
        /// Sets the lookup field to a list.
        /// </summary>
        /// <param name="web">The web the field and list will be in.</param>
        /// <param name="fieldId">The lookup field id.</param>
        /// <param name="listUrl">The list URL of the list we want to get the information from.</param>
        /// <exception cref="System.ArgumentNullException">All null parameters.</exception>
        /// <exception cref="System.ArgumentException">Unable to find the lookup field.;lookupField</exception>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "2#", Justification = "The GetList method for SP requires a string url.")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        [Obsolete("Use method 'SetLookupToList' with SPFieldCollection as first parameter.")]
        void SetLookupToList(SPWeb web, Guid fieldId, string listUrl);

        /// <summary>
        /// Sets the lookup to a list.
        /// </summary>
        /// <param name="fieldCollection">The field collection.</param>
        /// <param name="fieldId">The field identifier of the lookup field.</param>
        /// <param name="lookupList">The lookup list.</param>
        /// <exception cref="System.ArgumentNullException">
        /// fieldCollection
        /// or
        /// fieldId
        /// or
        /// lookupList
        /// </exception>
        /// <exception cref="System.ArgumentException">Unable to find the lookup field.;fieldId</exception>
        void SetLookupToList(SPFieldCollection fieldCollection, Guid fieldId, SPList lookupList);

        /// <summary>
        /// Sets the lookup to a list.
        /// </summary>
        /// <param name="lookupField">The lookup field.</param>
        /// <param name="lookupList">The lookup list.</param>
        /// <exception cref="System.ArgumentNullException">
        /// The parameter 'lookupField' cannot be null.;lookupField
        /// or
        /// The parameter 'lookupList' cannot be null.;lookupList
        /// </exception>
        void SetLookupToList(SPFieldLookup lookupField, SPList lookupList);

        /// <summary>
        /// Gets the field by identifier.
        /// Returns null if the field is not found in the collection.
        /// </summary>
        /// <param name="fieldCollection">The field collection.</param>
        /// <param name="fieldId">The field identifier.</param>
        /// <returns>The SPField.</returns>
        SPField GetFieldById(SPFieldCollection fieldCollection, Guid fieldId);

        /// <summary>Adds a collection of fields defined in xml to a collection of fields.</summary>
        /// <param name="fieldCollection">The SPField collection.</param>
        /// <param name="fieldInfos">The field Infos.</param>
        /// <returns>A collection of strings that contain the internal name of the new fields.</returns>
        /// <exception cref="System.ArgumentNullException">Null fieldsXml parameter</exception>
        IEnumerable<string> EnsureField(SPFieldCollection fieldCollection, ICollection<IFieldInfo> fieldInfos);

        /// <summary>
        /// Adds a field defined in xml to a collection of fields.
        /// </summary>
        /// <param name="fieldCollection">The SPField collection.</param>
        /// <param name="fieldXml">The field XML schema.</param>
        /// <returns>
        /// A string that contains the internal name of the new field.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// fieldCollection
        /// or
        /// fieldXml
        /// </exception>
        /// <exception cref="System.FormatException">Invalid xml.</exception>
        string EnsureField(SPFieldCollection fieldCollection, XElement fieldXml);

        /// <summary>The ensure field.</summary>
        /// <param name="fieldCollection">The field collection.</param>
        /// <param name="fieldsXml">The fields xml.</param>
        /// <returns>The <see cref="IList"/>.</returns>
        IList<string> EnsureField(SPFieldCollection fieldCollection, XDocument fieldsXml);

        /// <summary>The ensure field.</summary>
        /// <param name="fieldCollection">The field collection.</param>
        /// <param name="fieldInfo">The field info.</param>
        /// <returns>The <see cref="string"/>.</returns>
        string EnsureField(SPFieldCollection fieldCollection, IFieldInfo fieldInfo);

        /// <summary>The ensure field.</summary>
        /// <param name="fieldCollection">The field collection.</param>
        /// <param name="fieldInfo">The field info.</param>
        /// <returns>The <see cref="string"/>.</returns>
        string EnsureField(SPFieldCollection fieldCollection, TaxonomyFieldInfo fieldInfo);
    }
}
