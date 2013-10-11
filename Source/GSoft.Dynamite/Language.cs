using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace GSoft.Dynamite.Sharepoint2013
{
    /// <summary>
    /// A class representing a language.
    /// </summary>
    public class Language
    {
        /// <summary>
        /// The French language.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "This object is immutable.")]
        public static readonly Language French = new Language(new CultureInfo("fr-FR"));

        /// <summary>
        /// The English language.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "This object is immutable.")]
        public static readonly Language English = new Language(new CultureInfo("en-US"));

        /// <summary>
        /// Initializes a new instance of the <see cref="Language"/> class.
        /// </summary>
        /// <param name="culture">The culture.</param>
        public Language(CultureInfo culture)
        {
            this.Culture = culture;
        }

        /// <summary>
        /// The Culture. 
        /// </summary>
        public CultureInfo Culture { get; private set; }
    }
}
