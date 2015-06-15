using System;

namespace GSoft.Dynamite.Search
{
    /// <summary>
    /// Class to hold the IDs of the built-in ranking models
    /// </summary>
    public static class BuiltInRankingModels
    {
        /// <summary>
        /// Gets the Default Search Model ID.
        /// </summary>
        public static Guid DefaultSearchModelId
        {
            get { return new Guid("8f6fd0bc-06f9-43cf-bbab-08c377e083f4"); }
        }
    }
}
