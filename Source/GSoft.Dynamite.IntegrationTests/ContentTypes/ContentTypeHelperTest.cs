using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Autofac;
using GSoft.Dynamite.Binding;
using GSoft.Dynamite.ContentTypes;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.Fields.Constants;
using GSoft.Dynamite.Fields.Types;
using GSoft.Dynamite.Lists;
using GSoft.Dynamite.ValueTypes;
using GSoft.Dynamite.ValueTypes.Writers;
using Microsoft.SharePoint;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSoft.Dynamite.IntegrationTests.ContentTypes
{
    /// <summary>
    /// Validates the entire stack of behavior behind <see cref="ContentTypeHelper"/>.
    /// The GSoft.Dynamite.wsp package (GSoft.Dynamite.SP project) needs to be 
    /// deployed to the current server environment before running these tests.
    /// Redeploy the WSP package every time GSoft.Dynamite.dll changes.
    /// </summary>
    [TestClass]
    public class ContentTypeHelperTest
    {
        #region "Ensure" should mean "Create if new or return existing"

        /// <summary>
        /// Validates that EnsureContentType adds a content type to the site collection if it did not exist previously
        /// </summary>
        [TestMethod]
        [TestCategory(IntegrationTestCategories.Sanity)]
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

                    var contentTypeRefetched = testScope.SiteCollection.RootWeb.ContentTypes["NameKey"];
                    Assert.IsNotNull(contentTypeRefetched);
                    Assert.AreEqual(expectedContentTypeId, contentTypeRefetched.Id);
                    Assert.AreEqual(expectedDisplayName, contentTypeRefetched.NameResource.Value);
                    Assert.AreEqual(expectedDescription, contentTypeRefetched.DescriptionResource.Value);
                    Assert.AreEqual(expectedGroup, contentTypeRefetched.Group);
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

                    var contentTypeRefetched = testScope.SiteCollection.RootWeb.ContentTypes["NameKey"];
                    Assert.IsNotNull(contentTypeRefetched);
                    Assert.AreEqual(expectedContentTypeId, contentTypeRefetched.Id);
                    Assert.AreEqual(expectedDisplayName, contentTypeRefetched.NameResource.Value);
                    Assert.AreEqual(expectedDescription, contentTypeRefetched.DescriptionResource.Value);
                    Assert.AreEqual(expectedGroup, contentTypeRefetched.Group);
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

        #region "Ensure" should also mean "Update existing content type(s) if definition in ContentTypeInfo changed since first provisioning"

        /// <summary>
        /// Validates that EnsureContentType returns the existing content type to the site collection with updated resources, if a content type with the same ID previously existed
        /// </summary>
        [TestMethod]
        [TestCategory(IntegrationTestCategories.Sanity)]
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

        #region CT's fields should be provisioned as site columns before being associated to content type

        /// <summary>
        /// Validates that EnsureContentType provisions the missing fields as site column (root web CT creation scenario)
        /// </summary>
        [TestMethod]
        public void EnsureContentType_WhenCreatingRootWebCT_AndFieldsNotAlreadyExists_ShouldProvisionFieldsAsSiteColumn()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var fieldId = new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}");
                TextFieldInfo textFieldInfo = new TextFieldInfo(
                    "TestInternalName",
                    fieldId,
                    "Test_FieldTitle",
                    "Test_FieldDescription",
                    "Test_ContentGroup")
                {
                    MaxLength = 50,
                    Required = RequiredType.Required
                };

                var contentTypeId = string.Format(
                    CultureInfo.InvariantCulture,
                    "0x0100{0:N}",
                    new Guid("{F8B6FF55-2C9E-4FA2-A705-F55FE3D18777}"));

                var contentTypeInfo = new ContentTypeInfo(contentTypeId, "NameKey", "DescriptionKey", "GroupKey")
                {
                    Fields = new List<IFieldInfo>()
                    {
                        textFieldInfo
                    }
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var contentTypeHelper = injectionScope.Resolve<IContentTypeHelper>();

                    // Act
                    var actualContentType = contentTypeHelper.EnsureContentType(testScope.SiteCollection.RootWeb.ContentTypes, contentTypeInfo);

                    // Assert
                    var contentTypeRefetched = testScope.SiteCollection.RootWeb.ContentTypes["NameKey"];

                    // Field should be on ensured CT
                    Assert.IsNotNull(actualContentType.Fields[fieldId]);
                    Assert.IsNotNull(contentTypeRefetched.Fields[fieldId]);

                    // Field should be a site column now also
                    Assert.IsNotNull(testScope.SiteCollection.RootWeb.Fields[fieldId]);
                }
            }
        }

        /// <summary>
        /// Validates that EnsureContentType provisions the missing fields as site column (root web CT update scenario)
        /// </summary>
        [TestMethod]
        public void EnsureContentType_WhenUpdatingRootWebCT_AndFieldsNotAlreadyExists_ShouldProvisionFieldsAsSiteColumn()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var fieldId = new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}");
                TextFieldInfo textFieldInfo = new TextFieldInfo(
                    "TestInternalName",
                    fieldId,
                    "Test_FieldTitle",
                    "Test_FieldDescription",
                    "Test_ContentGroup")
                {
                    MaxLength = 50,
                    Required = RequiredType.Required
                };

                var contentTypeId = string.Format(
                    CultureInfo.InvariantCulture,
                    "0x0100{0:N}",
                    new Guid("{F8B6FF55-2C9E-4FA2-A705-F55FE3D18777}"));

                var contentTypeInfo = new ContentTypeInfo(contentTypeId, "NameKey", "DescriptionKey", "GroupKey");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var contentTypeHelper = injectionScope.Resolve<IContentTypeHelper>();

                    // Provision the CT a first time, without the field
                    var actualContentType = contentTypeHelper.EnsureContentType(testScope.SiteCollection.RootWeb.ContentTypes, contentTypeInfo);

                    // Change the CTInfo to add field
                    contentTypeInfo.Fields = new List<IFieldInfo>() { textFieldInfo };

                    // Act
                    var ensuredContentType = contentTypeHelper.EnsureContentType(testScope.SiteCollection.RootWeb.ContentTypes, contentTypeInfo);

                    // Assert
                    var contentTypeRefetched = testScope.SiteCollection.RootWeb.ContentTypes["NameKey"];

                    // Field should be on ensured CT
                    Assert.IsNotNull(ensuredContentType.Fields[fieldId]);
                    Assert.IsNotNull(contentTypeRefetched.Fields[fieldId]);

                    // Field should be a site column now also
                    Assert.IsNotNull(testScope.SiteCollection.RootWeb.Fields[fieldId]);
                }
            }
        }

        #endregion

        #region Attaching a CT directly on a list should provision CT on RootWeb beforehand, by convention

        /// <summary>
        /// Validates that EnsureContentType provisions the missing CT on root web and fields as site column (root web list CT creation scenario)
        /// </summary>
        [TestMethod]
        public void EnsureContentType_WhenCreatingRootWebListCT_ShouldProvisionContentTypeOnRootWebAndFieldsAsSiteColumn()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var fieldId = new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}");
                TextFieldInfo textFieldInfo = new TextFieldInfo(
                    "TestInternalName",
                    fieldId,
                    "Test_FieldTitle",
                    "Test_FieldDescription",
                    "Test_ContentGroup")
                {
                    MaxLength = 50,
                    Required = RequiredType.Required
                };

                var contentTypeId = string.Format(
                    CultureInfo.InvariantCulture,
                    "0x0100{0:N}",
                    new Guid("{F8B6FF55-2C9E-4FA2-A705-F55FE3D18777}"));

                var contentTypeInfo = new ContentTypeInfo(contentTypeId, "NameKey", "DescriptionKey", "GroupKey")
                {
                    Fields = new List<IFieldInfo>()
                    {
                        textFieldInfo
                    }
                };

                ListInfo listInfo = new ListInfo("sometestlistpath", "DynamiteTestListNameKey", "DynamiteTestListDescriptionKey");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var rootWeb = testScope.SiteCollection.RootWeb;
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var contentTypeHelper = injectionScope.Resolve<IContentTypeHelper>();

                    // Start by provisioning a list without CT
                    var ensuredList = listHelper.EnsureList(rootWeb, listInfo);

                    // Act
                    var ensuredListContentType = contentTypeHelper.EnsureContentType(ensuredList.ContentTypes, contentTypeInfo);

                    // Assert
                    var contentTypeWebRefetched = testScope.SiteCollection.RootWeb.ContentTypes["NameKey"];
                    var contentTypeListRefetched = testScope.SiteCollection.RootWeb.Lists[ensuredList.ID].ContentTypes["NameKey"];

                    // CT should be on RootWeb
                    Assert.IsNotNull(contentTypeWebRefetched);

                    // CT should be on List
                    Assert.IsNotNull(ensuredList.ContentTypes["NameKey"]);
                    Assert.IsNotNull(contentTypeWebRefetched);

                    // Field should be on ensured CTs (web + list)
                    Assert.IsNotNull(ensuredListContentType.Fields[fieldId]);
                    Assert.IsNotNull(contentTypeListRefetched.Fields[fieldId]);
                    Assert.IsNotNull(contentTypeWebRefetched.Fields[fieldId]);

                    // Field should be a site column now also
                    Assert.IsNotNull(testScope.SiteCollection.RootWeb.Fields[fieldId]);
                }
            }
        }

        /// <summary>
        /// Validates that EnsureContentType provisions the missing CT on root web and fields as site column (sub-web list CT creation scenario)
        /// </summary>
        [TestMethod]
        public void EnsureContentType_WhenCreatingSubWebListCT_ShouldProvisionContentTypeOnRootWebAndFieldsAsSiteColumn()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var fieldId = new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}");
                TextFieldInfo textFieldInfo = new TextFieldInfo(
                    "TestInternalName",
                    fieldId,
                    "Test_FieldTitle",
                    "Test_FieldDescription",
                    "Test_ContentGroup")
                {
                    MaxLength = 50,
                    Required = RequiredType.Required
                };

                var contentTypeId = string.Format(
                    CultureInfo.InvariantCulture,
                    "0x0100{0:N}",
                    new Guid("{F8B6FF55-2C9E-4FA2-A705-F55FE3D18777}"));

                var contentTypeInfo = new ContentTypeInfo(contentTypeId, "NameKey", "DescriptionKey", "GroupKey")
                {
                    Fields = new List<IFieldInfo>()
                    {
                        textFieldInfo
                    }
                };

                ListInfo listInfo = new ListInfo("sometestlistpath", "DynamiteTestListNameKey", "DynamiteTestListDescriptionKey");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var subWeb = testScope.SiteCollection.RootWeb.Webs.Add("subweb");

                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var contentTypeHelper = injectionScope.Resolve<IContentTypeHelper>();

                    // Start by provisioning a list without CT
                    var ensuredList = listHelper.EnsureList(subWeb, listInfo);

                    // Act
                    var ensuredListContentType = contentTypeHelper.EnsureContentType(ensuredList.ContentTypes, contentTypeInfo);

                    // Assert
                    var contentTypeWebRefetched = testScope.SiteCollection.RootWeb.ContentTypes["NameKey"];
                    var contentTypeListRefetched = testScope.SiteCollection.RootWeb.Webs["subweb"].Lists[ensuredList.ID].ContentTypes["NameKey"];

                    // CT should be on RootWeb
                    Assert.IsNotNull(contentTypeWebRefetched);

                    // CT should be on List
                    Assert.IsNotNull(ensuredList.ContentTypes["NameKey"]);
                    Assert.IsNotNull(contentTypeWebRefetched);

                    // Field should be on ensured CTs (web + list)
                    Assert.IsNotNull(ensuredListContentType.Fields[fieldId]);
                    Assert.IsNotNull(contentTypeListRefetched.Fields[fieldId]);
                    Assert.IsNotNull(contentTypeWebRefetched.Fields[fieldId]);

                    // Field should be a site column now also
                    Assert.IsNotNull(testScope.SiteCollection.RootWeb.Fields[fieldId]);
                }
            }
        }

        #endregion

        #region Attaching a CT on a list when that CT already exists on RootWeb means a child CT should be attached to list

        /// <summary>
        /// Validates that EnsureContentType re-uses the root web CT (root web list CT attach scenario)
        /// </summary>
        [TestMethod]
        public void EnsureContentType_WhenOnRootWebList_AndRootWebCTAlreadyExists_ShouldProvisionChildContentTypeOnList()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var fieldId = new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}");
                TextFieldInfo textFieldInfo = new TextFieldInfo(
                    "TestInternalName",
                    fieldId,
                    "Test_FieldTitle",
                    "Test_FieldDescription",
                    "Test_ContentGroup")
                {
                    MaxLength = 50,
                    Required = RequiredType.Required
                };

                var contentTypeId = string.Format(
                    CultureInfo.InvariantCulture,
                    "0x0100{0:N}",
                    new Guid("{F8B6FF55-2C9E-4FA2-A705-F55FE3D18777}"));

                var contentTypeInfo = new ContentTypeInfo(contentTypeId, "NameKey", "DescriptionKey", "GroupKey")
                {
                    Fields = new List<IFieldInfo>()
                    {
                        textFieldInfo
                    }
                };

                ListInfo listInfo = new ListInfo("sometestlistpath", "DynamiteTestListNameKey", "DynamiteTestListDescriptionKey");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var rootWeb = testScope.SiteCollection.RootWeb;
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var contentTypeHelper = injectionScope.Resolve<IContentTypeHelper>();

                    // Start by provisioning a list without CT
                    var ensuredList = listHelper.EnsureList(rootWeb, listInfo);

                    // Also provision the existing CT on the root web
                    var ensuredRootWebCT = contentTypeHelper.EnsureContentType(rootWeb.ContentTypes, contentTypeInfo);

                    // Act
                    var ensuredListContentType = contentTypeHelper.EnsureContentType(ensuredList.ContentTypes, contentTypeInfo);

                    // Assert
                    Assert.IsTrue(ensuredListContentType.Id.IsChildOf(ensuredRootWebCT.Id));
                }
            }
        }

        /// <summary>
        /// Validates that EnsureContentType re-uses the root web CT (sub-web list CT attach scenario)
        /// </summary>
        [TestMethod]
        public void EnsureContentType_WhenOnSubWebList_AndRootWebCTAlreadyExists_ShouldProvisionChildContentTypeOnList()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var fieldId = new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}");
                TextFieldInfo textFieldInfo = new TextFieldInfo(
                    "TestInternalName",
                    fieldId,
                    "Test_FieldTitle",
                    "Test_FieldDescription",
                    "Test_ContentGroup")
                {
                    MaxLength = 50,
                    Required = RequiredType.Required
                };

                var contentTypeId = string.Format(
                    CultureInfo.InvariantCulture,
                    "0x0100{0:N}",
                    new Guid("{F8B6FF55-2C9E-4FA2-A705-F55FE3D18777}"));

                var contentTypeInfo = new ContentTypeInfo(contentTypeId, "NameKey", "DescriptionKey", "GroupKey")
                {
                    Fields = new List<IFieldInfo>()
                    {
                        textFieldInfo
                    }
                };

                ListInfo listInfo = new ListInfo("sometestlistpath", "DynamiteTestListNameKey", "DynamiteTestListDescriptionKey");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var rootWeb = testScope.SiteCollection.RootWeb;
                    var subWeb = rootWeb.Webs.Add("subweb");

                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var contentTypeHelper = injectionScope.Resolve<IContentTypeHelper>();

                    // Start by provisioning a list without CT
                    var ensuredSubWebList = listHelper.EnsureList(subWeb, listInfo);

                    // Also provision the existing CT on the root web
                    var ensuredRootWebCT = contentTypeHelper.EnsureContentType(rootWeb.ContentTypes, contentTypeInfo);

                    // Act
                    var ensuredListContentType = contentTypeHelper.EnsureContentType(ensuredSubWebList.ContentTypes, contentTypeInfo);

                    // Assert
                    Assert.IsTrue(ensuredListContentType.Id.IsChildOf(ensuredRootWebCT.Id));
                }
            }
        }

        #endregion

        #region Attempting to ensure a CT on a sub-web's CT collection should simply ensure that CT on the root web

        /// <summary>
        /// Validates that EnsureContentType re-uses (and updates) the root web CT (sub-web CT attach scenario)
        /// </summary>
        [TestMethod]
        public void EnsureContentType_WhenOnSubWeb_AndRootWebCTAlreadyExists_ShouldUpdateAndReturnExistingRootWebCT()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var fieldId = new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}");
                TextFieldInfo textFieldInfo = new TextFieldInfo(
                    "TestInternalName",
                    fieldId,
                    "Test_FieldTitle",
                    "Test_FieldDescription",
                    "Test_ContentGroup")
                {
                    MaxLength = 50,
                    Required = RequiredType.Required
                };

                var contentTypeId = string.Format(
                    CultureInfo.InvariantCulture,
                    "0x0100{0:N}",
                    new Guid("{F8B6FF55-2C9E-4FA2-A705-F55FE3D18777}"));

                var contentTypeInfo = new ContentTypeInfo(contentTypeId, "NameKey", "DescriptionKey", "GroupKey");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var rootWeb = testScope.SiteCollection.RootWeb;
                    var subWeb = rootWeb.Webs.Add("subweb");

                    var contentTypeHelper = injectionScope.Resolve<IContentTypeHelper>();

                    // Also provision the existing CT on the root web
                    var ensuredRootWebCT = contentTypeHelper.EnsureContentType(rootWeb.ContentTypes, contentTypeInfo);

                    // Change CT definition a little bit
                    contentTypeInfo.DescriptionResourceKey = "DescriptionKeyAlt";
                    contentTypeInfo.Fields = new List<IFieldInfo>()
                    {
                        textFieldInfo
                    };

                    // Act
                    var ensuredSubWebContentType = contentTypeHelper.EnsureContentType(subWeb.ContentTypes, contentTypeInfo);

                    // Assert
                    Assert.AreEqual(ensuredRootWebCT.Id, ensuredSubWebContentType.Id);

                    var refetchedRootWebCT = rootWeb.ContentTypes["NameKey"];
                    Assert.IsNotNull(refetchedRootWebCT.Fields[textFieldInfo.Id]);
                    Assert.AreEqual("DescriptionKeyAlt", refetchedRootWebCT.Description);
                }
            }
        }

        /// <summary>
        /// Validates that EnsureContentType on a sub-web ensures the the root web CT instead (sub-web CT creation scenario)
        /// </summary>
        [TestMethod]
        public void EnsureContentType_WhenOnSubWeb_AndRootWebCTDoesntAlreadyExists_ShouldProvisionRootWebCT()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var fieldId = new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}");
                TextFieldInfo textFieldInfo = new TextFieldInfo(
                    "TestInternalName",
                    fieldId,
                    "Test_FieldTitle",
                    "Test_FieldDescription",
                    "Test_ContentGroup")
                {
                    MaxLength = 50,
                    Required = RequiredType.Required
                };

                var contentTypeId = string.Format(
                    CultureInfo.InvariantCulture,
                    "0x0100{0:N}",
                    new Guid("{F8B6FF55-2C9E-4FA2-A705-F55FE3D18777}"));

                var contentTypeInfo = new ContentTypeInfo(contentTypeId, "NameKey", "DescriptionKey", "GroupKey")
                {
                    Fields = new List<IFieldInfo>()
                    {
                        textFieldInfo
                    }
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var rootWeb = testScope.SiteCollection.RootWeb;
                    var subWeb = rootWeb.Webs.Add("subweb");

                    var contentTypeHelper = injectionScope.Resolve<IContentTypeHelper>();

                    // Act
                    var ensuredSubWebContentType = contentTypeHelper.EnsureContentType(subWeb.ContentTypes, contentTypeInfo);

                    // Assert
                    var refetchedRootWebCT = rootWeb.ContentTypes["NameKey"];
                    Assert.IsNotNull(refetchedRootWebCT);

                    // CT shouldn't ever end up on sub-web exclusively (we wanna force the creation of RootWeb CT instead)
                    Assert.IsNull(subWeb.ContentTypes.Cast<SPContentType>().SingleOrDefault(ct => ct.Id == ensuredSubWebContentType.Id));
                }
            }
        }

        #endregion

        #region Content type Title, Description and Content Group should be easy to translate (if you configured your IResourceLocatorConfig properly)

        /// <summary>
        /// Validates that English CT name is initialized on English-language web
        /// </summary>
        [TestMethod]
        public void EnsureContentType_WhenEnglishOnlySiteCollection_ShouldCreateCTWithEnglishDisplayName()
        {
            using (var testScope = SiteTestScope.BlankSite(Language.English.Culture.LCID))
            {
                var contentTypeId = string.Format(
                    CultureInfo.InvariantCulture,
                    "0x0100{0:N}",
                    new Guid("{F8B6FF55-2C9E-4FA2-A705-F55FE3D18777}"));

                var contentTypeInfo = new ContentTypeInfo(contentTypeId, "Test_ContentTypeTitle", "Test_ContentTypeDescription", "Test_ContentGroup");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IContentTypeHelper contentTypeHelper = injectionScope.Resolve<IContentTypeHelper>();
                    var rootWebContentTypeCollection = testScope.SiteCollection.RootWeb.ContentTypes;

                    SPContentType contentType = contentTypeHelper.EnsureContentType(rootWebContentTypeCollection, contentTypeInfo);

                    Assert.AreEqual("EN Content Type Title", contentType.Name);
                    Assert.AreEqual("EN Content Type Description", contentType.Description);
                    Assert.AreEqual("EN Content Group", contentType.Group);

                    SPContentType contentTypeFromOldCollection = rootWebContentTypeCollection[contentType.Id];

                    Assert.AreEqual("EN Content Type Title", contentTypeFromOldCollection.Name);
                    Assert.AreEqual("EN Content Type Description", contentTypeFromOldCollection.Description);
                    Assert.AreEqual("EN Content Group", contentTypeFromOldCollection.Group);

                    SPContentType contentTypeRefetched = testScope.SiteCollection.RootWeb.ContentTypes[contentType.Id];

                    Assert.AreEqual("EN Content Type Title", contentTypeRefetched.Name);
                    Assert.AreEqual("EN Content Type Description", contentTypeRefetched.Description);
                    Assert.AreEqual("EN Content Group", contentTypeRefetched.Group);
                }
            }
        }

        /// <summary>
        /// Validates that French CT name is initialized on French-language web
        /// </summary>
        [TestMethod]
        public void EnsureContentType_WhenFrenchOnlySiteCollection_ShouldCreateCTWithFrenchDisplayName()
        {
            using (var testScope = SiteTestScope.BlankSite(Language.French.Culture.LCID))
            {
                var contentTypeId = string.Format(
                    CultureInfo.InvariantCulture,
                    "0x0100{0:N}",
                    new Guid("{F8B6FF55-2C9E-4FA2-A705-F55FE3D18777}"));

                var contentTypeInfo = new ContentTypeInfo(contentTypeId, "Test_ContentTypeTitle", "Test_ContentTypeDescription", "Test_ContentGroup");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IContentTypeHelper contentTypeHelper = injectionScope.Resolve<IContentTypeHelper>();
                    var rootWebContentTypeCollection = testScope.SiteCollection.RootWeb.ContentTypes;

                    SPContentType contentType = contentTypeHelper.EnsureContentType(rootWebContentTypeCollection, contentTypeInfo);
                    SPContentType contentTypeFromOldCollection = rootWebContentTypeCollection[contentType.Id];
                    SPContentType contentTypeRefetched = testScope.SiteCollection.RootWeb.ContentTypes[contentType.Id];

                    // Set MUI to french
                    var ambientThreadCulture = Thread.CurrentThread.CurrentUICulture;
                    Thread.CurrentThread.CurrentUICulture = Language.French.Culture;

                    Assert.AreEqual("FR Nom de type de contenu", contentType.Name);
                    Assert.AreEqual("FR Description de type de contenu", contentType.Description);
                    Assert.AreEqual("FR Groupe de contenu", contentType.Group);

                    Assert.AreEqual("FR Nom de type de contenu", contentTypeFromOldCollection.Name);
                    Assert.AreEqual("FR Description de type de contenu", contentTypeFromOldCollection.Description);
                    Assert.AreEqual("FR Groupe de contenu", contentTypeFromOldCollection.Group);

                    Assert.AreEqual("FR Nom de type de contenu", contentTypeRefetched.Name);
                    Assert.AreEqual("FR Description de type de contenu", contentTypeRefetched.Description);
                    Assert.AreEqual("FR Groupe de contenu", contentTypeRefetched.Group);

                    // Reset MUI to its old abient value
                    Thread.CurrentThread.CurrentUICulture = ambientThreadCulture;
                }
            }
        }

        /// <summary>
        /// Validates that CT name is initialized in both languages
        /// </summary>
        [TestMethod]
        [TestCategory(IntegrationTestCategories.Sanity)]
        public void EnsureContentType_WhenEnglishAndFrenchSiteCollection_ShouldCreateCTWithBothDisplayNames()
        {
            using (var testScope = SiteTestScope.BlankSite(Language.English.Culture.LCID))
            {
                // Add French so that both languages are supported
                var rootWeb = testScope.SiteCollection.RootWeb;
                rootWeb.AddSupportedUICulture(Language.French.Culture);
                rootWeb.Update();

                var contentTypeId = string.Format(
                    CultureInfo.InvariantCulture,
                    "0x0100{0:N}",
                    new Guid("{F8B6FF55-2C9E-4FA2-A705-F55FE3D18777}"));

                var contentTypeInfo = new ContentTypeInfo(contentTypeId, "Test_ContentTypeTitle", "Test_ContentTypeDescription", "Test_ContentGroup");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IContentTypeHelper contentTypeHelper = injectionScope.Resolve<IContentTypeHelper>();
                    var rootWebContentTypeCollection = testScope.SiteCollection.RootWeb.ContentTypes;

                    SPContentType contentType = contentTypeHelper.EnsureContentType(rootWebContentTypeCollection, contentTypeInfo);
                    SPContentType contentTypeFromOldCollection = rootWebContentTypeCollection[contentType.Id];
                    SPContentType contentTypeRefetched = testScope.SiteCollection.RootWeb.ContentTypes[contentType.Id];
                    
                    // Set MUI to english
                    var ambientThreadCulture = Thread.CurrentThread.CurrentUICulture;
                    Thread.CurrentThread.CurrentUICulture = Language.English.Culture;

                    Assert.AreEqual("EN Content Type Title", contentType.Name);
                    Assert.AreEqual("EN Content Type Description", contentType.Description);
                    Assert.AreEqual("EN Content Group", contentType.Group);

                    Assert.AreEqual("EN Content Type Title", contentTypeFromOldCollection.Name);
                    Assert.AreEqual("EN Content Type Description", contentTypeFromOldCollection.Description);
                    Assert.AreEqual("EN Content Group", contentTypeFromOldCollection.Group);

                    Assert.AreEqual("EN Content Type Title", contentTypeRefetched.Name);
                    Assert.AreEqual("EN Content Type Description", contentTypeRefetched.Description);
                    Assert.AreEqual("EN Content Group", contentTypeRefetched.Group);

                    // Set MUI to french
                    Thread.CurrentThread.CurrentUICulture = Language.French.Culture;

                    Assert.AreEqual("FR Nom de type de contenu", contentType.Name);
                    Assert.AreEqual("FR Description de type de contenu", contentType.Description);
                    Assert.AreEqual("FR Groupe de contenu", contentType.Group);

                    Assert.AreEqual("FR Nom de type de contenu", contentTypeFromOldCollection.Name);
                    Assert.AreEqual("FR Description de type de contenu", contentTypeFromOldCollection.Description);
                    Assert.AreEqual("FR Groupe de contenu", contentTypeFromOldCollection.Group);

                    Assert.AreEqual("FR Nom de type de contenu", contentTypeRefetched.Name);
                    Assert.AreEqual("FR Description de type de contenu", contentTypeRefetched.Description);
                    Assert.AreEqual("FR Groupe de contenu", contentTypeRefetched.Group);

                    // Reset MUI to its old abient value
                    Thread.CurrentThread.CurrentUICulture = ambientThreadCulture;
                }
            }
        }

        #endregion

        #region Using OOTB fields as part of Content Type definition should work, but only if the site columns already exist

        /// <summary>
        /// Validates that MinimalFieldInfos can be used to define additions to content types (provided the OOTB site column exists in the site collection)
        /// </summary>
        [TestMethod]
        public void EnsureContentType_WhenEnsuringAMinimalFieldInfoOOTBColumnAsFieldOnContentType_AndOOTBSiteColumnIsAvailable_ShouldMakeFieldAvailableOnCT()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                ContentTypeInfo contentTypeInfo = new ContentTypeInfo(
                    ContentTypeIdBuilder.CreateChild(new SPContentTypeId("0x01"), Guid.NewGuid()),
                    "CTNameKey",
                    "CTDescrKey",
                    "GroupKey")
                    {
                        Fields = new List<IFieldInfo>()
                        {
                            BuiltInFields.AssignedTo,   // OOTB User field
                            BuiltInFields.Cellphone,    // OOTB Text field
                            BuiltInFields.EnterpriseKeywords    // OOTB Taxonomy Multi field
                        }
                    };

                ListInfo listInfo = new ListInfo("somelistpath", "ListNameKey", "ListDescrKey")
                    {
                        ContentTypes = new List<ContentTypeInfo>()
                        {
                            contentTypeInfo
                        }
                    };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IContentTypeHelper contentTypeHelper = injectionScope.Resolve<IContentTypeHelper>();
                    var contentTypeCollection = testScope.SiteCollection.RootWeb.ContentTypes;

                    // Act
                    SPContentType contentType = contentTypeHelper.EnsureContentType(contentTypeCollection, contentTypeInfo);

                    // Assert
                    Assert.IsNotNull(contentType.Fields[BuiltInFields.AssignedTo.Id]);
                    Assert.IsNotNull(contentType.Fields[BuiltInFields.Cellphone.Id]);
                    Assert.IsNotNull(contentType.Fields[BuiltInFields.EnterpriseKeywords.Id]);

                    // Use the CT's OOTB fields in a list and create an item just for kicks
                    IListHelper listHelper = injectionScope.Resolve<IListHelper>();
                    SPList list = listHelper.EnsureList(testScope.SiteCollection.RootWeb, listInfo);
                    SPListItem item = list.AddItem();
                    item.Update();

                    var ensuredUser1 = testScope.SiteCollection.RootWeb.EnsureUser(Environment.UserName);

                    IFieldValueWriter writer = injectionScope.Resolve<IFieldValueWriter>();
                    writer.WriteValuesToListItem(
                        item,
                        new List<FieldValueInfo>()
                        {
                            new FieldValueInfo(BuiltInFields.AssignedTo, new UserValue(ensuredUser1)),
                            new FieldValueInfo(BuiltInFields.Cellphone, "Test Cellphone Value"),
                            new FieldValueInfo(BuiltInFields.EnterpriseKeywords, new TaxonomyValueCollection())
                        });

                    item.Update();

                    Assert.IsNotNull(item[BuiltInFields.AssignedTo.Id]);
                    Assert.IsNotNull(item[BuiltInFields.Cellphone.Id]);
                    Assert.IsNotNull(item[BuiltInFields.EnterpriseKeywords.Id]);
                }
            }
        }

        /// <summary>
        /// Validates that MinimalFieldInfos cannot be used to define additions to content types when the relevant site column doesn't exist
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void EnsureContentType_WhenEnsuringAMinimalFieldInfoOOTBColumnAsFieldOnContentType_AndOOTBSiteColumnIsNOTAvailable_ShouldFailBecauseSuchOOTBSiteColumnShouldBeAddedByOOTBFeatures()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                ContentTypeInfo contentTypeInfo = new ContentTypeInfo(
                    ContentTypeIdBuilder.CreateChild(new SPContentTypeId("0x01"), Guid.NewGuid()),
                    "CTNameKey",
                    "CTDescrKey",
                    "GroupKey")
                {
                    Fields = new List<IFieldInfo>()
                        {
                            PublishingFields.PublishingPageContent  // Should be missing from site columns (only available in Publishing sites)
                        }
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IContentTypeHelper contentTypeHelper = injectionScope.Resolve<IContentTypeHelper>();
                    var contentTypeCollection = testScope.SiteCollection.RootWeb.ContentTypes;

                    // Act + Assert
                    SPContentType contentType = contentTypeHelper.EnsureContentType(contentTypeCollection, contentTypeInfo);
                }
            }
        }

        #endregion
    }
}
