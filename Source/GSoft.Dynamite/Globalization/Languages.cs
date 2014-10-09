using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSoft.Dynamite.Globalization
{
    /// <summary>
    /// Class to interact with all the languages
    /// </summary>
    public class Languages
    {
        /// <summary>
        /// Private list of all languages available in Dynamite
        /// </summary>
        private static readonly List<Language> AvailableLanguages = new List<Language>() { Language.English, Language.French, Language.Inuktitut };

        /// <summary>
        /// Method to get the language full name from the TwoLetter Iso language
        /// </summary>
        /// <param name="twoLetterIsoLanguage">The two letter representing the language in ISO</param>
        /// <returns>The full name string</returns>
        public static string TwoLetterISOLanguageNameToFullName(string twoLetterIsoLanguage)
        {
            var candidate = AvailableLanguages.FirstOrDefault(language => language.Culture.TwoLetterISOLanguageName.ToLowerInvariant() == twoLetterIsoLanguage.ToLowerInvariant());

            if (candidate == null)
            {
                return string.Empty;
            }

            // Take only the first Name
            return candidate.Culture.NativeName.Split(' ').FirstOrDefault();
        }
    }
}
