using System;
using System.Collections.Generic;
using Autofac;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.Fields.Constants;
using GSoft.Dynamite.Fields.Types;
using GSoft.Dynamite.ValueTypes.Writers;
using GSoft.Dynamite.ValueTypes.Writers.Fakes;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSoft.Dynamite.UnitTests.Binding.IO
{
    /// <summary>
    /// Tests for the SPItemValueWriter class
    /// </summary>
    [TestClass]
    public class SPItemValueWriterTest
    {
        #region WriteValuesToSPListItem
        /// <summary>
        /// Test for the WriteValuesToSPListItem method.
        /// When updating five fields on a list item, the item is updated five times.
        /// </summary>
        [TestMethod]
        public void WriteValuesToSPListItem_WhenGiven5FieldValues_ShouldCallWriteValueToSPListItem5Times()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                var actualCallCount = 0;
                var expectedCallCount = 5;

                var fieldValueInfos = new List<FieldValueInfo>()
                {
                    new FieldValueInfo(BuiltInFields.Title, null),
                    new FieldValueInfo(BuiltInFields.Title, null),
                    new FieldValueInfo(BuiltInFields.Title, null),
                    new FieldValueInfo(BuiltInFields.Title, null),
                    new FieldValueInfo(BuiltInFields.Title, null)
                };

                ShimFieldValueWriter.AllInstances.WriteValueToListItemSPListItemFieldValueInfo = (writerInst, listItemParam, fieldValuesParam) =>
                 {
                     actualCallCount++;
                 };

                var fakeListItem = new ShimSPListItem().Instance;

                IFieldValueWriter writer;
                using (var scope = UnitTestServiceLocator.BeginLifetimeScope())
                {
                    writer = scope.Resolve<IFieldValueWriter>();
                }

                // Act
                writer.WriteValuesToListItem(fakeListItem, fieldValueInfos);

                // Assert
                Assert.AreEqual(expectedCallCount, actualCallCount, string.Format("The call was made {0} out of {1} times.", actualCallCount, expectedCallCount));
            }
        }
        #endregion

        #region WriteValueToSPListItem
        /// <summary>
        /// Test for the WriteValueToSPListItem method.
        /// When updating the value of a Date time field, use the Base value writer.
        /// </summary>
        [TestMethod]
        public void WriteValueToSPListItem_GivenDateTimeFieldInfo_ShouldUseSPItemBaseValueWriter()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                var correctWriterWasUsed = false;
                var fieldInfo = new DateTimeFieldInfo("InternalName", Guid.NewGuid(), string.Empty, string.Empty, string.Empty);

                ShimStringValueWriter.AllInstances.WriteValueToListItemSPListItemFieldValueInfo = (inst, listItem, fieldValueInfo) =>
                {
                    correctWriterWasUsed = true;
                };

                var fakeListItem = new ShimSPListItem().Instance;

                IFieldValueWriter writer;
                using (var scope = UnitTestServiceLocator.BeginLifetimeScope())
                {
                    writer = scope.Resolve<IFieldValueWriter>();
                }

                // Act
                writer.WriteValueToListItem(fakeListItem, new FieldValueInfo(fieldInfo, null));

                // Assert
                Assert.IsTrue(correctWriterWasUsed, "The SPItemBaseValueWriter should have been used for the DateTimeFieldInfo type.");
            }
        }

        /// <summary>
        /// Test for the WriteValueToSPListItem method.
        /// When updating the value of a Guid field, use the Base value writer.
        /// </summary>
        [TestMethod]
        public void WriteValueToSPListItem_GivenGuidFieldInfo_ShouldUseSPItemBaseValueWriter()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                var correctWriterWasUsed = false;
                var fieldInfo = new GuidFieldInfo("InternalName", Guid.NewGuid(), string.Empty, string.Empty, string.Empty);

                ShimStringValueWriter.AllInstances.WriteValueToListItemSPListItemFieldValueInfo = (inst, listItem, fieldValueInfo) =>
                {
                    correctWriterWasUsed = true;
                };

                var fakeListItem = new ShimSPListItem().Instance;

                IFieldValueWriter writer;
                using (var scope = UnitTestServiceLocator.BeginLifetimeScope())
                {
                    writer = scope.Resolve<IFieldValueWriter>();
                }

                // Act
                writer.WriteValueToListItem(fakeListItem, new FieldValueInfo(fieldInfo, null));

                // Assert
                Assert.IsTrue(correctWriterWasUsed, "The SPItemBaseValueWriter should have been used for the GuidFieldInfo type.");
            }
        }

        /// <summary>
        /// Test for the WriteValueToSPListItem method.
        /// When updating the value of a Html field, use the Base value writer.
        /// </summary>
        [TestMethod]
        public void WriteValueToSPListItem_GivenHtmlFieldInfo_ShouldUseSPItemBaseValueWriter()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                var correctWriterWasUsed = false;
                var fieldInfo = new HtmlFieldInfo("InternalName", Guid.NewGuid(), string.Empty, string.Empty, string.Empty);

                ShimStringValueWriter.AllInstances.WriteValueToListItemSPListItemFieldValueInfo = (inst, listItem, fieldValueInfo) =>
                {
                    correctWriterWasUsed = true;
                };

                var fakeListItem = new ShimSPListItem().Instance;

                IFieldValueWriter writer;
                using (var scope = UnitTestServiceLocator.BeginLifetimeScope())
                {
                    writer = scope.Resolve<IFieldValueWriter>();
                }

                // Act
                writer.WriteValueToListItem(fakeListItem, new FieldValueInfo(fieldInfo, null));

                // Assert
                Assert.IsTrue(correctWriterWasUsed, "The SPItemBaseValueWriter should have been used for the HtmlFieldInfo type.");
            }
        }

        /// <summary>
        /// Test for the WriteValueToSPListItem method.
        /// When updating the value of a Image field, use the Image value writer.
        /// </summary>
        [TestMethod]
        public void WriteValueToSPListItem_GivenImageFieldInfo_ShouldUseSPItemImageValueWriter()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                var correctWriterWasUsed = false;
                var fieldInfo = new ImageFieldInfo("InternalName", Guid.NewGuid(), string.Empty, string.Empty, string.Empty);

                ShimImageValueWriter.AllInstances.WriteValueToListItemSPListItemFieldValueInfo = (inst, listItem, fieldValueInfo) =>
                {
                    correctWriterWasUsed = true;
                };

                var fakeListItem = new ShimSPListItem().Instance;

                IFieldValueWriter writer;
                using (var scope = UnitTestServiceLocator.BeginLifetimeScope())
                {
                    writer = scope.Resolve<IFieldValueWriter>();
                }

                // Act
                writer.WriteValueToListItem(fakeListItem, new FieldValueInfo(fieldInfo, null));

                // Assert
                Assert.IsTrue(correctWriterWasUsed, "The SPItemImageValueWriter should have been used for the ImageFieldInfo type.");
            }
        }

        /// <summary>
        /// Test for the WriteValueToSPListItem method.
        /// When updating the value of a Lookup field, use the Lookup value writer.
        /// </summary>
        [TestMethod]
        public void WriteValueToSPListItem_GivenLookupFieldInfo_ShouldUseSPItemLookupValueWriter()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                var correctWriterWasUsed = false;
                var fieldInfo = new LookupFieldInfo("InternalName", Guid.NewGuid(), string.Empty, string.Empty, string.Empty);

                ShimLookupValueWriter.AllInstances.WriteValueToListItemSPListItemFieldValueInfo = (inst, listItem, fieldValueInfo) =>
                {
                    correctWriterWasUsed = true;
                };

                var fakeListItem = new ShimSPListItem().Instance;

                IFieldValueWriter writer;
                using (var scope = UnitTestServiceLocator.BeginLifetimeScope())
                {
                    writer = scope.Resolve<IFieldValueWriter>();
                }

                // Act
                writer.WriteValueToListItem(fakeListItem, new FieldValueInfo(fieldInfo, null));

                // Assert
                Assert.IsTrue(correctWriterWasUsed, "The SPItemLookupValueWriter should have been used for the LookupFieldInfo type.");
            }
        }

        /// <summary>
        /// Test for the WriteValueToSPListItem method.
        /// When updating the value of a Multi value lookup field, a not supported exception is thrown.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void WriteValueToSPListItem_GivenLookupMultiFieldInfo_ExpectNotSupportedException()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                var fieldInfo = new LookupMultiFieldInfo("InternalName", Guid.NewGuid(), string.Empty, string.Empty, string.Empty);
                var fakeListItem = new ShimSPListItem().Instance;

                IFieldValueWriter writer;
                using (var scope = UnitTestServiceLocator.BeginLifetimeScope())
                {
                    writer = scope.Resolve<IFieldValueWriter>();
                }

                // Act
                writer.WriteValueToListItem(fakeListItem, new FieldValueInfo(fieldInfo, null));

                // Assert
                // Expect Not Supported Exception 
            }
        }

        /// <summary>
        /// Test for the WriteValueToSPListItem method.
        /// When updating the value of a field with a MinimalFieldInfo, use the Base value writer.
        /// </summary>
        [TestMethod]
        public void WriteValueToSPListItem_GivenMinimalFieldInfo_ShouldUseSPItemBaseValueWriter()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                var correctWriterWasUsed = false;
                var fieldInfo = new MinimalFieldInfo("InternalName", Guid.NewGuid());

                ShimStringValueWriter.AllInstances.WriteValueToListItemSPListItemFieldValueInfo = (inst, listItem, fieldValueInfo) =>
                {
                    correctWriterWasUsed = true;
                };

                var fakeListItem = new ShimSPListItem().Instance;

                IFieldValueWriter writer;
                using (var scope = UnitTestServiceLocator.BeginLifetimeScope())
                {
                    writer = scope.Resolve<IFieldValueWriter>();
                }

                // Act
                writer.WriteValueToListItem(fakeListItem, new FieldValueInfo(fieldInfo, null));

                // Assert
                Assert.IsTrue(correctWriterWasUsed, "The SPItemBaseValueWriter should have been used for the MinimalFieldInfo type.");
            }
        }

        /// <summary>
        /// Test for the WriteValueToSPListItem method.
        /// When updating the value of a Note field, use the Base value writer.
        /// </summary>
        [TestMethod]
        public void WriteValueToSPListItem_GivenNoteFieldInfo_ShouldUseSPItemBaseValueWriter()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                var correctWriterWasUsed = false;
                var fieldInfo = new NoteFieldInfo("InternalName", Guid.NewGuid(), string.Empty, string.Empty, string.Empty);

                ShimStringValueWriter.AllInstances.WriteValueToListItemSPListItemFieldValueInfo = (inst, listItem, fieldValueInfo) =>
                {
                    correctWriterWasUsed = true;
                };

                var fakeListItem = new ShimSPListItem().Instance;

                IFieldValueWriter writer;
                using (var scope = UnitTestServiceLocator.BeginLifetimeScope())
                {
                    writer = scope.Resolve<IFieldValueWriter>();
                }

                // Act
                writer.WriteValueToListItem(fakeListItem, new FieldValueInfo(fieldInfo, null));

                // Assert
                Assert.IsTrue(correctWriterWasUsed, "The SPItemBaseValueWriter should have been used for the NoteFieldInfo type.");
            }
        }

        /// <summary>
        /// Test for the WriteValueToSPListItem method.
        /// When updating the value of a Number field, use the Base value writer.
        /// </summary>
        [TestMethod]
        public void WriteValueToSPListItem_GivenNumberFieldInfo_ShouldUseSPItemBaseValueWriter()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                var correctWriterWasUsed = false;
                var fieldInfo = new NumberFieldInfo("InternalName", Guid.NewGuid(), string.Empty, string.Empty, string.Empty);

                ShimStringValueWriter.AllInstances.WriteValueToListItemSPListItemFieldValueInfo = (inst, listItem, fieldValueInfo) =>
                {
                    correctWriterWasUsed = true;
                };

                var fakeListItem = new ShimSPListItem().Instance;

                IFieldValueWriter writer;
                using (var scope = UnitTestServiceLocator.BeginLifetimeScope())
                {
                    writer = scope.Resolve<IFieldValueWriter>();
                }

                // Act
                writer.WriteValueToListItem(fakeListItem, new FieldValueInfo(fieldInfo, null));

                // Assert
                Assert.IsTrue(correctWriterWasUsed, "The SPItemBaseValueWriter should have been used for the NumberFieldInfo type.");
            }
        }

        /// <summary>
        /// Test for the WriteValueToSPListItem method.
        /// When updating the value of a Taxonomy field, use the Taxonomy value writer.
        /// </summary>
        [TestMethod]
        public void WriteValueToSPListItem_GivenTaxonomyFieldInfo_ShouldUseSPItemTaxonomyValueWriter()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                var correctWriterWasUsed = false;
                var fieldInfo = new TaxonomyFieldInfo("InternalName", Guid.NewGuid(), string.Empty, string.Empty, string.Empty);

                ShimTaxonomyFullValueWriter.AllInstances.WriteValueToListItemSPListItemFieldValueInfo = (inst, listItem, fieldValueInfo) =>
                {
                    correctWriterWasUsed = true;
                };

                var fakeListItem = new ShimSPListItem().Instance;

                IFieldValueWriter writer;
                using (var scope = UnitTestServiceLocator.BeginLifetimeScope())
                {
                    writer = scope.Resolve<IFieldValueWriter>();
                }

                // Act
                writer.WriteValueToListItem(fakeListItem, new FieldValueInfo(fieldInfo, null));

                // Assert
                Assert.IsTrue(correctWriterWasUsed, "The SPItemTaxonomyValueWriter should have been used for the TaxonomyFieldInfo type.");
            }
        }

        /// <summary>
        /// Test for the WriteValueToSPListItem method.
        /// When updating the value of a Taxonomy Multi field, use the Taxonomy Multi value writer.
        /// </summary>
        [TestMethod]
        public void WriteValueToSPListItem_GivenTaxonomyMultiFieldInfo_ShouldUseSPItemTaxonomyMultiValueWriter()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                var correctWriterWasUsed = false;
                var fieldInfo = new TaxonomyMultiFieldInfo("InternalName", Guid.NewGuid(), string.Empty, string.Empty, string.Empty);

                ShimTaxonomyFullValueCollectionWriter.AllInstances.WriteValueToListItemSPListItemFieldValueInfo = (inst, listItem, fieldValueInfo) =>
                {
                    correctWriterWasUsed = true;
                };

                var fakeListItem = new ShimSPListItem().Instance;

                IFieldValueWriter writer;
                using (var scope = UnitTestServiceLocator.BeginLifetimeScope())
                {
                    writer = scope.Resolve<IFieldValueWriter>();
                }

                // Act
                writer.WriteValueToListItem(fakeListItem, new FieldValueInfo(fieldInfo, null));

                // Assert
                Assert.IsTrue(correctWriterWasUsed, "The SPItemTaxonomyMultiValueWriter should have been used for the TaxonomyMultiFieldInfo type.");
            }
        }

        /// <summary>
        /// Test for the WriteValueToSPListItem method.
        /// When updating the value of a Text field, use the Base value writer.
        /// </summary>
        [TestMethod]
        public void WriteValueToSPListItem_GivenTextFieldInfo_ShouldUseSPItemBaseValueWriter()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                var correctWriterWasUsed = false;
                var fieldInfo = new TextFieldInfo("InternalName", Guid.NewGuid(), string.Empty, string.Empty, string.Empty);

                ShimStringValueWriter.AllInstances.WriteValueToListItemSPListItemFieldValueInfo = (inst, listItem, fieldValueInfo) =>
                {
                    correctWriterWasUsed = true;
                };

                var fakeListItem = new ShimSPListItem().Instance;

                IFieldValueWriter writer;
                using (var scope = UnitTestServiceLocator.BeginLifetimeScope())
                {
                    writer = scope.Resolve<IFieldValueWriter>();
                }

                // Act
                writer.WriteValueToListItem(fakeListItem, new FieldValueInfo(fieldInfo, null));

                // Assert
                Assert.IsTrue(correctWriterWasUsed, "The SPItemBaseValueWriter should have been used for the TextFieldInfo type.");
            }
        }

        /// <summary>
        /// Test for the WriteValueToSPListItem method.
        /// When updating the value of a Url field, use the url value writer.
        /// </summary>
        [TestMethod]
        public void WriteValueToSPListItem_GivenUrlFieldFieldInfo_ShouldUseSPItemUrlValueWriter()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                var correctWriterWasUsed = false;
                var fieldInfo = new UrlFieldInfo("InternalName", Guid.NewGuid(), string.Empty, string.Empty, string.Empty);

                ShimUrlValueWriter.AllInstances.WriteValueToListItemSPListItemFieldValueInfo = (inst, listItem, fieldValueInfo) =>
                {
                    correctWriterWasUsed = true;
                };

                var fakeListItem = new ShimSPListItem().Instance;

                IFieldValueWriter writer;
                using (var scope = UnitTestServiceLocator.BeginLifetimeScope())
                {
                    writer = scope.Resolve<IFieldValueWriter>();
                }

                // Act
                writer.WriteValueToListItem(fakeListItem, new FieldValueInfo(fieldInfo, null));

                // Assert
                Assert.IsTrue(correctWriterWasUsed, "The SPItemUrlValueWriter should have been used for the UrlFieldFieldInfo type.");
            }
        }

        /// <summary>
        /// Test for the WriteValueToSPListItem method.
        /// When updating the value of a User field, use the User value writer.
        /// </summary>
        [TestMethod]
        public void WriteValueToSPListItem_GivenUserFieldFieldInfo_ShouldUseSPItemUserValueWriter()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                var correctWriterWasUsed = false;
                var fieldInfo = new UserFieldInfo("InternalName", Guid.NewGuid(), string.Empty, string.Empty, string.Empty);

                ShimUserValueWriter.AllInstances.WriteValueToListItemSPListItemFieldValueInfo = (inst, listItem, fieldValueInfo) =>
                {
                    correctWriterWasUsed = true;
                };

                var fakeListItem = new ShimSPListItem().Instance;

                IFieldValueWriter writer;
                using (var scope = UnitTestServiceLocator.BeginLifetimeScope())
                {
                    writer = scope.Resolve<IFieldValueWriter>();
                }

                // Act
                writer.WriteValueToListItem(fakeListItem, new FieldValueInfo(fieldInfo, null));

                // Assert
                Assert.IsTrue(correctWriterWasUsed, "The SPItemUserValueWriter should have been used for the UserFieldFieldInfo type.");
            }
        }

        /// <summary>
        /// Test for the WriteValueToSPListItem method.
        /// When updating the value of a User Multi field, a not supported exception is thrown.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void WriteValueToSPListItem_GivenUserMultiFieldFieldInfo_ExpectNotSupportedException()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                var fieldInfo = new UserMultiFieldInfo("InternalName", Guid.NewGuid(), string.Empty, string.Empty, string.Empty);
                var fakeListItem = new ShimSPListItem().Instance;

                IFieldValueWriter writer;
                using (var scope = UnitTestServiceLocator.BeginLifetimeScope())
                {
                    writer = scope.Resolve<IFieldValueWriter>();
                }

                // Act
                writer.WriteValueToListItem(fakeListItem, new FieldValueInfo(fieldInfo, null));

                // Assert
                // Expect Not Supported Exception 
            }
        }

        /// <summary>
        /// Test for the WriteValueToSPListItem method.
        /// When updating a string field, the field is updated.
        /// </summary>
        [TestMethod]
        public void WriteValueToSPListItem_WhenGivenFieldValue_ShouldUpdateFieldValue()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                var setValue = "SomeTitle";
                var setField = BuiltInFields.Title;

                var fakeListItemShim = new ShimSPListItem()
                {
                    ItemSetStringObject = (internalName, value) =>
                    {
                        Assert.AreEqual(setValue, value as string);
                        Assert.AreEqual(setField.InternalName, internalName);
                    }
                };

                IFieldValueWriter writer;
                SPListItem fakeListItem = fakeListItemShim.Instance;

                using (var scope = UnitTestServiceLocator.BeginLifetimeScope())
                {
                    writer = scope.Resolve<IFieldValueWriter>();
                }

                // Act
                writer.WriteValueToListItem(fakeListItem, new FieldValueInfo(setField, setValue));

                // Assert
            }
        }
        #endregion
    }
}