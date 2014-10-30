using System;
using System.Collections.Generic;
using System.Globalization;

namespace GSoft.Dynamite.Taxonomy
{
    /// <summary>
    /// Encapsulates taxonomy Term Set properties
    /// </summary>
    public class TermSetInfo
    {
        /// <summary>
        /// Default constructor for TermSetInfo for serialization purposes
        /// </summary>
        public TermSetInfo()
        {
            this.Labels = new Dictionary<CultureInfo, string>();     
        }

        /// <summary>
        /// Constructor for single language (CurrentUICulture) TermSetInfo belonging to default site collection term group
        /// </summary>
        public TermSetInfo(Guid id, string label)
            : this()
        {
            this.Id = id;
            this.Label = label;
            this.Group = null;      // should assume site-collection specific term group
        }

        /// <summary>
        /// Constructor for single language (CurrentUICulture) TermSetInfo belonging to specfic farm-wide term group
        /// </summary>
        public TermSetInfo(Guid id, string label, TermGroupInfo termGroup)
            : this(id, label)
        {
            this.Group = termGroup;     // global farm term group
        }

        /// <summary>
        /// Constructor for fully translated TermSetInfo belonging to default site collection term group
        /// </summary>
        public TermSetInfo(Guid id, IDictionary<CultureInfo, string> labels) 
            : this()
        {
            this.Id = id;
            this.Labels = labels;
            this.Group = null;      // should assume site-collection specific term group
        }

        /// <summary>
        /// Constructor for fully translated TermSetInfo belonging to specfic farm-wide term group
        /// </summary>
        public TermSetInfo(Guid id, IDictionary<CultureInfo, string> labels, TermGroupInfo termGroup)
            : this(id, labels)
        {
            this.Group = termGroup;     // global farm term group
        }

        /// <summary>
        /// Id of the term set
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Term set label in the current MUI language
        /// </summary>
        public string Label
        {
            get
            {
                // get the label for the current UI thread culture
                return this.Labels.ContainsKey(CultureInfo.CurrentUICulture) ?
                    this.Labels[CultureInfo.CurrentUICulture] : string.Empty;
            }

            set
            {
                // set the label for the current UI thread culture
                this.Labels[CultureInfo.CurrentUICulture] = value;
            }
        }

        /// <summary>
        /// Labels by languages (LCID) for the Term Set
        /// </summary>
        public IDictionary<CultureInfo, string> Labels { get; set; }

        /// <summary>
        /// Parent Term Group definition. If this value is null, assume 
        /// default site collection term group and default farm term store.
        /// </summary>
        public TermGroupInfo Group { get; set; }
    }
}
