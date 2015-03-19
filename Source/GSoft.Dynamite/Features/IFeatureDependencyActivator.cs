using System.Collections.Generic;
using GSoft.Dynamite.Features.Types;

namespace GSoft.Dynamite.Features
{
    /// <summary>
    /// Feature dependency activator interface.
    /// </summary>
    public interface IFeatureDependencyActivator
    {
        /// <summary>
        /// Ensures the feature activation.
        /// </summary>
        /// <param name="featureDependency">The feature dependency.</param>
        void EnsureFeatureActivation(FeatureDependencyInfo featureDependency);

        /// <summary>
        /// Ensures the feature activation.
        /// </summary>
        /// <param name="featureDependencies">The feature dependencies.</param>
        void EnsureFeatureActivation(IEnumerable<FeatureDependencyInfo> featureDependencies);

        /// <summary>
        /// Ensures the feature activation.
        /// </summary>
        /// <param name="featureDependencyConfig">The feature dependency configuration.</param>
        void EnsureFeatureActivation(IFeatureDependencyConfig featureDependencyConfig);
    }
}
