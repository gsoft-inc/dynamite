using System;
using System.Collections.Generic;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Taxonomy;

namespace GSoft.Dynamite.Caml
{
    /// <summary>
    /// CAML builder interface.
    /// </summary>
    public interface ICamlBuilder
    {
        /// <summary>
        /// Creates CAML and with the specified left and right conditions.
        /// </summary>
        /// <param name="leftCondition">The left condition.</param>
        /// <param name="rightCondition">The right condition.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string And(string leftCondition, string rightCondition);

        /// <summary>
        /// Creates CAML begins with with the specified field reference and value.
        /// </summary>
        /// <param name="fieldRefElement">The field reference element.</param>
        /// <param name="valueElement">The value element.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string BeginsWith(string fieldRefElement, string valueElement);

        /// <summary>
        /// Creates CAML contains with the specified field reference and value.
        /// </summary>
        /// <param name="fieldRefElement">The field reference element.</param>
        /// <param name="valueElement">The value element.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string Contains(string fieldRefElement, string valueElement);

        /// <summary>
        /// Creates CAML date ranges overlap with the specified field reference and value.
        /// </summary>
        /// <param name="fieldRefElement">The field reference element.</param>
        /// <param name="valueElement">The value element.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string DateRangesOverlap(string fieldRefElement, string valueElement);

        /// <summary>
        /// Calendar view date ranges overlap.
        /// </summary>
        /// <param name="startDateFieldName">The start date field name.</param>
        /// <param name="endDateFieldName">The end date field name.</param>
        /// <param name="period">The period.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string CalendarViewDateRangesOverlap(string startDateFieldName, string endDateFieldName, string period);

        /// <summary>
        /// Calendar view date ranges overlap.
        /// </summary>
        /// <param name="startDateFieldName">The start date field name.</param>
        /// <param name="endDateFieldName">The end date field name.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string CalendarViewDateRangesOverlap(string startDateFieldName, string endDateFieldName);

        /// <summary>
        /// Creates CAML equal with the specified left and right conditions.
        /// </summary>
        /// <param name="leftCondition">The left condition.</param>
        /// <param name="rightCondition">The right condition.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string Equal(string leftCondition, string rightCondition);

        /// <summary>
        /// Creates CAML field reference with the specified field name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string FieldRef(string fieldName);

        /// <summary>
        /// Creates CAML field reference with the specified field name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="sortType">Type of the sort.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string FieldRef(string fieldName, CamlEnums.SortType sortType);

        /// <summary>
        /// Creates CAML greater than or equal with the specified left and right conditions.
        /// </summary>
        /// <param name="leftCondition">The left condition.</param>
        /// <param name="rightCondition">The right condition.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string GreaterThanOrEqual(string leftCondition, string rightCondition);

        /// <summary>
        /// Creates CAML group by with the specified field reference.
        /// </summary>
        /// <param name="fieldRefElement">The field reference element.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string GroupBy(string fieldRefElement);

        /// <summary>
        /// Creates CAML group by with the specified field reference.
        /// </summary>
        /// <param name="fieldRefElement">The field reference element.</param>
        /// <param name="collapse">if set to <c>true</c> [collapse].</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string GroupBy(string fieldRefElement, bool collapse);

        /// <summary>
        /// Creates CAML greater than with the specified left and right conditions.
        /// </summary>
        /// <param name="leftCondition">The left condition.</param>
        /// <param name="rightCondition">The right condition.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string GreaterThan(string leftCondition, string rightCondition);

        /// <summary>
        /// Creates CAML equal with the specified left and right conditions.
        /// </summary>
        /// <param name="leftCondition">The left condition.</param>
        /// <param name="rightCondition">The right condition.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string In(string leftCondition, string rightCondition);

        /// <summary>
        /// Creates a CAML query to determine whether [is content type] [the specified content type identifier].
        /// </summary>
        /// <param name="contentTypeId">The content type identifier.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string IsContentType(SPContentTypeId contentTypeId);

        /// <summary>
        /// Creates a CAML query to determine whether [is or inherits content type] [the specified content type identifier].
        /// </summary>
        /// <param name="contentTypeId">The content type identifier.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string IsOrInheritsContentType(SPContentTypeId contentTypeId);

        /// <summary>
        /// Determines whether [is publishing started] based on today's date.
        /// </summary>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string IsPublished();

        /// <summary>
        /// Determines whether this instance is published.
        /// </summary>
        /// <param name="includeTimeValue">if set to <c>true</c> [include time value].</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string IsPublished(bool includeTimeValue);

        /// <summary>
        /// Determines whether this instance is published based on a specified start and end date.
        /// </summary>
        /// <param name="startDateTime">The start date time.</param>
        /// <param name="expirationDateTime">The expiration date time.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string IsPublished(DateTime startDateTime, DateTime expirationDateTime);

        /// <summary>
        /// Determines whether this instance is published based on a specified start and end offset.
        /// </summary>
        /// <param name="startOffsetDays">The number of offset days from today for the starting date.</param>
        /// <param name="expirationOffsetDays">The number of offset days from today for the expiration date.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string IsPublished(int startOffsetDays, int expirationOffsetDays);

        /// <summary>
        /// Determines whether [is publishing expired] based on today's date.
        /// </summary>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string IsBeforePublishingExpiryDate();

        /// <summary>
        /// Determines whether [is publishing expired] based on today's date.
        /// </summary>
        /// <param name="includeTimeValue">if set to <c>true</c> [include time value].</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string IsBeforePublishingExpiryDate(bool includeTimeValue);

        /// <summary>
        /// Determines whether [is publishing expired] based on a specified date.
        /// </summary>
        /// <param name="expirationDateTime">The expiration date time.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string IsBeforePublishingExpiryDate(DateTime expirationDateTime);

        /// <summary>
        /// Determines whether [is publishing expired] based on a specified date.
        /// </summary>
        /// <param name="expirationOffsetDays">The number of offset days from today for the expiration date.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string IsBeforePublishingExpiryDate(int expirationOffsetDays);

        /// <summary>
        /// Determines whether [is publishing started] based on today's date.
        /// </summary>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string IsAfterPublishingStartDate();

        /// <summary>
        /// Determines whether [is publishing started] based on today's date.
        /// </summary>
        /// <param name="includeTimeValue">if set to <c>true</c> [include time value].</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string IsAfterPublishingStartDate(bool includeTimeValue);

        /// <summary>
        /// Determines whether [is publishing started] based on a specified date.
        /// </summary>
        /// <param name="startDateTime">The start date time.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string IsAfterPublishingStartDate(DateTime startDateTime);

        /// <summary>
        /// Determines whether [is publishing started] based on a specified date.
        /// </summary>
        /// <param name="startOffsetDays">The number of offset days from today for the starting date.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string IsAfterPublishingStartDate(int startOffsetDays);

        /// <summary>
        /// Creates CAML is null by with the specified field reference.
        /// </summary>
        /// <param name="fieldRefElement">The field reference element.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string IsNotNull(string fieldRefElement);

        /// <summary>
        /// Creates CAML is null by with the specified field reference.
        /// </summary>
        /// <param name="fieldRefElement">The field reference element.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string IsNull(string fieldRefElement);

        /// <summary>
        /// Creates CAML lesser than or equal by with the specified left and right conditions.
        /// </summary>
        /// <param name="leftCondition">The left condition.</param>
        /// <param name="rightCondition">The right condition.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string LesserThanOrEqual(string leftCondition, string rightCondition);

        /// <summary>
        /// Creates CAML safe identifier by with the specified identifier value.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string SafeIdentifier(string identifier);

        /// <summary>
        /// Creates CAML lists by with the specified arguments.
        /// </summary>
        /// <param name="listId">The list identifier.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string List(Guid listId);

        /// <summary>
        /// Creates CAML lists by with the specified arguments.
        /// </summary>
        /// <param name="listElements">The list elements.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string Lists(string listElements);

        /// <summary>
        /// Creates CAML lists by with the specified arguments.
        /// </summary>
        /// <param name="listElements">The list elements.</param>
        /// <param name="includeHiddenLists">if set to <c>true</c> [include hidden lists].</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string Lists(string listElements, bool includeHiddenLists);

        /// <summary>
        /// Creates CAML lists by with the specified arguments.
        /// </summary>
        /// <param name="listElements">The list elements.</param>
        /// <param name="maxListLimit">The maximum list limit.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string Lists(string listElements, int maxListLimit);

        /// <summary>
        /// Creates CAML lists by with the specified arguments.
        /// </summary>
        /// <param name="listElements">The list elements.</param>
        /// <param name="serverTemplate">The server template.</param>
        /// <param name="includeHiddenLists">if set to <c>true</c> [include hidden lists].</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string Lists(string listElements, string serverTemplate, bool includeHiddenLists);

        /// <summary>
        /// Creates CAML lists by with the specified arguments.
        /// </summary>
        /// <param name="baseType">Type of the base.</param>
        /// <param name="listElements">The list elements.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string Lists(CamlEnums.BaseType baseType, string listElements);

        /// <summary>
        /// Creates CAML lists by with the specified arguments.
        /// </summary>
        /// <param name="baseType">Type of the base.</param>
        /// <param name="listElements">The list elements.</param>
        /// <param name="serverTemplate">The server template.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string Lists(CamlEnums.BaseType baseType, string listElements, string serverTemplate);

        /// <summary>
        /// Creates CAML lists by with the specified arguments.
        /// </summary>
        /// <param name="baseType">Type of the base.</param>
        /// <param name="listElements">The list elements.</param>
        /// <param name="serverTemplate">The server template.</param>
        /// <param name="includeHiddenLists">if set to <c>true</c> [include hidden lists].</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string Lists(CamlEnums.BaseType baseType, string listElements, string serverTemplate, bool includeHiddenLists);

        /// <summary>
        /// Creates CAML lists by with the specified arguments.
        /// </summary>
        /// <param name="baseType">Type of the base.</param>
        /// <param name="listElements">The list elements.</param>
        /// <param name="serverTemplate">The server template.</param>
        /// <param name="includeHiddenLists">if set to <c>true</c> [include hidden lists].</param>
        /// <param name="maxListLimit">The maximum list limit.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string Lists(CamlEnums.BaseType baseType, string listElements, string serverTemplate, bool includeHiddenLists, int maxListLimit);

        /// <summary>
        /// Creates CAML with index than with the specified field ID and value.
        /// </summary>
        /// <param name="fieldId">The field identifier.</param>
        /// <param name="fieldValue">The field value.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string WithIndex(Guid fieldId, string fieldValue);

        /// <summary>
        /// Creates CAML lesser than by with the specified left and right conditions.
        /// </summary>
        /// <param name="leftCondition">The left condition.</param>
        /// <param name="rightCondition">The right condition.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string LesserThan(string leftCondition, string rightCondition);

        /// <summary>
        /// Creates CAML membership by with the specified membership type and value.
        /// </summary>
        /// <param name="type">The membership type.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string Membership(CamlEnums.MembershipType type);

        /// <summary>
        /// Creates CAML membership by with the specified membership type and value.
        /// </summary>
        /// <param name="type">The membership type.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string Membership(CamlEnums.MembershipType type, string value);

        /// <summary>
        /// Creates CAML not equal by with the specified left and right conditions.
        /// </summary>
        /// <param name="leftCondition">The left condition.</param>
        /// <param name="rightCondition">The right condition.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string NotEqual(string leftCondition, string rightCondition);

        /// <summary>
        /// Return the now CAML value.
        /// </summary>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string Now();

        /// <summary>
        /// Creates CAML or by with the specified left and right conditions.
        /// </summary>
        /// <param name="leftCondition">The left condition.</param>
        /// <param name="rightCondition">The right condition.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string Or(string leftCondition, string rightCondition);

        /// <summary>
        /// Creates CAML order by with the specified field references.
        /// </summary>
        /// <param name="fieldRefElements">The field reference elements.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string OrderBy(string fieldRefElements);

        /// <summary>
        /// Creates CAML order by with the specified arguments.
        /// </summary>
        /// <param name="arguments">The query arguments.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string OrderBy(params object[] arguments);

        /// <summary>
        /// Creates CAML value with the specified value.
        /// </summary>
        /// <param name="fieldValue">The field value.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string Value(string fieldValue);

        /// <summary>
        /// Creates CAML value with the specified value.
        /// </summary>
        /// <param name="fieldValue">The field value.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string Value(int fieldValue);

        /// <summary>
        /// Creates CAML value with the specified value.
        /// </summary>
        /// <param name="fieldValue">The field value.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string Value(DateTime fieldValue);

        /// <summary>
        /// Values the specified field value.
        /// </summary>
        /// <param name="fieldValue">The field value.</param>
        /// <param name="includeTimeValue">if set to <c>true</c> [include time value].</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string Value(DateTime fieldValue, bool includeTimeValue);

        /// <summary>
        /// Creates CAML value with the specified value.
        /// </summary>
        /// <param name="fieldValue">The field value.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string Value(bool fieldValue);

        /// <summary>
        /// Creates CAML value with the specified type and value.
        /// </summary>
        /// <param name="valueType">Type of the value.</param>
        /// <param name="fieldValue">The field value.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string Value(string valueType, string fieldValue);

        /// <summary>
        /// Creates CAML view fields with the specified fields.
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string ViewFields(params object[] fields);

        /// <summary>
        /// Creates CAML view fields with the specified entity type.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string ViewFieldsForEntityType(Type entityType);

        /// <summary>
        /// Creates CAML webs with the specified scope.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string Webs(CamlEnums.QueryScope scope);

        /// <summary>
        /// Creates CAML project property with the specified property name.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string ProjectProperty(string propertyName);

        /// <summary>
        /// Creates CAML project property with the specified property name and default value.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string ProjectProperty(string propertyName, string defaultValue);

        /// <summary>
        /// Creates CAML project property with the specified condition.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="autoHyperlinkType">Type of the automatic hyperlink.</param>
        /// <param name="autoNewLine">if set to <c>true</c> [automatic new line].</param>
        /// <param name="expandXml">if set to <c>true</c> [expand XML].</param>
        /// <param name="htmlEncode">if set to <c>true</c> [HTML encode].</param>
        /// <param name="stripWhiteSpace">if set to <c>true</c> [strip white space].</param>
        /// <param name="urlEncodingType">Type of the URL encoding.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        string ProjectProperty(string propertyName, string defaultValue, CamlEnums.AutoHyperlinkType autoHyperlinkType, bool autoNewLine, bool expandXml, bool htmlEncode, bool stripWhiteSpace, CamlEnums.UrlEncodingType urlEncodingType);

        /// <summary>
        /// Creates CAML where with the specified condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <returns>A string representation of the CAML query.</returns>
        string Where(string condition);

        /// <summary>
        /// Creates CAML XML with the specified condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <returns>A string representation of the CAML query.</returns>
        string Xml(string condition);

        /// <summary>
        /// Generates a CAML filter for a Taxonomy Term
        /// </summary>
        /// <param name="list">The list over which the query will be done</param>
        /// <param name="taxonomyFieldInternalName">The name of the site column associated with the term set</param>
        /// <param name="term">Term to match for</param>
        /// <param name="includeDescendants">Whether the Term's child terms should be query hits as well</param>
        /// <returns>A string representation of the CAML query.</returns>
        string TermFilter(SPList list, string taxonomyFieldInternalName, Term term, bool includeDescendants);

        /// <summary>
        /// Generates a CAML filter for a Taxonomy Term
        /// </summary>
        /// <param name="list">The list over which the query will be done</param>
        /// <param name="taxonomyFieldInternalName">The name of the site column associated with the term set</param>
        /// <param name="termId">Term identifier to match for</param>
        /// <param name="includeDescendants">Whether the Term's child terms should be query hits as well</param>
        /// <returns>A string representation of the CAML query.</returns>
        string TermFilter(SPList list, string taxonomyFieldInternalName, Guid termId, bool includeDescendants);

        /// <summary>
        /// Generates a CAML filter for a Taxonomy Term
        /// </summary>
        /// <param name="list">The list over which the query will be done</param>
        /// <param name="taxonomyFieldInternalName">The name of the site column associated with the term set</param>
        /// <param name="terms">List of terms for why we want to match in an OR fashion</param>
        /// <param name="includeDescendants">Whether the Term's child terms should be query hits as well</param>
        /// <returns>A string representation of the CAML query.</returns>
        string TermFilter(SPList list, string taxonomyFieldInternalName, IList<Term> terms, bool includeDescendants);

        /// <summary>
        /// Generates a CAML filter for a Taxonomy Term from the site-collection-reserved term store group
        /// </summary>
        /// <param name="list">The list over which the query will be done</param>
        /// <param name="taxonomyFieldInternalName">The name of the site column associated with the term set</param>
        /// <param name="termSetName">Name of the term set</param>
        /// <param name="termLabel">Label by which to find the term (dupes not supported)</param>
        /// <param name="includeDescendants">Whether the Term's child terms should be query hits as well</param>
        /// <returns>A string representation of the CAML query.</returns>
        string TermFilter(SPList list, string taxonomyFieldInternalName, string termSetName, string termLabel, bool includeDescendants);

        /// <summary>
        /// Generates a CAML filter for a Taxonomy Term in a global farm term store group
        /// </summary>
        /// <param name="list">The list over which the query will be done</param>
        /// <param name="taxonomyFieldInternalName">The name of the site column associated with the term set</param>
        /// <param name="termStoreGroupName">Name of the global farm term store group</param>
        /// <param name="termSetName">Name of the term set</param>
        /// <param name="termLabel">Label by which to find the term (dupes not supported)</param>
        /// <param name="includeDescendants">Whether the Term's child terms should be query hits as well</param>
        /// <returns>A string representation of the CAML query.</returns>
        string TermFilter(SPList list, string taxonomyFieldInternalName, string termStoreGroupName, string termSetName, string termLabel, bool includeDescendants);

        /// <summary>
        /// Returns the today value tag.
        /// </summary>
        /// <returns>
        /// The today value tag.
        /// </returns>
        string Today();

        /// <summary>
        /// Todays the specified include time value.
        /// </summary>
        /// <param name="includeTimeValue">if set to <c>true</c> [include time value].</param>
        /// <returns>A string representation of the CAML query.</returns>
        string Today(bool includeTimeValue);

        /// <summary>
        /// Returns the today value tag.
        /// </summary>
        /// <param name="offsetDays">The offset, in days.</param>
        /// <returns>
        /// The today value tag.
        /// </returns>
        string Today(int offsetDays);

        /// <summary>
        /// Todays the specified offset days.
        /// </summary>
        /// <param name="offsetDays">The offset days.</param>
        /// <param name="includeTimeValue">if set to <c>true</c> [include time value].</param>
        /// <returns>A string representation of the CAML query.</returns>
        string Today(int offsetDays, bool includeTimeValue);
    }
}