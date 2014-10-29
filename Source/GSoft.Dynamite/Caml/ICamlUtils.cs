namespace GSoft.Dynamite.Caml
{
    using System.Diagnostics.CodeAnalysis;

    public interface ICamlUtils
    {
        /// <summary>
        /// Trimming utility for rich text content returned from SPQueries
        /// </summary>
        /// <param name="toTrim">The content to trim</param>
        /// <returns>The trimmed string</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of static members discouraged in favor of non-static public member for more consistency with dependency injection")]
        string TrimIfNotNullOrEmpty(string toTrim);
    }
}