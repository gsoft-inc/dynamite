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
    }
}
