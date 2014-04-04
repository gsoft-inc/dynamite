using System;

namespace GSoft.Dynamite.Caching
{
    /// <summary>
    /// Defines contract for bilingual (french/english)
    /// content caching keys
    /// </summary>
    [Obsolete]
    public interface ICacheKey
    {
        /// <summary>
        /// Get english key
        /// </summary>
        string InEnglish
        {
            get;
        }

        /// <summary>
        /// Get french key
        /// </summary>
        string InFrench
        {
            get;
        }
    }
}
