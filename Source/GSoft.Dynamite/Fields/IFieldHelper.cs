namespace GSoft.Dynamite.Fields
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Xml.Linq;

    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Taxonomy;

    /// <summary>
    /// Helper for managing SP Fields.
    /// </summary>
    public interface IFieldHelper
    {
        /// <summary>Adds a collection of fields defined in xml to a collection of fields.</summary>
        /// <param name="fieldCollection">The SPField collection.</param>
        /// <param name="fieldInfos">The fields' information.</param>
        /// <returns>A collection of the new fields.</returns>
        /// <exception cref="System.ArgumentNullException">Null fieldsXml parameter</exception>
        IEnumerable<SPField> EnsureField(SPFieldCollection fieldCollection, ICollection<BaseFieldInfo> fieldInfos);

        /// <summary>The ensure field.</summary>
        /// <param name="fieldCollection">The field collection.</param>
        /// <param name="fieldInfo">The field info.</param>
        /// <returns>The field.</returns>
        SPField EnsureField(SPFieldCollection fieldCollection, BaseFieldInfo fieldInfo);
    }
}
