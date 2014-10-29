namespace GSoft.Dynamite.Globalization
{
    using System.Diagnostics.CodeAnalysis;

    using Microsoft.SharePoint;

    public interface IRegionalSettingsHelper
    {
        /// <summary>
        /// The french canadian custom regional settings
        /// </summary>
        /// <param name="web">The current web</param>
        /// <param name="isUserSetting">Whether the setting is meant for a user or a SPWeb</param>
        /// <returns>The french canadian settings</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        SPRegionalSettings FrenchCanadianSettings(SPWeb web, bool isUserSetting);

        /// <summary>
        /// The en-CA default web regional settings
        /// </summary>
        /// <param name="web">The current web</param>
        /// <param name="isUserSetting">Whether the setting is meant for a user or a SPWeb</param>
        /// <returns>The en-CA settings</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        SPRegionalSettings EnglishCanadianSettings(SPWeb web, bool isUserSetting);

        /// <summary>
        /// Applies regional settings on the current user that correspond to the
        /// language the user is switching towards.
        /// </summary>
        /// <param name="targetLcid">The target language LCID, either 1033 for English or 1036 for French.</param>
        void SwitchCurrentUserRegionalSettings(int targetLcid);

        /// <summary>
        /// Registers the canadian english culture as default web regional settings
        /// </summary>
        /// <param name="web">The current web</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        void InitializeWebDefaultRegionalSettings(SPWeb web);
    }
}