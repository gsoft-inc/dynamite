using Microsoft.Office.Server.Search.Administration;

namespace GSoft.Dynamite.Search
{
    /// <summary>
    /// Result type rule metadata
    /// </summary>
    public class ResultTypeRuleInfo
    {
        /// <summary>
        /// Initializes a new result type rule
        /// </summary>
        /// <param name="property">Managed property metadata</param>
        /// <param name="propertyOperator">The operator</param>
        /// <param name="values">The associated values</param>
        public ResultTypeRuleInfo(ManagedPropertyInfo property, PropertyRuleOperator.DefaultOperator propertyOperator, string[] values)
        {
            this.PropertyName = property.Name;
            this.Operator = propertyOperator;
            this.Values = values;
        }

        /// <summary>
        /// The managed property name
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// The operator
        /// </summary>
        public PropertyRuleOperator.DefaultOperator Operator { get; private set; }

        /// <summary>
        /// The associated values
        /// </summary>
        public string[] Values { get; private set; }   
    }
}
