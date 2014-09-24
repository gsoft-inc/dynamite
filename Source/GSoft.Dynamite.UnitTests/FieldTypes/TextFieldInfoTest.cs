using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GSoft.Dynamite.Definitions;
using GSoft.Dynamite.FieldTypes;

namespace GSoft.Dynamite.UnitTests.FieldTypes
{
    /// <summary>
    /// Summary description for TextFieldInfo
    /// </summary>
    [TestClass]
    public class TextFieldInfoTest
    {
        [TestMethod]
        public void ShouldHaveValueTypeString()
        {
            var textFieldDefinition = this.CreateTextFieldInfo();

            Assert.AreEqual(typeof(string), textFieldDefinition.AssociatedValueType);
        }

        [TestMethod]
        public void ShouldBeInitializedWithTypeText()
        {
            var textFieldDefinition = this.CreateTextFieldInfo();

            Assert.AreEqual("Text", textFieldDefinition.Type);
        }

        [TestMethod]
        public void ShouldBeInitializedWithDefaultMaxLength255()
        {
            var textFieldDefinition = this.CreateTextFieldInfo();

            Assert.AreEqual(255, textFieldDefinition.MaxLength);
        }

        [TestMethod]
        public void Schema_ShouldOutputValidFieldXml()
        {
            var textFieldDefinition = this.CreateTextFieldInfo();

            var validXml = "";

            Assert.AreEqual(validXml, textFieldDefinition.Schema.ToString());
        }

        [TestMethod]
        public void toString_ShouldOutputValidFieldXml()
        {
            var textFieldDefinition = this.CreateTextFieldInfo();

            var validXml = "";

            Assert.AreEqual(validXml, textFieldDefinition.ToString());
        }

        private TextFieldInfo CreateTextFieldInfo(
            string internalName = "SomeInternalName",
            Guid id = new Guid(),
            string displayNameResourceKey = "SomeDisplayName",
            string descriptionResourceKey = "SomeDescription",
            string group = "Test")
        {
            return new TextFieldInfo(internalName, id, displayNameResourceKey, descriptionResourceKey, group);
        }
    }
}
