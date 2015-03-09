using System.Diagnostics.CodeAnalysis;

namespace GSoft.Dynamite.Caml
{
    /// <summary>
    /// CAML utility methods.
    /// </summary>
    public class CamlUtils : ICamlUtils
    {
        /// <summary>
        /// Trimming utility for rich text content returned from SPQueries
        /// </summary>
        /// <param name="toTrim">The content to trim</param>
        /// <returns>The trimmed string</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of static members discouraged in favor of non-static public member for more consistency with dependency injection")]
        public string TrimIfNotNullOrEmpty(string toTrim)
        {
            var trimmed = string.Empty;
            if (!string.IsNullOrEmpty(toTrim))
            {
                // trim zero-width-space characters (HTML: &#8203; or &#x200b;) that SharePoint likes to insert automatically
                trimmed = toTrim.Trim('\u200B');
            }

            return trimmed;
        }
    }
}
