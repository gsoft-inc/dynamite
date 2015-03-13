using System;
using System.Collections.Generic;
using System.Linq;
using GSoft.Dynamite.Features.Types;
using GSoft.Dynamite.Logging;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Features
{
    /// <summary>
    /// Feature dependency activator.
    /// </summary>
    public class FeatureDependencyActivator : IFeatureDependencyActivator
    {
        private readonly SPSite currentSite;
        private readonly SPWeb currentWeb;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureDependencyActivator" /> class.
        /// </summary>
        /// <param name="currentSite">The current site.</param>
        /// <param name="logger">The logger.</param>
        public FeatureDependencyActivator(SPSite currentSite, ILogger logger) : this(logger)
        {
            this.currentSite = currentSite;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureDependencyActivator" /> class.
        /// </summary>
        /// <param name="currentWeb">The current web.</param>
        /// <param name="logger">The logger.</param>
        public FeatureDependencyActivator(SPWeb currentWeb, ILogger logger) : this(logger)
        {
            this.currentWeb = currentWeb;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureDependencyActivator" /> class.
        /// </summary>
        /// <param name="currentSite">The current site.</param>
        /// <param name="currentWeb">The current web.</param>
        /// <param name="logger">The logger.</param>
        public FeatureDependencyActivator(SPSite currentSite, SPWeb currentWeb, ILogger logger)
            : this(logger)
        {
            this.currentSite = currentSite;
            this.currentWeb = currentWeb;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureDependencyActivator" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        private FeatureDependencyActivator(ILogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Ensures the feature activation.
        /// </summary>
        /// <param name="featureDependencyConfig">The feature dependency configuration.</param>
        public void EnsureFeatureActivation(IFeatureDependencyConfig featureDependencyConfig)
        {
            if (featureDependencyConfig != null)
            {
                this.EnsureFeatureActivation(featureDependencyConfig.FeatureDependencies);
            }
        }

        /// <summary>
        /// Ensures the feature activation.
        /// </summary>
        /// <param name="featureDependencies">The feature dependencies.</param>
        public void EnsureFeatureActivation(IEnumerable<FeatureDependencyInfo> featureDependencies)
        {
            foreach (var featureDependency in featureDependencies)
            {
                this.EnsureFeatureActivation(featureDependency);
            }
        }

        /// <summary>
        /// Ensures the feature activation.
        /// </summary>
        /// <param name="featureDependency">The feature dependency.</param>
        public void EnsureFeatureActivation(FeatureDependencyInfo featureDependency)
        {
            // Validate arguments
            if (featureDependency.FeatureId.Equals(Guid.Empty))
            {
                throw new ArgumentException("Feature ID is empty in FeatureDependencyInfo", "featureDependency");
            }

            switch (featureDependency.FeatureActivationMode)
            {
                case FeatureActivationMode.CurrentSite:

                    if (this.currentSite != null)
                    {
                        this.logger.Info(
                            "Activating the feature with id '{0}'on site '{1}'.",
                            featureDependency.FeatureId,
                            this.currentSite.Url);

                        this.InnerEnsureFeatureActivation(featureDependency, this.currentSite.Features);
                    }
                    else
                    {
                        throw new InvalidOperationException(@"Please ensure you specified the correct 'FeatureActivationMode' 
                            and injected the 'currentSite' parameter in the constructor.");
                    }

                    break;

                case FeatureActivationMode.CurrentWeb:

                    if (this.currentWeb != null)
                    {
                        this.logger.Info(
                            "Activating the feature with id '{0}'on web '{1}'.",
                            featureDependency.FeatureId,
                            this.currentWeb.Url);

                        this.InnerEnsureFeatureActivation(featureDependency, this.currentWeb.Features);
                    }
                    else
                    {
                        throw new InvalidOperationException(@"Please ensure you specified the correct 'FeatureActivationMode' 
                            and injected the 'currentWeb' parameter in the constructor.");
                    }

                    break;

                default:
                    throw new NotImplementedException("Only the 'CurrentSite' and 'CurrentWeb' activation modes are currently supported.");
            }
        }

        private void InnerEnsureFeatureActivation(FeatureDependencyInfo featureDependency, SPFeatureCollection featureCollection)
        {
            // If already activated
            if (featureCollection.Any(sf => sf.DefinitionId == featureDependency.FeatureId))
            {
                if (featureDependency.ForceReactivation)
                {
                    this.logger.Info(
                        "Disactivating the feature with id '{0}' because the 'ForceReactivation' property was used.",
                        featureDependency.FeatureId);

                    // Deactivate and reactivate feature
                    featureCollection.Remove(featureDependency.FeatureId);
                    featureCollection.Add(featureDependency.FeatureId);
                }
                else
                {
                    this.logger.Warn(
                        @"Feature with id '{0}' is already activated. If you wish to force 
                        it's reactivation, please use the 'ForceReactivation' property.",
                        featureDependency.FeatureId);
                }
            }
            else
            {
                // Activate feature
                featureCollection.Add(featureDependency.FeatureId);
            }
        }
    }
}
