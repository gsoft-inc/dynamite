using System;
using System.Text.RegularExpressions;

namespace GSoft.Dynamite.Extensions
{
    /// <summary>
    /// Extension class to try parse a string to GUID
    /// </summary>
    public static class GuidExtension
    {
        private const string GuidMatchPattern = "^[A-Fa-f0-9]{32}$|" +
            "^({|\\()?[A-Fa-f0-9]{8}-([A-Fa-f0-9]{4}-){3}[A-Fa-f0-9]{12}(}|\\))?$|" +
            "^({)?[0xA-Fa-f0-9]{3,10}(, {0,1}[0xA-Fa-f0-9]{3,6}){2}, {0,1}({)([0xA-Fa-f0-9]{3,4}, {0,1}){7}[0xA-Fa-f0-9]{3,4}(}})$";

        /// <summary>
        /// Method to try parse a string to a GUID using regex.
        /// </summary>
        /// <param name="s">the input string</param>
        /// <param name="result">the GUID to return</param>
        /// <returns>true if parsed, false otherwise</returns>
        public static bool TryParse(string s, out Guid result)
        {
            result = Guid.Empty;
            if (string.IsNullOrEmpty(s) || !(new Regex(GuidMatchPattern)).IsMatch(s)) 
            { 
                return false; 
            }

            result = new Guid(s);
            return true;
        }
    }
}
