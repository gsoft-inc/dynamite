using System.Xml.Linq;
using GSoft.Dynamite.Binding;
using System;
using System.Globalization;
using GSoft.Dynamite.ValueTypes;

namespace GSoft.Dynamite.Fields
{
    /// <summary>
    /// Definition of a UserField info
    /// </summary>
    public class UserFieldFieldInfo : FieldInfo<UserValue>
    {
        /// <summary>
        /// Initializes a new UserFieldFieldInfo
        /// </summary>
        /// <param name="internalName">The internal name of the field</param>
        /// <param name="id">The field identifier</param>
        /// <param name="displayNameResourceKey">Display name resource key</param>
        /// <param name="descriptionResourceKey">Description resource key</param>
        /// <param name="groupResourceKey">Description resource key</param>
        public UserFieldFieldInfo(string internalName, Guid id, string displayNameResourceKey, string descriptionResourceKey, string groupResourceKey)
            : base(internalName, id, "User", displayNameResourceKey, descriptionResourceKey, groupResourceKey)
        {
            // default person name
            this.ShowField = "ImnName";
            this.UserSelectionMode = "PeopleOnly";
            this.UserSelectionScope = 0;   // default is 0 for no group constraint
        }

        /// <summary>
        /// Creates a new FieldInfo object from an existing field schema XML
        /// </summary>
        /// <param name="fieldSchemaXml">Field's XML definition</param>
        public UserFieldFieldInfo(XElement fieldSchemaXml)
            : base(fieldSchemaXml)
        {
            if (fieldSchemaXml.Attribute("ShowField") != null)
            {
                this.ShowField = fieldSchemaXml.Attribute("ShowField").Value;
            }

            if (fieldSchemaXml.Attribute("UserSelectionMode") != null)
            {
                this.UserSelectionMode = fieldSchemaXml.Attribute("UserSelectionMode").Value;
            }

            if (fieldSchemaXml.Attribute("UserSelectionScope") != null)
            {
                this.UserSelectionScope = int.Parse(fieldSchemaXml.Attribute("UserSelectionScope").Value, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// User profile property that will be shown
        /// </summary>
        public string ShowField { get; set; }

        /// <summary>
        /// PeopleOnly or PeopleAndGroups
        /// </summary>
        public string UserSelectionMode { get; set; }

        /// <summary>
        /// The id of the group from which we want people to select people
        /// </summary>
        public int UserSelectionScope { get; set; }

        /// <summary>
        /// The XML schema of the Note field
        /// </summary>
        public override XElement Schema
        {
            get
            {
                var schema = this.BasicFieldSchema;

                schema.Add(new XAttribute("List", "UserInfo"));
                schema.Add(new XAttribute("ShowField", this.ShowField));
                schema.Add(new XAttribute("UserSelectionMode", this.UserSelectionMode));
                schema.Add(new XAttribute("UserSelectionScope", this.UserSelectionScope));

                return schema;
            }
        }
    }
}
