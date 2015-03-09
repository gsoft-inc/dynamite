using System;

namespace GSoft.Dynamite.Features.Types
{
    /// <summary>
    /// Information object defining a feature dependency.
    /// </summary>
    public class FeatureDependencyInfo
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the feature identifier.
        /// </summary>
        /// <value>
        /// The feature identifier.
        /// </value>
        public Guid FeatureId { get; set; }

        /// <summary>
        /// Gets or sets the feature activation mode.
        /// </summary>
        /// <value>
        /// The feature activation mode.
        /// </value>
        public FeatureActivationMode FeatureActivationMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [force reactivation].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [force reactivation]; otherwise, <c>false</c>.
        /// </value>
        public bool ForceReactivation { get; set; }
    }
}
