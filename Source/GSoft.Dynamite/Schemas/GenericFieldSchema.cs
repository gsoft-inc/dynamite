using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GSoft.Dynamite.Schemas
{
    /// <summary>
    /// Generic XML schema for a SharePoint field
    /// </summary>
    public abstract class GenericFieldSchema
    {
        protected string _fieldName;
        protected string _fieldStaticName;
        protected string _fieldType;
        protected string _fieldDisplayName;
        protected string _fieldDescription;
        protected string _fieldGroup;
        protected Guid _fieldId;

        protected XElement _fieldSchema;

        #region Properties

        public string FieldName
        {
            get { return _fieldName; }
            set { _fieldName = value; }
        }

        public string FieldStaticName
        {
            get { return _fieldStaticName; }
            set { _fieldStaticName = value; }
        }

        public string FieldType
        {
            get { return _fieldType; }
            set { _fieldType = value; }
        }
     
        public string FieldDisplayName
        {
            get { return _fieldDisplayName; }
            set { _fieldDisplayName = value; }
        }

        public string FieldDescription
        {
            get { return _fieldDescription; }
            set { _fieldDescription = value; }
        }

        public string FieldGroup
        {
            get { return _fieldGroup; }
            set { _fieldGroup = value; }
        }

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public GenericFieldSchema()
        {
            this._fieldName = string.Empty;
            this._fieldStaticName = string.Empty;
            this._fieldDisplayName = string.Empty;
            this._fieldDescription = string.Empty;
            this._fieldGroup = string.Empty;
            this._fieldId = new Guid();
        }

        /// <summary>
        /// Get the XML schema of the field.
        /// </summary>
        /// <returns>The XML schema.</returns>
        public abstract XElement ToXElement();

        /// <summary>
        /// Get the XML schema as string of the field.
        /// </summary>
        /// <returns>A string that represents the XML schema.</returns>
        public abstract string ToString();
        
    }
}
