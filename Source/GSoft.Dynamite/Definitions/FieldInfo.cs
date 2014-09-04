using System;
using System.Data;
using System.Xml.Linq;
using GSoft.Dynamite.Binding;
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

        public FieldInfo() { }

        public FieldInfo(string internalName, Guid Id)
        {
            this.InternalName = InternalName;
            this.Id = Id;
        }

        public string InternalName
        {
            get { return _internalName; }
            set
            {
                _internalName = value;

                // Set the static name identical to the internal name
                StaticName = value;
            }
        }

        public Guid Id { get; set; }

        public RequiredTypes RequiredType { get; set; }

        public bool EnforceUniqueValues { get; set; }

        public string StaticName { get; set; }

        public string Type { get; set; }

        public XElement Schema { get; set; }


        public virtual XElement ToXElement(){return null;}

        public override string ToString()
        {
            return this.ToXElement().ToString();
        }
    }
}
