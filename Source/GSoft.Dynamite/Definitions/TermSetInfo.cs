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
        /// Default constructor for TermSetInfo for serialization purposes
        /// </summary>
        public TermSetInfo()
        {          
        }

        /// <summary>
        /// Constructor for TermSetInfo belonging to default site collection term group
        /// </summary>
        public TermSetInfo(Guid id, IDictionary<CultureInfo, string> labels)
        {
            this.Id = id;
            this.Labels = labels;
        }

        /// <summary>
        /// Constructor for TermSetInfo belonging to specfic farm-wide term group
        /// </summary>
        public TermSetInfo(Guid id, IDictionary<CultureInfo, string> labels, TermGroupInfo termGroup)
        {
            this.Id = id;
            this.Labels = labels;
            this.Group = termGroup;
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
        /// Parent Term Group definition
        /// </summary>
        public TermGroupInfo Group { get; set; }
    }
}
