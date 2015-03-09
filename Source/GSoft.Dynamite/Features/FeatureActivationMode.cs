namespace GSoft.Dynamite.Features
{
    /// <summary>
    /// Feature activation mode
    /// </summary>
    public enum FeatureActivationMode
    {
        /// <summary>
        /// Activates the site scoped feature on the current site collection.
        /// </summary>
        CurrentSite = 0,

        /// <summary>
        /// Activates the web scoped feature on the current web.
        /// </summary>
        CurrentWeb = 1
    }
}
