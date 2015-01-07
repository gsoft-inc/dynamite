using Autofac;
using GSoft.Dynamite.Binding.IO;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.Fields.Constants;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.SharePoint;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSoft.Dynamite.UnitTests.Binding.IO
{
    [TestClass]
    public class SPItemValueWriterTest
    {
        [TestMethod]
        public void WriteValueToSPListItem_WhenGivenFieldValue_ShouldUpdateFieldValue()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                var setValue = "SomeTitle";
                var setField = BuiltInFields.Title;

                var fakeListItemShim = new Microsoft.SharePoint.Fakes.ShimSPListItem()
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
    }
}