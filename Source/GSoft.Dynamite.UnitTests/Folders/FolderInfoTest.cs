using System;
using System.Collections.Generic;
using System.Linq;
using GSoft.Dynamite.Branding;
using GSoft.Dynamite.Catalogs;
using GSoft.Dynamite.ContentTypes;
using GSoft.Dynamite.ContentTypes.Constants;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.Fields.Constants;
using GSoft.Dynamite.Fields.Types;
using GSoft.Dynamite.Folders;
using GSoft.Dynamite.Lists;
using GSoft.Dynamite.Lists.Constants;
using GSoft.Dynamite.Pages;
using GSoft.Dynamite.Search;
using GSoft.Dynamite.Serializers;
using GSoft.Dynamite.WebParts;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebPartPages;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSoft.Dynamite.UnitTests.Catalogs
{
    /// <summary>
    /// Validates the behavior of <see cref="FolderInfo"/>
    /// </summary>
    [TestClass]
    public class FolderInfoTest : BaseSerializationTest
    {
        /// <summary>
        /// Validates that *info object supports serialization, since that is part of their purpose
        /// </summary>
        [TestMethod]
        public void FolderInfo_ShouldSupportStringSerializationAndDeserialization()
        {
            var serializer = this.GetSerializer();
            var articleLeftPageLayout = new PageLayoutInfo("ArticleLeft.aspx", new SPContentTypeId("0x010100C568DB52D9D0A14D9B2FDCC96666E9F2007948130EC3DB064584E219954237AF3900242457EFB8B24247815D688C526CD44D"));
            var welcomePageLayout = new PageLayoutInfo("WelcomeSplash.aspx", new SPContentTypeId("0x010100C568DB52D9D0A14D9B2FDCC96666E9F2007948130EC3DB064584E219954237AF390064DEA0F50FC8C147B0B6EA0636C4A7D4"));

            var objectToSerialize = new FolderInfo("somepath")
            {
                Subfolders = new List<FolderInfo>()
                    {
                        new FolderInfo("somelevel2path")
                        {
                            Pages = new List<PageInfo>()
                            {
                                new PageInfo("Hello-lvl-2-page-path", articleLeftPageLayout)
                                {
                                    FieldValues = new List<FieldValueInfo>()
                                    {
                                        new FieldValueInfo(PublishingFields.PublishingPageContent, "<div><p>Hi LVL 2!!! My HTML rocks!!!</p></div>")
                                    },
                                    WebParts = new[] 
                                    {
                                        new WebPartInfo("Main", new ContentEditorWebPart(), 5),
                                        new WebPartInfo("Main", new ContentEditorWebPart(), 10),
                                    }
                                }
                            }
                        }
                    },
                Pages = new List<PageInfo>()
                    {
                        new PageInfo("Hello-root-page-path", welcomePageLayout)
                        {
                            FieldValues = new List<FieldValueInfo>()
                            {
                                new FieldValueInfo(PublishingFields.PublishingPageContent, "<div><p>My HTML rocks!!!</p></div>")
                            }
                        }
                    }
            };

            string serializedRepresentation = serializer.Serialize(objectToSerialize);

            var deserializedObject = serializer.Deserialize<FolderInfo>(serializedRepresentation);

            Assert.AreEqual(objectToSerialize.Name, deserializedObject.Name);

            Assert.AreEqual(objectToSerialize.Pages.Count, deserializedObject.Pages.Count);
            Assert.AreEqual(objectToSerialize.Pages.ElementAt(0).FileName, deserializedObject.Pages.ElementAt(0).FileName);
            Assert.AreEqual(objectToSerialize.Pages.ElementAt(0).FieldValues.Count, deserializedObject.Pages.ElementAt(0).FieldValues.Count);
            Assert.AreEqual(
                objectToSerialize.Pages.ElementAt(0).FieldValues.ElementAt(0).FieldInfo.InternalName, 
                deserializedObject.Pages.ElementAt(0).FieldValues.ElementAt(0).FieldInfo.InternalName);
            Assert.AreEqual(
                objectToSerialize.Pages.ElementAt(0).FieldValues.ElementAt(0).FieldInfo.AssociatedValueType,
                deserializedObject.Pages.ElementAt(0).FieldValues.ElementAt(0).FieldInfo.AssociatedValueType);
            Assert.AreEqual(
                objectToSerialize.Pages.ElementAt(0).FieldValues.ElementAt(0).Value,
                deserializedObject.Pages.ElementAt(0).FieldValues.ElementAt(0).Value);
            Assert.AreEqual(
                objectToSerialize.Pages.ElementAt(0).PageLayout.AssociatedContentTypeId,
                deserializedObject.Pages.ElementAt(0).PageLayout.AssociatedContentTypeId);

            Assert.AreEqual(objectToSerialize.Subfolders.Count, deserializedObject.Subfolders.Count);
            Assert.AreEqual(
                objectToSerialize.Subfolders.ElementAt(0).Pages.ElementAt(0).WebParts.Count,
                deserializedObject.Subfolders.ElementAt(0).Pages.ElementAt(0).WebParts.Count);
        }
    }
}
