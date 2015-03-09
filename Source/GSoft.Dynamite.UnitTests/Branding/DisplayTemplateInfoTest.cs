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
    }
}
