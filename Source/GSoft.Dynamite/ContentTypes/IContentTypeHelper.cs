namespace GSoft.Dynamite.ContentTypes
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using GSoft.Dynamite.Fields;
    using Microsoft.SharePoint;

    /// <summary>
    /// Helper for managing content types.
    /// </summary>
    public interface IContentTypeHelper
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
        SPContentType EnsureContentType(SPContentTypeCollection contentTypeCollection, ContentTypeInfo contentTypeInfo);

        /// <summary>The ensure content type.</summary>
        /// <param name="contentTypeCollection">The content type collection.</param>
        /// <param name="contentTypeInfos">The content type information</param>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        IEnumerable<SPContentType> EnsureContentType(
            SPContentTypeCollection contentTypeCollection,
            ICollection<ContentTypeInfo> contentTypeInfos);

        /// <summary>
        /// Deletes the content type if it has no SPContentTypeUsages.
        /// If it does, the content type will be deleted from the usages that are lists where it has no items.
        /// </summary>
        /// <param name="contentType">The content type.</param>
        /// <exception cref="System.ArgumentNullException">For any null parameter.</exception>
        void DeleteContentTypeIfNotUsed(SPContentType contentType);

        /// <summary>
        /// Reorders fields in the content type according to index position.
        /// </summary>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="orderedFields">A collection of indexes (0 based) and their corresponding field information.</param>
        void ReorderFieldsInContentType(SPContentType contentType, ICollection<IFieldInfo> orderedFields);
    }
}