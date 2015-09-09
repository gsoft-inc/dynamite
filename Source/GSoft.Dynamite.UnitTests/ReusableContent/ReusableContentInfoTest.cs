using System;
using GSoft.Dynamite.ReusableContent;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSoft.Dynamite.UnitTests.ReusableContent
{
    /// <summary>
    /// Validates the behavior of <see cref="ReusableContentInfo"/>
    /// </summary>
    [TestClass]
    public class ReusableContentInfoTest : BaseSerializationTest
    {
        /// <summary>
        /// Validates that *info object supports serialization, since that is part of their purpose
        /// </summary>
        [TestMethod]
        public void ReusableContentInfo_ShouldSupportStringSerializationAndDeserialization()
        {
            // Arrange
            var serializer = this.GetSerializer();
            var objectToSerialize = new ReusableContentInfo("Reusable Content Name", "Dynamite", true, false, "filename.html", "GSoft.Dynamite/html");
            objectToSerialize.Content = "<h1>Hello World!</h1>";

            // Act
            string serializedRepresentation = serializer.Serialize(objectToSerialize);

            var deserializedObject = serializer.Deserialize<ReusableContentInfo>(serializedRepresentation);

            // Assert
            Assert.AreEqual(objectToSerialize.Title, deserializedObject.Title);
            Assert.IsTrue(deserializedObject.IsAutomaticUpdate);
            Assert.IsFalse(deserializedObject.IsShowInRibbon);
            Assert.AreEqual(objectToSerialize.HTMLFilePath, deserializedObject.HTMLFilePath);
            Assert.AreEqual(objectToSerialize.Content, deserializedObject.Content);
        }
    }
}
