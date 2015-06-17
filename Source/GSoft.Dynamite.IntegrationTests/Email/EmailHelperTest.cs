using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using Autofac;
using GSoft.Dynamite.Email;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSoft.Dynamite.IntegrationTests.Email
{
    /// <summary>
    /// Validates the entire stack of behavior behind <see cref="EmailHelperTest"/>.
    /// The GSoft.Dynamite.wsp package (GSoft.Dynamite.SP project) needs to be 
    /// deployed to the current server environment before running these tests.
    /// Redeploy the WSP package every time GSoft.Dynamite.dll changes.
    /// </summary>
    [TestClass]
    public class EmailHelperTest
    {
        /// <summary>
        /// Validates that SendEmail only sends the email to the recipient override email and modifies the body of the email to include the original recipients.
        /// </summary>
        [TestMethod]
        public void SendEmail_WhenRecipientOverrideEnabled_ShouldSendEmailOnlyToRecipientOverrideAddressAndModifyEmailBody()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var web = testScope.SiteCollection.RootWeb;
                var webApplication = testScope.SiteCollection.WebApplication;

                var RecipientOverrideEmail = "edouard.shaar@gsoft.com";
                var emailInformation = new EmailInfo();
                emailInformation.To.Add("yohan.belval@gsoft.com");
                emailInformation.To.Add("marianne.lemay@gsoft.com");
                emailInformation.CarbonCopy.Add("donald.brosseau@gsoft.com");
                emailInformation.BlindCarbonCopy.Add("julien.boulanger@gsoft.com");
                emailInformation.Subject = "Quoi faire à Barcelone?";
                emailInformation.Body = "J'ai un ami qui me propose un hike! :)";

                var originalBody = emailInformation.Body;

                // Actual values
                StringDictionary actualHeaders = null;
                string actualBody = null;
                bool? actualIsRecipientOverrideEnabled = null;

                using (ShimsContext.Create())
                {
                    // Mock the SendEmail method so no emails are actualy sent.
                    ShimSPUtility.SendEmailSPWebStringDictionaryString = (pWeb, pHeaders, pBody) =>
                    {
                        actualHeaders = pHeaders;
                        actualBody = pBody;
                        return true;
                    };

                    using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                    {
                        var emailHelper = injectionScope.Resolve<IEmailHelper>();

                        // Act
                        emailHelper.EnableRecipientOverride(webApplication, RecipientOverrideEmail);
                        actualIsRecipientOverrideEnabled = emailHelper.IsRecipientOverrideEnabled(webApplication);
                        emailHelper.SendEmail(web, emailInformation);

                        // Assert
                        Assert.IsTrue(actualIsRecipientOverrideEnabled.HasValue && actualIsRecipientOverrideEnabled.Value, "Recipient override should have been enabled.");
                        Assert.IsTrue(actualHeaders["to"] == RecipientOverrideEmail, "The email should have been sent only to the recipient override email address.");
                        Assert.IsTrue(!actualHeaders.ContainsKey("cc"), "No carbon copy should have been in the email headers.");
                        Assert.IsTrue(!actualHeaders.ContainsKey("bcc"), "No blind carbon copy should have been in the email headers.");
                        Assert.IsTrue(actualBody.Length > originalBody.Length, "Text should have been added to the body of the email.");
                    }
                }
            }
        }

        /// <summary>
        /// Validates that SendEmail sends the email to the entended addresses and does not change the email content.
        /// </summary>
        [TestMethod]
        public void SendEmail_WhenRecipientOverrideDisabled_ShouldSendEmailWithoutManipulatingTheReceiversOrChangingTheEmailContent()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var web = testScope.SiteCollection.RootWeb;
                var webApplication = testScope.SiteCollection.WebApplication;

                var emailInformation = new EmailInfo();
                emailInformation.To.Add("yohan.belval@gsoft.com");
                emailInformation.To.Add("marianne.lemay@gsoft.com");
                emailInformation.CarbonCopy.Add("donald.brosseau@gsoft.com");
                emailInformation.BlindCarbonCopy.Add("julien.boulanger@gsoft.com");
                emailInformation.Subject = "Quoi faire à Barcelone?";
                emailInformation.Body = "J'ai un ami qui me propose un hike! :)";

                // Expected values
                string expectedBody = emailInformation.Body;
                StringDictionary expectedHeaders = new StringDictionary();
                expectedHeaders.Add("to", string.Join(",", emailInformation.To));
                expectedHeaders.Add("cc", string.Join(",", emailInformation.CarbonCopy));
                expectedHeaders.Add("bcc", string.Join(",", emailInformation.BlindCarbonCopy));
                expectedHeaders.Add("subject", emailInformation.Subject);

                // Actual values
                StringDictionary actualHeaders = null;
                string actualBody = null;
                bool? actualIsRecipientOverrideEnabled = null;

                using (ShimsContext.Create())
                {
                    // Mock the SendEmail method so no emails are actualy sent.
                    ShimSPUtility.SendEmailSPWebStringDictionaryString = (pWeb, pHeaders, pBody) =>
                    {
                        actualHeaders = pHeaders;
                        actualBody = pBody;
                        return true;
                    };

                    using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                    {
                        var emailHelper = injectionScope.Resolve<IEmailHelper>();

                        // Act
                        actualIsRecipientOverrideEnabled = emailHelper.IsRecipientOverrideEnabled(webApplication);
                        emailHelper.SendEmail(web, emailInformation);

                        // Assert
                        Assert.IsTrue(actualIsRecipientOverrideEnabled.HasValue && !actualIsRecipientOverrideEnabled.Value, "Recipient override should not have been enabled.");
                        Assert.IsTrue(actualHeaders.Count == expectedHeaders.Count, "The headers should not have changed.");
                        Assert.IsTrue(actualBody == expectedBody, "The email body should not have changed.");

                        foreach (string key in actualHeaders.Keys)
                        {
                            Assert.IsTrue(actualHeaders[key] == expectedHeaders[key], "Header with key '{0}' should not have changed.", key);
                        }
                    }
                }
            }
        }
    }
}
