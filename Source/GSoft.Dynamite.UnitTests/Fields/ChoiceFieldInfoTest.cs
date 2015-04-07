using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using GSoft.Dynamite.Collections;
using GSoft.Dynamite.Fields.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSoft.Dynamite.UnitTests.Fields
{
    /// <summary>
    /// Validates behavior of <see cref="ChoiceFieldInfoTest"/>
    /// </summary>
    [TestClass]
    public class ChoiceFieldInfoTest
    {
        /// <summary>
        /// Validates that field schema is of type 'Choice'
        /// </summary>
        [TestMethod]
        public void ChoiceFieldInfo_ShouldBeOfTypeChoice()
        {
            // Arrange
            var expectedFieldType = "Choice";

            // Act
            var fieldInfo = new ChoiceFieldInfo(
                "InternalName",
                Guid.NewGuid(),
                "DisplayNameResourceKey",
                "DescriptionNameResourceKey",
                "GroupResourceKey");
            var actualFieldType = fieldInfo.FieldType;

            // Assert
            Assert.AreEqual(expectedFieldType, actualFieldType);
        }

        /// <summary>
        /// Validates that field schema contains the 'Choices' node with 'Choice' child nodes
        /// </summary>
        [TestMethod]
        public void ChoiceFieldInfo_ShouldContainChoicesXmlInSchema()
        {
            // Arrange
            var id = Guid.NewGuid();
            var internalName = "InternalName";
            var displayName = "DisplayNameResourceKey";
            var description = "DescriptionNameResourceKey";
            var group = "GroupResourceKey";
            var expectedChoices = new[]
            {
                "My choice 1",
                "My choice 2",
            };

            // Act
            var fieldInfo = new ChoiceFieldInfo(internalName, id, displayName, description, group)
            {
                DefaultValue = "My choice 1"
            };

            fieldInfo.Choices.AddRange(expectedChoices);

            var baseFieldXml = GetBaseFieldXml(internalName, id, displayName, description, group);
            var fieldSchema = fieldInfo.Schema(baseFieldXml);
            var choicesNode = fieldSchema.Descendants("CHOICES").Single();
            var choiceValues = choicesNode.Descendants("CHOICE").Select(node => node.Value);

            // Assert
            Assert.IsNotNull(choicesNode);
            Assert.AreEqual(2, choiceValues.Count());
        }

        /// <summary>
        /// Validates that field schema is of type 'MultiChoice'
        /// </summary>
        [TestMethod]
        public void MultiChoiceFieldInfo_ShouldBeOfTypeMultiChoice()
        {
            // Arrange
            var expectedFieldType = "MultiChoice";

            // Act
            var fieldInfo = new MultiChoiceFieldInfo(
                "InternalName",
                Guid.NewGuid(),
                "DisplayNameResourceKey",
                "DescriptionNameResourceKey",
                "GroupResourceKey");
            var actualFieldType = fieldInfo.FieldType;

            // Assert
            Assert.AreEqual(expectedFieldType, actualFieldType);
        }

        /// <summary>
        /// Validates that field schema contains the 'Choices' node with 'Choice' child nodes
        /// </summary>
        [TestMethod]
        public void MultiChoiceFieldInfo_ShouldContainChoicesXmlInSchema()
        {
            // Arrange
            var id = Guid.NewGuid();
            var internalName = "InternalName";
            var displayName = "DisplayNameResourceKey";
            var description = "DescriptionNameResourceKey";
            var group = "GroupResourceKey";
            var expectedChoices = new[]
            {
                "My choice 1",
                "My choice 2",
                "My choice 3",
                "My choice 4"
            };

            // Act
            var fieldInfo = new MultiChoiceFieldInfo(internalName, id, displayName, description, group)
            {
                DefaultValue = "My choice 1"
            };

            fieldInfo.Choices.AddRange(expectedChoices);

            var baseFieldXml = GetBaseFieldXml(internalName, id, displayName, description, group);
            var fieldSchema = fieldInfo.Schema(baseFieldXml);
            var choicesNode = fieldSchema.Descendants("CHOICES").Single();
            var choiceValues = choicesNode.Descendants("CHOICE").Select(node => node.Value);

            // Assert
            Assert.IsNotNull(choicesNode);
            Assert.AreEqual(4, choiceValues.Count());
        }

        private static XElement GetBaseFieldXml(string internalName, Guid id, string displayName, string description, string group)
        {
            return XElement.Parse(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "<Field Name=\"{0}\" Type=\"MultiChoice\" ID=\"{1:B}\" StaticName=\"{0}\" DisplayName=\"{2}\" Description=\"{3}\" Group=\"{4}\" />",
                    internalName,
                    id,
                    displayName,
                    description,
                    group));
        }
    }
}
