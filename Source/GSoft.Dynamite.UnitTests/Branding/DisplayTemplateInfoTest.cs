using System;
using GSoft.Dynamite.Branding;
using GSoft.Dynamite.Serializers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSoft.Dynamite.UnitTests.Branding
{
    /// <summary>
    /// Validates the behavior of <see cref="DisplayTemplateInfo"/>
    /// </summary>
    [TestClass]
    public class DisplayTemplateInfoTest : BaseSerializationTest
    {
        /// <summary>
        /// Validates that *info object supports serialization, since that is part of their purpose
        /// </summary>
        [TestMethod]
        public void DisplayTemplateInfo_ShouldSupportStringSerializationAndDeserialization()
        {
            var serializer = this.GetSerializer();

            var objectToSerialize = new DisplayTemplateInfo("TestName", DisplayTemplateCategory.Search);

            string serializedRepresentation = serializer.Serialize(objectToSerialize);

            var deserializedObject = serializer.Deserialize<DisplayTemplateInfo>(serializedRepresentation);

            Assert.AreEqual(objectToSerialize.Name, deserializedObject.Name);
            Assert.AreEqual(objectToSerialize.Category, deserializedObject.Category);
        }

        /// <summary>
        /// Validates that the display template info correctly generates the tokenized path
        /// </summary>
        [TestMethod]
        public void DisplayTemplateInfo_ShouldGenerateTokenizedPath()
        {
            // Arrange
            var displayTemplateInfo = new DisplayTemplateInfo("TestName", DisplayTemplateCategory.ContentSearch);
            var expectedTokenizedPath = "~sitecollection/_catalogs/masterpage/Display Templates/Content Web Parts/TestName.js";

            // Act
            var actualTokenizedPath = displayTemplateInfo.ItemTemplateTokenizedPath;

            // Assert
            Assert.AreEqual(expectedTokenizedPath, actualTokenizedPath);
        }
    }
}
