namespace GSoft.Dynamite.Globalization
{
    using System.Diagnostics.CodeAnalysis;

    using Microsoft.SharePoint;

    public interface IMuiHelper
    {
        /// <summary>
        /// Ensures the language support for the specified language.
        /// </summary>
        /// <param name="web">The web</param>
        /// <param name="language">The language</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        void EnsureLanguageSupport(SPWeb web, Language language);
    }
}