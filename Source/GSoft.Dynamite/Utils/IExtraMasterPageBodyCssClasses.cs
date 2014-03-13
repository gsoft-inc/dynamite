namespace GSoft.Dynamite.Utils
{
    /// <summary>
    /// Used to add CSS classes to the body of the document
    /// </summary>
    public interface IExtraMasterPageBodyCssClasses
    {
        /// <summary>
        /// Detects the user's culture, browser agent and current group/permissions and returns a string with their abbreviation
        /// </summary>
        /// <returns>Returns a combination of useful classes to do browser sniffing and other context-dependent rendering in our CSS.</returns>
        string AllExtraCssClasses { get; }

        /// <summary>
        /// Detects the current UI culture
        /// </summary>
        string CultureDetection { get; }

        /// <summary>
        /// Browser sniffing
        /// </summary>
        string BrowserDetection { get; }

        /// <summary>
        /// Edit-mode detection
        /// </summary>
        string PageModeDetection { get; }

        /// <summary>
        /// Access/permissions detection
        /// </summary>
        string PermissionsDetection { get; }
    }
}
