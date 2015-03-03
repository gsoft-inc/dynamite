using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.SharePoint.Taxonomy;

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
        /// <param name="id">The term set's ID</param>
        /// <param name="label">The term set's default name</param>
        public TermSetInfo(Guid id, string label)
            : this()
        {
            this.Id = id;
            this.Label = label;
            this.Group = null;      // should assume site-collection specific term group
        }

        /// <summary>
        /// Constructor for single language (CurrentUICulture) TermSetInfo belonging to specific farm-wide term group
        /// </summary>
        /// <param name="id">The term set's ID</param>
        /// <param name="label">The term set's default name</param>
        /// <param name="termGroup">The parent term group</param>
        public TermSetInfo(Guid id, string label, TermGroupInfo termGroup)
            : this(id, label)
        {
            this.Group = termGroup;     // global farm term group
        }

        /// <summary>
        /// Constructor for fully translated TermSetInfo belonging to default site collection term group
        /// </summary>
        /// <param name="id">The term set's ID</param>
        /// <param name="labels">The term set's default labels</param>
        public TermSetInfo(Guid id, IDictionary<CultureInfo, string> labels) 
            : this()
        {
            this.Id = id;
            this.Labels = labels;
            this.Group = null;      // should assume site-collection specific term group
        }

        /// <summary>
        /// Constructor for fully translated TermSetInfo belonging to specific farm-wide term group
        /// </summary>
        /// <param name="id">The term set's ID</param>
        /// <param name="labels">The term set's default labels</param>
        /// <param name="termGroup">The parent term group</param>
        public TermSetInfo(Guid id, IDictionary<CultureInfo, string> labels, TermGroupInfo termGroup)
            : this(id, labels)
        {
            this.Group = termGroup;     // global farm term group
        }

        /// <summary>
        /// Convenience constructor to create TermSetInfo instances from SharePoint
        /// term set objects
        /// </summary>
        /// <param name="sharePointTermSet">The SharePoint taxonomy term set</param>
        public TermSetInfo(TermSet sharePointTermSet)
            : this(sharePointTermSet.Id, sharePointTermSet.Name, new TermGroupInfo(sharePointTermSet.Group))
        {
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
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Allow overwrite of backing store to enable easier initialization of object.")]
        public IDictionary<CultureInfo, string> Labels { get; set; }

        /// <summary>
        /// Parent Term Group definition. If this value is null, assume 
        /// default site collection term group and default farm term store.
        /// </summary>
        public TermGroupInfo Group { get; set; }
    }
}
