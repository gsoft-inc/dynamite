using System;
using System.Text.RegularExpressions;

namespace GSoft.Dynamite.Extensions
{
    public class GuidExtension
    {
        
        private static readonly string _guidMatchPattern =  "^[A-Fa-f0-9]{32}$|" + 
            "^({|\\()?[A-Fa-f0-9]{8}-([A-Fa-f0-9]{4}-){3}[A-Fa-f0-9]{12}(}|\\))?$|" +
            "^({)?[0xA-Fa-f0-9]{3,10}(, {0,1}[0xA-Fa-f0-9]{3,6}){2}, {0,1}({)([0xA-Fa-f0-9]{3,4}, {0,1}){7}[0xA-Fa-f0-9]{3,4}(}})$";

        public static bool TryParse(string s, out Guid result)
        {
            result = Guid.Empty;
            if (string.IsNullOrEmpty(s) || !(new Regex(_guidMatchPattern)).IsMatch(s)) { return false; }
            result = new Guid(s);
            return true;
        }
    }
}
