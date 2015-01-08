using System;
using System.Collections.Generic;
using Autofac;
using GSoft.Dynamite.Binding.IO;
using GSoft.Dynamite.Binding.IO.Fakes;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.Fields.Constants;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSoft.Dynamite.UnitTests.Binding.IO
{
    [TestClass]
    public class SPItemValueWriterTest
    {
        #region WriteValuesToSPListItem
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

                ShimSPItemValueWriter.AllInstances.WriteValueToSPListItemSPListItemFieldValueInfo = (writerInst, listItemParam, fieldValuesParam) =>
                {
                    actualCallCount++;
                    return listItemParam;
                };

                var fakeListItem = new ShimSPListItem().Instance;

                ISPItemValueWriter writer;
                using (var scope = UnitTestServiceLocator.BeginLifetimeScope())
                {
                    writer = scope.Resolve<ISPItemValueWriter>();
                }

                // Act
                writer.WriteValuesToSPListItem(fakeListItem, fieldValueInfos);

                // Assert
                Assert.AreEqual(expectedCallCount, actualCallCount, string.Format("The call was made {0} out of {1} times.", actualCallCount, expectedCallCount));
            }
        }
        #endregion

        #region WriteValueToSPListItem
        [TestMethod]
        public void WriteValueToSPListItem_GivenDateTimeFieldInfo_ShouldUseSPItemBaseValueWriter()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                var correctWriterWasUsed = false;
                var fieldInfo = new DateTimeFieldInfo("InternalName", Guid.NewGuid(), string.Empty, string.Empty, string.Empty);

                ShimSPItemBaseValueWriter.AllInstances.WriteValueToSPListItemSPListItemFieldValueInfo = (inst, listItem, fieldValueInfo) =>
                {
                    correctWriterWasUsed = true;

                    return listItem;
                };

                var fakeListItem = new ShimSPListItem().Instance;

                ISPItemValueWriter writer;
                using (var scope = UnitTestServiceLocator.BeginLifetimeScope())
                {
                    writer = scope.Resolve<ISPItemValueWriter>();
                }

                // Act
                writer.WriteValueToSPListItem(fakeListItem, new FieldValueInfo(fieldInfo, null));

                // Assert
                Assert.IsTrue(correctWriterWasUsed, "The SPItemBaseValueWriter should have been used for the DateTimeFieldInfo type.");
            }
        }

        [TestMethod]
        public void WriteValueToSPListItem_GivenGuidFieldInfo_ShouldUseSPItemBaseValueWriter()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                var correctWriterWasUsed = false;
                var fieldInfo = new GuidFieldInfo("InternalName", Guid.NewGuid(), string.Empty, string.Empty, string.Empty);

                ShimSPItemBaseValueWriter.AllInstances.WriteValueToSPListItemSPListItemFieldValueInfo = (inst, listItem, fieldValueInfo) =>
                {
                    correctWriterWasUsed = true;

                    return listItem;
                };

                var fakeListItem = new ShimSPListItem().Instance;

                ISPItemValueWriter writer;
                using (var scope = UnitTestServiceLocator.BeginLifetimeScope())
                {
                    writer = scope.Resolve<ISPItemValueWriter>();
                }

                // Act
                writer.WriteValueToSPListItem(fakeListItem, new FieldValueInfo(fieldInfo, null));

                // Assert
                Assert.IsTrue(correctWriterWasUsed, "The SPItemBaseValueWriter should have been used for the GuidFieldInfo type.");
            }
        }

        [TestMethod]
        public void WriteValueToSPListItem_GivenHtmlFieldInfo_ShouldUseSPItemBaseValueWriter()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                var correctWriterWasUsed = false;
                var fieldInfo = new HtmlFieldInfo("InternalName", Guid.NewGuid(), string.Empty, string.Empty, string.Empty);

                ShimSPItemBaseValueWriter.AllInstances.WriteValueToSPListItemSPListItemFieldValueInfo = (inst, listItem, fieldValueInfo) =>
                {
                    correctWriterWasUsed = true;

                    return listItem;
                };

                var fakeListItem = new ShimSPListItem().Instance;

                ISPItemValueWriter writer;
                using (var scope = UnitTestServiceLocator.BeginLifetimeScope())
                {
                    writer = scope.Resolve<ISPItemValueWriter>();
                }

                // Act
                writer.WriteValueToSPListItem(fakeListItem, new FieldValueInfo(fieldInfo, null));

                // Assert
                Assert.IsTrue(correctWriterWasUsed, "The SPItemBaseValueWriter should have been used for the HtmlFieldInfo type.");
            }
        }

        [TestMethod]
        public void WriteValueToSPListItem_GivenImageFieldInfo_ShouldUseSPItemImageValueWriter()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                var correctWriterWasUsed = false;
                var fieldInfo = new ImageFieldInfo("InternalName", Guid.NewGuid(), string.Empty, string.Empty, string.Empty);

                ShimSPItemImageValueWriter.AllInstances.WriteValueToSPListItemSPListItemFieldValueInfo = (inst, listItem, fieldValueInfo) =>
                {
                    correctWriterWasUsed = true;

                    return listItem;
                };

                var fakeListItem = new ShimSPListItem().Instance;

                ISPItemValueWriter writer;
                using (var scope = UnitTestServiceLocator.BeginLifetimeScope())
                {
                    writer = scope.Resolve<ISPItemValueWriter>();
                }

                // Act
                writer.WriteValueToSPListItem(fakeListItem, new FieldValueInfo(fieldInfo, null));

                // Assert
                Assert.IsTrue(correctWriterWasUsed, "The SPItemImageValueWriter should have been used for the ImageFieldInfo type.");
            }
        }

        [TestMethod]
        public void WriteValueToSPListItem_GivenLookupFieldInfo_ShouldUseSPItemLookupValueWriter()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                var correctWriterWasUsed = false;
                var fieldInfo = new LookupFieldInfo("InternalName", Guid.NewGuid(), string.Empty, string.Empty, string.Empty);

                ShimSPItemLookupValueWriter.AllInstances.WriteValueToSPListItemSPListItemFieldValueInfo = (inst, listItem, fieldValueInfo) =>
                {
                    correctWriterWasUsed = true;

                    return listItem;
                };

                var fakeListItem = new ShimSPListItem().Instance;

                ISPItemValueWriter writer;
                using (var scope = UnitTestServiceLocator.BeginLifetimeScope())
                {
                    writer = scope.Resolve<ISPItemValueWriter>();
                }

                // Act
                writer.WriteValueToSPListItem(fakeListItem, new FieldValueInfo(fieldInfo, null));

                // Assert
                Assert.IsTrue(correctWriterWasUsed, "The SPItemLookupValueWriter should have been used for the LookupFieldInfo type.");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void WriteValueToSPListItem_GivenLookupMultiFieldInfo_ExpectNotSupportedException()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                var fieldInfo = new LookupMultiFieldInfo("InternalName", Guid.NewGuid(), string.Empty, string.Empty, string.Empty);
                var fakeListItem = new ShimSPListItem().Instance;

                ISPItemValueWriter writer;
                using (var scope = UnitTestServiceLocator.BeginLifetimeScope())
                {
                    writer = scope.Resolve<ISPItemValueWriter>();
                }

                // Act
                writer.WriteValueToSPListItem(fakeListItem, new FieldValueInfo(fieldInfo, null));

                // Assert
                // Expect Not Supported Exception 
            }
        }

        [TestMethod]
        public void WriteValueToSPListItem_GivenMinimalFieldInfo_ShouldUseSPItemBaseValueWriter()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                var correctWriterWasUsed = false;
                var fieldInfo = new MinimalFieldInfo("InternalName", Guid.NewGuid());

                ShimSPItemBaseValueWriter.AllInstances.WriteValueToSPListItemSPListItemFieldValueInfo = (inst, listItem, fieldValueInfo) =>
                {
                    correctWriterWasUsed = true;

                    return listItem;
                };

                var fakeListItem = new ShimSPListItem().Instance;

                ISPItemValueWriter writer;
                using (var scope = UnitTestServiceLocator.BeginLifetimeScope())
                {
                    writer = scope.Resolve<ISPItemValueWriter>();
                }

                // Act
                writer.WriteValueToSPListItem(fakeListItem, new FieldValueInfo(fieldInfo, null));

                // Assert
                Assert.IsTrue(correctWriterWasUsed, "The SPItemBaseValueWriter should have been used for the MinimalFieldInfo type.");
            }
        }

        [TestMethod]
        public void WriteValueToSPListItem_GivenNoteFieldInfo_ShouldUseSPItemBaseValueWriter()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                var correctWriterWasUsed = false;
                var fieldInfo = new NoteFieldInfo("InternalName", Guid.NewGuid(), string.Empty, string.Empty, string.Empty);

                ShimSPItemBaseValueWriter.AllInstances.WriteValueToSPListItemSPListItemFieldValueInfo = (inst, listItem, fieldValueInfo) =>
                {
                    correctWriterWasUsed = true;

                    return listItem;
                };

                var fakeListItem = new ShimSPListItem().Instance;

                ISPItemValueWriter writer;
                using (var scope = UnitTestServiceLocator.BeginLifetimeScope())
                {
                    writer = scope.Resolve<ISPItemValueWriter>();
                }

                // Act
                writer.WriteValueToSPListItem(fakeListItem, new FieldValueInfo(fieldInfo, null));

                // Assert
                Assert.IsTrue(correctWriterWasUsed, "The SPItemBaseValueWriter should have been used for the NoteFieldInfo type.");
            }
        }

        [TestMethod]
        public void WriteValueToSPListItem_GivenNumberFieldInfo_ShouldUseSPItemBaseValueWriter()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                var correctWriterWasUsed = false;
                var fieldInfo = new NumberFieldInfo("InternalName", Guid.NewGuid(), string.Empty, string.Empty, string.Empty);

                ShimSPItemBaseValueWriter.AllInstances.WriteValueToSPListItemSPListItemFieldValueInfo = (inst, listItem, fieldValueInfo) =>
                {
                    correctWriterWasUsed = true;

                    return listItem;
                };

                var fakeListItem = new ShimSPListItem().Instance;

                ISPItemValueWriter writer;
                using (var scope = UnitTestServiceLocator.BeginLifetimeScope())
                {
                    writer = scope.Resolve<ISPItemValueWriter>();
                }

                // Act
                writer.WriteValueToSPListItem(fakeListItem, new FieldValueInfo(fieldInfo, null));

                // Assert
                Assert.IsTrue(correctWriterWasUsed, "The SPItemBaseValueWriter should have been used for the NumberFieldInfo type.");
            }
        }

        [TestMethod]
        public void WriteValueToSPListItem_GivenTaxonomyFieldInfo_ShouldUseSPItemTaxonomyValueWriter()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                var correctWriterWasUsed = false;
                var fieldInfo = new TaxonomyFieldInfo("InternalName", Guid.NewGuid(), string.Empty, string.Empty, string.Empty);

                ShimSPItemTaxonomyValueWriter.AllInstances.WriteValueToSPListItemSPListItemFieldValueInfo = (inst, listItem, fieldValueInfo) =>
                {
                    correctWriterWasUsed = true;

                    return listItem;
                };

                var fakeListItem = new ShimSPListItem().Instance;

                ISPItemValueWriter writer;
                using (var scope = UnitTestServiceLocator.BeginLifetimeScope())
                {
                    writer = scope.Resolve<ISPItemValueWriter>();
                }

                // Act
                writer.WriteValueToSPListItem(fakeListItem, new FieldValueInfo(fieldInfo, null));

                // Assert
                Assert.IsTrue(correctWriterWasUsed, "The SPItemTaxonomyValueWriter should have been used for the TaxonomyFieldInfo type.");
            }
        }

        [TestMethod]
        public void WriteValueToSPListItem_GivenTaxonomyMultiFieldInfo_ShouldUseSPItemTaxonomyMultiValueWriter()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                var correctWriterWasUsed = false;
                var fieldInfo = new TaxonomyMultiFieldInfo("InternalName", Guid.NewGuid(), string.Empty, string.Empty, string.Empty);

                ShimSPItemTaxonomyMultiValueWriter.AllInstances.WriteValueToSPListItemSPListItemFieldValueInfo = (inst, listItem, fieldValueInfo) =>
                {
                    correctWriterWasUsed = true;

                    return listItem;
                };

                var fakeListItem = new ShimSPListItem().Instance;

                ISPItemValueWriter writer;
                using (var scope = UnitTestServiceLocator.BeginLifetimeScope())
                {
                    writer = scope.Resolve<ISPItemValueWriter>();
                }

                // Act
                writer.WriteValueToSPListItem(fakeListItem, new FieldValueInfo(fieldInfo, null));

                // Assert
                Assert.IsTrue(correctWriterWasUsed, "The SPItemTaxonomyMultiValueWriter should have been used for the TaxonomyMultiFieldInfo type.");
            }
        }

        [TestMethod]
        public void WriteValueToSPListItem_GivenTextFieldInfo_ShouldUseSPItemBaseValueWriter()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                var correctWriterWasUsed = false;
                var fieldInfo = new TextFieldInfo("InternalName", Guid.NewGuid(), string.Empty, string.Empty, string.Empty);

                ShimSPItemBaseValueWriter.AllInstances.WriteValueToSPListItemSPListItemFieldValueInfo = (inst, listItem, fieldValueInfo) =>
                {
                    correctWriterWasUsed = true;

                    return listItem;
                };

                var fakeListItem = new ShimSPListItem().Instance;

                ISPItemValueWriter writer;
                using (var scope = UnitTestServiceLocator.BeginLifetimeScope())
                {
                    writer = scope.Resolve<ISPItemValueWriter>();
                }

                // Act
                writer.WriteValueToSPListItem(fakeListItem, new FieldValueInfo(fieldInfo, null));

                // Assert
                Assert.IsTrue(correctWriterWasUsed, "The SPItemBaseValueWriter should have been used for the TextFieldInfo type.");
            }
        }

        [TestMethod]
        public void WriteValueToSPListItem_GivenUrlFieldFieldInfo_ShouldUseSPItemBaseValueWriter()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                var correctWriterWasUsed = false;
                var fieldInfo = new UrlFieldFieldInfo("InternalName", Guid.NewGuid(), string.Empty, string.Empty, string.Empty);

                ShimSPItemUrlValueWriter.AllInstances.WriteValueToSPListItemSPListItemFieldValueInfo = (inst, listItem, fieldValueInfo) =>
                {
                    correctWriterWasUsed = true;

                    return listItem;
                };

                var fakeListItem = new ShimSPListItem().Instance;

                ISPItemValueWriter writer;
                using (var scope = UnitTestServiceLocator.BeginLifetimeScope())
                {
                    writer = scope.Resolve<ISPItemValueWriter>();
                }

                // Act
                writer.WriteValueToSPListItem(fakeListItem, new FieldValueInfo(fieldInfo, null));

                // Assert
                Assert.IsTrue(correctWriterWasUsed, "The SPItemUrlValueWriter should have been used for the UrlFieldFieldInfo type.");
            }
        }

        [TestMethod]
        public void WriteValueToSPListItem_GivenUserFieldFieldInfo_ShouldUseSPItemUserValueWriter()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                var correctWriterWasUsed = false;
                var fieldInfo = new UserFieldFieldInfo("InternalName", Guid.NewGuid(), string.Empty, string.Empty, string.Empty);

                ShimSPItemUserValueWriter.AllInstances.WriteValueToSPListItemSPListItemFieldValueInfo = (inst, listItem, fieldValueInfo) =>
                {
                    correctWriterWasUsed = true;

                    return listItem;
                };

                var fakeListItem = new ShimSPListItem().Instance;

                ISPItemValueWriter writer;
                using (var scope = UnitTestServiceLocator.BeginLifetimeScope())
                {
                    writer = scope.Resolve<ISPItemValueWriter>();
                }

                // Act
                writer.WriteValueToSPListItem(fakeListItem, new FieldValueInfo(fieldInfo, null));

                // Assert
                Assert.IsTrue(correctWriterWasUsed, "The SPItemUserValueWriter should have been used for the UserFieldFieldInfo type.");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void WriteValueToSPListItem_GivenUserMultiFieldFieldInfo_ExpectNotSupportedException()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                var fieldInfo = new UserMultiFieldFieldInfo("InternalName", Guid.NewGuid(), string.Empty, string.Empty, string.Empty);
                var fakeListItem = new ShimSPListItem().Instance;

                ISPItemValueWriter writer;
                using (var scope = UnitTestServiceLocator.BeginLifetimeScope())
                {
                    writer = scope.Resolve<ISPItemValueWriter>();
                }

                // Act
                writer.WriteValueToSPListItem(fakeListItem, new FieldValueInfo(fieldInfo, null));

                // Assert
                // Expect Not Supported Exception 
            }
        }

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

                ISPItemValueWriter writer;
                SPListItem fakeListItem = fakeListItemShim.Instance;

                using (var scope = UnitTestServiceLocator.BeginLifetimeScope())
                {
                    writer = scope.Resolve<ISPItemValueWriter>();
                }

                // Act
                writer.WriteValueToSPListItem(fakeListItem, new FieldValueInfo(setField, setValue));

                // Assert
            }
        }
        #endregion
    }
}