using System.Collections.Generic;
using Microsoft.Office.Server.Search.Administration;

namespace GSoft.Dynamite.Search
{
    /// <summary>
    /// Result type rule metadata
    /// </summary>
    public class ResultTypeRuleInfo
    {
        /// <summary>
        /// Default constructor for serialization purposes
        /// </summary>
        public ResultTypeRuleInfo()
        {
        }

        /// <summary>
        /// Initializes a new result type rule
        /// </summary>
        /// <param name="property">Managed property metadata</param>
        /// <param name="propertyOperator">The operator</param>
        /// <param name="values">The associated values</param>
        public ResultTypeRuleInfo(ManagedPropertyInfo property, PropertyRuleOperator.DefaultOperator propertyOperator, ICollection<string> values)
        {
            this.PropertyName = property.Name;
            this.Operator = propertyOperator;
            this.Values = values;
        }

        /// <summary>
        /// The managed property name
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// The operator
        /// </summary>
        public PropertyRuleOperator.DefaultOperator Operator { get; set; }

        /// <summary>
        /// The associated values
        /// </summary>
        public ICollection<string> Values { get; set; }   
    }
}
