using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using GSoft.Dynamite.Binding;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.Taxonomy;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Taxonomy;
using Microsoft.SharePoint.Utilities;

namespace GSoft.Dynamite.Caml
{
    /// <summary>
    /// CAML query builder, replacing the use of John Holiday's this.NET library.
    /// (thanks John, it was nice while it lasted!)
    /// </summary>
    public class CamlBuilder : ICamlBuilder
    {
        private readonly ITaxonomyService _taxonomyService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CamlBuilder" /> class.
        /// </summary>
        /// <param name="taxonomyService">The taxonomy service.</param>
        /// <param name="logger">The logger.</param>
        public CamlBuilder(ITaxonomyService taxonomyService, ILogger logger)
        {
            this._taxonomyService = taxonomyService;
            this._logger = logger;
        }

        /// <summary>
        /// Creates CAML and with the specified left and right conditions.
        /// </summary>
        /// <param name="leftCondition">The left condition.</param>
        /// <param name="rightCondition">The right condition.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        public string And(string leftCondition, string rightCondition)
        {
            return Tag(CamlConstants.And, null, null, leftCondition + rightCondition);
        }

        /// <summary>
        /// Creates CAML begins with with the specified field reference and value.
        /// </summary>
        /// <param name="fieldRefElement">The field reference element.</param>
        /// <param name="valueElement">The value element.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        public string BeginsWith(string fieldRefElement, string valueElement)
        {
            return Tag(CamlConstants.BeginsWith, null, null, fieldRefElement + valueElement);
        }

        /// <summary>
        /// Creates CAML contains with the specified field reference and value.
        /// </summary>
        /// <param name="fieldRefElement">The field reference element.</param>
        /// <param name="valueElement">The value element.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        public string Contains(string fieldRefElement, string valueElement)
        {
            return Tag(CamlConstants.Contains, null, null, fieldRefElement + valueElement);
        }

        /// <summary>
        /// Creates CAML date ranges overlap with the specified field reference and value.
        /// </summary>
        /// <param name="fieldRefElement">The field reference element.</param>
        /// <param name="valueElement">The value element.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        public string DateRangesOverlap(string fieldRefElement, string valueElement)
        {
            return Tag(CamlConstants.DateRangesOverlap, null, null, fieldRefElement + this.FieldRef("EndDate") + this.FieldRef("RecurrenceID") + valueElement);
        }

        /// <summary>
        /// Creates CAML equal with the specified left and right conditions.
        /// </summary>
        /// <param name="leftCondition">The left condition.</param>
        /// <param name="rightCondition">The right condition.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        public string Equal(string leftCondition, string rightCondition)
        {
            return Tag(CamlConstants.Equal, null, null, leftCondition + rightCondition);
        }

        /// <summary>
        /// Creates CAML field reference with the specified field name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        public string FieldRef(string fieldName)
        {
            return Tag(CamlConstants.FieldRef, CamlConstants.Name, this.SafeIdentifier(fieldName), null);
        }

        /// <summary>
        /// Creates CAML field reference with the specified field name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="sortType">Type of the sort.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        public string FieldRef(string fieldName, CamlEnums.SortType sortType)
        {
            var attributes = new object[]
            {
                "Ascending",
                (sortType == CamlEnums.SortType.Ascending) ? "TRUE" : "FALSE",
                CamlConstants.Name,
                this.SafeIdentifier(fieldName)
            };

            return Tag(CamlConstants.FieldRef, null, attributes);
        }

        /// <summary>
        /// Creates CAML greater than or equal with the specified left and right conditions.
        /// </summary>
        /// <param name="leftCondition">The left condition.</param>
        /// <param name="rightCondition">The right condition.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        public string GreaterThanOrEqual(string leftCondition, string rightCondition)
        {
            return Tag(CamlConstants.GreaterThanOrEqual, null, null, leftCondition + rightCondition);
        }

        /// <summary>
        /// Creates CAML group by with the specified field reference.
        /// </summary>
        /// <param name="fieldRefElement">The field reference element.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        public string GroupBy(string fieldRefElement)
        {
            return this.GroupBy(fieldRefElement, false);
        }

        /// <summary>
        /// Creates CAML group by with the specified field reference.
        /// </summary>
        /// <param name="fieldRefElement">The field reference element.</param>
        /// <param name="collapse">if set to <c>true</c> [collapse].</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        public string GroupBy(string fieldRefElement, bool collapse)
        {
            return Tag(CamlConstants.GroupBy, CamlConstants.Collapse, collapse ? "TRUE" : "FALSE", fieldRefElement);
        }

        /// <summary>
        /// Creates CAML greater than with the specified left and right conditions.
        /// </summary>
        /// <param name="leftCondition">The left condition.</param>
        /// <param name="rightCondition">The right condition.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        public string GreaterThan(string leftCondition, string rightCondition)
        {
            return Tag(CamlConstants.GreaterThan, null, null, leftCondition + rightCondition);
        }

        /// <summary>
        /// Creates a CAML query to determine whether [is content type] [the specified content type identifier].
        /// </summary>
        /// <param name="contentTypeId">The content type identifier.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        public string IsContentType(SPContentTypeId contentTypeId)
        {
            return this.Equal(this.FieldRef("ContentTypeId"), this.Value("ContentTypeId", contentTypeId.ToString()));
        }

        /// <summary>
        /// Creates a CAML query to determine whether [is or inherits content type] [the specified content type identifier].
        /// </summary>
        /// <param name="contentTypeId">The content type identifier.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        public string IsOrInheritsContentType(SPContentTypeId contentTypeId)
        {
            return this.BeginsWith(this.FieldRef("ContentTypeId"), this.Value("ContentTypeId", contentTypeId.ToString()));
        }

        /// <summary>
        /// Creates CAML is null by with the specified field reference.
        /// </summary>
        /// <param name="fieldRefElement">The field reference element.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        public string IsNotNull(string fieldRefElement)
        {
            return Tag(CamlConstants.IsNotNull, null, null, fieldRefElement);
        }

        /// <summary>
        /// Creates CAML is null by with the specified field reference.
        /// </summary>
        /// <param name="fieldRefElement">The field reference element.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        public string IsNull(string fieldRefElement)
        {
            return Tag(CamlConstants.IsNull, null, null, fieldRefElement);
        }

        /// <summary>
        /// Creates CAML lesser than or equal by with the specified left and right conditions.
        /// </summary>
        /// <param name="leftCondition">The left condition.</param>
        /// <param name="rightCondition">The right condition.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        public string LesserThanOrEqual(string leftCondition, string rightCondition)
        {
            return Tag(CamlConstants.LesserThanOrEqual, null, null, leftCondition + rightCondition);
        }

        /// <summary>
        /// Creates CAML safe identifier by with the specified identifier value.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        public string SafeIdentifier(string identifier)
        {
            return identifier.Replace(" ", "_x0020_");
        }

        /// <summary>
        /// Creates CAML lists by with the specified arguments.
        /// </summary>
        /// <param name="listId">The list identifier.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        public string List(Guid listId)
        {
            return Tag(CamlConstants.List, "ID", listId.ToString().Replace("{", string.Empty).Replace("}", string.Empty), null);
        }

        /// <summary>
        /// Creates CAML lists by with the specified arguments.
        /// </summary>
        /// <param name="listElements">The list elements.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        public string Lists(string listElements)
        {
            return this.Lists(CamlEnums.BaseType.GenericList, listElements, null, false, 0);
        }

        /// <summary>
        /// Creates CAML lists by with the specified arguments.
        /// </summary>
        /// <param name="listElements">The list elements.</param>
        /// <param name="includeHiddenLists">if set to <c>true</c> [include hidden lists].</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        public string Lists(string listElements, bool includeHiddenLists)
        {
            return this.Lists(CamlEnums.BaseType.GenericList, listElements, null, includeHiddenLists);
        }

        /// <summary>
        /// Creates CAML lists by with the specified arguments.
        /// </summary>
        /// <param name="listElements">The list elements.</param>
        /// <param name="maxListLimit">The maximum list limit.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        public string Lists(string listElements, int maxListLimit)
        {
            return this.Lists(CamlEnums.BaseType.GenericList, listElements, null, false, maxListLimit);
        }

        /// <summary>
        /// Creates CAML lists by with the specified arguments.
        /// </summary>
        /// <param name="listElements">The list elements.</param>
        /// <param name="serverTemplate">The server template.</param>
        /// <param name="includeHiddenLists">if set to <c>true</c> [include hidden lists].</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        public string Lists(string listElements, string serverTemplate, bool includeHiddenLists)
        {
            return this.Lists(CamlEnums.BaseType.GenericList, listElements, serverTemplate, includeHiddenLists);
        }

        /// <summary>
        /// Creates CAML lists by with the specified arguments.
        /// </summary>
        /// <param name="baseType">Type of the base.</param>
        /// <param name="listElements">The list elements.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        public string Lists(CamlEnums.BaseType baseType, string listElements)
        {
            return this.Lists(baseType, listElements, null, false, 0);
        }

        /// <summary>
        /// Creates CAML lists by with the specified arguments.
        /// </summary>
        /// <param name="baseType">Type of the base.</param>
        /// <param name="listElements">The list elements.</param>
        /// <param name="serverTemplate">The server template.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        public string Lists(CamlEnums.BaseType baseType, string listElements, string serverTemplate)
        {
            return this.Lists(baseType, listElements, serverTemplate, false, 0);
        }

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
        public string Lists(CamlEnums.BaseType baseType, string listElements, string serverTemplate, bool includeHiddenLists)
        {
            return this.Lists(baseType, listElements, serverTemplate, includeHiddenLists, 0);
        }

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
        public string Lists(CamlEnums.BaseType baseType, string listElements, string serverTemplate, bool includeHiddenLists, int maxListLimit)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("<Lists BaseType=\"{0}\"", (int)baseType);
            if (!string.IsNullOrEmpty(serverTemplate))
            {
                stringBuilder.AppendFormat(" ServerTemplate=\"{0}\"", serverTemplate);
            }

            stringBuilder.AppendFormat(" Hidden=\"{0}\"", includeHiddenLists ? "TRUE" : "FALSE");
            stringBuilder.AppendFormat(" MaxListLimit=\"{0}\"", maxListLimit);
            stringBuilder.AppendFormat(">{0}</Lists>", listElements);

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Creates CAML with index than with the specified field ID and value.
        /// </summary>
        /// <param name="fieldId">The field identifier.</param>
        /// <param name="fieldValue">The field value.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        public string WithIndex(Guid fieldId, string fieldValue)
        {
            var attributes = new object[]
            {
                "FieldId",
                fieldId.ToString().Replace("{", string.Empty).Replace("}", string.Empty),
                "Type",
                "Text",
                "Value",
                fieldValue
            };

            return Tag(CamlConstants.WithIndex, null, attributes);
        }

        /// <summary>
        /// Creates CAML lesser than by with the specified left and right conditions.
        /// </summary>
        /// <param name="leftCondition">The left condition.</param>
        /// <param name="rightCondition">The right condition.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        public string LesserThan(string leftCondition, string rightCondition)
        {
            return Tag(CamlConstants.LesserThan, null, null, leftCondition + rightCondition);
        }

        /// <summary>
        /// Creates CAML membership by with the specified membership type and value.
        /// </summary>
        /// <param name="type">The membership type.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        public string Membership(CamlEnums.MembershipType type)
        {
            return this.Membership(type, null);
        }

        /// <summary>
        /// Creates CAML membership by with the specified membership type and value.
        /// </summary>
        /// <param name="type">The membership type.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        public string Membership(CamlEnums.MembershipType type, string value)
        {
            switch (type)
            {
                case CamlEnums.MembershipType.SPWebAllUsers:
                    return Tag(CamlConstants.Membership, CamlConstants.Type, CamlConstants.SPWebAllUsers, value);
                case CamlEnums.MembershipType.SPGroup:
                    return Tag(CamlConstants.Membership, CamlConstants.Type, CamlConstants.SPGroup, value);
                case CamlEnums.MembershipType.SPWebGroups:
                    return Tag(CamlConstants.Membership, CamlConstants.Type, CamlConstants.SPWebGroups, value);
                case CamlEnums.MembershipType.CurrentUserGroups:
                    return Tag(CamlConstants.Membership, CamlConstants.Type, CamlConstants.CurrentUserGroups, value);
                case CamlEnums.MembershipType.SPWebUsers:
                    return Tag(CamlConstants.Membership, CamlConstants.Type, CamlConstants.SPWebUsers, value);
                default:
                    return Tag(CamlConstants.Membership, CamlConstants.Type, CamlConstants.CurrentUserGroups, value);
            }
        }

        /// <summary>
        /// Creates CAML not equal by with the specified left and right conditions.
        /// </summary>
        /// <param name="leftCondition">The left condition.</param>
        /// <param name="rightCondition">The right condition.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        public string NotEqual(string leftCondition, string rightCondition)
        {
            return Tag(CamlConstants.NotEqual, null, null, leftCondition + rightCondition);
        }

        /// <summary>
        /// Return the now CAML value.
        /// </summary>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        public string Now()
        {
            return "<Value Type=\"DateTime\" IncludeTimeValue=\"TRUE\">" + SPUtility.CreateISO8601DateTimeFromSystemDateTime(DateTime.Now) + "</Value>";
        }

        /// <summary>
        /// Creates CAML or by with the specified left and right conditions.
        /// </summary>
        /// <param name="leftCondition">The left condition.</param>
        /// <param name="rightCondition">The right condition.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        public string Or(string leftCondition, string rightCondition)
        {
            return Tag(CamlConstants.Or, null, null, leftCondition + rightCondition);
        }

        /// <summary>
        /// Creates CAML order by with the specified field references.
        /// </summary>
        /// <param name="fieldRefElements">The field reference elements.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        public string OrderBy(string fieldRefElements)
        {
            return Tag(CamlConstants.OrderBy, null, null, fieldRefElements);
        }

        /// <summary>
        /// Creates CAML order by with the specified arguments.
        /// </summary>
        /// <param name="arguments">The query arguments.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        public string OrderBy(params object[] arguments)
        {
            var text = string.Empty;
            for (var i = 0; i < arguments.Length; i++)
            {
                object obj = arguments[i];
                text += obj.ToString();
            }

            return Tag(CamlConstants.OrderBy, null, null, text);
        }

        /// <summary>
        /// Creates CAML value with the specified value.
        /// </summary>
        /// <param name="fieldValue">The field value.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        public string Value(string fieldValue)
        {
            return Tag(CamlConstants.Value, CamlConstants.Type, CamlConstants.Text, fieldValue);
        }

        /// <summary>
        /// Creates CAML value with the specified value.
        /// </summary>
        /// <param name="fieldValue">The field value.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        public string Value(int fieldValue)
        {
            return Tag(CamlConstants.Value, CamlConstants.Type, CamlConstants.Integer, fieldValue.ToString());
        }

        /// <summary>
        /// Creates CAML value with the specified value.
        /// </summary>
        /// <param name="fieldValue">The field value.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        public string Value(DateTime fieldValue)
        {
            return Tag(CamlConstants.Value, CamlConstants.Type, CamlConstants.DateTime, fieldValue.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Creates CAML value with the specified value.
        /// </summary>
        /// <param name="fieldValue">The field value.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        public string Value(bool fieldValue)
        {
            return Tag(CamlConstants.Value, CamlConstants.Type, CamlConstants.Boolean, fieldValue.ToString());
        }

        /// <summary>
        /// Creates CAML value with the specified type and value.
        /// </summary>
        /// <param name="valueType">Type of the value.</param>
        /// <param name="fieldValue">The field value.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        public string Value(string valueType, string fieldValue)
        {
            return Tag(CamlConstants.Value, CamlConstants.Type, valueType, fieldValue);
        }

        /// <summary>
        /// Creates CAML view fields with the specified fields.
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        public string ViewFields(params object[] fields)
        {
            var text = string.Empty;
            for (var i = 0; i < fields.Length; i++)
            {
                object obj = fields[i];
                text += obj.ToString();
            }

            return Tag(CamlConstants.ViewFields, null, null, text);
        }

        /// <summary>
        /// Creates CAML view fields with the specified entity type.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        public string ViewFieldsForEntityType(Type entityType)
        {
            var viewFieldsString = string.Empty;
            var propertyInfos = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var info in propertyInfos)
            {
                var customAttributes = info.GetCustomAttributes(typeof(PropertyAttribute), false);
                var propertyDetails = customAttributes.OfType<PropertyAttribute>().FirstOrDefault();

                if (propertyDetails != null)
                {
                    var fieldInternalName = !string.IsNullOrEmpty(propertyDetails.PropertyName) ? propertyDetails.PropertyName : info.Name;
                    viewFieldsString += string.Format(CultureInfo.InvariantCulture, "<FieldRef Name='{0}' />", fieldInternalName);
                }
            }

            return viewFieldsString;
        }

        /// <summary>
        /// Creates CAML webs with the specified scope.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        public string Webs(CamlEnums.QueryScope scope)
        {
            return Tag(CamlConstants.Webs, null, CamlConstants.Scope, scope.ToString());
        }

        /// <summary>
        /// Creates CAML project property with the specified property name.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        public string ProjectProperty(string propertyName)
        {
            return Tag(CamlConstants.ProjectProperty, CamlConstants.Select, propertyName, null);
        }

        /// <summary>
        /// Creates CAML project property with the specified property name and default value.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>
        /// A string representation of the CAML query.
        /// </returns>
        public string ProjectProperty(string propertyName, string defaultValue)
        {
            var attributes = new object[]
            {
                CamlConstants.Select,
                propertyName,
                CamlConstants.Default,
                defaultValue
            };

            return Tag(CamlConstants.ProjectProperty, null, attributes);
        }

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
        public string ProjectProperty(string propertyName, string defaultValue, CamlEnums.AutoHyperlinkType autoHyperlinkType, bool autoNewLine, bool expandXml, bool htmlEncode, bool stripWhiteSpace, CamlEnums.UrlEncodingType urlEncodingType)
        {
            var attributes = new object[]
            {
                CamlConstants.Select,
                propertyName,
                CamlConstants.Default,
                defaultValue,
                (autoHyperlinkType == CamlEnums.AutoHyperlinkType.Plain) ? CamlConstants.AutoHyperLinkNoEncoding : CamlConstants.AutoHyperLink,
                (autoHyperlinkType == CamlEnums.AutoHyperlinkType.None) ? "FALSE" : "TRUE",
                CamlConstants.AutoNewLine,
                autoNewLine ? "TRUE" : "FALSE",
                CamlConstants.HTMLEncode,
                htmlEncode ? "TRUE" : "FALSE",
                CamlConstants.StripWhitespace,
                stripWhiteSpace ? "TRUE" : "FALSE",
                (urlEncodingType == CamlEnums.UrlEncodingType.EncodeAsUrl) ? CamlConstants.UrlEncodeAsUrl : CamlConstants.UrlEncode,
                (urlEncodingType == CamlEnums.UrlEncodingType.None) ? "FALSE" : "TRUE"
            };

            return Tag(CamlConstants.ProjectProperty, null, attributes);
        }

        /// <summary>
        /// Creates CAML where with the specified condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <returns>A string representation of the CAML query.</returns>
        public string Where(string condition)
        {
            return Tag(CamlConstants.Where, null, null, condition);
        }

        /// <summary>
        /// Creates CAML XML with the specified condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <returns>A string representation of the CAML query.</returns>
        public string Xml(string condition)
        {
            return Tag(CamlConstants.Xml, null, null, condition);
        }

        /// <summary>
        /// Generates a CAML filter for a Taxonomy Term
        /// </summary>
        /// <param name="list">The list over which the query will be done</param>
        /// <param name="taxonomyFieldInternalName">The name of the site column associated with the term set</param>
        /// <param name="term">Term to match for</param>
        /// <param name="includeDescendants">Whether the Term's child terms should be query hits as well</param>
        /// <returns>A string representation of the CAML query.</returns>
        public string TermFilter(SPList list, string taxonomyFieldInternalName, Term term, bool includeDescendants)
        {
            return this.TermFilter(list, taxonomyFieldInternalName, new List<Term>() { term }, includeDescendants);
        }

        /// <summary>
        /// Generates a CAML filter for a Taxonomy Term
        /// </summary>
        /// <param name="list">The list over which the query will be done</param>
        /// <param name="taxonomyFieldInternalName">The name of the site column associated with the term set</param>
        /// <param name="terms">List of terms for why we want to match in an OR fashion</param>
        /// <param name="includeDescendants">Whether the Term's child terms should be query hits as well</param>
        /// <returns>A string representation of the CAML query.</returns>
        public string TermFilter(SPList list, string taxonomyFieldInternalName, IList<Term> terms, bool includeDescendants)
        {
            string values = string.Empty;

            foreach (var term in terms)
            {
                try
                {
                    values += this.GetAllWssIdByTerm(list, term, includeDescendants);
                }
                catch (ArgumentException)
                {
                    // ignore the not-found labels
                }
            }

            // Filter over the taxonomy field with the proper SID lookup id to the taxonomy hidden list
            if (!string.IsNullOrEmpty(values))
            {
                var query = string.Format(
                    CultureInfo.InvariantCulture,
                    "<In><FieldRef Name='{0}' LookupId='TRUE'/><Values>{1}</Values></In>",
                    taxonomyFieldInternalName,
                    values);

                return query;
            }

            return string.Empty;
        }

        /// <summary>
        /// Generates a CAML filter for a Taxonomy Term from the site-collection-reserved term store group
        /// </summary>
        /// <param name="list">The list over which the query will be done</param>
        /// <param name="taxonomyFieldInternalName">The name of the site column associated with the term set</param>
        /// <param name="termSetName">Name of the term set</param>
        /// <param name="termLabel">Label by which to find the term (dupes not supported)</param>
        /// <param name="includeDescendants">Whether the Term's child terms should be query hits as well</param>
        /// <returns>A string representation of the CAML query.</returns>
        public string TermFilter(SPList list, string taxonomyFieldInternalName, string termSetName, string termLabel, bool includeDescendants)
        {
            var taxonomyTerm = this._taxonomyService.GetTermForLabel(list.ParentWeb.Site, termSetName, termLabel);

            if (taxonomyTerm == null)
            {
                string msg = string.Format(CultureInfo.InvariantCulture, "Unable to find term with label '{0}' in site '{1}' while creating query filter.", termLabel, list.ParentWeb.Site);
                throw new ArgumentException(msg);
            }

            return this.TermFilter(list, taxonomyFieldInternalName, taxonomyTerm, includeDescendants);
        }

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
        public string TermFilter(SPList list, string taxonomyFieldInternalName, string termStoreGroupName, string termSetName, string termLabel, bool includeDescendants)
        {
            var taxonomyTerm = this._taxonomyService.GetTermForLabel(list.ParentWeb.Site, termStoreGroupName, termSetName, termLabel);

            if (taxonomyTerm == null)
            {
                string msg = string.Format(CultureInfo.InvariantCulture, "Unable to find term with label '{0}' in site '{1}' while creating query filter.", termLabel, list.ParentWeb.Site);
                throw new ArgumentException(msg);
            }

            return this.TermFilter(list, taxonomyFieldInternalName, new List<Term>() { taxonomyTerm }, includeDescendants);
        }

        /// <summary>
        /// Generates a CAML filter for a Taxonomy Term in a global farm term store group
        /// </summary>
        /// <param name="list">The list over which the query will be done</param>
        /// <param name="taxonomyFieldInternalName">The name of the site column associated with the term set</param>
        /// <param name="termId">ID by which to find the term (dupes not supported)</param>
        /// <param name="includeDescendants">Whether the Term's child terms should be query hits as well</param>
        /// <returns>A string representation of the CAML query.</returns>
        public string TermFilter(SPList list, string taxonomyFieldInternalName, Guid termId, bool includeDescendants)
        {
            var taxonomyTerm = this._taxonomyService.GetTermForId(list.ParentWeb.Site, termId);

            if (taxonomyTerm == null)
            {
                var msg = string.Format(CultureInfo.InvariantCulture, "Unable to find term with ID '{0:B}' in site '{1}' while creating query filter.", termId, list.ParentWeb.Site);
                throw new ArgumentException(msg);
            }

            return this.TermFilter(list, taxonomyFieldInternalName, new List<Term>() { taxonomyTerm }, includeDescendants);
        }

        private static string Tag(string tag, string attribute, string attributeValue, string value)
        {
            if (string.IsNullOrEmpty(attribute) || string.IsNullOrEmpty(attributeValue))
            {
                return !string.IsNullOrEmpty(value) ? string.Format("<{0}>{1}</{0}>", tag, value) : string.Format("<{0} />", tag);
            }

            if (!string.IsNullOrEmpty(value))
            {
                var formatArguments = new object[]
                {
                    tag,
                    attribute,
                    attributeValue,
                    value
                };
                return string.Format("<{0} {1}=\"{2}\">{3}</{0}>", formatArguments);
            }

            return string.Format("<{0} {1}=\"{2}\" />", tag, attribute, attributeValue);
        }

        private static string Tag(string tag, string value, params object[] attributeValuePairs)
        {
            var stringBuilder = new StringBuilder("<" + tag);
            for (var i = 0; i < attributeValuePairs.Length - 1; i += 2)
            {
                stringBuilder.AppendFormat(" {0}=\"{1}\"", attributeValuePairs[i], attributeValuePairs[i + 1]);
            }

            if (string.IsNullOrEmpty(value))
            {
                stringBuilder.Append(" />");
            }
            else
            {
                stringBuilder.AppendFormat(">{0}</{1}>", value, tag);
            }

            return stringBuilder.ToString();
        }

        private string GetAllWssIdByTerm(SPList list, Term term, bool includeDescendants)
        {
            if (term != null)
            {
                // Get the lookup Ids of all taxonomy field values that point to this term or its decendants in the taxonomy hidden list
                var wssIds = TaxonomyField.GetWssIdsOfTerm(list.ParentWeb.Site, term.TermStore.Id, term.TermSet.Id, term.Id, includeDescendants, int.MaxValue);
                if (wssIds.Count() > 0)
                {
                    // Filter over the taxonomy field with the proper SID lookup id to the taxonomy hidden list
                    return string.Join(string.Empty, wssIds.Select(wssId => "<Value Type=\"Integer\">" + wssId + "</Value>").ToArray());
                }
                else
                {
                    this._logger.Warn("Failed to find any item in the site collection that matches the term '" + term.Name + "'");
                    throw new ArgumentException("No usage found for term with id " + term.Id);
                }
            }
            else
            {
                throw new ArgumentNullException("term");
            }
        }
    }
}
