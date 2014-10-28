namespace GSoft.Dynamite.Definitions
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    using Microsoft.SharePoint;

    public interface IContentTypeBuilder
    {
        /// <summary>
        /// Ensure the content type based on its content type info. 
        /// Sets the description and Groups resource, adds the fields and calls update.
        /// </summary>
        /// <param name="contentTypeCollection">The content type collection.</param>
        /// <param name="contentTypeInfo">The content type information.</param>
        /// <returns>
        /// The created and configured content type.
        /// </returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        SPContentType EnsureContentType(SPContentTypeCollection contentTypeCollection, ContentTypeInfo contentTypeInfo);

        /// <summary>
        /// Ensures the SPContentType is in the collection. If not, it will be created and added.
        /// </summary>
        /// <param name="contentTypeCollection">The content type collection.</param>
        /// <param name="contentTypeId">The content type id.</param>
        /// <param name="contentTypeName">Name of the content type.</param>
        /// <returns><c>True</c> if it was added, else <c>False</c>.</returns>
        /// <exception cref="System.ArgumentNullException">For any null parameter.</exception>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        SPContentType EnsureContentType(SPContentTypeCollection contentTypeCollection, SPContentTypeId contentTypeId, string contentTypeName);

        /// <summary>
        /// Deletes the content type if not used.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="contentTypeId">The content type id.</param>
        /// <exception cref="System.ArgumentNullException">For any null parameter.</exception>
        /// <exception cref="Microsoft.SharePoint.SPContentTypeReadOnlyException">If the contentype is readonly.</exception>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        void DeleteContentTypeIfNotUsed(SPContentTypeCollection collection, SPContentTypeId contentTypeId);

        /// <summary>
        /// Deletes the content type if it has no SPContentTypeUsages.
        /// If it does, the content type will be deleted from the usages that are lists where it has no items.
        /// </summary>
        /// <param name="contentType">The content type.</param>
        /// <exception cref="System.ArgumentNullException">For any null parameter.</exception>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        void DeleteContentTypeIfNotUsed(SPContentType contentType);

        /// <summary>
        /// Ensures the SPField is in the content type. If not, it will be added and the content type updated.
        /// </summary>
        /// <param name="contentType">Type content type.</param>
        /// <param name="fieldInfo">The field info.</param>
        /// <returns>Null if the field does not exist, else the field is returned.</returns>
        /// <exception cref="System.ArgumentNullException">For any null parameter.</exception>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        SPField EnsureFieldInContentType(SPContentType contentType, FieldInfo fieldInfo);

        /// <summary>
        /// Ensures the SPFields are in the content type. If not, they will be added and the content type updated.
        /// </summary>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="fieldInfos">The field information.</param>
        /// <returns>IEnumerable of SPFields that where found.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        IEnumerable<SPField> EnsureFieldInContentType(SPContentType contentType, ICollection<FieldInfo> fieldInfos);

        /// <summary>
        /// Adds the event receiver definition to the content type.
        /// </summary>
        /// <param name="contentType">The content type.</param>
        /// <param name="type">The receiver type.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="className">Name of the class.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        void AddEventReceiverDefinition(SPContentType contentType, SPEventReceiverType type, string assemblyName, string className);

        /// <summary>
        /// Adds the event receiver definition to the content type.
        /// </summary>
        /// <param name="contentType">The content type.</param>
        /// <param name="type">The receiver type.</param>
        /// <param name="assembly">The assembly.</param>
        /// <param name="className">Name of the class.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        void AddEventReceiverDefinition(SPContentType contentType, SPEventReceiverType type, Assembly assembly, string className);

        /// <summary>
        /// Reorders fields in the content type according to index position.
        /// </summary>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="orderedFields">A collection of indexes (0 based) and their corresponding field information.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        void ReorderFieldsInContentType(SPContentType contentType, ICollection<FieldInfo> orderedFields);
    }
}