using System;
using System.Xml.Linq;
using GSoft.Dynamite.Fields;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSoft.Dynamite.UnitTests.Fields
{
    /// <summary>
    /// Validation of NoteFieldInfo expected behavior
    /// </summary>
    [TestClass]
    public class NoteFieldInfoTest
    {
        [TestMethod]
        public void ShouldHaveAssociationToValueTypeString()
        {
            var noteFieldDefinition = this.CreateNoteFieldInfo(Guid.NewGuid());

            Assert.AreEqual(typeof(string), noteFieldDefinition.AssociatedValueType);
        }

        [TestMethod]
        public void ShouldBeInitializedWithTypeNote()
        {
            var noteFieldDefinition = this.CreateNoteFieldInfo(Guid.NewGuid());

            Assert.AreEqual("Note", noteFieldDefinition.Type);
        }

        [TestMethod]
        public void ShouldBeInitializedWithDefaultNumLines6()
        {
            var noteFieldDefinition = this.CreateNoteFieldInfo(Guid.NewGuid());

            Assert.AreEqual(6, noteFieldDefinition.NumLines);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldHaveId()
        {
            var noteFieldDefinition = this.CreateNoteFieldInfo(Guid.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldHaveInternalName()
        {
            var noteFieldDefinition = this.CreateNoteFieldInfo(Guid.Empty, internalName: "SomeName");
        }

        [TestMethod]
        public void ShouldBeAbleToCreateFromXml()
        {
            var xElement = XElement.Parse("<Field Name=\"SomeInternalName\" Type=\"Note\" ID=\"{7a937493-3c82-497c-938a-d7a362bd8086}\" StaticName=\"SomeInternalName\" DisplayName=\"SomeDisplayName\" Description=\"SomeDescription\" Group=\"Test\" EnforceUniqueValues=\"FALSE\" ShowInListSettings=\"TRUE\" NumLines=\"6\" />");
            var noteFieldDefinition = new NoteFieldInfo(xElement);

            Assert.AreEqual("SomeInternalName", noteFieldDefinition.InternalName);
            Assert.AreEqual("Note", noteFieldDefinition.Type);
            Assert.AreEqual(new Guid("{7a937493-3c82-497c-938a-d7a362bd8086}"), noteFieldDefinition.Id);
            Assert.AreEqual("SomeDisplayName", noteFieldDefinition.DisplayName);
            Assert.AreEqual("SomeDescription", noteFieldDefinition.Description);
            Assert.AreEqual("Test", noteFieldDefinition.Group);
            Assert.AreEqual(6, noteFieldDefinition.NumLines);
        }

        [TestMethod]
        public void Schema_ShouldOutputValidFieldXml()
        {
            var noteFieldDefinition = this.CreateNoteFieldInfo(new Guid("{7a937493-3c82-497c-938a-d7a362bd8086}"));
            noteFieldDefinition.NumLines = 4;           // testing out the NumLines param
            noteFieldDefinition.HasRichText = true;     // testing out RichText=On, look out for RichTextMode="FullHtml"

            var validXml = "<Field Name=\"SomeInternalName\" Type=\"Note\" ID=\"{7a937493-3c82-497c-938a-d7a362bd8086}\" StaticName=\"SomeInternalName\" DisplayName=\"SomeDisplayName\" Description=\"SomeDescription\" Group=\"Test\" EnforceUniqueValues=\"FALSE\" ShowInListSettings=\"TRUE\" NumLines=\"4\" RichText=\"TRUE\" RichTextMode=\"FullHtml\" />";

            Assert.AreEqual(validXml, noteFieldDefinition.Schema.ToString());
        }

        [TestMethod]
        public void ToString_ShouldOutputValidFieldXml()
        {
            var noteFieldDefinition = this.CreateNoteFieldInfo(new Guid("{7a937493-3c82-497c-938a-d7a362bd8086}"));
            
            // testing out RichText=Off, look out for RichTextMode="Compatible"
            var validXml = "<Field Name=\"SomeInternalName\" Type=\"Note\" ID=\"{7a937493-3c82-497c-938a-d7a362bd8086}\" StaticName=\"SomeInternalName\" DisplayName=\"SomeDisplayName\" Description=\"SomeDescription\" Group=\"Test\" EnforceUniqueValues=\"FALSE\" ShowInListSettings=\"TRUE\" NumLines=\"6\" RichText=\"FALSE\" RichTextMode=\"Compatible\" />";

            Assert.AreEqual(validXml, noteFieldDefinition.ToString());
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
