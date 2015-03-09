using System;
using System.Collections.Generic;
using System.Linq;
using GSoft.Dynamite.Branding;
using GSoft.Dynamite.Catalogs;
using GSoft.Dynamite.ContentTypes;
using GSoft.Dynamite.ContentTypes.Constants;
using GSoft.Dynamite.Fields.Types;
using GSoft.Dynamite.Lists;
using GSoft.Dynamite.Lists.Constants;
using GSoft.Dynamite.Search;
using GSoft.Dynamite.Serializers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSoft.Dynamite.UnitTests.Catalogs
{
    /// <summary>
    /// Validates the behavior of <see cref="CatalogInfo"/>
    /// </summary>
    [TestClass]
    public class CatalogInfoTest : BaseSerializationTest
    {
        /// <summary>
        /// Validates that *info object supports serialization, since that is part of their purpose
        /// </summary>
        [TestMethod]
        public void CatalogInfo_ShouldSupportStringSerializationAndDeserialization()
        {
            var serializer = this.GetSerializer();

            var taxoFieldInfo = new TaxonomyFieldInfo("TaxoField", Guid.NewGuid(), "TaxoFieldName", "TaxoFieldDescr", "GroupKey");
            var objectToSerialize = new CatalogInfo("pathto/list", "NameKey", "DescrKey")
                {
                    ContentTypes = new[] 
                    {
                        new ContentTypeInfo(ContentTypeIdBuilder.CreateChild(BuiltInContentTypes.Item, Guid.NewGuid()), "CT1Name", "CT1Descr", "GroupKey"),
                        new ContentTypeInfo(ContentTypeIdBuilder.CreateChild(BuiltInContentTypes.Item, Guid.NewGuid()), "CT2Name", "CT2Descr", "GroupKey")
                    },
                    DraftVisibilityType = Microsoft.SharePoint.DraftVisibilityType.Approver,
                    TaxonomyFieldMap = taxoFieldInfo,
                    Overwrite = true,
                    ManagedProperties = new[] 
                    {
                        new ManagedPropertyInfo("Title")
                        {
                            SortableType = Microsoft.Office.Server.Search.Administration.SortableType.Enabled
                        },
                        new ManagedPropertyInfo("ows_taxid_TaxoFieldName")
                        {
                            SortableType = Microsoft.Office.Server.Search.Administration.SortableType.Latent,
                            Refinable = true
                        }
                    },
                    ListTemplateInfo = BuiltInListTemplates.CustomList,
                    FieldDefinitions = new[] 
                    {
                        taxoFieldInfo
                    }
                };

            string serializedRepresentation = serializer.Serialize(objectToSerialize);

            var deserializedObject = serializer.Deserialize<CatalogInfo>(serializedRepresentation);

            Assert.AreEqual(objectToSerialize.DisplayNameResourceKey, deserializedObject.DisplayNameResourceKey);
            Assert.AreEqual(objectToSerialize.DescriptionResourceKey, deserializedObject.DescriptionResourceKey);
            Assert.AreEqual(objectToSerialize.GroupResourceKey, deserializedObject.GroupResourceKey);

            Assert.AreEqual(objectToSerialize.DraftVisibilityType, deserializedObject.DraftVisibilityType);
            Assert.AreEqual(objectToSerialize.TaxonomyFieldMap.Id, deserializedObject.TaxonomyFieldMap.Id);
            Assert.AreEqual(objectToSerialize.TaxonomyFieldMap.InternalName, deserializedObject.TaxonomyFieldMap.InternalName);
            Assert.AreEqual(objectToSerialize.Overwrite, deserializedObject.Overwrite);

            // Check content types
            Assert.AreEqual(2, deserializedObject.ContentTypes.Count);
            Assert.AreEqual(objectToSerialize.ContentTypes.ElementAt(0).ContentTypeId, deserializedObject.ContentTypes.ElementAt(0).ContentTypeId);
            Assert.AreEqual(objectToSerialize.ContentTypes.ElementAt(0).DisplayNameResourceKey, deserializedObject.ContentTypes.ElementAt(0).DisplayNameResourceKey);
            Assert.AreEqual(objectToSerialize.ContentTypes.ElementAt(1).ContentTypeId, deserializedObject.ContentTypes.ElementAt(1).ContentTypeId);
            Assert.AreEqual(objectToSerialize.ContentTypes.ElementAt(1).DisplayNameResourceKey, deserializedObject.ContentTypes.ElementAt(1).DisplayNameResourceKey);

            // Managed properties
            Assert.AreEqual(2, deserializedObject.ManagedProperties.Count);
            Assert.AreEqual(objectToSerialize.ManagedProperties.ElementAt(0).Name, deserializedObject.ManagedProperties.ElementAt(0).Name);
            Assert.AreEqual(objectToSerialize.ManagedProperties.ElementAt(0).SortableType, deserializedObject.ManagedProperties.ElementAt(0).SortableType);
            Assert.AreEqual(objectToSerialize.ManagedProperties.ElementAt(1).Name, deserializedObject.ManagedProperties.ElementAt(1).Name);
            Assert.AreEqual(objectToSerialize.ManagedProperties.ElementAt(1).SortableType, deserializedObject.ManagedProperties.ElementAt(1).SortableType);
            Assert.AreEqual(objectToSerialize.ManagedProperties.ElementAt(1).Refinable, deserializedObject.ManagedProperties.ElementAt(1).Refinable);

            // List template
            Assert.AreEqual(objectToSerialize.ListTemplateInfo.ListTempateTypeId, deserializedObject.ListTemplateInfo.ListTempateTypeId);
            Assert.AreEqual(objectToSerialize.ListTemplateInfo.FeatureId, deserializedObject.ListTemplateInfo.FeatureId);
            
            // Field defs
            Assert.AreEqual(1, deserializedObject.FieldDefinitions.Count);
            Assert.AreEqual(objectToSerialize.FieldDefinitions.ElementAt(0).Id, deserializedObject.FieldDefinitions.ElementAt(0).Id);
            Assert.AreEqual(objectToSerialize.FieldDefinitions.ElementAt(0).InternalName, deserializedObject.FieldDefinitions.ElementAt(0).InternalName);

            Assert.AreEqual(objectToSerialize.FieldDefinitions.ElementAt(0).AssociatedValueType, deserializedObject.FieldDefinitions.ElementAt(0).AssociatedValueType);
        }
    }
}
