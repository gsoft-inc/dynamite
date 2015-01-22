using System;
using Autofac;
using GSoft.Dynamite.Binding.IO;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.Fields.Types;
using GSoft.Dynamite.ValueTypes;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSoft.Dynamite.UnitTests.Binding.IO
{
    /// <summary>
    /// Test for the SPItemUrlValueWriter class.
    /// </summary>
    [TestClass]
    public class SPItemUrlValueWriterTest
    {
        #region WriteValuesToSPListItem
        /// <summary>
        /// Test for the WriteValuesToSPListItem method.
        /// When supplying a null Url, the SPListItem returned is null
        /// </summary>
        [TestMethod]
        public void WriteValuesToSpListItem_WhenGivenNullUrl_ShouldCopyNullUrl()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                var expectedField = new UrlFieldFieldInfo("InternalName", Guid.NewGuid(), null, null, null);
                var actualUrlValue = new SPFieldUrlValue();

                var fakeListItemShim = new ShimSPListItem()
                {
                    ItemSetStringObject = (internalName, value) =>
                    {
                        actualUrlValue = value as SPFieldUrlValue;
                    }
                };

                ISPItemValueWriter writer;
                var fakeListItem = fakeListItemShim.Instance;
                using (var scope = UnitTestServiceLocator.BeginLifetimeScope())
                {
                    writer = scope.Resolve<ISPItemValueWriter>();
                }

                // Act
                writer.WriteValueToSPListItem(fakeListItem, new FieldValueInfo(expectedField, null));

                // Assert
                Assert.IsNull(actualUrlValue);
            }
        }
        #endregion

        #region WriteValuesToSPListItem
        /// <summary>
        /// Test for the WriteValuesToSPListItem method.
        /// When supplying an Url value and a description, the SPListItem returned is properly updated.
        /// </summary>
        [TestMethod]
        public void WriteValuesToSpListItem_WhenGivenValueAndDescription_ShouldCopyValueAndDescription()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                var actualFieldName = string.Empty;
                var expectedField = new UrlFieldFieldInfo("InternalName", Guid.NewGuid(), null, null, null);

                var actualUrlValue = new SPFieldUrlValue();
                var expectedUrlValue = new UrlValue()
                {
                    Description = "Awesome sauce description!",
                    Url = "http://www.gsoft.com/team"
                };

                var fakeListItemShim = new ShimSPListItem()
                {
                    ItemSetStringObject = (internalName, value) =>
                    {
                        actualUrlValue = value as SPFieldUrlValue;
                        actualFieldName = internalName;
                    }
                };

                ISPItemValueWriter writer;
                var fakeListItem = fakeListItemShim.Instance;
                using (var scope = UnitTestServiceLocator.BeginLifetimeScope())
                {
                    writer = scope.Resolve<ISPItemValueWriter>();
                }

                // Act
                writer.WriteValueToSPListItem(fakeListItem, new FieldValueInfo(expectedField, expectedUrlValue));

                // Assert
                Assert.AreEqual(expectedUrlValue.Url, actualUrlValue.Url);
                Assert.AreEqual(expectedUrlValue.Description, actualUrlValue.Description);
                Assert.AreEqual(expectedField.InternalName, actualFieldName);
            }
        }
        #endregion
    }
}