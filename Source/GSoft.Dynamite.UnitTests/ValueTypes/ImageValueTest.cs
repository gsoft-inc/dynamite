using GSoft.Dynamite.ValueTypes;
////using Microsoft.QualityTools.Testing.Emulators;
////using Microsoft.SharePoint.Emulators;
using Microsoft.SharePoint.Publishing.Fields;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSoft.Dynamite.UnitTests.ValueTypes
{
    [TestClass]
    public class ImageValueTest
    {
        private const string NewImageUrl = "/SiteCollectionImages/SampleImage.jpg";
        private const string NewHyperlink = "/Pages/SamplePage.aspx";
        private const bool NewOpenHyperlinkInNewWindow = true;
        private const string NewAlignment = "right";
        private const string NewAlternateText = "Sample alternate text for the image";
        private const int NewBorderWidth = 4;
        private const int NewHeight = 100;
        private const int NewWidth = 150;
        private const int NewHorizontalSpacing = 10;
        private const int NewVerticalSpacing = 15;

        [TestMethod]
        public void GivenAnImageFieldValueToImageValueConstructorExpectPropertiesToBeSetted()
        {
            ////using (new SharePointEmulationScope(EmulationMode.Enabled))
            ////{

            ////    //Arrange
            ////    var spImageFieldValue = new ImageFieldValue() { ImageUrl = NewImageUrl, Hyperlink = NewHyperlink, OpenHyperlinkInNewWindow = NewOpenHyperlinkInNewWindow, Alignment = NewAlignment, AlternateText = NewAlternateText, BorderWidth = NewBorderWidth, Height = NewHeight, Width = NewWidth, HorizontalSpacing = NewHorizontalSpacing, VerticalSpacing = NewVerticalSpacing };

            ////    //Act
            ////    var imageValue = new ImageValue(spImageFieldValue);

            ////    //Assert
            ////    Assert.AreEqual(NewImageUrl, imageValue.ImageUrl);
            ////    Assert.AreEqual(NewHyperlink, imageValue.Hyperlink);
            ////    Assert.AreEqual(NewOpenHyperlinkInNewWindow, imageValue.OpenHyperlinkInNewWindow);
            ////    Assert.AreEqual(NewAlignment, imageValue.Alignment);
            ////    Assert.AreEqual(NewAlternateText, imageValue.AlternateText);
            ////    Assert.AreEqual(NewBorderWidth, imageValue.BorderWidth);
            ////    Assert.AreEqual(NewHeight, imageValue.Height);
            ////    Assert.AreEqual(NewWidth, imageValue.Width);
            ////    Assert.AreEqual(NewHorizontalSpacing, imageValue.HorizontalSpacing);
            ////    Assert.AreEqual(NewVerticalSpacing, imageValue.VerticalSpacing);
            ////}
        }
    }
}
