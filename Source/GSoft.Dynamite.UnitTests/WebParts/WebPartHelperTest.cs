using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI.WebControls;

using Autofac;

using GSoft.Dynamite.Logging;
using GSoft.Dynamite.Serializers;
using GSoft.Dynamite.WebParts;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSoft.Dynamite.UnitTests.WebParts
{
    /// <summary>
    /// Validates the behavior of <see cref="WebPartHelper"/>
    /// </summary>
    [TestClass]
    public class WebPartHelperTest
    {
        /// <summary>
        /// Validates that WebPartHelper properly creates a ResponsivePlaceholderWebPart
        /// </summary>
        [TestMethod]
        public void WebPartHelper_ShouldCreateAResponsivePlaceholderWebPartWithCorrectValues()
        {
            // Arrange
            ILogger logger;
            IXmlHelper xmlHelper;

            using (var scope = UnitTestServiceLocator.BeginLifetimeScope())
            {
                logger = scope.Resolve<ILogger>();
                xmlHelper = scope.Resolve<IXmlHelper>();
            }

            var webPartHelper = new WebPartHelper(xmlHelper, logger);

            // Define values for the placeholder
            var height = 300;
            var backgroundColor = "#0092d7";
            var fontColor = "#ffffff";
            var text = "Testing";

            var expectedWebPartContent = string.Format(
                CultureInfo.InvariantCulture,
                "<div class='responsive-placeholder' style='height:{0}px;line-height:{0}px;background-color:{1};color:{2};text-align:center;'>{3}</div>",
                height,
                backgroundColor,
                fontColor,
                text);

            // Act
            var responsivePlaceholderWebPart = webPartHelper.CreateResponsivePlaceholderWebPart(height, backgroundColor, fontColor, text);

            // Assert
            Assert.IsNotNull(responsivePlaceholderWebPart);
            Assert.AreEqual(expectedWebPartContent, responsivePlaceholderWebPart.Content.InnerText);
        }

        /// <summary>
        /// Validates that WebPartHelper properly creates a ResponsivePlaceholderWebPart when adding extra css classes
        /// </summary>
        [TestMethod]
        public void WebPartHelper_ShouldCreateAResponsivePlaceholderWebPartWithCorrectValuesAndExtraCssClasses()
        {
            // Arrange
            ILogger logger;
            IXmlHelper xmlHelper;

            using (var scope = UnitTestServiceLocator.BeginLifetimeScope())
            {
                logger = scope.Resolve<ILogger>();
                xmlHelper = scope.Resolve<IXmlHelper>();
            }

            var webPartHelper = new WebPartHelper(xmlHelper, logger);

            // Define values for the placeholder
            var height = 300;
            var backgroundColor = "#0092d7";
            var fontColor = "#ffffff";
            var text = "Testing";
            var cssClasses = new List<string>
            {
                "test-class1",
                "test-class2"
            };

            var expectedClasses = "responsive-placeholder test-class1 test-class2";

            var expectedWebPartContent = string.Format(
                CultureInfo.InvariantCulture,
                "<div class='{0}' style='height:{1}px;line-height:{1}px;background-color:{2};color:{3};text-align:center;'>{4}</div>",
                expectedClasses,
                height,
                backgroundColor,
                fontColor,
                text);

            // Act
            var responsivePlaceholderWebPart = webPartHelper.CreateResponsivePlaceholderWebPart(height, backgroundColor, fontColor, text, cssClasses);

            // Assert
            Assert.IsNotNull(responsivePlaceholderWebPart);
            Assert.AreEqual(expectedWebPartContent, responsivePlaceholderWebPart.Content.InnerText);
        }
    }
}
