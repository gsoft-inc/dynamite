using System;
using GSoft.Dynamite.Branding;
using GSoft.Dynamite.Serializers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSoft.Dynamite.UnitTests.Branding
{
    /// <summary>
    /// Validates the behavior of <see cref="ImageRenditionInfo"/>
    /// </summary>
    [TestClass]
    public class ImageRenditionInfoTest : BaseSerializationTest
    {
        /// <summary>
        /// Validates that *info object supports serialization, since that is part of their purpose
        /// </summary>
        [TestMethod]
        public void ImageRenditionInfo_ShouldSupportStringSerializationAndDeserialization()
        {
            var serializer = this.GetSerializer();

            var objectToSerialize = new ImageRenditionInfo("TestName", 50, 50);

            string serializedRepresentation = serializer.Serialize(objectToSerialize);

            var deserializedObject = serializer.Deserialize<ImageRenditionInfo>(serializedRepresentation);

            Assert.AreEqual(objectToSerialize.Name, deserializedObject.Name);
            Assert.AreEqual(objectToSerialize.Width, deserializedObject.Width);
            Assert.AreEqual(objectToSerialize.Height, deserializedObject.Height);
        }
    }
}
