using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using GSoft.Dynamite.Logging;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;

namespace GSoft.Dynamite.Fields
{
    /// <summary>
    /// Helps in configuring lookup fields
    /// </summary>
    public class FieldLookupHelper : IFieldLookupHelper
    {
        private ILogger logger;
        private IFieldLocator fieldLocator;

        /// <summary>
        /// Initializes a new <see cref="FieldLookupHelper"/> instance
        /// </summary>
        /// <param name="fieldLocator">Field finder</param>
        /// <param name="logger">Logging utility</param>
        public FieldLookupHelper(IFieldLocator fieldLocator, ILogger logger)
        {
            this.fieldLocator = fieldLocator;
            this.logger = logger;
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
        [Obsolete("Use method 'SetLookupToList' with SPFieldCollection as first parameter.")]
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

            this.logger.Info("Start method 'SetLookupToList' for field id: '{0}'", fieldId);

            // Get the field.
            SPFieldLookup lookupField = this.fieldLocator.GetFieldById(web.Fields, fieldId) as SPFieldLookup;
            if (lookupField == null)
            {
                throw new ArgumentException("Unable to find the lookup field.", "fieldId");
            }

            // Get the list
            SPList lookupList = web.GetList(SPUtility.ConcatUrls(web.ServerRelativeUrl, listUrl));

            // Configure the lookup field.
            this.SetLookupToList(lookupField, lookupList);

            this.logger.Info("End method 'SetLookupToList'.");
        }

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
        public void SetLookupToList(SPFieldCollection fieldCollection, Guid fieldId, SPList lookupList)
        {
            if (fieldCollection == null)
            {
                throw new ArgumentNullException("fieldCollection");
            }

            if (fieldId == null)
            {
                throw new ArgumentNullException("fieldId");
            }

            if (lookupList == null)
            {
                throw new ArgumentNullException("lookupList");
            }

            this.logger.Info("Start method 'SetLookupToList' for field id: '{0}'", fieldId);

            // Get the field.
            SPFieldLookup lookupField = this.fieldLocator.GetFieldById(fieldCollection, fieldId) as SPFieldLookup;
            if (lookupField == null)
            {
                throw new ArgumentException("Unable to find the lookup field.", "fieldId");
            }

            // Configure the lookup field.
            this.SetLookupToList(lookupField, lookupList);

            this.logger.Info("End method 'SetLookupToList'.");
        }

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
        public void SetLookupToList(SPFieldLookup lookupField, SPList lookupList)
        {
            if (lookupField == null)
            {
                throw new ArgumentNullException("lookupField", "The parameter 'lookupField' cannot be null.");
            }

            if (lookupList == null)
            {
                throw new ArgumentNullException("lookupList", "The parameter 'lookupList' cannot be null.");
            }

            this.logger.Info("Start method 'SetLookupToList' for field with id '{0}'", lookupField.Id);

            // Get the fields schema xml.
            XDocument fieldSchema = XDocument.Parse(lookupField.SchemaXml);
            XElement root = fieldSchema.Root;

            // Reset the attributes list and source id.
            root.SetAttributeValue("List", lookupList.ID);
            root.SetAttributeValue("SourceID", lookupList.ParentWeb.ID);

            // Update the lookup field.
            lookupField.SchemaXml = fieldSchema.ToString();

            this.logger.Info("End method 'SetLookupToList'.");
        }
    }
}
