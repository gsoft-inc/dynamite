using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using GSoft.Dynamite.Logging;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;

namespace GSoft.Dynamite.Utils
{
    /// <summary>
    /// Helper class for managing SP Fields.
    /// </summary>
    public class FieldHelper
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Default constructor with dependency injection
        /// </summary>
        /// <param name="logger">The logger</param>
        public FieldHelper(ILogger logger)
        {
            this._logger = logger;
        }

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
        public void SetLookupToList(SPWeb web, Guid fieldId, string listUrl)
        {
            if (web == null)
            {
                throw new ArgumentNullException("web");
            }

            if (fieldId == null)
            {
                throw new ArgumentNullException("fieldId");
            }

            if (string.IsNullOrEmpty(listUrl))
            {
                throw new ArgumentNullException("listUrl");
            }

            this._logger.Info("Start method 'SetLookupToList' for field id: '{0}'", fieldId);
            
            // Get the field and the list.
            SPFieldLookup lookupField = web.Fields[fieldId] as SPFieldLookup;
            SPList lookupList = web.GetList(SPUtility.ConcatUrls(web.ServerRelativeUrl, listUrl));

            if (lookupField == null)
            {
                throw new ArgumentException("Unable to find the lookup field.", "fieldId");
            }

            // Get the fields schema xml.
            XDocument fieldSchema = XDocument.Parse(lookupField.SchemaXml);
            XElement root = fieldSchema.Root;

            // Reset the attributes list and source id.
            root.SetAttributeValue("List", lookupList.ID);
            root.SetAttributeValue("SourceID", lookupList.ParentWeb.ID);

            // Update the lookup field.
            lookupField.SchemaXml = fieldSchema.ToString();

            this._logger.Info("End method 'SetLookupToList'.");
        }

        /// <summary>
        /// Adds a collection of fields defined in xml to a collection of fields.
        /// </summary>
        /// <param name="fieldCollection">The SPField collection.</param>
        /// <param name="fieldsXml">The field schema XMLs.</param>
        /// <returns>A collection of strings that contain the internal name of the new fields.</returns>
        /// <exception cref="System.ArgumentNullException">Null fieldsXml parameter</exception>
        public IList<string> AddFields(SPFieldCollection fieldCollection, XDocument fieldsXml)
        {
            if (fieldsXml == null)
            {
                throw new ArgumentNullException("fieldsXml");
            }

            this._logger.Info("Start method 'AddFields'");

            IList<string> internalNames = new List<string>();

            // Get all the field declerations in the XmlDocument.
            var fields = fieldsXml.Root.Elements("Field");

            this._logger.Info("Found '{0}' fields to add.", fields.Count());

            foreach (XElement field in fields)
            {
                // Add the field to the collection.
                string internalname = this.AddField(fieldCollection, field);
                if (!string.IsNullOrEmpty(internalname))
                {
                    internalNames.Add(internalname);
                }
            }

            this._logger.Info("End method 'AddFields'. Returning '{0}' internal names.", internalNames.Count);

            // Return a list of the fields that where created.
            return internalNames;
        }

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
        public string AddField(SPFieldCollection fieldCollection, XElement fieldXml)
        {
            if (fieldCollection == null)
            {
                throw new ArgumentNullException("fieldCollection");
            }

            if (fieldXml == null)
            {
                throw new ArgumentNullException("fieldXml");
            }

            this._logger.Info("Start method 'AddField'");

            Guid id = Guid.Empty;
            string displayName = string.Empty;
            string internalName = string.Empty;

            // Validate the xml of the field and get its 
            if (this.IsFieldXmlValid(fieldXml, out id, out displayName, out internalName))
            {
                // Check if the field already exists. Skip the creation if so.
                if (!this.FieldExists(fieldCollection, displayName, id))
                {
                    // If its a lookup we need to fix up the xml.
                    if (this.IsLookup(fieldXml))
                    {
                        fieldXml = this.FixLookupFieldXml(fieldCollection.Web, fieldXml);
                    }

                    string addedInternalName = fieldCollection.AddFieldAsXml(fieldXml.ToString(), false, SPAddFieldOptions.Default);

                    this._logger.Info("End method 'AddField'. Added field with internal name '{0}'", addedInternalName);

                    return addedInternalName;
                }
                else
                {
                    this._logger.Warn("End method 'AddField'. Field with id '{0}' and display name '{1}' was not added because it already exists in the collection.", id, displayName);
                    return string.Empty;
                }
            }
            else
            {
                string msg = string.Format(CultureInfo.InvariantCulture, "Unable to create field. Invalid xml. id: '{0}' DisplayName: '{1}' Name: '{2}'", id, displayName, internalName);
                throw new FormatException(msg);
            }
        }

        private static string GetAttributeValue(XElement fieldXml, string key)
        {
            XAttribute attribute = fieldXml.Attribute(key);
            if (attribute != null)
            {
                return attribute.Value;
            }
            else
            {
                return string.Empty;
            }
        }

        private bool FieldExists(SPFieldCollection fieldCollection, string displayName, Guid fieldId)
        {
            if (fieldCollection.Contains(fieldId))
            {
                // If Id is found in the collection.
                this._logger.Warn("Field with id '{0}' is already in the collection.", fieldId);
                return true;
            }

            SPField field;
            try
            {
                // Throws argument exception if not in collection.
                field = fieldCollection.GetField(displayName);
            }
            catch (ArgumentException)
            {
                return false;
            }

            if (field == null)
            {
                // Still can't find the field in the collection
                return false;
            }
            else
            {
                // We found it!
                this._logger.Warn("Field with display name '{0}' is already in the collection.", displayName);
                return true;
            }
        }

        private XElement FixLookupFieldXml(SPWeb web, XElement fieldXml)
        {
            this._logger.Info("Fixing up lookup field xml.");

            // Validate the list attribute is present.
            string list = GetAttributeValue(fieldXml, "List");
            if (string.IsNullOrEmpty(list))
            {
                string displayName = GetAttributeValue(fieldXml, "DisplayName");
                string msg = string.Format(CultureInfo.InvariantCulture, "Unable to create Lookup Field '{0}' because it is missing the 'List' attribute.", displayName);
                throw new ArgumentException(msg);
            }

            // Get the lookup list in the current web.
            string listPath = SPUtility.ConcatUrls(web.ServerRelativeUrl, list);
            SPList lookupList = web.GetList(listPath);

            // Get the required attribute.
            bool required;
            if (!bool.TryParse(GetAttributeValue(fieldXml, "Required"), out required))
            {
                required = false;
            }

            // prepare xml values the same way SharePoint does it...
            string listValue = lookupList.ID.ToString("B").ToUpper(CultureInfo.InvariantCulture);
            string webIdValue = web.ID.ToString();
            string requiredValue = required ? "TRUE" : "FALSE";

            this._logger.Info("Setting field xml attributes, List: '{0}' WebId: '{1}' Required: '{2}'", listValue, webIdValue, requiredValue);

            // Update the xml.
            fieldXml.SetAttributeValue("List", listValue);
            fieldXml.SetAttributeValue("WebId", webIdValue);
            fieldXml.SetAttributeValue("Required", requiredValue);

            // Return the modified xml.
            return fieldXml;
        }

        private bool IsLookup(XElement fieldXml)
        {
            string fieldType = GetAttributeValue(fieldXml, "Type");
            this._logger.Info("Field is of type '{0}'", fieldType);
            return string.Compare(fieldType, "Lookup", true, CultureInfo.InvariantCulture) == 0;
        }

        private bool IsFieldXmlValid(XElement fieldXml, out Guid id, out string displayName, out string internalName)
        {
            id = Guid.Empty;
            displayName = string.Empty;
            internalName = string.Empty;

            // Validate the ID attribute
            string strId = GetAttributeValue(fieldXml, "ID");
            if (string.IsNullOrEmpty(strId))
            {
                this._logger.Fatal("Attribute 'ID' is required.");
                return false;
            }
            else
            {
                try
                {
                    id = new Guid(strId);
                }
                catch (FormatException)
                {
                    this._logger.Fatal("Attribute ID: '{0}' needs to be a guid.", strId);
                    return false;
                }
                catch (OverflowException)
                {
                    this._logger.Fatal("Attribute ID: '{0}' needs to be a guid.", strId);
                    return false;
                }
            }

            // Validate display Name
            displayName = GetAttributeValue(fieldXml, "DisplayName");
            if (string.IsNullOrEmpty(displayName))
            {
                this._logger.Fatal("Attribute 'DisplayName' is required for field with id: '{0}'.", id);
                return false;
            }
            
            // Validate internal name
            internalName = GetAttributeValue(fieldXml, "Name");
            if (string.IsNullOrEmpty(internalName))
            {
                this._logger.Fatal("Attribute 'Name' is required for field with id: '{0}'.", id);
                return false;
            }

            // Everything is valid.
            return true;
        }
    }
}
