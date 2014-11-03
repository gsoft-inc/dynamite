using System;
using System.Xml.Linq;
using GSoft.Dynamite.Fields;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSoft.Dynamite.UnitTests.Fields
{
    /// <summary>
    /// Validates behavior of <see cref="TextFieldInfo"/>
    /// </summary>
    [TestClass]
    public class TextFieldInfoTest
    {
        /// <summary>
        /// Validates that string is the associated value type
        /// </summary>
        [TestMethod]
        public void ShouldHaveAssociationToValueTypeString()
        {
            var textFieldDefinition = this.CreateTextFieldInfo(Guid.NewGuid());

            Assert.AreEqual(typeof(string), textFieldDefinition.AssociatedValueType);
        }

        /// <summary>
        /// Validates that Text is the site column type name
        /// </summary>
        [TestMethod]
        public void ShouldBeInitializedWithTypeText()
        {
            var textFieldDefinition = this.CreateTextFieldInfo(Guid.NewGuid());

            Assert.AreEqual("Text", textFieldDefinition.Type);
        }

        /// <summary>
        /// Validates that maximum length should be 255 by default
        /// </summary>
        [TestMethod]
        public void ShouldBeInitializedWithDefaultMaxLength255()
        {
            var textFieldDefinition = this.CreateTextFieldInfo(Guid.NewGuid());

            Assert.AreEqual(255, textFieldDefinition.MaxLength);
        }

        /// <summary>
        /// Validates that ID is always given
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldHaveId()
        {
            var textFieldDefinition = this.CreateTextFieldInfo(Guid.Empty);
        }

        /// <summary>
        /// Validates that Name is always given
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldHaveInternalName()
        {
            var textFieldDefinition = this.CreateTextFieldInfo(Guid.Empty, internalName: "SomeName");
        }

        /// <summary>
        /// Validates that XML definition can be used as input
        /// </summary>
        [TestMethod]
        public void ShouldBeAbleToCreateFromXml()
        {
            var xmlElement = XElement.Parse("<Field Name=\"SomeInternalName\" Type=\"Text\" ID=\"{7a937493-3c82-497c-938a-d7a362bd8086}\" StaticName=\"SomeInternalName\" DisplayName=\"SomeDisplayName\" Description=\"SomeDescription\" Group=\"Test\" EnforceUniqueValues=\"FALSE\" ShowInListSettings=\"TRUE\" MaxLength=\"255\" />");
            var textFieldDefinition = new TextFieldInfo(xmlElement);

            Assert.AreEqual("SomeInternalName", textFieldDefinition.InternalName);
            Assert.AreEqual("Text", textFieldDefinition.Type);
            Assert.AreEqual(new Guid("{7a937493-3c82-497c-938a-d7a362bd8086}"), textFieldDefinition.Id);
            Assert.AreEqual("SomeDisplayName", textFieldDefinition.DisplayName);
            Assert.AreEqual("SomeDescription", textFieldDefinition.Description);
            Assert.AreEqual("Test", textFieldDefinition.Group);
            Assert.AreEqual(255, textFieldDefinition.MaxLength);
        }

        /// <summary>
        /// Validates that XML definition can be printed as output through Schema
        /// </summary>
        [TestMethod]
        public void Schema_ShouldOutputValidFieldXml()
        {
            var textFieldDefinition = this.CreateTextFieldInfo(new Guid("{7a937493-3c82-497c-938a-d7a362bd8086}"));
            textFieldDefinition.MaxLength = 400;    // test out the MaxLength param

            var validXml = "<Field Name=\"SomeInternalName\" Type=\"Text\" ID=\"{7a937493-3c82-497c-938a-d7a362bd8086}\" StaticName=\"SomeInternalName\" DisplayName=\"SomeDisplayName\" Description=\"SomeDescription\" Group=\"Test\" EnforceUniqueValues=\"FALSE\" ShowInListSettings=\"TRUE\" MaxLength=\"400\" />";

            Assert.AreEqual(validXml, textFieldDefinition.Schema.ToString());
        }

        /// <summary>
        /// Validates that XML definition can be printed as output through ToString
        /// </summary>
        [TestMethod]
        public void ToString_ShouldOutputValidFieldXml()
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
