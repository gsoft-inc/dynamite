using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Globalization
{
    /// <summary>
    /// A helper class for dealing with the Multilanguage UI.
    /// </summary>
    public class MuiHelper
    {
        /// <summary>
        /// Ensures the language support for the specified language.
        /// </summary>
        /// <param name="web">The web</param>
        /// <param name="language">The language</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public void EnsureLanguageSupport(SPWeb web, Language language)
        {
            if (!web.SupportedUICultures.Contains(language.Culture))
            {
                if (!web.IsMultilingual)
                {
                    web.IsMultilingual = true;
                }

                web.AddSupportedUICulture(language.Culture);
                web.Update();
            }
        }
    }
}
