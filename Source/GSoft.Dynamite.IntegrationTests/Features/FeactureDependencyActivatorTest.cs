using System;
using System.Globalization;
using System.Linq;
using Autofac;
using GSoft.Dynamite.ContentTypes;
using GSoft.Dynamite.Features;
using GSoft.Dynamite.Features.Types;
using Microsoft.SharePoint;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSoft.Dynamite.IntegrationTests.Features
{
    /// <summary>
    /// Validates the entire stack of behavior behind <see cref="FeactureDependencyActivatorTest"/>.
    /// The GSoft.Dynamite.wsp package (GSoft.Dynamite.SP project) needs to be 
    /// deployed to the current server environment before running these tests.
    /// Redeploy the WSP package every time GSoft.Dynamite.dll changes.
    /// </summary>
    [TestClass]
    public class FeactureDependencyActivatorTest
    {
        /// <summary>
        /// Validates that EnsureFeatureActivation activates the site scoped feature on the site collection if it hasn't already been activated.
        /// </summary>
        [TestMethod]
        [TestCategory(IntegrationTestCategories.Sanity)]
        public void EnsureFeatureActivation_WhenSiteScopedFeatureNotActivated_ShouldActivate()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var site = testScope.SiteCollection;

                // Use the "GSoft.Dynamite Javascript Imports" site scoped feature
                var featureDependency = new FeatureDependencyInfo()
                {
                    FeatureId = new Guid("7ed769f5-b01b-4597-9a91-3cfcdf8cc49a"),
                    FeatureActivationMode = FeatureActivationMode.CurrentSite
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var featureDependencyActivator = injectionScope.Resolve<IFeatureDependencyActivator>(new TypedParameter(typeof(SPSite), site));

                    // Act
                    featureDependencyActivator.EnsureFeatureActivation(featureDependency);

                    // Assert
                    var isFeatureActivated = site.Features.Any(feature => feature.DefinitionId.Equals(featureDependency.FeatureId));
                    Assert.IsTrue(isFeatureActivated);
                }
            }
        }

        /// <summary>
        /// Validates that EnsureFeatureActivation activates the web scoped feature on the web if it hasn't already been activated.
        /// </summary>
        [TestMethod]
        [TestCategory(IntegrationTestCategories.Sanity)]
        public void EnsureFeatureActivation_WhenWebScopedFeatureNotActivated_ShouldActivate()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var web = testScope.SiteCollection.RootWeb;

                var featureDependency = new FeatureDependencyInfo()
                {
                    Name = "OOTB task list",
                    FeatureId = new Guid("00BFEA71-A83E-497E-9BA0-7A5C597D0107"),
                    FeatureActivationMode = FeatureActivationMode.CurrentWeb
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var featureDependencyActivator = injectionScope.Resolve<IFeatureDependencyActivator>(new TypedParameter(typeof(SPWeb), web));

                    // Act
                    featureDependencyActivator.EnsureFeatureActivation(featureDependency);

                    // Assert
                    var isFeatureActivated = web.Features.Any(feature => feature.DefinitionId.Equals(featureDependency.FeatureId));
                    Assert.IsTrue(isFeatureActivated);
                }
            }
        }

        /// <summary>
        /// Validates that EnsureFeatureActivation activates the web scoped feature on the web if it's already
        /// activated and the "ForceReactivation" is set to true
        /// </summary>
        [TestMethod]
        [TestCategory(IntegrationTestCategories.Sanity)]
        public void EnsureFeatureActivation_WhenFeatureIsActivatedAndForceReactivation_SholdDeactivationAndReactivate()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var web = testScope.SiteCollection.RootWeb;

                var featureDependency = new FeatureDependencyInfo()
                {
                    Name = "OOTB task list",
                    FeatureId = new Guid("00BFEA71-A83E-497E-9BA0-7A5C597D0107"),
                    FeatureActivationMode = FeatureActivationMode.CurrentWeb,
                    ForceReactivation = true
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var featureDependencyActivator = injectionScope.Resolve<IFeatureDependencyActivator>(new TypedParameter(typeof(SPWeb), web));
                    featureDependencyActivator.EnsureFeatureActivation(featureDependency);
                    var isFeaturePreviouslyActivated = web.Features.Any(feature => feature.DefinitionId.Equals(featureDependency.FeatureId));

                    // Act
                    featureDependencyActivator.EnsureFeatureActivation(featureDependency);
                    var isFeatureStillActivated = web.Features.Any(feature => feature.DefinitionId.Equals(featureDependency.FeatureId));

                    // Assert
                    Assert.IsTrue(isFeaturePreviouslyActivated);
                    Assert.IsTrue(isFeatureStillActivated);
                }
            }
        }

        /// <summary>
        /// Validates that EnsureFeatureActivation throws an invalid operation exception when 
        /// using the wrong activation mode or injecting the wrong object according to the selected feature activation mode.
        /// ex: Try to activate a web scoped feature while injecting the web object but using the "CurrentSite" activation mode.
        /// </summary>
        [TestMethod]
        [TestCategory(IntegrationTestCategories.Sanity)]
        [ExpectedException(typeof(InvalidOperationException), "Invalid operation exception not thrown")]
        public void EnsureFeatureActivation_WhenWebScopedFeatureActivatedWithWrongActivationMode_ShouldThrowInvalidOperationException()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var web = testScope.SiteCollection.RootWeb;

                var featureDependency = new FeatureDependencyInfo()
                {
                    Name = "OOTB task list with wrong activation mode",
                    FeatureId = new Guid("00BFEA71-A83E-497E-9BA0-7A5C597D0107"),
                    FeatureActivationMode = FeatureActivationMode.CurrentSite
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var featureDependencyActivator = injectionScope.Resolve<IFeatureDependencyActivator>(new TypedParameter(typeof(SPWeb), web));

                    // Act
                    featureDependencyActivator.EnsureFeatureActivation(featureDependency);

                    // Assert invalid operation exception
                }
            }
        }

        /// <summary>
        /// Validates that EnsureFeatureActivation throws an invalid operation exception when 
        /// trying to activate the feature on the wrong scope.
        /// ex: Try to activate a web scoped feature while injecting a site object and using the "CurrentSite" activation mode.
        /// </summary>
        [TestMethod]
        [TestCategory(IntegrationTestCategories.Sanity)]
        [ExpectedException(typeof(InvalidOperationException), "Invalid operation exception not thrown by SharePoint")]
        public void EnsureFeatureActivation_WhenWebScopedFeatureActivatedOnWrongScope_ShouldThrowInvalidOperationException()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var site = testScope.SiteCollection;

                var featureDependency = new FeatureDependencyInfo()
                {
                    Name = "OOTB task list on wrong scope",
                    FeatureId = new Guid("00BFEA71-A83E-497E-9BA0-7A5C597D0107"),
                    FeatureActivationMode = FeatureActivationMode.CurrentSite
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var featureDependencyActivator = injectionScope.Resolve<IFeatureDependencyActivator>(new TypedParameter(typeof(SPSite), site));

                    // Act
                    featureDependencyActivator.EnsureFeatureActivation(featureDependency);

                    // Assert invalid operation exception
                }
            }
        }

        /// <summary>
        /// Validates that EnsureFeatureActivation throws an argument exception when 
        /// trying to activate the feature and not defining a feature ID.
        /// </summary>
        [TestMethod]
        [TestCategory(IntegrationTestCategories.Sanity)]
        [ExpectedException(typeof(ArgumentException), "Argument exception not thrown")]
        public void EnsureFeatureActivation_WhenFeatureActivatedWithNoIdSpecified_ShouldThrowArgumentException()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var site = testScope.SiteCollection;

                var featureDependency = new FeatureDependencyInfo()
                {
                    Name = "Unknown feature because there's no ID",
                    FeatureActivationMode = FeatureActivationMode.CurrentSite
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var featureDependencyActivator = injectionScope.Resolve<IFeatureDependencyActivator>(new TypedParameter(typeof(SPSite), site));

                    // Act
                    featureDependencyActivator.EnsureFeatureActivation(featureDependency);

                    // Assert argument exception
                }
            }
        }
    }
}
