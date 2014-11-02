namespace GSoft.Dynamite.Globalization
{
    using System;

    using Microsoft.SharePoint;

    /// <summary>
    /// SharePoint Date Helper
    /// </summary>
    public interface IDateHelper
    {
        /// <summary>
        /// Get the current date corresponding to the local SharePoint SPWeb Time zone.
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="dateTime">Date to convert.</param>
        /// <returns>Current date for the SharePoint web.</returns>
        DateTime GetSharePointDateUtc(SPWeb web, DateTime dateTime);
    }
}