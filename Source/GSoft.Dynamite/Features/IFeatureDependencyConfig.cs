using System.Collections.Generic;
using GSoft.Dynamite.Features.Types;

namespace GSoft.Dynamite.Features
{
    /// <summary>
    /// Adds feature dependencies to your configuration implementation.
    /// </summary>
    public interface IFeatureDependencyConfig
    {
        /// <summary>
        /// Gets or sets the feature dependencies.
        /// </summary>
        /// <value>
        /// The feature dependencies.
        /// </value>
        IList<FeatureDependencyInfo> FeatureDependencies { get; }
    }
}
