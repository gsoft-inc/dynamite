using System;
using System.Collections.Generic;
using Autofac;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.Fields.Constants;
using GSoft.Dynamite.Fields.Types;
using GSoft.Dynamite.ValueTypes;
using GSoft.Dynamite.ValueTypes.Writers;
using GSoft.Dynamite.ValueTypes.Writers.Fakes;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSoft.Dynamite.UnitTests.Binding.IO
{
    /// <summary>
    /// Tests for the ValueWriter class
    /// </summary>
    [TestClass]
    public class FieldValueWriterTest
    {
        #region WriteValuesToListItem
        /// <summary>
        /// Test for the WriteValuesToListItem method.
        /// When updating five fields on a list item, the item is updated five times.
        /// </summary>
        [TestMethod]
        public void WriteValuesToListItem_WhenGiven5FieldValues_ShouldCallWriteValueToListItem5Times()
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

        #region WriteValueToListItem
        /// <summary>
        /// Test for the WriteValueToListItem method.
        /// When updating the value of a Date time field, use the DateTime value writer.
        /// </summary>
        [TestMethod]
        public void WriteValueToListItem_GivenDateTimeFieldInfo_ShouldUseDateTimeValueWriter()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                var correctWriterWasUsed = false;
                var fieldInfo = new DateTimeFieldInfo("InternalName", Guid.NewGuid(), string.Empty, string.Empty, string.Empty);

                ShimDateTimeValueWriter.AllInstances.WriteValueToListItemSPListItemFieldValueInfo = (inst, listItem, fieldValueInfo) =>
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
                Assert.IsTrue(correctWriterWasUsed, "The DateTimeValueWriter should have been used for the DateTimeFieldInfo type.");
            }
        }

        /// <summary>
        /// Test for the WriteValueToListItem method.
        /// When updating the value of a Guid field, use the Guid value writer.
        /// </summary>
        [TestMethod]
        public void WriteValueToListItem_GivenGuidFieldInfo_ShouldUseGuidValueWriter()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                var correctWriterWasUsed = false;
                var fieldInfo = new GuidFieldInfo("InternalName", Guid.NewGuid(), string.Empty, string.Empty, string.Empty);

                ShimGuidValueWriter.AllInstances.WriteValueToListItemSPListItemFieldValueInfo = (inst, listItem, fieldValueInfo) =>
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
                Assert.IsTrue(correctWriterWasUsed, "The GuidValueWriter should have been used for the GuidFieldInfo type.");
            }
        }

        /// <summary>
        /// Test for the WriteValueToListItem method.
        /// When updating the value of a Html field, use the Base value writer.
        /// </summary>
        [TestMethod]
        public void WriteValueToListItem_GivenHtmlFieldInfo_ShouldUseBaseValueWriter()
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
                Assert.IsTrue(correctWriterWasUsed, "The BaseValueWriter should have been used for the HtmlFieldInfo type.");
            }
        }

        /// <summary>
        /// Test for the WriteValueToListItem method.
        /// When updating the value of a Image field, use the Image value writer.
        /// </summary>
        [TestMethod]
        public void WriteValueToListItem_GivenImageFieldInfo_ShouldUseImageValueWriter()
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
                Assert.IsTrue(correctWriterWasUsed, "The ImageValueWriter should have been used for the ImageFieldInfo type.");
            }
        }

        /// <summary>
        /// Test for the WriteValueToListItem method.
        /// When updating the value of a Lookup field, use the Lookup value writer.
        /// </summary>
        [TestMethod]
        public void WriteValueToListItem_GivenLookupFieldInfo_ShouldUseLookupValueWriter()
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
                Assert.IsTrue(correctWriterWasUsed, "The LookupValueWriter should have been used for the LookupFieldInfo type.");
            }
        }

        /// <summary>
        /// Test for the WriteValueToListItem method.
        /// When updating the value of a Multi value lookup field, use the LookupCollection value writer.
        /// </summary>
        [TestMethod]
        public void WriteValueToListItem_GivenLookupMultiFieldInfo_ShouldUseLookupValueCollectionWriter()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                var correctWriterWasUsed = false;
                var fieldInfo = new LookupMultiFieldInfo("InternalName", Guid.NewGuid(), string.Empty, string.Empty, string.Empty);

                ShimLookupValueCollectionWriter.AllInstances.WriteValueToListItemSPListItemFieldValueInfo = (inst, listItem, fieldValueInfo) =>
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
                Assert.IsTrue(correctWriterWasUsed, "The LookupValueCollectionWriter should have been used for the LookupFieldInfo type.");
            }
        }

        /// <summary>
        /// Test for the WriteValueToListItem method.
        /// When updating the value of a field with a MinimalFieldInfo, use the associated value writer.
        /// </summary>
        [TestMethod]
        public void WriteValueToListItem_GivenMinimalFieldInfo_ShouldUseCorrespondingValueTypeValueWriter()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                var correctWriterWasUsed = false;
                var fieldInfo = new MinimalFieldInfo<UrlValue>("InternalName", Guid.NewGuid());

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
                Assert.IsTrue(correctWriterWasUsed, "The UrlValueWriter should have been used for the MinimalFieldInfo type.");
            }
        }

        /// <summary>
        /// Test for the WriteValueToListItem method.
        /// When updating the value of a Note field, use the Base value writer.
        /// </summary>
        [TestMethod]
        public void WriteValueToListItem_GivenNoteFieldInfo_ShouldUseBaseValueWriter()
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
                Assert.IsTrue(correctWriterWasUsed, "The BaseValueWriter should have been used for the NoteFieldInfo type.");
            }
        }

        /// <summary>
        /// Test for the WriteValueToListItem method.
        /// When updating the value of a Number field, use the double value writer.
        /// </summary>
        [TestMethod]
        public void WriteValueToListItem_GivenNumberFieldInfo_ShouldUseDoubleValueWriter()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                var correctWriterWasUsed = false;
                var fieldInfo = new NumberFieldInfo("InternalName", Guid.NewGuid(), string.Empty, string.Empty, string.Empty);

                ShimDoubleValueWriter.AllInstances.WriteValueToListItemSPListItemFieldValueInfo = (inst, listItem, fieldValueInfo) =>
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
                Assert.IsTrue(correctWriterWasUsed, "The DoubleValueWriter should have been used for the NumberFieldInfo type.");
            }
        }

        /// <summary>
        /// Test for the WriteValueToListItem method.
        /// When updating the value of a Taxonomy field, use the Taxonomy value writer.
        /// </summary>
        [TestMethod]
        public void WriteValueToListItem_GivenTaxonomyFieldInfo_ShouldUseTaxonomyValueWriter()
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
                Assert.IsTrue(correctWriterWasUsed, "The TaxonomyValueWriter should have been used for the TaxonomyFieldInfo type.");
            }
        }

        /// <summary>
        /// Test for the WriteValueToListItem method.
        /// When updating the value of a Taxonomy Multi field, use the Taxonomy Multi value writer.
        /// </summary>
        [TestMethod]
        public void WriteValueToListItem_GivenTaxonomyMultiFieldInfo_ShouldUseTaxonomyMultiValueWriter()
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
                Assert.IsTrue(correctWriterWasUsed, "The TaxonomyMultiValueWriter should have been used for the TaxonomyMultiFieldInfo type.");
            }
        }

        /// <summary>
        /// Test for the WriteValueToListItem method.
        /// When updating the value of a Text field, use the Base value writer.
        /// </summary>
        [TestMethod]
        public void WriteValueToListItem_GivenTextFieldInfo_ShouldUseBaseValueWriter()
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
                Assert.IsTrue(correctWriterWasUsed, "The BaseValueWriter should have been used for the TextFieldInfo type.");
            }
        }

        /// <summary>
        /// Test for the WriteValueToListItem method.
        /// When updating the value of a Url field, use the url value writer.
        /// </summary>
        [TestMethod]
        public void WriteValueToListItem_GivenUrlFieldFieldInfo_ShouldUseUrlValueWriter()
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
                Assert.IsTrue(correctWriterWasUsed, "The UrlValueWriter should have been used for the UrlFieldFieldInfo type.");
            }
        }

        /// <summary>
        /// Test for the WriteValueToListItem method.
        /// When updating the value of a User field, use the User value writer.
        /// </summary>
        [TestMethod]
        public void WriteValueToListItem_GivenUserFieldFieldInfo_ShouldUseUserValueWriter()
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
                Assert.IsTrue(correctWriterWasUsed, "The UserValueWriter should have been used for the UserFieldFieldInfo type.");
            }
        }

        /// <summary>
        /// Test for the WriteValueToListItem method.
        /// When updating the value of a User Multi field, use the UserCollection value writer.
        /// </summary>
        [TestMethod]
        public void WriteValueToListItem_GivenUserMultiFieldFieldInfo_ShouldUseUserValueCollectionWriter()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                var correctWriterWasUsed = false;
                var fieldInfo = new UserMultiFieldInfo("InternalName", Guid.NewGuid(), string.Empty, string.Empty, string.Empty);

                ShimUserValueCollectionWriter.AllInstances.WriteValueToListItemSPListItemFieldValueInfo = (inst, listItem, fieldValueInfo) =>
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
                Assert.IsTrue(correctWriterWasUsed, "The UserValueCollectionWriter should have been used for the UserMultiFieldFieldInfo type.");
            }
        }

        /// <summary>
        /// Test for the WriteValueToListItem method.
        /// When updating a string field, the field is updated.
        /// </summary>
        [TestMethod]
        public void WriteValueToListItem_WhenGivenFieldValue_ShouldUpdateFieldValue()
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