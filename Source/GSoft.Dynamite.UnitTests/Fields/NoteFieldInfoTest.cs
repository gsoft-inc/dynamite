using System;
using System.Collections.Generic;
using System.Xml.Linq;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.Globalization;
using GSoft.Dynamite.ServiceLocator;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSoft.Dynamite.UnitTests.Fields
{
    /// <summary>
    /// Validation of NoteFieldInfo expected behavior
    /// </summary>
    [TestClass]
    public class NoteFieldInfoTest
    {
        /// <summary>
        /// Validates that value type is string
        /// </summary>
        [TestMethod]
        public void ShouldHaveAssociationToValueTypeString()
        {
            var noteFieldDefinition = this.CreateNoteFieldInfo(Guid.NewGuid());

            Assert.AreEqual(typeof(string), noteFieldDefinition.AssociatedValueType);
        }

        /// <summary>
        /// Validates that Note is the site column type
        /// </summary>
        [TestMethod]
        public void ShouldBeInitializedWithTypeNote()
        {
            var noteFieldDefinition = this.CreateNoteFieldInfo(Guid.NewGuid());

            Assert.AreEqual("Note", noteFieldDefinition.Type);
        }

        /// <summary>
        /// Validates that number of lines should be 6 by default
        /// </summary>
        [TestMethod]
        public void ShouldBeInitializedWithDefaultNumLines6()
        {
            var noteFieldDefinition = this.CreateNoteFieldInfo(Guid.NewGuid());

            Assert.AreEqual(6, noteFieldDefinition.NumLines);
        }

        /// <summary>
        /// Validates that an ID should always be given
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldHaveId()
        {
            var noteFieldDefinition = this.CreateNoteFieldInfo(Guid.Empty);
        }

        /// <summary>
        /// Validates that a Name should always be given
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldHaveInternalName()
        {
            var noteFieldDefinition = this.CreateNoteFieldInfo(Guid.Empty, internalName: "SomeName");
        }

        /// <summary>
        /// Validates that XML definition can be used as input
        /// </summary>
        [TestMethod]
        public void ShouldBeAbleToCreateFromXml()
        {
            var xmlElement = XElement.Parse("<Field Name=\"SomeInternalName\" Type=\"Note\" ID=\"{7a937493-3c82-497c-938a-d7a362bd8086}\" StaticName=\"SomeInternalName\" DisplayName=\"SomeDisplayName\" Description=\"SomeDescription\" Group=\"Test\" EnforceUniqueValues=\"FALSE\" ShowInListSettings=\"TRUE\" NumLines=\"6\" />");
            var noteFieldDefinition = new NoteFieldInfo(xmlElement);

            Assert.AreEqual("SomeInternalName", noteFieldDefinition.InternalName);
            Assert.AreEqual("Note", noteFieldDefinition.Type);
            Assert.AreEqual(new Guid("{7a937493-3c82-497c-938a-d7a362bd8086}"), noteFieldDefinition.Id);
            Assert.AreEqual("SomeDisplayName", noteFieldDefinition.DisplayNameResourceKey);
            Assert.AreEqual("SomeDescription", noteFieldDefinition.DescriptionResourceKey);
            Assert.AreEqual("Test", noteFieldDefinition.GroupResourceKey);
            Assert.AreEqual(6, noteFieldDefinition.NumLines);
        }

        /// <summary>
        /// Validates that XML definition can be printed as output through Schema
        /// </summary>
        [TestMethod]
        public void Schema_ShouldOutputValidFieldXml()
        {
            // TODO: fix schema validation - change to integration test maybe? because we need field schema helper now...
            ////var fieldSchemaHelper = new FieldSchemaHelper(new ResourceLocator(new List<IResourceLocatorConfig>() { new DefaultResourceLocatorConfig() }));

            ////var noteFieldDefinition = this.CreateNoteFieldInfo(new Guid("{7a937493-3c82-497c-938a-d7a362bd8086}"));
            ////noteFieldDefinition.NumLines = 4;           // testing out the NumLines param
            ////noteFieldDefinition.HasRichText = true;     // testing out RichText=On, look out for RichTextMode="FullHtml"

            ////var validXml = "<Field Name=\"SomeInternalName\" Type=\"Note\" ID=\"{7a937493-3c82-497c-938a-d7a362bd8086}\" StaticName=\"SomeInternalName\" DisplayName=\"SomeDisplayName\" Description=\"SomeDescription\" Group=\"Test\" EnforceUniqueValues=\"FALSE\" ShowInListSettings=\"TRUE\" NumLines=\"4\" RichText=\"TRUE\" RichTextMode=\"FullHtml\" />";

            ////Assert.AreEqual(validXml, fieldSchemaHelper.SchemaForField(noteFieldDefinition).ToString());
        }

        private NoteFieldInfo CreateNoteFieldInfo(
            Guid id,
            string internalName = "SomeInternalName",
            string displayNameResourceKey = "SomeDisplayName",
            string descriptionResourceKey = "SomeDescription",
            string group = "Test")
        {
            return new NoteFieldInfo(internalName, id, displayNameResourceKey, descriptionResourceKey, group);
        }
    }
}
