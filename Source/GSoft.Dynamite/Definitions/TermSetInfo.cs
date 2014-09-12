using System;
using System.Collections.Generic;
using System.Globalization;

namespace GSoft.Dynamite.Definitions
{
    /// <summary>
    /// Encapsulates Taxonomy Term Set properties
    /// </summary>
    public class TermSetInfo
    {
        /// <summary>
        /// Default constructor for TermSetInfo
        /// </summary>
        public TermSetInfo()
        {          
        }

        /// <summary>
        /// Id of the term set
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Labels by languages (LCID) for the Term Set
        /// </summary>
        public IDictionary<CultureInfo, string> Labels { get; set; }
        
        /// <summary>
        /// Terms in the term set
        /// </summary>
        public IDictionary<string, TermInfo> Terms { get; set; }   
    }
}
