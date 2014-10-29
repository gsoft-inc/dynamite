using System;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Globalization
{
    /// <summary>
    /// SharePoint Date Helper Class
    /// </summary>
    public class DateHelper : IDateHelper
    {
        /// <summary>
        /// Get the current date corresponding to the local SharePoint SPWeb Time zone.
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="dateTime">Date to convert.</param>
        /// <returns>Current date for the SharePoint web.</returns>
        public DateTime GetSharePointDateUtc(SPWeb web, DateTime dateTime)
        {
            return web.RegionalSettings.TimeZone.LocalTimeToUTC(dateTime);
        }
    }
}
