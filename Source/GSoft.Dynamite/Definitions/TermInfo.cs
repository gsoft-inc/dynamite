using System;

namespace GSoft.Dynamite.Definitions
{
    /// <summary>
    /// Definition of a Taxonomy Term 
    /// </summary>
    public class TermInfo
    {
        /// <summary>
        /// Default constructor for TermInfo
        /// </summary>
        public TermInfo()
        {           
        }

        /// <summary>
        /// Name of the term
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// GUID of the term
        /// </summary>
        public Guid Id { get; set; }
    }
}
