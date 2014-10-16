using Microsoft.Office.Server.Search.Administration;

namespace GSoft.Dynamite.Definitions
{
    
    public class ResultTypeRuleInfo
    {
        public ResultTypeRuleInfo(ManagedPropertyInfo propertyName,PropertyRuleOperator.DefaultOperator propertyOperator, string[] values)
        {
            this.PropertyName = propertyName.Name;
            this.Operator = propertyOperator;
            this.Values = values;
        }

        public string PropertyName { get; private set; }

        public PropertyRuleOperator.DefaultOperator Operator { get; private set; }

        public string [] Values { get; private set; }   
    }
}
