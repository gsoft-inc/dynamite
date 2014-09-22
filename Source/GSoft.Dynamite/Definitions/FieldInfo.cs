using System;
using System.Data;
using System.Xml.Linq;
using GSoft.Dynamite.Binding;
using GSoft.Dynamite.Definitions.Values;
using Microsoft.Office.Server.ApplicationRegistry.MetadataModel;

namespace GSoft.Dynamite.Definitions
{
    /// <summary>
    /// Defines the field info structure.
    /// </summary>
    public class FieldInfo : BaseTypeInfo
    {
        #region Properties backing fields

        private string _internalName;

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public FieldInfo()
        {            
        }

        /// <summary>
        /// Initializes a new FieldInfo
        /// </summary>
        /// <param name="internalName">The internal name of the field</param>
        /// <param name="Id">The field identifier</param>
        public FieldInfo(string internalName, Guid Id)
        {
            this.InternalName = internalName;
            this.Id = Id;
        }

        /// <summary>
        /// The internal name of the field
        /// </summary>
        public string InternalName
        {
            get
            {
                return this._internalName;
            }

            set
            {
                this._internalName = value;

                // Set the static name identical to the internal name
                this.StaticName = value;
            }
        }

        /// <summary>
        /// Unique identifier of the field
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Indicates if the field is required
        /// </summary>
        public RequiredTypes RequiredType { get; set; }

        /// <summary>
        /// Indicates if the field must enforce unique values
        /// </summary>
        public bool EnforceUniqueValues { get; set; }

        /// <summary>
        /// The static name of the field
        /// </summary>
        public string StaticName { get; set; }

        /// <summary>
        /// Type of the field
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The XML schema of the field
        /// </summary>
        public XElement Schema { get; set; }

        /// <summary>
        /// Default mapping configuration for the field
        /// </summary>
        public IFieldInfoValue DefaultValue { get; set; }

        /// <summary>
        /// The XElement XML format of the field
        /// </summary>
        /// <returns>The XML schema of the field as XElement</returns>
        public virtual XElement ToXElement()
        {
            return null;
        }

        /// <summary>
        /// The string XML format of the field
        /// </summary>
        /// <returns>The XML schema of the field as string</returns>
        public override string ToString()
        {
            return this.ToXElement().ToString();
        }
    }
}
