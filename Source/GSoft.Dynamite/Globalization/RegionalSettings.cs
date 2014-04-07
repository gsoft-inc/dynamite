using Microsoft.SharePoint;

namespace GSoft.Dynamite.Globalization
{
    /// <summary>
    /// Regional settings entity.
    /// </summary>
    public class RegionalSettings
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RegionalSettings"/> class.
        /// </summary>
        /// <param name="regionalSettings">The regional settings.</param>
        public RegionalSettings(SPRegionalSettings regionalSettings)
        {
            this.LocaleId = (int)regionalSettings.LocaleId;
            this.TimeZoneId = regionalSettings.TimeZone.ID;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the locale id.
        /// </summary>
        public int LocaleId { get; private set; }

        /// <summary>
        /// Gets the time zone id.
        /// </summary>
        public int TimeZoneId { get; private set; }

        #endregion
    }
}