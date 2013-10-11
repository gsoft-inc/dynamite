using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace GSoft.Dynamite.Sharepoint.Utils
{
    /// <summary>
    /// Helps in setting custom regional settings for users as they switch MUI.
    /// Assumes that the web is created in English and that the French language
    /// pack is installed.
    /// Also assumes that InitializeWebDefaultRegionalSettings is called during
    /// the setup phase of the site to change the web's regional settings to en-CA.
    /// </summary>
    [CLSCompliant(false)]
    public class RegionalSettingsHelper
    {
        /// <summary>
        /// The default time zone - ID 10 is UTC-5 (Eastern Time)
        /// </summary>
        private static ushort DefaultTimeZoneID = 10;

        /// <summary>
        /// The french canadian custom regional settings
        /// </summary>
        /// <param name="web">The current web</param>
        /// <param name="isUserSetting">Whether the setting is meant for a user or a SPWeb</param>
        /// <returns>The french canadian settings</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public SPRegionalSettings FrenchCanadianSettings(SPWeb web, bool isUserSetting)
        {
            var settings = new SPRegionalSettings(web, isUserSetting);
            settings.LocaleId = (uint)new CultureInfo("fr-CA").LCID;
            settings.TimeZone.ID = DefaultTimeZoneID; // UTC-5 (Eastern Time)
            settings.Time24 = true;
            return settings;
        }

        /// <summary>
        /// The en-CA default web regional settings
        /// </summary>
        /// <param name="web">The current web</param>
        /// <param name="isUserSetting">Whether the setting is meant for a user or a SPWeb</param>
        /// <returns>The en-CA settings</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public SPRegionalSettings EnglishCanadianSettings(SPWeb web, bool isUserSetting)
        {
            var settings = new SPRegionalSettings(web, isUserSetting);
            settings.LocaleId = (uint)new CultureInfo("en-CA").LCID;
            settings.TimeZone.ID = DefaultTimeZoneID;
            settings.Time24 = false;
            return settings;
        }

        /// <summary>
        /// Applies regional settings on the current user that correspond to the
        /// language the user is switching towards.
        /// </summary>
        /// <param name="targetLcid">The target language LCID, either 1033 for English or 1036 for French.</param>
        public void SwitchCurrentUserRegionalSettings(int targetLcid)
        {
            if (SPContext.Current.Web.CurrentUser != null)
            {
                // Don't try to change regional settings if anonymous
                int userId = SPContext.Current.Web.CurrentUser.ID;
                ushort currentUserOriginalTimeZoneID = DefaultTimeZoneID;

                if (SPContext.Current.Web.CurrentUser.RegionalSettings != null)
                {
                    currentUserOriginalTimeZoneID = SPContext.Current.Web.CurrentUser.RegionalSettings.TimeZone.ID;
                }

                SPContext.Current.Web.RunAsSystem(web =>
                {
                    SPUser currentUser = web.AllUsers.GetByID(userId);

                    // Note that a user's regional settings will be changed _if and only if_ he has no 
                    // regional settings yet or if his Time24 setting doesn't fit with English (AM/PM) 
                    // or French (24h) convention
                    if (targetLcid == Language.English.Culture.LCID)
                    {
                        currentUser.RegionalSettings = this.EnglishCanadianSettings(web, true);
                        currentUser.RegionalSettings.TimeZone.ID = currentUserOriginalTimeZoneID;   // Don't overwrite the user's timezone with default EST
                        currentUser.Update();
                    }
                    else if (targetLcid == Language.French.Culture.LCID)
                    {
                        currentUser.RegionalSettings = this.FrenchCanadianSettings(web, true);
                        currentUser.RegionalSettings.TimeZone.ID = currentUserOriginalTimeZoneID;   // Don't overwrite the user's timezone with default EST
                        currentUser.Update();
                    }
                });
            }
        }

        /// <summary>
        /// Registers the canadian english culture as default web regional settings
        /// </summary>
        /// <param name="web">The current web</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public void InitializeWebDefaultRegionalSettings(SPWeb web)
        {
            // Web regional settings should en-CA or fr-CA, depending on web default language
            var settings = web.Language == (uint)Language.English.Culture.LCID ? this.EnglishCanadianSettings(web, false) : this.FrenchCanadianSettings(web, false);
            web.RegionalSettings = settings;
            web.Update();
        }
    }
}
