using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GSoft.Dynamite.Definitions;
using GSoft.Dynamite.FieldTypes;
using System.Xml.Linq;

namespace GSoft.Dynamite.UnitTests.FieldTypes
{
    /// <summary>
    /// Summary description for TextFieldInfo
    /// </summary>
    [TestClass]
    public class TextFieldInfoTest
    {
        [TestMethod]
        public void ShouldHaveAssociationToValueTypeString()
        {
            var textFieldDefinition = this.CreateTextFieldInfo(Guid.NewGuid());

            Assert.AreEqual(typeof(string), textFieldDefinition.AssociatedValueType);
        }

        [TestMethod]
        public void ShouldBeInitializedWithTypeText()
        {
            var textFieldDefinition = this.CreateTextFieldInfo(Guid.NewGuid());

            Assert.AreEqual("Text", textFieldDefinition.Type);
        }

        [TestMethod]
        public void ShouldBeInitializedWithDefaultMaxLength255()
        {
            var textFieldDefinition = this.CreateTextFieldInfo(Guid.NewGuid());

            Assert.AreEqual(255, textFieldDefinition.MaxLength);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldHaveId()
        {
            var textFieldDefinition = this.CreateTextFieldInfo(Guid.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldHaveInternalName()
        {
            var textFieldDefinition = this.CreateTextFieldInfo(Guid.Empty, internalName: "SomeName");
        }

        [TestMethod]
        public void ShouldBeAbleToCreateFromXml()
        {
            var xElement = XElement.Parse("<Field Name=\"SomeInternalName\" Type=\"Text\" ID=\"{7a937493-3c82-497c-938a-d7a362bd8086}\" StaticName=\"SomeInternalName\" DisplayName=\"SomeDisplayName\" Description=\"SomeDescription\" Group=\"Test\" EnforceUniqueValues=\"FALSE\" ShowInListSettings=\"TRUE\" MaxLength=\"255\" />");
            var textFieldDefinition = new TextFieldInfo(xElement);

            Assert.AreEqual("SomeInternalName", textFieldDefinition.InternalName);
            Assert.AreEqual("Text", textFieldDefinition.Type);
            Assert.AreEqual(new Guid("{7a937493-3c82-497c-938a-d7a362bd8086}"), textFieldDefinition.Id);
            Assert.AreEqual("SomeDisplayName", textFieldDefinition.DisplayName);
            Assert.AreEqual("SomeDescription", textFieldDefinition.Description);
            Assert.AreEqual("Test", textFieldDefinition.Group);
            Assert.AreEqual(255, textFieldDefinition.MaxLength);
        }

        [TestMethod]
        public void Schema_ShouldOutputValidFieldXml()
        {
            var textFieldDefinition = this.CreateTextFieldInfo(new Guid("{7a937493-3c82-497c-938a-d7a362bd8086}"));

            var validXml = "<Field Name=\"SomeInternalName\" Type=\"Text\" ID=\"{7a937493-3c82-497c-938a-d7a362bd8086}\" StaticName=\"SomeInternalName\" DisplayName=\"SomeDisplayName\" Description=\"SomeDescription\" Group=\"Test\" EnforceUniqueValues=\"FALSE\" ShowInListSettings=\"TRUE\" MaxLength=\"255\" />";

            Assert.AreEqual(validXml, textFieldDefinition.Schema.ToString());
        }

        [TestMethod]
        public void toString_ShouldOutputValidFieldXml()
        {
            var textFieldDefinition = this.CreateTextFieldInfo(new Guid("{7a937493-3c82-497c-938a-d7a362bd8086}"));

            var validXml = "<Field Name=\"SomeInternalName\" Type=\"Text\" ID=\"{7a937493-3c82-497c-938a-d7a362bd8086}\" StaticName=\"SomeInternalName\" DisplayName=\"SomeDisplayName\" Description=\"SomeDescription\" Group=\"Test\" EnforceUniqueValues=\"FALSE\" ShowInListSettings=\"TRUE\" MaxLength=\"255\" />";

            Assert.AreEqual(validXml, textFieldDefinition.ToString());
        }

        private TextFieldInfo CreateTextFieldInfo(
            Guid id,
            string internalName = "SomeInternalName",
            string displayNameResourceKey = "SomeDisplayName",
            string descriptionResourceKey = "SomeDescription",
            string group = "Test")
        {
            return new TextFieldInfo(internalName, id, displayNameResourceKey, descriptionResourceKey, group);
        }
    }
}
