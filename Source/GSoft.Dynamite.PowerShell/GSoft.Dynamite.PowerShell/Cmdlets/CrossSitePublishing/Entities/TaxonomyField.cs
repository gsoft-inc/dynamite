using System.Xml.Serialization;

namespace GSoft.Dynamite.PowerShell.Cmdlets.CrossSitePublishing.Entities
{
    /// <summary>
    /// Taxonomy field definition.
    /// </summary>
    public class TaxonomyField : BaseField
    {
        /// <summary>
        /// Gets or sets a value indicating whether [is multiple].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is multiple]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute]
        public bool IsMultiple { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [is open].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is open]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute]
        public bool IsOpen { get; set; }

        /// <summary>
        /// Gets or sets the name of the term set group.
        /// </summary>
        /// <value>
        /// The name of the term set group.
        /// </value>
        [XmlAttribute]
        public string TermSetGroupName { get; set; }

        /// <summary>
        /// Gets or sets the name of the term set.
        /// </summary>
        /// <value>
        /// The name of the term set.
        /// </value>
        [XmlAttribute]
        public string TermSetName { get; set; }

        /// <summary>
        /// Gets or sets the name of the term subset.
        /// </summary>
        /// <value>
        /// The name of the term subset.
        /// </value>
        [XmlAttribute]
        public string TermSubsetName { get; set; }
    }
}
