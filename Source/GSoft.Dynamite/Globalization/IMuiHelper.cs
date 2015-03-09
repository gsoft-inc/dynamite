namespace GSoft.Dynamite.Globalization
{
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using Microsoft.SharePoint;

    /// <summary>
    /// A helper for dealing with the Multilanguage UI.
    /// </summary>
    public interface IMuiHelper
    {
        /// <summary>
        /// Ensures the language support for the specified language.
        /// </summary>
        /// <param name="web">The web</param>
        /// <param name="language">The UI language you wish to ensure support for (language pack must be installed)</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        void EnsureLanguageSupport(SPWeb web, CultureInfo language);
    }
}