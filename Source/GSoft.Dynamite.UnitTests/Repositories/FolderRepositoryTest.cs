using System;
using System.Fakes;
using GSoft.Dynamite.Repositories;
using Microsoft.QualityTools.Testing.Emulators;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Emulators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSoft.Dynamite.UnitTests.Repositories
{
    [TestClass]
    public class FolderRepositoryTest
    {

        [TestMethod]
        public void TestSharePointEmulationScope()
        {
            using (new SharePointEmulationScope(EmulationMode.Enabled))
            {
                // Arrange
                var expectedTitle = "abc";
                var tSite = new SPSite("http://server");
                var tWeb = tSite.RootWeb;
                var tList_Id = tWeb.Lists.Add("sample list", "this is a sample list that only exists in emulation", SPListTemplateType.GenericList);
                var tList = tWeb.Lists[tList_Id];

                // Act
                var tItem = tList.Items.Add();
                tItem["Title"] = expectedTitle;
                tItem.Update();

                // Assert
                Assert.AreEqual(expectedTitle, tItem["Title"]);

            } // using emulation
        }
        [TestMethod]
        public void TestShims()
        {
            using (new SharePointEmulationScope(EmulationMode.Enabled))
            {
                // Shims can be used only in a ShimsContext:
                using (ShimsContext.Create())
                {

                    // Arrange:
                    SPSite site = new SPSite("http://localhost");
                    var expectedDate = new DateTime(2000, 1, 1); 
                    // Shim DateTime.Now to return a fixed date:
                    ShimDateTime.NowGet = () => expectedDate;
                    
                    // Instantiate the component under test:
                    //var repo = new FolderRepository();


                    // Act:
                    var result = DateTime.Now;

                    // Assert: 
                    Assert.AreEqual(expectedDate, result);
    
                }
            }
        }
    }
}
