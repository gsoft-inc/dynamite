using System;
using System.Globalization;
using System.Linq;
using Autofac;
using GSoft.Dynamite.ContentTypes;
using Microsoft.SharePoint;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSoft.Dynamite.IntegrationTests.ContentTypes
{
    /// <summary>
    /// Content type helper integration tests.
    /// </summary>
    [TestClass]
    public class ContentTypeHelperTest
    {
        #region "Ensure" should mean "Create if new or return existing"

        /// <summary>
        /// Validates that EnsureContentType adds a content type to the site collection if it did not exist previously
        /// </summary>
        [TestMethod]
        public void EnsureContentType_WhenNotAlreadyExists_ShouldAddAndReturnContentType()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var contentTypeId = string.Format(
                    CultureInfo.InvariantCulture,
                    "0x0100{0:N}",
                    new Guid("{F8B6FF55-2C9E-4FA2-A705-F55FE3D18777}"));

                var contentTypeInfo = new ContentTypeInfo(contentTypeId, "NameKey", "DescriptionKey", "GroupKey");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var contentTypeHelper = injectionScope.Resolve<IContentTypeHelper>();
                    var contentTypeCollection = testScope.SiteCollection.RootWeb.ContentTypes;
                    var expectedNumberOfContentTypes = contentTypeCollection.Count + 1;
                    var expectedContentTypeId = new SPContentTypeId(contentTypeId);
                    var expectedDisplayName = contentTypeInfo.DisplayNameResourceKey;
                    var expectedDescription = contentTypeInfo.DescriptionResourceKey;
                    var expectedGroup = contentTypeInfo.GroupResourceKey;

                    // Act
                    var actualContentType = contentTypeHelper.EnsureContentType(contentTypeCollection, contentTypeInfo);
                    var actualNumberOfContentTypes = contentTypeCollection.Count;

                    // Assert
                    Assert.AreEqual(expectedNumberOfContentTypes, actualNumberOfContentTypes);
                    Assert.IsNotNull(actualContentType);
                    Assert.AreEqual(expectedContentTypeId, actualContentType.Id);
                    Assert.AreEqual(expectedDisplayName, actualContentType.NameResource.Value);
                    Assert.AreEqual(expectedDescription, actualContentType.DescriptionResource.Value);
                    Assert.AreEqual(expectedGroup, actualContentType.Group);
                }
            }
        }

        /// <summary>
        /// Validates that EnsureContentType adds multiple content types to the site collection if they did not exist previously
        /// </summary>
        [TestMethod]
        public void EnsureContentType_WhenNotAlreadyExists_ShouldAddAndReturnMultipleContentTypes()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var contentTypeId1 = string.Format(
                    CultureInfo.InvariantCulture,
                    "0x0100{0:N}",
                    new Guid("{F8B6FF55-2C9E-4FA2-A705-F55FE3D18777}"));
                var contentTypeId2 = string.Format(
                    CultureInfo.InvariantCulture,
                    "0x0100{0:N}",
                    new Guid("{88E71756-CDA4-467A-908C-A0E231B94402}"));
                var contentTypeId3 = string.Format(
                    CultureInfo.InvariantCulture,
                    "0x0100{0:N}",
                    new Guid("{F8071D9F-3F15-4D6F-AE44-781C9CA2818E}"));

                var contentTypeInfos = new[]
                {
                    new ContentTypeInfo(contentTypeId1, "NameKey1", "DescriptionKey1", "GroupKey1"),
                    new ContentTypeInfo(contentTypeId2, "NameKey2", "DescriptionKey2", "GroupKey2"),
                    new ContentTypeInfo(contentTypeId3, "NameKey3", "DescriptionKey3", "GroupKey3")                
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var contentTypeHelper = injectionScope.Resolve<IContentTypeHelper>();
                    var contentTypeCollection = testScope.SiteCollection.RootWeb.ContentTypes;
                    var expectedNumberOfContentTypes = contentTypeCollection.Count + 3;
                    var expectedContentTypeId1 = new SPContentTypeId(contentTypeId1);
                    var expectedContentTypeId2 = new SPContentTypeId(contentTypeId2);
                    var expectedContentTypeId3 = new SPContentTypeId(contentTypeId3);
                    var expectedDisplayName1 = contentTypeInfos[0].DisplayNameResourceKey;
                    var expectedDisplayName2 = contentTypeInfos[1].DisplayNameResourceKey;
                    var expectedDisplayName3 = contentTypeInfos[2].DisplayNameResourceKey;
                    var expectedDescription1 = contentTypeInfos[0].DescriptionResourceKey;
                    var expectedDescription2 = contentTypeInfos[1].DescriptionResourceKey;
                    var expectedDescription3 = contentTypeInfos[2].DescriptionResourceKey;
                    var expectedGroup1 = contentTypeInfos[0].GroupResourceKey;
                    var expectedGroup2 = contentTypeInfos[1].GroupResourceKey;
                    var expectedGroup3 = contentTypeInfos[2].GroupResourceKey;

                    // Act
                    var actualContentTypes = contentTypeHelper.EnsureContentType(contentTypeCollection, contentTypeInfos).ToArray();
                    var actualNumberOfContentTypes = contentTypeCollection.Count;

                    // Assert
                    Assert.AreEqual(expectedNumberOfContentTypes, actualNumberOfContentTypes);
                    Assert.IsNotNull(actualContentTypes[0]);
                    Assert.IsNotNull(actualContentTypes[1]);
                    Assert.IsNotNull(actualContentTypes[2]);
                    Assert.AreEqual(expectedContentTypeId1, actualContentTypes[0].Id);
                    Assert.AreEqual(expectedContentTypeId2, actualContentTypes[1].Id);
                    Assert.AreEqual(expectedContentTypeId3, actualContentTypes[2].Id);
                    Assert.AreEqual(expectedDisplayName1, actualContentTypes[0].NameResource.Value);
                    Assert.AreEqual(expectedDisplayName2, actualContentTypes[1].NameResource.Value);
                    Assert.AreEqual(expectedDisplayName3, actualContentTypes[2].NameResource.Value);
                    Assert.AreEqual(expectedDescription1, actualContentTypes[0].DescriptionResource.Value);
                    Assert.AreEqual(expectedDescription2, actualContentTypes[1].DescriptionResource.Value);
                    Assert.AreEqual(expectedDescription3, actualContentTypes[2].DescriptionResource.Value);
                    Assert.AreEqual(expectedGroup1, actualContentTypes[0].Group);
                    Assert.AreEqual(expectedGroup2, actualContentTypes[1].Group);
                    Assert.AreEqual(expectedGroup3, actualContentTypes[2].Group);
                }
            }
        }

        /// <summary>
        /// Validates that EnsureContentType returns the same content type to the site collection if it previously existed
        /// </summary>
        [TestMethod]
        public void EnsureContentType_WhenAlreadyExists_ShouldReturnSameContentType()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var contentTypeId = string.Format(
                    CultureInfo.InvariantCulture,
                    "0x0100{0:N}",
                    new Guid("{F8B6FF55-2C9E-4FA2-A705-F55FE3D18777}"));

                var contentTypeInfo = new ContentTypeInfo(contentTypeId, "NameKey", "DescriptionKey", "GroupKey");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var contentTypeHelper = injectionScope.Resolve<IContentTypeHelper>();
                    var contentTypeCollection = testScope.SiteCollection.RootWeb.ContentTypes;
                    var expectedNumberOfContentTypes = contentTypeCollection.Count + 1;
                    var expectedContentTypeId = new SPContentTypeId(contentTypeId);
                    var expectedDisplayName = contentTypeInfo.DisplayNameResourceKey;
                    var expectedDescription = contentTypeInfo.DescriptionResourceKey;
                    var expectedGroup = contentTypeInfo.GroupResourceKey;

                    // Act
                    contentTypeHelper.EnsureContentType(contentTypeCollection, contentTypeInfo);
                    var actualContentType = contentTypeHelper.EnsureContentType(contentTypeCollection, contentTypeInfo);
                    var actualNumberOfContentTypes = contentTypeCollection.Count;

                    // Assert
                    Assert.AreEqual(expectedNumberOfContentTypes, actualNumberOfContentTypes);
                    Assert.IsNotNull(actualContentType);
                    Assert.AreEqual(expectedContentTypeId, actualContentType.Id);
                    Assert.AreEqual(expectedDisplayName, actualContentType.NameResource.Value);
                    Assert.AreEqual(expectedDescription, actualContentType.DescriptionResource.Value);
                    Assert.AreEqual(expectedGroup, actualContentType.Group);
                }
            }
        }

        /// <summary>
        /// Validates that EnsureContentType adds multiple content types to the site collection if they did not exist previously 
        /// and returns them if they did exist.
        /// </summary>
        [TestMethod]
        public void EnsureContentType_WhenSomeAlreadyExist_ShouldReturnExistingAndAddNewContentTypes()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var contentTypeId1 = string.Format(
                    CultureInfo.InvariantCulture,
                    "0x0100{0:N}",
                    new Guid("{F8B6FF55-2C9E-4FA2-A705-F55FE3D18777}"));
                var contentTypeId2 = string.Format(
                    CultureInfo.InvariantCulture,
                    "0x0100{0:N}",
                    new Guid("{88E71756-CDA4-467A-908C-A0E231B94402}"));
                var contentTypeId3 = string.Format(
                    CultureInfo.InvariantCulture,
                    "0x0100{0:N}",
                    new Guid("{F8071D9F-3F15-4D6F-AE44-781C9CA2818E}"));

                var existingContentTypeInfos = new[]
                {
                    new ContentTypeInfo(contentTypeId1, "ExistingNameKey1", "ExistingDescriptionKey1", "ExistingGroupKey1"),
                    new ContentTypeInfo(contentTypeId2, "ExistingNameKey2", "ExistingDescriptionKey2", "ExistingGroupKey2")
                };

                var newContentTypeInfos = new[]
                {
                    new ContentTypeInfo(contentTypeId1, "ExistingNameKey1", "ExistingDescriptionKey1", "ExistingGroupKey1"),
                    new ContentTypeInfo(contentTypeId2, "ExistingNameKey2", "ExistingDescriptionKey2", "ExistingGroupKey2"),
                    new ContentTypeInfo(contentTypeId3, "NameKey3", "DescriptionKey3", "GroupKey3")                
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var contentTypeHelper = injectionScope.Resolve<IContentTypeHelper>();
                    var contentTypeCollection = testScope.SiteCollection.RootWeb.ContentTypes;
                    var expectedNumberOfContentTypes = contentTypeCollection.Count + 3;
                    var expectedContentTypeId1 = new SPContentTypeId(contentTypeId1);
                    var expectedContentTypeId2 = new SPContentTypeId(contentTypeId2);
                    var expectedContentTypeId3 = new SPContentTypeId(contentTypeId3);
                    var expectedDisplayName1 = newContentTypeInfos[0].DisplayNameResourceKey;
                    var expectedDisplayName2 = newContentTypeInfos[1].DisplayNameResourceKey;
                    var expectedDisplayName3 = newContentTypeInfos[2].DisplayNameResourceKey;
                    var expectedDescription1 = newContentTypeInfos[0].DescriptionResourceKey;
                    var expectedDescription2 = newContentTypeInfos[1].DescriptionResourceKey;
                    var expectedDescription3 = newContentTypeInfos[2].DescriptionResourceKey;
                    var expectedGroup1 = newContentTypeInfos[0].GroupResourceKey;
                    var expectedGroup2 = newContentTypeInfos[1].GroupResourceKey;
                    var expectedGroup3 = newContentTypeInfos[2].GroupResourceKey;

                    // Act
                    contentTypeHelper.EnsureContentType(contentTypeCollection, existingContentTypeInfos);
                    var actualContentTypes = contentTypeHelper.EnsureContentType(contentTypeCollection, newContentTypeInfos).ToArray();
                    var actualNumberOfContentTypes = contentTypeCollection.Count;

                    // Assert
                    Assert.AreEqual(expectedNumberOfContentTypes, actualNumberOfContentTypes);
                    Assert.IsNotNull(actualContentTypes[0]);
                    Assert.IsNotNull(actualContentTypes[1]);
                    Assert.IsNotNull(actualContentTypes[2]);
                    Assert.AreEqual(expectedContentTypeId1, actualContentTypes[0].Id);
                    Assert.AreEqual(expectedContentTypeId2, actualContentTypes[1].Id);
                    Assert.AreEqual(expectedContentTypeId3, actualContentTypes[2].Id);
                    Assert.AreEqual(expectedDisplayName1, actualContentTypes[0].NameResource.Value);
                    Assert.AreEqual(expectedDisplayName2, actualContentTypes[1].NameResource.Value);
                    Assert.AreEqual(expectedDisplayName3, actualContentTypes[2].NameResource.Value);
                    Assert.AreEqual(expectedDescription1, actualContentTypes[0].DescriptionResource.Value);
                    Assert.AreEqual(expectedDescription2, actualContentTypes[1].DescriptionResource.Value);
                    Assert.AreEqual(expectedDescription3, actualContentTypes[2].DescriptionResource.Value);
                    Assert.AreEqual(expectedGroup1, actualContentTypes[0].Group);
                    Assert.AreEqual(expectedGroup2, actualContentTypes[1].Group);
                    Assert.AreEqual(expectedGroup3, actualContentTypes[2].Group);
                }
            }
        }

        #endregion

        #region Update existing content type(s)

        /// <summary>
        /// Validates that EnsureContentType returns the existing content type to the site collection with updated resources, if a content type with the same ID previously existed
        /// </summary>
        [TestMethod]
        public void EnsureContentType_WhenOtherContentTypeWithSameIdAlreadyExists_ShouldUpdateResourcesAndReturnExistingMatch()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var contentTypeId = string.Format(
                    CultureInfo.InvariantCulture,
                    "0x0100{0:N}",
                    new Guid("{F8B6FF55-2C9E-4FA2-A705-F55FE3D18777}"));

                var existingContentTypeInfo = new ContentTypeInfo(contentTypeId, "ExistingNameKey", "ExistingDescriptionKey", "ExistingGroupKey");
                var newContentTypeInfo = new ContentTypeInfo(contentTypeId, "NameKey", "DescriptionKey", "GroupKey");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var contentTypeHelper = injectionScope.Resolve<IContentTypeHelper>();
                    var contentTypeCollection = testScope.SiteCollection.RootWeb.ContentTypes;
                    var expectedNumberOfContentTypes = contentTypeCollection.Count + 1;
                    var expectedContentTypeId = new SPContentTypeId(contentTypeId);

                    // Expect to return the new content type name, description and group
                    var expectedDisplayName = newContentTypeInfo.DisplayNameResourceKey;
                    var expectedDescription = newContentTypeInfo.DescriptionResourceKey;
                    var expectedGroup = newContentTypeInfo.GroupResourceKey;

                    // Act
                    contentTypeHelper.EnsureContentType(contentTypeCollection, existingContentTypeInfo);
                    var actualContentType = contentTypeHelper.EnsureContentType(contentTypeCollection, newContentTypeInfo);
                    var actualNumberOfContentTypes = contentTypeCollection.Count;

                    // Assert
                    Assert.AreEqual(expectedNumberOfContentTypes, actualNumberOfContentTypes);
                    Assert.IsNotNull(actualContentType);
                    Assert.AreEqual(expectedContentTypeId, actualContentType.Id);
                    Assert.AreEqual(expectedDisplayName, actualContentType.NameResource.Value);
                    Assert.AreEqual(expectedDescription, actualContentType.DescriptionResource.Value);
                    Assert.AreEqual(expectedGroup, actualContentType.Group);
                }
            }
        }

        /// <summary>
        /// Validates that EnsureContentType returns the existing content types to the site collection with updated resources, if content types with the same ID previously existed
        /// </summary>
        [TestMethod]
        public void EnsureContentType_WhenOtherContentTypesWithSameIdsAlreadyExists_ShouldUpdateResourcesAndReturnExistingMatches()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var contentTypeId1 = string.Format(
                    CultureInfo.InvariantCulture,
                    "0x0100{0:N}",
                    new Guid("{F8B6FF55-2C9E-4FA2-A705-F55FE3D18777}"));
                var contentTypeId2 = string.Format(
                    CultureInfo.InvariantCulture,
                    "0x0100{0:N}",
                    new Guid("{88E71756-CDA4-467A-908C-A0E231B94402}"));
                var contentTypeId3 = string.Format(
                    CultureInfo.InvariantCulture,
                    "0x0100{0:N}",
                    new Guid("{F8071D9F-3F15-4D6F-AE44-781C9CA2818E}"));

                var existingContentTypeInfos = new[]
                {
                    new ContentTypeInfo(contentTypeId1, "ExistingNameKey1", "ExistingDescriptionKey1", "ExistingGroupKey1"),
                    new ContentTypeInfo(contentTypeId2, "ExistingNameKey2", "ExistingDescriptionKey2", "ExistingGroupKey2"),
                    new ContentTypeInfo(contentTypeId3, "ExistingNameKey3", "ExistingDescriptionKey3", "ExistingGroupKey3")                
                };

                var newContentTypeInfos = new[]
                {
                    new ContentTypeInfo(contentTypeId1, "NewNameKey1", "NewDescriptionKey1", "NewGroupKey1"),
                    new ContentTypeInfo(contentTypeId2, "NewNameKey2", "NewDescriptionKey2", "NewGroupKey2"),
                    new ContentTypeInfo(contentTypeId3, "NewNameKey3", "NewDescriptionKey3", "NewGroupKey3")                  
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var contentTypeHelper = injectionScope.Resolve<IContentTypeHelper>();
                    var contentTypeCollection = testScope.SiteCollection.RootWeb.ContentTypes;
                    var expectedNumberOfContentTypes = contentTypeCollection.Count + 3;
                    var expectedContentTypeId1 = new SPContentTypeId(contentTypeId1);
                    var expectedContentTypeId2 = new SPContentTypeId(contentTypeId2);
                    var expectedContentTypeId3 = new SPContentTypeId(contentTypeId3);
                    var expectedDisplayName1 = newContentTypeInfos[0].DisplayNameResourceKey;
                    var expectedDisplayName2 = newContentTypeInfos[1].DisplayNameResourceKey;
                    var expectedDisplayName3 = newContentTypeInfos[2].DisplayNameResourceKey;
                    var expectedDescription1 = newContentTypeInfos[0].DescriptionResourceKey;
                    var expectedDescription2 = newContentTypeInfos[1].DescriptionResourceKey;
                    var expectedDescription3 = newContentTypeInfos[2].DescriptionResourceKey;
                    var expectedGroup1 = newContentTypeInfos[0].GroupResourceKey;
                    var expectedGroup2 = newContentTypeInfos[1].GroupResourceKey;
                    var expectedGroup3 = newContentTypeInfos[2].GroupResourceKey;

                    // Act
                    contentTypeHelper.EnsureContentType(contentTypeCollection, existingContentTypeInfos);
                    var actualContentTypes = contentTypeHelper.EnsureContentType(contentTypeCollection, newContentTypeInfos).ToArray();
                    var actualNumberOfContentTypes = contentTypeCollection.Count;

                    // Assert
                    Assert.AreEqual(expectedNumberOfContentTypes, actualNumberOfContentTypes);
                    Assert.IsNotNull(actualContentTypes[0]);
                    Assert.IsNotNull(actualContentTypes[1]);
                    Assert.IsNotNull(actualContentTypes[2]);
                    Assert.AreEqual(expectedContentTypeId1, actualContentTypes[0].Id);
                    Assert.AreEqual(expectedContentTypeId2, actualContentTypes[1].Id);
                    Assert.AreEqual(expectedContentTypeId3, actualContentTypes[2].Id);
                    Assert.AreEqual(expectedDisplayName1, actualContentTypes[0].NameResource.Value);
                    Assert.AreEqual(expectedDisplayName2, actualContentTypes[1].NameResource.Value);
                    Assert.AreEqual(expectedDisplayName3, actualContentTypes[2].NameResource.Value);
                    Assert.AreEqual(expectedDescription1, actualContentTypes[0].DescriptionResource.Value);
                    Assert.AreEqual(expectedDescription2, actualContentTypes[1].DescriptionResource.Value);
                    Assert.AreEqual(expectedDescription3, actualContentTypes[2].DescriptionResource.Value);
                    Assert.AreEqual(expectedGroup1, actualContentTypes[0].Group);
                    Assert.AreEqual(expectedGroup2, actualContentTypes[1].Group);
                    Assert.AreEqual(expectedGroup3, actualContentTypes[2].Group);
                }
            }
        }
        #endregion
    }
}
